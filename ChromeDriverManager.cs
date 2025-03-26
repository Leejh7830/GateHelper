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
        private IWebDriver _driver;
        private Config _config;
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

        public IWebDriver StartMonitoring(IWebDriver driver, Config config)
        {
            _driver = driver;
            _config = config;

            _monitoringTimer = new Timer();
            _monitoringTimer.Interval = 5000;
            _monitoringTimer.Tick += (s, e) =>
            {
                if (CheckInternetConnection())
                {
                    var checkedDriver = MonitorDriverStatus();
                    if (checkedDriver != _driver)
                    {
                        _driver = checkedDriver; // ✅ 문제가 있을 때만 갱신
                    }
                }
            };
            _monitoringTimer.Start();

            return _driver; // 첫 진입 시에는 기존 driver 그대로 반환
        }

        private IWebDriver MonitorDriverStatus()
        {
            try
            {
                // 새로고침 대신 간단한 접근 시도
                var check = _driver.Title;
                LogMessage("Driver OK", Level.Info);
                return _driver;
            }
            catch
            {
                LogMessage("Driver error. Restarting...", Level.Critical);
                return RestartDriver();
            }
        }

        private IWebDriver RestartDriver()
        {
            try { _driver.Quit(); } catch { }

            _driver = Util.InitializeDriver(_config);
            return _driver;
        }


        private bool CheckInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204")) // 네트워크 확인용 페이지
                {
                    return true; // 인터넷 연결 있음
                }
            }
            catch
            {
                return false; // 인터넷 연결 없음
            }
        }


    }
}
