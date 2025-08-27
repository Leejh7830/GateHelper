using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Windows.Forms;
using System.Threading;
using static GateHelper.LogManager;
using static GateHelper.Util_Element;
using BrightIdeasSoftware;

namespace GateHelper
{
    public static class Util_Connect
    {
        public static bool ConnectToServer(
            IWebDriver driver,
            string mainHandle,
            Config config,
            string serverName,
            ObjectListView listView,
            bool isDuplicateCheck)
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

                            Thread.Sleep(1000);

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
                            Util_ServerList.AddServerToListView(listView, serverName, DateTime.Now, isDuplicateCheck);
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

        public static void AutoConnect_1_Step(IWebDriver driver, Form MainForm)
        {
            // 고급-안전하지않음으로이동-MPO Helper체크-확인
            try
            {
                if (driver == null)
                {
                    MessageBox.Show("드라이버가 초기화되지 않았습니다. 먼저 시작 버튼을 눌러주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ClickElementByXPath(driver, "/html/body/div/div[2]/button[3]"); // 고급
                ClickElementByXPath(driver, "/html/body/div/div[3]/p[2]/a"); // 안전하지않음으로이동

                Util.InputKeys("{Tab},SPACE,{Tab},SPACE"); // MPO Helper

                // Util_Control.MoveFormToTop(MainForm);

            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
        }

        public static void AutoConnect_2_Step(IWebDriver driver, Config _config, string mainHandle)
        {
            // EnID,PW 입력
            try
            {
                Util.SwitchToMainHandle(driver, mainHandle);
                //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                //Util_Control.SendKeysToElement(driver, "//*[@id='USERID']", _config.EnportalID); 
                //Util_Control.SendKeysToElement(driver, "//*[@id='PASSWD']", _config.EnportalPW);
                //Util.InputKeys("{ENTER}", 100);

                Util.InputKeys(_config.EnportalID, 200);
                Util.InputKeys("{TAB}", 200);
                Util.InputKeys(_config.EnportalPW, 200);
                Util.InputKeys("{ENTER}", 200);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
        }

        public static void AutoConnect_3_Step(IWebDriver driver)
        {
            Thread.Sleep(3000);
            ClickElementByXPath(driver, "//*[@id='login_return']");
            // Util_Element.FindAndAlertElement(driver, "//*[@id='login_return']");
        }

    }

}
