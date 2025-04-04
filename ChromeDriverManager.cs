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

        public static ChromeOptions ChromeDriverOptionSet(string chromePath)
        {
            var options = new ChromeOptions();
            options.BinaryLocation = chromePath;
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");

            // 사용자 데이터 디렉토리 및 프로필 제거 (지금 필요 없음)
            // options.AddArgument($"--user-data-dir={userDataDir}");
            // options.AddArgument("--profile-directory=Profile 1");

            // Add the remote debugging port argument
            options.AddArgument("--remote-debugging-port=9222");

            // Optional: If Chrome is crashing, disable sandbox mode
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            return options;
        } 

        public bool IsDriverAlive(IWebDriver _driver)
        {
            try { var title = _driver.Title; return true; }
            catch { return false; }
        }

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


    }
}
