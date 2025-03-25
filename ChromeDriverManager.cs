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
    class ChromeDriverManager
    {
        private Timer _monitoringTimer;

        public static ChromeOptions ChromeDriverOptionSet(string chromePath)
        {
            var options = new ChromeOptions();
            options.BinaryLocation = chromePath;
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");

            // 사용자 데이터 디렉토리 설정
            string userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data");
            options.AddArgument($"--user-data-dir={userDataDir}");
            options.AddArgument("--profile-directory=Profile 1");

            // Add the remote debugging port argument
            options.AddArgument("--remote-debugging-port=9222"); // 지정된 포트를 사용

            // Optional: If Chrome is crashing, disable sandbox mode
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            return options;
        }


        private void MonitorConnectionAndDriver(IWebDriver driver)
        {
            // 인터넷 연결 상태 확인
            bool isInternetConnected = CheckInternetConnection();

            if (isInternetConnected)
            {
                LogMessage("Internet connection is active.", Level.Info);
                // 인터넷 연결 복구 시 드라이버 상태를 점검하고 복구 시도
                MonitorDriverStatus(driver); // 드라이버를 넘겨줌
            }
            else
            {
                LogMessage("Internet connection lost.", Level.Error);
                // 인터넷이 재연결될 때까지 기다리기
            }
        }

        private bool CheckInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204")) // 구글 204 페이지를 확인
                {
                    return true; // 인터넷 연결 있음
                }
            }
            catch
            {
                return false; // 인터넷 연결 없음
            }
        }

        public static void MonitorDriverStatus(IWebDriver driver)
        {
            try
            {
                // 페이지 새로 고침
                driver.Navigate().Refresh();

                // 새로 고침 후 페이지가 정상적으로 로드되었는지 확인
                Console.WriteLine("Page has been refreshed, and the driver is functioning normally.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with ChromeDriver: {ex.Message}");

                RestartDriver(driver);
            }
        }

        public static void RestartDriver(IWebDriver driver)
        {
            LogMessage("Restarting WebDriver...", Level.Info);

            driver.Quit();

            string chromePath = "C:\\path\\to\\chrome.exe";
            ChromeOptions options = ChromeDriverOptionSet(chromePath);

            driver = new ChromeDriver(options);

            LogMessage("WebDriver has been restarted.", Level.Info);
        }

        public void StartMonitoring(IWebDriver driver, string chromePath)
        {
            driver = new ChromeDriver(ChromeDriverOptionSet(chromePath));

            // 타이머 설정마다 인터넷 연결 상태 및 드라이버 상태 확인
            _monitoringTimer = new Timer();
            _monitoringTimer.Interval = 5000; // 시간 간격 설정
            _monitoringTimer.Tick += (sender, e) => MonitorConnectionAndDriver(driver);
            _monitoringTimer.Start();
        }



    }
}
