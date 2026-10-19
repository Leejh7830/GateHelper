using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace GateHelper.Dashboard
{
    internal static class DashboardManager
    {
        private static readonly object _syncLock = new object();
        public static string DashboardDirectory => Path.Combine(Util.MetaFolder, "dashboard");
        public static string StatsFilePath => Path.Combine(DashboardDirectory, "dashboard_stats.json");

        public static DashboardStats Stats { get; private set; } = new DashboardStats();

        private static readonly Stopwatch _sessionStopwatch = new Stopwatch();
        public static DateTime SessionStartTime { get; private set; } = DateTime.Now;

        // 최근 중요 로그 링 버퍼 (최대 30개)
        private static readonly ConcurrentQueue<DashboardLogItem> _recentLogs = new ConcurrentQueue<DashboardLogItem>();
        private const int MaxRecentLogCount = 30;

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

                EnsureDashboardDirectory();
                LoadStats();

                Stats.TotalLaunchCount++;
                Stats.LastLaunchDate = DateTime.Now;
                SaveStats();

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
        /// 프로그램 종료 시 호출하여 누적 가동 시간을 정산하고 저장합니다.
        /// </summary>
        public static void OnAppClosing()
        {
            lock (_syncLock)
            {
                if (!_isInitialized) return;

                _sessionStopwatch.Stop();
                double sessionMinutes = _sessionStopwatch.Elapsed.TotalMinutes;
                Stats.TotalUptimeMinutes += sessionMinutes;
                Stats.LastLaunchDate = DateTime.Now;

                SaveStats();
            }
        }

        public static TimeSpan GetSessionUptime()
        {
            return _sessionStopwatch.Elapsed;
        }

        #region [ File Load & Save ]

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
                if (File.Exists(StatsFilePath))
                {
                    string json = File.ReadAllText(StatsFilePath, Encoding.UTF8);
                    Stats = JsonConvert.DeserializeObject<DashboardStats>(json) ?? new DashboardStats();
                    if (Stats.ServerAccessCounts == null)
                    {
                        Stats.ServerAccessCounts = new Dictionary<string, int>();
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Warning, "Dashboard LoadStats");
            }

            Stats = new DashboardStats();
            if (Stats.ServerAccessCounts == null)
            {
                Stats.ServerAccessCounts = new Dictionary<string, int>();
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
                    File.WriteAllText(StatsFilePath, json, Encoding.UTF8);
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
                Stats = new DashboardStats
                {
                    TotalLaunchCount = 1,
                    TotalUptimeMinutes = _sessionStopwatch.Elapsed.TotalMinutes,
                    FirstLaunchDate = DateTime.Now,
                    LastLaunchDate = DateTime.Now
                };
                SaveStats();
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
                SaveStats();
            }
        }

        public static void RecordAutoLogin(bool isSuccess)
        {
            lock (_syncLock)
            {
                if (isSuccess) Stats.AutoLoginSuccessCount++;
                else Stats.AutoLoginFailureCount++;
                SaveStats();
            }
        }

        public static void RecordBitFlipWin()
        {
            lock (_syncLock)
            {
                Stats.BitFlipWins++;
                SaveStats();
            }
        }

        public static void RecordSignalLinkWin()
        {
            lock (_syncLock)
            {
                Stats.SignalLinkWins++;
                SaveStats();
            }
        }

        public static void RecordMysterySolved()
        {
            lock (_syncLock)
            {
                Stats.MysterySolves++;
                SaveStats();
            }
        }

        public static void RecordMysteryAttempt()
        {
            lock (_syncLock)
            {
                Stats.MysteryAttempts++;
                SaveStats();
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
        }

        #endregion

        #region [ Data Fetchers & Helpers ]

        public static List<DashboardLogItem> GetRecentLogs(int limit = 30)
        {
            var list = _recentLogs.ToList();

            // 파일에서도 최근 에러/경고/크리티컬 항목을 보완하여 결합
            var fileLogs = ReadRecentErrorsFromFile(limit);
            foreach (var fl in fileLogs)
            {
                if (!list.Any(x => Math.Abs((x.Timestamp - fl.Timestamp).TotalSeconds) < 2 && x.Message == fl.Message))
                {
                    list.Add(fl);
                }
            }

            return list.OrderByDescending(x => x.Timestamp).Take(limit).ToList();
        }

        private static List<DashboardLogItem> ReadRecentErrorsFromFile(int count)
        {
            var result = new List<DashboardLogItem>();
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(logDir)) return result;

                // 최근 3일간의 로그 파일 역순 순회
                var files = Directory.GetFiles(logDir, "Trace_*.log")
                    .OrderByDescending(f => f)
                    .Take(3)
                    .ToList();

                var koCulture = System.Globalization.CultureInfo.GetCultureInfo("ko-KR");

                foreach (var logFile in files)
                {
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

                                result.Add(new DashboardLogItem
                                {
                                    Timestamp = dt,
                                    Level = level,
                                    Caller = caller,
                                    Message = msg
                                });
                            }
                        }
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
    }
}
