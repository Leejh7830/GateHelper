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

        private bool IsDriverAlive()
        {
            try
            {
                // 간단한 속성 접근만으로도 연결 상태 확인 가능
                var title = _driver.Title;
                return true; // 정상 작동
            }
            catch (WebDriverException)
            {
                return false; // 드라이버와 연결 불가
            }
            catch
            {
                return false; // 다른 예외 → 비정상 상태로 간주
            }
        }

        private bool IsInternetAvailable()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


    }
}
