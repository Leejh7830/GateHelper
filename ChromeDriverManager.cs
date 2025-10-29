using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using static GateHelper.LogManager;
using System.Management;
using System.Drawing;
using System.Text.RegularExpressions; // 상단 using 추가

namespace GateHelper
{
    public class ChromeDriverManager
    {
        private int _aliveConsecutiveFails = 0;          // 연속 실패 횟수
        private const int AliveFailThreshold = 2;        // 이 횟수 이상 연속 실패해야 OFF로 간주
        private DateTime _lastAliveSuccessUtc = DateTime.MinValue;

        #region Ver3. Selenium Manager 사용
        public static IWebDriver InitializeDriverWithSeleniumManager(Config config)
        {
            try
            {
                // 1) Chrome 실행 경로 확인
                string chromePath = config.ChromePath;
                if (string.IsNullOrEmpty(chromePath) || !File.Exists(chromePath))
                {
                    chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                    if (!File.Exists(chromePath))
                        chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                    if (!File.Exists(chromePath))
                        chromePath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            @"Google\Chrome\Application\chrome.exe");
                }
                if (!File.Exists(chromePath))
                    throw new Exception($"Chrome 실행 파일을 찾을 수 없습니다.\n경로: {chromePath}");

                // 2) (선택) 진단 로그
                // Environment.SetEnvironmentVariable("SE_MANAGER_DIAGNOSTICS", "1");
                // Environment.SetEnvironmentVariable("SE_DOWNLOAD_PATH", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "selenium"));

                // 2.1) Chrome 버전 로깅
                var chromeFvi = FileVersionInfo.GetVersionInfo(chromePath);
                var chromeVersion = chromeFvi?.FileVersion ?? "unknown";
                LogMessage($"Chrome detected. Path={chromePath}, Version={chromeVersion}", Level.Info);

                // 3) 옵션 생성
                ChromeOptions options = ChromeDriverOptionSet(chromePath);

                // 4) Selenium Manager만 사용: 서비스 경로 지정하지 않음
                var service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                service.SuppressInitialDiagnosticInformation = true;

                // 5) 드라이버 생성 -> Selenium Manager가 자동 해석/다운로드/캐시 사용
                var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));

                // 6) 드라이버 실제 경로 탐색(재시도 + WMI 폴백)
                string resolvedPath = null;
                for (int i = 0; i < 3 && string.IsNullOrEmpty(resolvedPath); i++)
                {
                    resolvedPath = TryGetRunningChromeDriverPath()
                                   ?? TryGetRunningChromeDriverPathViaWmi()
                                   ?? TryLocateChromeDriver();
                    if (string.IsNullOrEmpty(resolvedPath))
                        Thread.Sleep(300);
                }

                if (!string.IsNullOrEmpty(resolvedPath) && File.Exists(resolvedPath))
                {
                    // (로그 부분) FileVersionInfo 대신 보강된 버전 추출 사용
                    var driverVersion = TryGetDriverVersion(resolvedPath);
                    var source = DescribeDriverSource(resolvedPath);
                    var lastWriteUtc = File.GetLastWriteTimeUtc(resolvedPath);
                    var isFresh = (DateTime.UtcNow - lastWriteUtc) <= TimeSpan.FromMinutes(5);

                    LogMessage($"Driver resolved. DriverVersion={driverVersion}, Path={resolvedPath}, Source={source}", Level.Info);
                    LogMessage(isFresh ? "NEW Driver downloaded." : "Driver reused from cache.", Level.Info);
                }
                else
                {
                    LogMessage("Driver path not resolved by probes. Selenium Manager may still be managing it internally.", Level.Info);
                }

