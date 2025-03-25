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

            // Add the remote debugging port argument
            options.AddArgument("--remote-debugging-port=9223"); // 지정된 포트를 사용

            // Optional: If Chrome is crashing, disable sandbox mode
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            return options;
        }


        private void MonitorConnectionAndDriver(IWebDriver driver)
        {
            bool isInternetConnected = CheckInternetConnection(); // 인터넷 연결 상태 확인

            if (isInternetConnected)
            {
                LogMessage("Internet connection is active.", Level.Info);
                MonitorDriverStatus(driver); // 드라이버 상태 점검
            }
            else
            {
                LogMessage("Internet connection lost.", Level.Error);
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
                LogMessage("Page has been refreshed, and the driver is functioning normally.", Level.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"Error with ChromeDriver: {ex.Message}", Level.Critical);

                RestartDriver(driver);  // 에러 발생 시에만 재시작
            }
        }

        public static void RestartDriver(IWebDriver driver)
        {
            if (driver != null)
            {
                LogMessage("Restarting WebDriver...", Level.Critical);
                driver.Quit();
            }

            // 드라이버 재시작
            string chromePath = "C:\\path\\to\\chrome.exe";
            ChromeOptions options = ChromeDriverOptionSet(chromePath);
            driver = new ChromeDriver(options);  // 새로운 드라이버 인스턴스 생성

            LogMessage("WebDriver has been restarted.", Level.Info);
        }

        public void StartMonitoring(IWebDriver driver, string chromePath)
        {
            if (driver == null)
            {
                driver = new ChromeDriver(ChromeDriverManager.ChromeDriverOptionSet(chromePath));
            }

            // 타이머 설정: 5초마다 인터넷 연결 상태 및 드라이버 상태 확인
            _monitoringTimer = new Timer();
            _monitoringTimer.Interval = 5000; // 시간 간격 설정
            _monitoringTimer.Tick += (sender, e) => MonitorConnectionAndDriver(driver);
            _monitoringTimer.Start();
        }



    }
}
