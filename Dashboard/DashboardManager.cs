using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace GateHelper.Dashboard
{
    internal static class DashboardManager
    {
        private static readonly object _syncLock = new object();
        public static string DashboardDirectory => Path.Combine(Util.MetaFolder, "dashboard");

        // 암호화된 통계 파일 (.dat) 및 기존 평문 마이그레이션용 (.json)
        public static string StatsFilePath => Path.Combine(DashboardDirectory, "dashboard_stats.dat");
        private static string LegacyStatsFilePath => Path.Combine(DashboardDirectory, "dashboard_stats.json");

        // AES-256 대칭키 (32 bytes) & IV (16 bytes)
        private static readonly byte[] CryptoKey = Encoding.UTF8.GetBytes("GateHelper_DashStats_SecKey_2026");
        private static readonly byte[] CryptoIV = Encoding.UTF8.GetBytes("GH_Dash_IV_2026!");

        public static DashboardStats Stats { get; private set; } = new DashboardStats();

        // 실시간 대시보드 뷰 갱신 이벤트
        public static event Action StatsUpdated;

        private static readonly Stopwatch _sessionStopwatch = new Stopwatch();
        public static DateTime SessionStartTime { get; private set; } = DateTime.Now;

        // 5분 주기 자동 플러시(Flush) 타이머 및 마지막 플러시 시점 세션 분
        private static Timer _flushTimer;
        private static double _lastFlushedMinutes = 0.0;

        // 최근 중요 로그 링 버퍼 (최대 100개)
        private static readonly ConcurrentQueue<DashboardLogItem> _recentLogs = new ConcurrentQueue<DashboardLogItem>();
        private const int MaxRecentLogCount = 100;

        // CPU 사용량 측정용 이전 상태
        private static DateTime _lastCpuSampleTime = DateTime.MinValue;
        private static TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;
        private static double _cachedCpuPercent = 0.0;

        private static bool _isInitialized = false;

        /// <summary>
        /// 프로그램 시작 시 1회 호출하여 통계를 로드하고 세션을 시작합니다.
        /// </summary>
        public static void Initialize()
        {
            lock (_syncLock)
            {
                if (_isInitialized) return;
                _isInitialized = true;

                SessionStartTime = DateTime.Now;
                _sessionStopwatch.Restart();
                _lastFlushedMinutes = 0.0;

                EnsureDashboardDirectory();
                LoadStats();

                Stats.TotalLaunchCount++;
                Stats.LastLaunchDate = DateTime.Now;
                SaveStats();

                // 5분 주기(300초) 가동시간 자동 정산 및 디스크 플러시 타이머 가동
                _flushTimer = new Timer(OnPeriodicFlush, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

                // CPU 샘플러 초기화
                try
                {
                    var curProc = Process.GetCurrentProcess();
                    _lastCpuSampleTime = DateTime.UtcNow;
                    _lastTotalProcessorTime = curProc.TotalProcessorTime;
                }
                catch { }
            }
        }

        /// <summary>
        /// 5분마다 백그라운드에서 호출되어 경과된 세션 시간을 정산하고 파일에 안전하게 기록합니다.
        /// </summary>
        private static void OnPeriodicFlush(object state)
        {
            lock (_syncLock)
            {
                if (!_isInitialized) return;

                double currentSessionMins = _sessionStopwatch.Elapsed.TotalMinutes;
                double delta = currentSessionMins - _lastFlushedMinutes;
                if (delta > 0)
                {
                    Stats.TotalUptimeMinutes += delta;
                    _lastFlushedMinutes = currentSessionMins;
                    Stats.LastLaunchDate = DateTime.Now;
                    SaveStats();
                    StatsUpdated?.Invoke();
                }
            }
        }

        /// <summary>
        /// 프로그램 종료 시 호출하여 누적 가동 시간을 최종 정산하고 저장합니다.
        /// </summary>
        public static void OnAppClosing()
        {
            lock (_syncLock)
            {
                if (!_isInitialized) return;

                try { _flushTimer?.Dispose(); } catch { }

                _sessionStopwatch.Stop();
                double currentSessionMins = _sessionStopwatch.Elapsed.TotalMinutes;
                double delta = currentSessionMins - _lastFlushedMinutes;
                if (delta > 0)
                {
                    Stats.TotalUptimeMinutes += delta;
                    _lastFlushedMinutes = currentSessionMins;
                }
                Stats.LastLaunchDate = DateTime.Now;

                SaveStats();
            }
        }

        public static TimeSpan GetSessionUptime()
        {
            return _sessionStopwatch.Elapsed;
        }

        /// <summary>
        /// 누적 정산된 시간 + 마지막 5분 플러시 이후 현재까지 흐른 실시간 분(Minutes)을 합산 반환합니다.
        /// </summary>
        public static double GetEffectiveTotalUptimeMinutes()
        {
            lock (_syncLock)
            {
                double unFlushedSessionMins = Math.Max(0.0, _sessionStopwatch.Elapsed.TotalMinutes - _lastFlushedMinutes);
                return Stats.TotalUptimeMinutes + unFlushedSessionMins;
            }
        }

        #region [ File Load & Save (AES-256 Encrypted) ]

        private static void EnsureDashboardDirectory()
        {
            try
            {
                if (!Directory.Exists(DashboardDirectory))
                {
                    Directory.CreateDirectory(DashboardDirectory);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "EnsureDashboardDirectory");
            }
        }

        private static void LoadStats()
        {
            try
            {
                // 1. 암호화된 .dat 파일이 존재하는 경우 (정상)
                if (File.Exists(StatsFilePath))
                {
                    byte[] cipherBytes = File.ReadAllBytes(StatsFilePath);
                    string json = DecryptString(cipherBytes);
                    if (!string.IsNullOrEmpty(json))
                    {
                        Stats = JsonConvert.DeserializeObject<DashboardStats>(json) ?? new DashboardStats();
                        EnsureStatsIntegrity();
                        return;
                    }
                }
                // 2. 기존 평문 .json 파일이 존재하는 경우 (암호화 마이그레이션)
                else if (File.Exists(LegacyStatsFilePath))
                {
                    string json = File.ReadAllText(LegacyStatsFilePath, Encoding.UTF8);
                    Stats = JsonConvert.DeserializeObject<DashboardStats>(json) ?? new DashboardStats();
                    EnsureStatsIntegrity();

                    // 암호화된 파일로 즉시 저장 후 이전 평문 파일 삭제
                    SaveStats();
                    try { File.Delete(LegacyStatsFilePath); } catch { }
                    return;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "Dashboard LoadStats");
            }

            Stats = new DashboardStats();
            EnsureStatsIntegrity();
        }

        private static void EnsureStatsIntegrity()
        {
            if (Stats.ServerAccessCounts == null)
            {
                Stats.ServerAccessCounts = new Dictionary<string, int>();
            }
            if (Stats.HourlyAccessCounts == null || Stats.HourlyAccessCounts.Length != 24)
            {
                Stats.HourlyAccessCounts = new int[24];
            }
            if (Stats.DailyHourlyAccessCounts == null)
            {
                Stats.DailyHourlyAccessCounts = new Dictionary<string, int[]>();
            }
            string todayStr = DateTime.Today.ToString("yyyy-MM-dd");
            if (Stats.DailyHourlyAccessCounts.Count == 0 && Stats.HourlyAccessCounts != null)
            {
                Stats.DailyHourlyAccessCounts[todayStr] = (int[])Stats.HourlyAccessCounts.Clone();
            }
        }

        public static void SaveStats()
        {
            lock (_syncLock)
            {
                try
                {
                    EnsureDashboardDirectory();
                    string json = JsonConvert.SerializeObject(Stats, Formatting.Indented);
                    byte[] cipherBytes = EncryptString(json);
                    File.WriteAllBytes(StatsFilePath, cipherBytes);
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex, LogManager.Level.Warning, "Dashboard SaveStats");
                }
            }
        }

        public static void ResetStats()
        {
            lock (_syncLock)
            {
                _lastFlushedMinutes = _sessionStopwatch.Elapsed.TotalMinutes;
                Stats = new DashboardStats
                {
                    TotalLaunchCount = 1,
                    TotalUptimeMinutes = 0,
                    FirstLaunchDate = DateTime.Now,
                    LastLaunchDate = DateTime.Now
                };
                SaveStats();
                StatsUpdated?.Invoke();
            }
        }

        private static byte[] EncryptString(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            using (var aes = Aes.Create())
            {
                aes.Key = CryptoKey;
                aes.IV = CryptoIV;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        private static string DecryptString(byte[] cipherBytes)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = CryptoKey;
                    aes.IV = CryptoIV;
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"대시보드 통계 복호화 실패: {ex.Message}", LogManager.Level.Warning);
                return null;
            }
        }

        #endregion

        #region [ Event Recording Hooks ]

        public static void RecordServerConnect(string serverName)
        {
            if (string.IsNullOrWhiteSpace(serverName)) return;

            lock (_syncLock)
            {
                Stats.TotalConnectAttempts++;
                string key = serverName.Trim();
                if (Stats.ServerAccessCounts.ContainsKey(key))
                {
                    Stats.ServerAccessCounts[key]++;
                }
                else
                {
                    Stats.ServerAccessCounts[key] = 1;
                }

                if (Stats.HourlyAccessCounts == null || Stats.HourlyAccessCounts.Length != 24)
                {
                    Stats.HourlyAccessCounts = new int[24];
                }

                if (Stats.DailyHourlyAccessCounts == null)
                {
                    Stats.DailyHourlyAccessCounts = new Dictionary<string, int[]>();
                }

                string todayKey = DateTime.Today.ToString("yyyy-MM-dd");
                if (!Stats.DailyHourlyAccessCounts.TryGetValue(todayKey, out var todayCounts) || todayCounts == null || todayCounts.Length != 24)
                {
                    todayCounts = new int[24];
                    Stats.DailyHourlyAccessCounts[todayKey] = todayCounts;
                }

                int hour = DateTime.Now.Hour;
                if (hour >= 0 && hour < 24)
                {
                    Stats.HourlyAccessCounts[hour]++;
                    todayCounts[hour]++;
                }

                // 60일 이상 지난 과거 날짜 자동 정리 (초경량 유지)
                if (Stats.DailyHourlyAccessCounts.Count > 60)
                {
                    var cutoff = DateTime.Today.AddDays(-60).ToString("yyyy-MM-dd");
                    var expiredKeys = Stats.DailyHourlyAccessCounts.Keys
                        .Where(k => string.Compare(k, cutoff, StringComparison.Ordinal) < 0)
                        .ToList();
                    foreach (var exp in expiredKeys)
                    {
                        Stats.DailyHourlyAccessCounts.Remove(exp);
                    }
                }

                SaveStats();
                StatsUpdated?.Invoke();
            }
        }

        public static int[] GetHourlyAccessCounts()
        {
            lock (_syncLock)
            {
                if (Stats.HourlyAccessCounts == null || Stats.HourlyAccessCounts.Length != 24)
                {
                    Stats.HourlyAccessCounts = new int[24];
                }
                return (int[])Stats.HourlyAccessCounts.Clone();
            }
        }

        public static int[] GetTodayHourlyCounts()
        {
            lock (_syncLock)
            {
                string todayKey = DateTime.Today.ToString("yyyy-MM-dd");
                if (Stats.DailyHourlyAccessCounts != null &&
                    Stats.DailyHourlyAccessCounts.TryGetValue(todayKey, out var arr) &&
                    arr != null && arr.Length == 24)
                {
                    return (int[])arr.Clone();
                }

                return new int[24];
            }
        }

        public static List<DailyActivityItem> GetWeeklyActivity()
        {
            lock (_syncLock)
            {
                var result = new List<DailyActivityItem>();
                DateTime today = DateTime.Today;

                if (Stats.DailyHourlyAccessCounts == null)
                {
                    Stats.DailyHourlyAccessCounts = new Dictionary<string, int[]>();
                }

                for (int i = 6; i >= 0; i--)
                {
                    DateTime targetDate = today.AddDays(-i);
                    string key = targetDate.ToString("yyyy-MM-dd");
                    bool isToday = (i == 0);

                    int[] hourly = null;
                    if (Stats.DailyHourlyAccessCounts.TryGetValue(key, out var arr) && arr != null && arr.Length == 24)
                    {
                        hourly = (int[])arr.Clone();
                    }
                    else
                    {
                        hourly = new int[24];
                    }

                    int sum = hourly.Sum();
                    string label = isToday ? $"오늘 ({targetDate:ddd})" : $"{targetDate:MM/dd} ({targetDate:ddd})";

                    result.Add(new DailyActivityItem
                    {
                        Date = targetDate,
                        DateLabel = label,
                        HourlyCounts = hourly,
                        TotalCount = sum,
                        IsToday = isToday
                    });
                }

                return result;
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);

        /// <summary>
        /// 원클릭 시스템 최적화: 가비지 컬렉션 및 OS 미사용 작업셋(WorkingSet) 즉시 반환
        /// </summary>
        /// <returns>확보된 RAM 용량 (MB)</returns>
        public static double OptimizeSystem()
        {
            lock (_syncLock)
            {
                double beforeMb = 0.0;
                try
                {
                    using (var cur = Process.GetCurrentProcess())
                    {
                        beforeMb = cur.WorkingSet64 / (1024.0 * 1024.0);
                    }
                }
                catch { }

                try
                {
                    GC.Collect(2, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(2, GCCollectionMode.Forced);

                    using (var cur = Process.GetCurrentProcess())
                    {
                        SetProcessWorkingSetSize(cur.Handle, -1, -1);
                    }
                }
                catch { }

                double afterMb = 0.0;
                try
                {
                    using (var cur = Process.GetCurrentProcess())
                    {
                        afterMb = cur.WorkingSet64 / (1024.0 * 1024.0);
                    }
                }
                catch { }

                return Math.Max(0.0, beforeMb - afterMb);
            }
        }

        public static void RecordAutoLogin(bool isSuccess)
        {
            lock (_syncLock)
            {
                if (isSuccess) Stats.AutoLoginSuccessCount++;
                else Stats.AutoLoginFailureCount++;
                SaveStats();
                StatsUpdated?.Invoke();
            }
        }

        public static void RecordScreenUnlock()
        {
            lock (_syncLock)
            {
                Stats.ScreenUnlockCount++;
                SaveStats();
                StatsUpdated?.Invoke();
            }
        }

        public static void RecordBitFlipWin()
        {
            lock (_syncLock)
            {
                Stats.BitFlipWins++;
                SaveStats();
                StatsUpdated?.Invoke();
            }
        }

        public static void RecordSignalLinkWin()
        {
            lock (_syncLock)
            {
                Stats.SignalLinkWins++;
                SaveStats();
                StatsUpdated?.Invoke();
            }
        }

        public static void RecordMysterySolved()
        {
            lock (_syncLock)
            {
                Stats.MysterySolves++;
                SaveStats();
                StatsUpdated?.Invoke();
            }
        }

        public static void RecordMysteryAttempt()
        {
            lock (_syncLock)
            {
                Stats.MysteryAttempts++;
                SaveStats();
                StatsUpdated?.Invoke();
            }
        }

        public static void RecordLogEntry(LogManager.Level level, string caller, string message)
        {
            if (level == LogManager.Level.Error)
            {
                lock (_syncLock) Stats.TotalErrorCount++;
            }
            else if (level == LogManager.Level.Critical)
            {
                lock (_syncLock) Stats.TotalCriticalCount++;
            }

            var item = new DashboardLogItem
            {
                Timestamp = DateTime.Now,
                Level = level.ToString(),
                Caller = caller ?? "",
                Message = message ?? ""
            };

            _recentLogs.Enqueue(item);
            while (_recentLogs.Count > MaxRecentLogCount)
            {
                _recentLogs.TryDequeue(out _);
            }

            StatsUpdated?.Invoke();
        }

        /// <summary>
        /// 현재까지의 모든 에러/경고 로그를 확인 완료(클리어) 처리하여 시스템 헬스를 정상(Clean)으로 복귀
        /// </summary>
        public static void AcknowledgeErrors()
        {
            lock (_syncLock)
            {
                Stats.LastErrorAcknowledgedTime = DateTime.Now;
                Stats.AcknowledgedErrorCount = Stats.TotalErrorCount;
                Stats.AcknowledgedCriticalCount = Stats.TotalCriticalCount;
                SaveStats();
                StatsUpdated?.Invoke();
            }
        }

        #endregion

        #region [ Data Fetchers & Helpers ]

        public static List<DashboardLogItem> GetRecentLogs(int limit = 50, bool onlyAlerts = false)
        {
            var list = _recentLogs.ToList();

            if (onlyAlerts)
            {
                list = list.Where(x => x.Level == "Warning" || x.Level == "Error" || x.Level == "Critical").ToList();
            }

            // 파일에서도 최근 에러/경고/크리티컬 항목을 보완하여 결합
            var fileLogs = ReadRecentErrorsFromFile(limit);
            foreach (var fl in fileLogs)
            {
                if (!list.Any(x => Math.Abs((x.Timestamp - fl.Timestamp).TotalSeconds) < 2 && x.Message == fl.Message))
                {
                    list.Add(fl);
                }
            }

            if (onlyAlerts)
            {
                list = list.Where(x => x.Level == "Warning" || x.Level == "Error" || x.Level == "Critical").ToList();
            }

            var ackTime = Stats.LastErrorAcknowledgedTime;
            var ordered = list.OrderByDescending(x => x.Timestamp).Take(limit).ToList();
            foreach (var item in ordered)
            {
                item.IsAcknowledged = ackTime.HasValue && item.Timestamp <= ackTime.Value;
            }

            return ordered;
        }

        private static List<DashboardLogItem> ReadRecentErrorsFromFile(int count)
        {
            var result = new List<DashboardLogItem>();
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(logDir)) return result;

                // 최근 3일간의 로그 파일 역순 순회 (가장 최신 파일부터)
                var files = Directory.GetFiles(logDir, "Trace_*.log")
                    .OrderByDescending(f => f)
                    .Take(3)
                    .ToList();

                var koCulture = System.Globalization.CultureInfo.GetCultureInfo("ko-KR");

                foreach (var logFile in files)
                {
                    var fileItems = new List<DashboardLogItem>();
                    using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string upper = line.ToUpperInvariant();
                            if (upper.Contains("[ERROR]") || upper.Contains("[CRITICAL]") || upper.Contains("[WARNING]"))
                            {
                                string level = upper.Contains("[CRITICAL]") ? "Critical" : (upper.Contains("[ERROR]") ? "Error" : "Warning");
                                DateTime dt = DateTime.MinValue;

                                int closeBracket = line.IndexOf(']');
                                if (line.StartsWith("[") && closeBracket > 1)
                                {
                                    string dtStr = line.Substring(1, closeBracket - 1).Trim();
                                    if (!DateTime.TryParse(dtStr, koCulture, System.Globalization.DateTimeStyles.None, out dt))
                                    {
                                        if (!DateTime.TryParse(dtStr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                                        {
                                            // 수동 파싱 (예: "2026-10-19 오후 1:32:22")
                                            try
                                            {
                                                string[] parts = dtStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (parts.Length >= 3)
                                                {
                                                    string datePart = parts[0];
                                                    bool isPm = parts[1].Contains("오후") || parts[1].Equals("PM", StringComparison.OrdinalIgnoreCase);
                                                    string timePart = parts[2];
                                                    string[] t = timePart.Split(':');
                                                    if (t.Length >= 3)
                                                    {
                                                        int h = int.Parse(t[0]);
                                                        if (isPm && h < 12) h += 12;
                                                        else if (!isPm && h == 12) h = 0;
                                                        int m = int.Parse(t[1]);
                                                        int s = int.Parse(t[2]);
                                                        string[] d = datePart.Split('-');
                                                        dt = new DateTime(int.Parse(d[0]), int.Parse(d[1]), int.Parse(d[2]), h, m, s);
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }

                                if (dt == DateTime.MinValue)
                                {
                                    dt = File.GetLastWriteTime(logFile);
                                }

                                string msg = line;
                                int sepIndex = line.IndexOf("::");
                                string caller = "";
                                if (sepIndex > 0)
                                {
                                    msg = line.Substring(sepIndex + 2).Trim();
                                    int callerStart = line.LastIndexOf(']', sepIndex);
                                    if (callerStart > 0)
                                    {
                                        caller = line.Substring(callerStart + 1, sepIndex - callerStart - 1).Trim();
                                    }
                                }

                                fileItems.Add(new DashboardLogItem
                                {
                                    Timestamp = dt,
                                    Level = level,
                                    Caller = caller,
                                    Message = msg
                                });
                            }
                        }
                    }

                    // 해당 파일의 가장 최근(파일 끝부분) 에러들을 역순으로 수집
                    int needed = count - result.Count;
                    if (needed > 0 && fileItems.Count > 0)
                    {
                        result.AddRange(fileItems.AsEnumerable().Reverse().Take(needed));
                    }

                    if (result.Count >= count) break;
                }
            }
            catch { }

            return result.OrderByDescending(x => x.Timestamp).Take(count).ToList();
        }

        public static List<KeyValuePair<string, int>> GetTopServers(int limit = 5)
        {
            lock (_syncLock)
            {
                if (Stats.ServerAccessCounts == null) return new List<KeyValuePair<string, int>>();
                return Stats.ServerAccessCounts
                    .OrderByDescending(kv => kv.Value)
                    .Take(limit)
                    .ToList();
            }
        }

        public static int GetAiTrendScrapCount()
        {
            try
            {
                string favPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_meta", "favorite_repos.json");
                if (File.Exists(favPath))
                {
                    string json = File.ReadAllText(favPath, Encoding.UTF8);
                    var items = JsonConvert.DeserializeObject<List<object>>(json);
                    return items?.Count ?? 0;
                }
            }
            catch { }
            return Stats.AiTrendScraps;
        }

        public static int GetCustomKeywordCount()
        {
            try
            {
                string kwPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_meta", "custom_keywords.json");
                if (File.Exists(kwPath))
                {
                    string json = File.ReadAllText(kwPath, Encoding.UTF8);
                    var items = JsonConvert.DeserializeObject<List<object>>(json);
                    return items?.Count ?? 0;
                }
            }
            catch { }
            return 0;
        }

        public static SystemResourceSnapshot GetResourceSnapshot()
        {
            var snap = new SystemResourceSnapshot();
            snap.SessionUptime = _sessionStopwatch.Elapsed;

            // 1. GateHelper Process
            try
            {
                var curProc = Process.GetCurrentProcess();
                curProc.Refresh();
                snap.ProcessRamMb = Math.Round((double)curProc.WorkingSet64 / (1024 * 1024), 1);
                snap.ProcessPrivateRamMb = Math.Round((double)curProc.PrivateMemorySize64 / (1024 * 1024), 1);
                snap.ThreadCount = curProc.Threads.Count;
                snap.HandleCount = curProc.HandleCount;

                // CPU % 계산
                DateTime now = DateTime.UtcNow;
                TimeSpan curTotal = curProc.TotalProcessorTime;
                if (_lastCpuSampleTime != DateTime.MinValue)
                {
                    double timeWindowMs = (now - _lastCpuSampleTime).TotalMilliseconds;
                    if (timeWindowMs > 300)
                    {
                        double cpuUsedMs = (curTotal - _lastTotalProcessorTime).TotalMilliseconds;
                        double rawCpu = (cpuUsedMs / (timeWindowMs * Environment.ProcessorCount)) * 100.0;
                        _cachedCpuPercent = Math.Round(Math.Max(0.0, Math.Min(100.0, rawCpu)), 1);

                        _lastCpuSampleTime = now;
                        _lastTotalProcessorTime = curTotal;
                    }
                }
                else
                {
                    _lastCpuSampleTime = now;
                    _lastTotalProcessorTime = curTotal;
                }
                snap.ProcessCpuPercent = _cachedCpuPercent;
            }
            catch
            {
                snap.ProcessCpuPercent = 0.0;
            }

            // 2. 네트워크 Ping 측정 (비동기 캐싱)
            TriggerBackgroundPingCheck();
            snap.NetworkPingMs = _cachedPingMs;
            snap.NetworkStatusText = _cachedPingStatus;

            // 3. ChromeDriver Status
            try
            {
                snap.ChromeDriverRunning = Process.GetProcessesByName("chromedriver").Length > 0;
                snap.IsSeleniumDriverAlive = (MainUI._driver != null);
            }
            catch
            {
                snap.ChromeDriverRunning = false;
                snap.IsSeleniumDriverAlive = false;
            }

            return snap;
        }

        // 네트워크 핑 캐시
        private static long _cachedPingMs = -1;
        private static string _cachedPingStatus = "초기화 중";
        private static DateTime _lastPingCheckTime = DateTime.MinValue;
        private static bool _isPingChecking = false;

        private static void TriggerBackgroundPingCheck()
        {
            if (_isPingChecking) return;
            if ((DateTime.UtcNow - _lastPingCheckTime).TotalSeconds < 3.0) return;

            _isPingChecking = true;
            _lastPingCheckTime = DateTime.UtcNow;

            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    using (var ping = new System.Net.NetworkInformation.Ping())
                    {
                        var reply = await ping.SendPingAsync("8.8.8.8", 1200);
                        if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        {
                            _cachedPingMs = reply.RoundtripTime;
                            if (_cachedPingMs <= 80) _cachedPingStatus = "정상";
                            else if (_cachedPingMs <= 150) _cachedPingStatus = "주의 (다소 지연)";
                            else _cachedPingStatus = "지연 감지";
                        }
                        else
                        {
                            _cachedPingMs = -1;
                            _cachedPingStatus = "응답 없음";
                        }
                    }
                }
                catch
                {
                    _cachedPingMs = -1;
                    _cachedPingStatus = "사내망/오프라인";
                }
                finally
                {
                    _isPingChecking = false;
                }
            });
        }

        #endregion

        #region [ Steam-Style Achievement Engine ]

        public static List<AchievementItem> GetAchievements()
        {
            var list = new List<AchievementItem>();
            int bf = Stats.BitFlipWins;
            int sl = Stats.SignalLinkWins;
            int mt = Stats.MysterySolves;
            int at = GetAiTrendScrapCount();

            // 1. BitFlip
            list.Add(new AchievementItem
            {
                Id = "BF_1",
                Category = AchievementCategory.BitFlip,
                CategoryName = "BitFlip",
                Title = "첫 번째 플립",
                Description = "BitFlip 첫 번째 퍼즐 클리어",
                Points = 20,
                RequiredCount = 1,
                CurrentCount = bf,
                IconSymbol = "🧩"
            });
            list.Add(new AchievementItem
            {
                Id = "BF_2",
                Category = AchievementCategory.BitFlip,
                CategoryName = "BitFlip",
                Title = "이진법 탐구자",
                Description = "BitFlip 누적 3회 클리어",
                Points = 30,
                RequiredCount = 3,
                CurrentCount = bf,
                IconSymbol = "💡"
            });
            list.Add(new AchievementItem
            {
                Id = "BF_3",
                Category = AchievementCategory.BitFlip,
                CategoryName = "BitFlip",
                Title = "양자 비트 마스터",
                Description = "BitFlip 누적 7회 클리어",
                Points = 50,
                RequiredCount = 7,
                CurrentCount = bf,
                IconSymbol = "👑"
            });

            // 2. SignalLink
            list.Add(new AchievementItem
            {
                Id = "SL_1",
                Category = AchievementCategory.SignalLink,
                CategoryName = "SignalLink",
                Title = "신호 수신 완료",
                Description = "SignalLink 첫 번째 회로 연결 성공",
                Points = 20,
                RequiredCount = 1,
                CurrentCount = sl,
                IconSymbol = "⚡"
            });
            list.Add(new AchievementItem
            {
                Id = "SL_2",
                Category = AchievementCategory.SignalLink,
                CategoryName = "SignalLink",
                Title = "패킷 라우터",
                Description = "SignalLink 누적 3회 클리어",
                Points = 30,
                RequiredCount = 3,
                CurrentCount = sl,
                IconSymbol = "📡"
            });
            list.Add(new AchievementItem
            {
                Id = "SL_3",
                Category = AchievementCategory.SignalLink,
                CategoryName = "SignalLink",
                Title = "광섬유 회로망",
                Description = "SignalLink 누적 7회 클리어",
                Points = 50,
                RequiredCount = 7,
                CurrentCount = sl,
                IconSymbol = "🌐"
            });

            // 3. MysteryTime
            list.Add(new AchievementItem
            {
                Id = "MT_1",
                Category = AchievementCategory.MysteryTime,
                CategoryName = "MysteryTime",
                Title = "사건 파일 개봉",
                Description = "MysteryTime 첫 번째 사건 해결",
                Points = 20,
                RequiredCount = 1,
                CurrentCount = mt,
                IconSymbol = "🔍"
            });
            list.Add(new AchievementItem
            {
                Id = "MT_2",
                Category = AchievementCategory.MysteryTime,
                CategoryName = "MysteryTime",
                Title = "타임라인 수사관",
                Description = "MysteryTime 누적 3회 사건 해결",
                Points = 30,
                RequiredCount = 3,
                CurrentCount = mt,
                IconSymbol = "🕵️"
            });
            list.Add(new AchievementItem
            {
                Id = "MT_3",
                Category = AchievementCategory.MysteryTime,
                CategoryName = "MysteryTime",
                Title = "명탐정의 직관",
                Description = "MysteryTime 누적 7회 사건 해결",
                Points = 50,
                RequiredCount = 7,
                CurrentCount = mt,
                IconSymbol = "🎖️"
            });

            // 4. AI TREND
            list.Add(new AchievementItem
            {
                Id = "AT_1",
                Category = AchievementCategory.AiTrend,
                CategoryName = "AI TREND",
                Title = "기술의 물결",
                Description = "오픈소스/뉴스 첫 번째 찜(스크랩) 등록",
                Points = 20,
                RequiredCount = 1,
                CurrentCount = at,
                IconSymbol = "⭐"
            });
            list.Add(new AchievementItem
            {
                Id = "AT_2",
                Category = AchievementCategory.AiTrend,
                CategoryName = "AI TREND",
                Title = "지식 아카이빙",
                Description = "관심 프로젝트 누적 3개 찜 달성",
                Points = 30,
                RequiredCount = 3,
                CurrentCount = at,
                IconSymbol = "📚"
            });
            list.Add(new AchievementItem
            {
                Id = "AT_3",
                Category = AchievementCategory.AiTrend,
                CategoryName = "AI TREND",
                Title = "글로벌 인텔리전스",
                Description = "관심 프로젝트 누적 7개 찜 달성",
                Points = 50,
                RequiredCount = 7,
                CurrentCount = at,
                IconSymbol = "🚀"
            });

            // 5. Special / All-rounder
            int anyCount = (bf > 0 ? 1 : 0) + (sl > 0 ? 1 : 0) + (mt > 0 ? 1 : 0) + (at > 0 ? 1 : 0);
            int gameCount = (bf > 0 ? 1 : 0) + (sl > 0 ? 1 : 0) + (mt > 0 ? 1 : 0);

            list.Add(new AchievementItem
            {
                Id = "SP_1",
                Category = AchievementCategory.Special,
                CategoryName = "스페셜",
                Title = "호기심의 시작",
                Description = "샌드박스 4대 분야 중 아무거나 1개 이상 달성",
                Points = 20,
                RequiredCount = 1,
                CurrentCount = anyCount,
                IconSymbol = "✨"
            });
            list.Add(new AchievementItem
            {
                Id = "SP_2",
                Category = AchievementCategory.Special,
                CategoryName = "스페셜",
                Title = "트리플 플레이어",
                Description = "미니게임 3종(비트플립, 신호링크, 미스터리) 각 1회 클리어",
                Points = 50,
                RequiredCount = 3,
                CurrentCount = gameCount,
                IconSymbol = "🎯"
            });
            list.Add(new AchievementItem
            {
                Id = "SP_3",
                Category = AchievementCategory.Special,
                CategoryName = "스페셜",
                Title = "샌드박스 정복자",
                Description = "4대 분야(미니게임 3종 + AI 찜) 모두 1개 이상 달성",
                Points = 50,
                RequiredCount = 4,
                CurrentCount = anyCount,
                IconSymbol = "🏆"
            });

            return list;
        }

        public static MasteryTierInfo GetMasteryTierInfo()
        {
            var achs = GetAchievements();
            int totalEarned = 0;
            int maxPoints = 0;
            int unlockedCount = 0;

            foreach (var a in achs)
            {
                maxPoints += a.Points;
                if (a.IsUnlocked)
                {
                    totalEarned += a.Points;
                    unlockedCount++;
                }
            }

            var info = new MasteryTierInfo
            {
                TotalEarnedPoints = totalEarned,
                MaxPoints = maxPoints,
                UnlockedCount = unlockedCount,
                TotalCount = achs.Count
            };

            // 5단계 티어 분류
            if (totalEarned >= 450)
            {
                info.TierLevel = 5;
                info.TierName = "★ Tier 5: Gate Mastermind";
                info.TierTitle = "샌드박스의 모든 규칙을 정복한 최고 권위자";
                info.AccentColor = Color.FromArgb(186, 85, 211); // Orchid Purple
            }
            else if (totalEarned >= 300)
            {
                info.TierLevel = 4;
                info.TierName = "★ Tier 4: Cyber Architect";
                info.TierTitle = "시스템 아키텍처와 논리를 자유자재로 다루는 설계자";
                info.AccentColor = Color.FromArgb(0, 220, 255); // Cyan Neon
            }
            else if (totalEarned >= 180)
            {
                info.TierLevel = 3;
                info.TierName = "★ Tier 3: Logic Breaker";
                info.TierTitle = "복잡한 문제의 해답을 찾아내는 숙련된 요원";
                info.AccentColor = Color.FromArgb(255, 215, 0); // Gold
            }
            else if (totalEarned >= 80)
            {
                info.TierLevel = 2;
                info.TierName = "★ Tier 2: Code Explorer";
                info.TierTitle = "시스템 곳곳의 숨겨진 기능을 탐색하는 탐험가";
                info.AccentColor = Color.FromArgb(192, 192, 210); // Silver
            }
            else
            {
                info.TierLevel = 1;
                info.TierName = "★ Tier 1: Novice";
                info.TierTitle = "샌드박스의 문을 두드린 호기심 많은 입문자";
                info.AccentColor = Color.FromArgb(205, 127, 50); // Bronze
            }

            return info;
        }

        public static List<CategoryTierInfo> GetCategoryTiers()
        {
            var result = new List<CategoryTierInfo>();

            int bf = Stats.BitFlipWins;
            int sl = Stats.SignalLinkWins;
            int mt = Stats.MysterySolves;
            int at = GetAiTrendScrapCount();

            result.Add(EvaluateCategoryTier(AchievementCategory.BitFlip, "BitFlip", bf,
                new[] { "미입문", "Lv.1 비트 연산자", "Lv.2 바이트 조작자", "Lv.3 게이트스미스", "Lv.4 퀀텀 마스터" }));

            result.Add(EvaluateCategoryTier(AchievementCategory.SignalLink, "SignalLink", sl,
                new[] { "미입문", "Lv.1 와이어 스플라이서", "Lv.2 패킷 라우터", "Lv.3 서킷 위버", "Lv.4 제로 레이턴시" }));

            result.Add(EvaluateCategoryTier(AchievementCategory.MysteryTime, "MysteryTime", mt,
                new[] { "미입문", "Lv.1 견습 수사관", "Lv.2 팩트 체커", "Lv.3 타임 프로파일러", "Lv.4 패러독스 솔버" }));

            result.Add(EvaluateCategoryTier(AchievementCategory.AiTrend, "AI TREND", at,
                new[] { "미입문", "Lv.1 테크 리더", "Lv.2 큐레이터", "Lv.3 인사이트 헌터", "Lv.4 퓨처리스트" }));

            return result;
        }

        private static CategoryTierInfo EvaluateCategoryTier(AchievementCategory cat, string name, int count, string[] rankNames)
        {
            int level = 0;
            Color color = Color.Gray;

            if (count >= 7)
            {
                level = 4;
                color = Color.FromArgb(0, 220, 255);
            }
            else if (count >= 5)
            {
                level = 3;
                color = Color.FromArgb(255, 215, 0);
            }
            else if (count >= 3)
            {
                level = 2;
                color = Color.FromArgb(192, 192, 192);
            }
            else if (count >= 1)
            {
                level = 1;
                color = Color.FromArgb(205, 127, 50);
            }

            return new CategoryTierInfo
            {
                Category = cat,
                CategoryName = name,
                CurrentLevel = level,
                CurrentCount = count,
                CurrentTierName = rankNames[level],
                TierColor = color
            };
        }

        #endregion
    }
}
