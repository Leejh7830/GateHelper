using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    public static class Util_Connect
    {
        public static bool ConnectToServer(
            IWebDriver driver,
            string mainHandle,
            Config config,
            string serverName,
            ListView listView)
        {
            try
            {
                int tbodyIndex = 1;

                while (true)
                {
                    string serverNameXpath = $"//*[@id='seltable']/tbody[{tbodyIndex}]/tr/td[4]";
                    var serverNameElements = driver.FindElements(By.XPath(serverNameXpath));

                    if (serverNameElements == null || serverNameElements.Count == 0)
                        break;

                    foreach (var element in serverNameElements)
                    {
                        if (element.Text == serverName)
                        {
                            string spanXpath = $"//*[@id='seltable']/tbody[{tbodyIndex}]/tr/td[5]/span[contains(@id, 'rdp')]";
                            var spanElement = driver.FindElement(By.XPath(spanXpath));
                            var aElement = spanElement.FindElement(By.TagName("a"));
                            aElement.Click();

                            System.Threading.Thread.Sleep(1000);

                            try
                             {
                                var alert = driver.SwitchTo().Alert();
                                alert.Accept();
                            }
                            catch (NoAlertPresentException)
                            {
                                SendKeys.SendWait(" ");
                            }

                            // 로그인 정보 입력
                            EnterCredentials(driver, config.GateID, config.GatePW);

                            Util.SwitchToMainHandle(driver, mainHandle);
                            LogMessage("접속 후 MainHandle: " + mainHandle, Level.Info);

                            // 접속 이력 추가
                            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            Util_ServerList.AddServerToListView(listView, serverName, now);
                            Util_ServerList.TrimHistoryList(listView, 30);
                            Util_ServerList.SaveServerDataToFile(listView);

                            return true;
                        }
                    }

                    tbodyIndex++;
                }

                MessageBox.Show($"서버 '{serverName}'를 찾을 수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        private static void SwitchToPopup(IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.WindowHandles.Count > 1);

                string original = driver.CurrentWindowHandle;
                foreach (string handle in driver.WindowHandles)
                {
                    if (handle != original)
                    {
                        driver.SwitchTo().Window(handle);
                        break;
                    }
                }
            }
            catch (WebDriverTimeoutException)
            {
                MessageBox.Show("팝업창이 열리지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"팝업창 전환 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void EnterCredentials(IWebDriver driver, string id, string pw)
        {
            try
            {
                SwitchToPopup(driver);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var idInput = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='userid']")));
                idInput.SendKeys(id);

                var pwInput = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='passwd']")));
                pwInput.SendKeys(pw);

                var loginBtn = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='pop_container']/div[2]/a")));
                loginBtn.Click();
            }
            catch (WebDriverTimeoutException)
            {
                MessageBox.Show("ID/PW 입력 필드 또는 접속 버튼을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"ID/PW 입력 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void AutoConnect_1_Step(IWebDriver driver, Form MainForm)
        {
            
            try
            {
                if (driver == null)
                {
                    MessageBox.Show("드라이버가 초기화되지 않았습니다. 먼저 시작 버튼을 눌러주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Util_Control.ClickElementByXPath(driver, "/html/body/div/div[2]/button[3]"); // 고급
                Util_Control.ClickElementByXPath(driver, "/html/body/div/div[3]/p[2]/a"); // 안전하지않음으로이동

                Util.InputKeys("{Tab},SPACE,{Tab},SPACE"); // MPO Helper

                Util_Control.MoveFormToTop(MainForm);

            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
        }

        public static void AutoConnect_2_Step(IWebDriver driver, Config config, string mainHandle)
        {
            try
            {
                Util.SwitchToMainHandle(driver, mainHandle);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var idInput = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='USERID_ENC']")));
                idInput.SendKeys(config.GateID);
                var pwInput = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='PASSWD']")));
                pwInput.SendKeys(config.GatePW);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
        }




    }

}
