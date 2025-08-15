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

        // Driver 체크용
        public bool IsDriverAlive(IWebDriver _driver)
        {
            try { var title = _driver.Title; return true; }
            catch { return false; }
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
            MessageBox.Show("ChromeDriver가 OFF 상태입니다. 드라이버를 실행해주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }
}