                return driver;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                throw;
            }
        }

        // 드라이버 파일 경로로 캐시 출처 설명
        private static string DescribeDriverSource(string path)
        {
            try
            {
                string p = path.Replace('/', '\\').ToLowerInvariant();
                string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace('/', '\\').ToLowerInvariant();
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                                   .Replace('/', '\\').ToLowerInvariant();
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                                     .Replace('/', '\\').ToLowerInvariant();

                if (p.Contains("\\.wdm\\")) return "WebDriverManager cache";
                if (p.StartsWith(Path.Combine(localApp, "selenium").ToLowerInvariant())) return "Selenium Manager cache";
                if (p.StartsWith(Path.Combine(userProfile, ".cache", "selenium").ToLowerInvariant())) return "Selenium Manager cache (.cache)";
                if (p.StartsWith(baseDir)) return "App base directory";

                var pathEnv = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                              .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(d => d.Trim().Replace('/', '\\').ToLowerInvariant());
                if (pathEnv.Any(d => p.StartsWith(d))) return "PATH folder";

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        // 드라이버 파일에서 버전을 최대한 정확히 추출
        private static string TryGetDriverVersion(string driverPath)
        {
            try
            {
                // 1) 파일 메타데이터 시도
                var fvi = FileVersionInfo.GetVersionInfo(driverPath);
                var v = !string.IsNullOrWhiteSpace(fvi?.FileVersion) ? fvi.FileVersion : fvi?.ProductVersion;
                if (!string.IsNullOrWhiteSpace(v)) return v;

                // 2) 경로(폴더명)에서 추출: ...\123.0.6312.122\chromedriver.exe
                var dir = Path.GetDirectoryName(driverPath);
                var m = Regex.Match(dir ?? "", @"\d+\.\d+\.\d+\.\d+");
                if (m.Success) return m.Value;

                // 3) 실행 결과에서 추출: "ChromeDriver 123.0.6312.122 (..)"
                var psi = new ProcessStartInfo
                {
                    FileName = driverPath,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    var output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(3000);
                    var m2 = Regex.Match(output ?? "", @"\d+\.\d+\.\d+\.\d+");
                    if (m2.Success) return m2.Value;
                }
            }
            catch { /* 무시하고 아래로 */ }
            return "unknown";
        }

        #endregion Ver3. Selenium Manager 사용










        #region Ver2. WebDriverManager 사용 / 대체 Selenium Manager
        /*
        // WebDriverManager를 사용하는 새로운 초기화 메서드
        public static IWebDriver InitializeDriverWithWebDriverManager(Config config)
        {
            try
            {
                // 1) Chrome 실행 경로 확인
                string chromePath = config.ChromePath;
                if (string.IsNullOrEmpty(chromePath) || !File.Exists(chromePath))
                {
                    chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                    if (!File.Exists(chromePath))
                        chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                    if (!File.Exists(chromePath))
                        chromePath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            @"Google\Chrome\Application\chrome.exe");
                }
                if (!File.Exists(chromePath))
                    throw new Exception($"Chrome 실행 파일을 찾을 수 없습니다.\n경로: {chromePath}\n설정 파일에서 지정한 경로를 확인해 주세요.");

                // 2) Chrome 버전 감지
                string chromeVersion = FileVersionInfo.GetVersionInfo(chromePath).FileVersion;
                LogMessage($"chromePath: {chromePath}, chromeVersion: {chromeVersion}", Level.Info);
                if (string.IsNullOrEmpty(chromeVersion) || !chromeVersion.Contains("."))
                    throw new Exception($"Chrome 버전 정보를 가져올 수 없습니다. 경로: {chromePath}");

                // 3) WebDriverManager 시도 (메이저 버전 → 미지정 폴백)
                try
                {
                    Version v;
                    var majorVersion = Version.TryParse(chromeVersion, out v) ? v.Major.ToString() : chromeVersion.Split('.')[0];
                    LogMessage($"WebDriverManager 시도 - majorVersion: {majorVersion}", Level.Info);
                    new DriverManager().SetUpDriver(new ChromeConfig(), majorVersion);
                }
                catch (WebException wex)
                {
                    LogException(wex, Level.Error);
                    LogMessage("WebDriverManager 재시도 - 버전 미지정(auto-resolve)", Level.Info);
                    new DriverManager().SetUpDriver(new ChromeConfig());
                }
                catch (Exception exWdm)
                {
                    LogException(exWdm, Level.Error);
                    LogMessage("WebDriverManager 사용 실패 - Selenium Manager로 위임", Level.Info);
                }

                // (선택) Selenium Manager 진단 로그 활성화
                // Environment.SetEnvironmentVariable("SE_MANAGER_DIAGNOSTICS", "1");

                // 4) 옵션 생성
                ChromeOptions options = ChromeDriverOptionSet(chromePath);

                // 5) ChromeDriverService 생성 + 콘솔창 숨김
                string driverExe = TryLocateChromeDriver();
                ChromeDriverService service = string.IsNullOrEmpty(driverExe)
                    ? ChromeDriverService.CreateDefaultService()
                    : ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(driverExe));
                service.HideCommandPromptWindow = true;                  // chromedriver 콘솔창 숨김
                service.SuppressInitialDiagnosticInformation = true;     // 초기 진단 출력 억제

                // 6) ChromeDriver 인스턴스 생성
                var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));

                // 7) 드라이버 실제 경로 탐색(재시도 + WMI 폴백)
                string resolvedPath = null;
                for (int i = 0; i < 3 && string.IsNullOrEmpty(resolvedPath); i++)
                {
                    resolvedPath = TryGetRunningChromeDriverPath()
                                   ?? TryGetRunningChromeDriverPathViaWmi()
                                   ?? TryLocateChromeDriver();
                    if (string.IsNullOrEmpty(resolvedPath))
                        Thread.Sleep(300);
                }

                if (!string.IsNullOrEmpty(resolvedPath))
                    LogMessage($"실행 중 ChromeDriver 경로: {resolvedPath}", Level.Info);
                else
                    LogMessage("ChromeDriver 경로를 찾지 못했습니다. (권한/타이밍/보안 소프트웨어 영향 가능)", Level.Info);

                return driver;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                throw;
            }
        }
        */
        #endregion Ver2. WebDriverManager 사용

        private static string TryGetRunningChromeDriverPath()
        {
            try
            {
                var procs = Process.GetProcessesByName("chromedriver");
                Process latest = null;
                foreach (var p in procs)
                {
                    try
                    {
                        if (latest == null || p.StartTime > latest.StartTime)
                            latest = p;
                    }
                    catch { }
                }
                if (latest != null)
                {
                    try
                    {
                        return latest.MainModule != null ? latest.MainModule.FileName : null;
                    }
                    catch { } // MainModule 권한 문제시 null
                }
            }
            catch { }
            return null;
        }

        private static string TryLocateChromeDriver()
        {
            // 1) 실행 폴더
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var candidate = Path.Combine(baseDir, "chromedriver.exe");
            if (File.Exists(candidate)) return candidate;

            // 2) WebDriverManager 캐시: %USERPROFILE%\.wdm\...
            try
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var wdmRoot = Path.Combine(userProfile, ".wdm");
                if (Directory.Exists(wdmRoot))
                {
                    var files = Directory.GetFiles(wdmRoot, "chromedriver.exe", SearchOption.AllDirectories);
                    var latest = files
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.LastWriteTimeUtc)
                        .FirstOrDefault();
                    if (latest != null) return latest.FullName;
                }
            }
            catch {  }

            // 3) Selenium Manager 캐시: %LOCALAPPDATA%\selenium\...
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var seRoot = Path.Combine(localAppData, "selenium");
                if (Directory.Exists(seRoot))
                {
                    var files = Directory.GetFiles(seRoot, "chromedriver.exe", SearchOption.AllDirectories);
                    var latest = files
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.LastWriteTimeUtc)
                        .FirstOrDefault();
                    if (latest != null) return latest.FullName;
                }
            }
            catch {  }

            // 4) PATH 내 디렉터리 스캔
            try
            {
                var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                foreach (var dir in pathEnv.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = dir.Trim();
                    if (!Directory.Exists(trimmed)) continue;
                    var path = Path.Combine(trimmed, "chromedriver.exe");
                    if (File.Exists(path)) return path;
                }
            }
            catch {  }

            return null;
        }
        
        private static string TryGetRunningChromeDriverPathViaWmi()
        {
            try
            {
                // System.Management 네임스페이스 필요
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT ExecutablePath FROM Win32_Process WHERE Name = 'chromedriver.exe'"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var path = obj["ExecutablePath"] as string;
                        if (!string.IsNullOrEmpty(path))
                            return path;
                    }
                }
            }
            catch { }
            return null;
        }



        #region Ver1. 기존 초기화 메서드
        /*
        // ChromeDriver 초기화 메소드
        public static IWebDriver InitializeDriver(Config config)
        {
            try
            {
                string driverDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string driverPath = Path.Combine(driverDirectory, "chromedriver.exe");

                // ChromeDriver 존재 확인
                if (!File.Exists(driverPath))
                {
                    MessageBox.Show("ChromeDriver가 존재하지 않습니다");
                    throw new Exception($"ChromeDriver가 존재하지 않습니다: {driverPath}");
                }

                // Chrome 실행 경로 확인
                string chromePath = config.ChromePath; // 설정 파일 경로 사용
                if (string.IsNullOrEmpty(chromePath) || !File.Exists(chromePath))
                {
                    chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"; // 기본 경로 (64비트)
                    if (!File.Exists(chromePath))
                    {
                        chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"; // 32비트 경로
                    }
                    if (!File.Exists(chromePath))
                    {
                        chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                    @"Google\Chrome\Application\chrome.exe"); // 사용자 폴더 경로
                    }
                }

                // Chrome 실행 파일 존재 확인
                if (!File.Exists(chromePath))
                {
                    throw new Exception($"Chrome 실행 파일을 찾을 수 없습니다.\n경로: {chromePath}\n설정 파일에서 지정한 경로를 확인해 주세요.");
                }

                // Chrome 옵션 설정
                ChromeOptions options = ChromeDriverManager.ChromeDriverOptionSet(chromePath);

                // ChromeDriver 실행
                var service = ChromeDriverService.CreateDefaultService(driverDirectory);
                service.HideCommandPromptWindow = true; // cmd 창 숨김

                return new ChromeDriver(service, options);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                throw;
            }
        }
        */
        #endregion Ver1. 기존 초기화 메서드


        public static ChromeOptions ChromeDriverOptionSet(string chromePath)
        {
            var options = new ChromeOptions();
            options.BinaryLocation = chromePath;
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--remote-debugging-port=9222");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.UnhandledPromptBehavior = UnhandledPromptBehavior.Ignore;
            return options;
        }

        // Driver 체크용
        public bool IsDriverAlive(IWebDriver _driver)
        {
            if (_driver == null) return false;

            // // 1) 알림(Alert) 존재 시 '살아있음'으로 간주 (확인/닫기 누르지 않음)
            try
            {
                var _ = _driver.SwitchTo().Alert();
                ResetAliveFails();
                return true;
            }
            catch (NoAlertPresentException)
            {
                // 알림 없음 → 다음 체크 진행
            }
            catch (WebDriverException)
            {
                // 드문 환경에서 Alert 조회가 WebDriverException을 던질 수 있음 → 다음 체크 진행
            }

            // // 2) 윈도우 핸들 조회로 세션 생존 확인
            try
            {
                var handles = _driver.WindowHandles;
                if (handles != null && handles.Count > 0)
                {
                    ResetAliveFails();
                    return true;
                }
            }
            catch (WebDriverException)
            {
                // 일시 예외 가능 → 연속 실패 카운트로 완화 처리
            }

            // // 3) 여기까지 왔으면 이번 회차는 '실패 1회'로 카운트
            _aliveConsecutiveFails++;

            // // 연속 실패가 임계치 미만이면 '일시 오류'로 보고 여전히 Alive 취급
            if (_aliveConsecutiveFails < AliveFailThreshold)
            {
                return true;
            }

            // // 연속으로 충분히 실패했을 때만 OFF 판정
            return false;
        }

        // // Alive 성공 시 카운터 리셋
        private void ResetAliveFails()
        {
            _aliveConsecutiveFails = 0;
            _lastAliveSuccessUtc = DateTime.UtcNow;
        }

        // Network 체크용
        public bool IsInternetAvailable()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        // 25.08.14 Added - Driver Check
        public bool IsDriverReady(IWebDriver _driver)
        {
            // _driver 객체가 있고, 드라이버가 활성화된 상태인지 확인합니다.
            if (_driver != null && IsDriverAlive(_driver))
            {
                return true;
            }

            // 드라이버가 준비되지 않았을 경우, 메시지 박스를 띄웁니다.
            MessageBox.Show("ChromeDriver가 OFF 상태입니다. [Start]버튼을 눌러 드라이버를 실행해주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // ChromeDriver 종료
        public static void CloseDriver(IWebDriver driver)
        {
            // 드라이버 종료 시도
            if (driver != null)
            {
                try
                {
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"드라이버 종료 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogException(ex, Level.Error);
                }
                finally
                {
                    LogMessage("ChromeDriver 종료", Level.Info);
                }
            }

            try // 25.03.27 Added - 백그라운드 chromedriver 프로세스 강제 종료
            {
                foreach (var process in Process.GetProcessesByName("chromedriver"))
                {
                    process.Kill();
                }
                LogMessage("남아있는 ChromeDriver 프로세스 강제 종료", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
        }

        
    }
}
