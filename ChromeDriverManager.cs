using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;
using static GateHelper.LogManager;

namespace GateHelper
{
    public class ChromeDriverManager // ChromeDriver에 관련된 메서드
    {
        private int _aliveConsecutiveFails = 0;          // 연속 실패 횟수
        private const int AliveFailThreshold = 2;        // 이 횟수 이상 연속 실패해야 OFF로 간주
        private DateTime _lastAliveSuccessUtc = DateTime.MinValue;

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

        public static ChromeOptions ChromeDriverOptionSet(string chromePath)
        {
            var options = new ChromeOptions();
            options.BinaryLocation = chromePath;
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");

            // 디버깅 포트
            options.AddArgument("--remote-debugging-port=9222");

            // 크래시 방지 옵션
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            // JS alert 자동 닫힘 방지: 닫지 말고 예외만 던지게
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
