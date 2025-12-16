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
            string GateID,
            string GatePW,
            string serverName,
            ObjectListView listView,
            bool isDuplicateCheck)
        {
            if (string.IsNullOrEmpty(GateID) || string.IsNullOrEmpty(GatePW))
            {
                MessageBox.Show("GateID/PW NOT Selected.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogMessage("GateID/PW NOT Selected.", Level.Critical);
                return false;
            }

            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(ExpectedConditions.ElementExists(By.Id("seltable")));

                // 페이지 최상단으로 이동
                try { ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);"); } catch { }

                bool foundAndConnected = false;
                bool scrollActivatedLogged = false; // 스크롤 로직 최초 발동 로깅 플래그

                // 스크롤하며 모든 표시 영역 탐색 (가상화/지연 로드 대응)
                long lastHeight = -1;
                for (int attempt = 0; attempt < 20; attempt++) // 최대 시도 제한
                {
                    // 현재 표시된 tbody/행에서 서버명 검색, rdp 버튼 클릭
                    int tbodyIndex = 1;
                    while (true)
                    {
                        string rowXpath = $"//*[@id='seltable']/tbody[{tbodyIndex}]/tr";
                        var rows = driver.FindElements(By.XPath(rowXpath));
                        if (rows == null || rows.Count == 0) break;

                        foreach (var row in rows)
                        {
                            var tds = row.FindElements(By.TagName("td"));
                            bool match = false;
                            foreach (var td in tds)
                            {
                                if (string.Equals(td.Text?.Trim(), serverName, StringComparison.Ordinal))
                                {
                                    match = true;
                                    break;
                                }
                            }
                            if (!match) continue;

                            // 같은 행의 rdp 버튼 찾기
                            IWebElement aElement = null;
                            try
                            {
                                var spanElement = row.FindElement(By.XPath(".//td/span[contains(@id, 'rdp')]"));
                                aElement = spanElement.FindElement(By.TagName("a"));
                            }
                            catch (NoSuchElementException)
                            {
                                continue;
                            }

                            // 버튼을 뷰포트로 스크롤 및 클릭 가능 대기
                            try { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", aElement); } catch { }

                            try
                            {
                                wait.Until(ExpectedConditions.ElementToBeClickable(aElement));
                                aElement.Click();
                            }
                            catch (WebDriverException)
                            {
                                try
                                {
                                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", aElement);
                                }
                                catch (Exception ex)
                                {
                                    LogException(ex, Level.Error);
                                    continue;
                                }
                            }

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
                            EnterCredentials(driver, GateID, GatePW);

                            Util.SwitchToMainHandle(driver, mainHandle);
                            LogMessage("접속 완료, 접속 후 MainHandle: " + mainHandle, Level.Info);

                            // 접속 이력 추가
                            Util_ServerList.AddServerToListView(listView, serverName, DateTime.Now, isDuplicateCheck);
                            Util_ServerList.SaveServerDataToFile(listView);

                            foundAndConnected = true;
                            break;
                        }

                        if (foundAndConnected) break;
                        tbodyIndex++;
                    }

                    if (foundAndConnected) return true;

                    // 더 스크롤할 수 있으면 아래로 계속 스크롤
                    try
                    {
                        // 스크롤 블록 진입 로깅 (최초 1회)
                        if (!scrollActivatedLogged)
                        {
                            LogMessage($"서버 '{serverName}' 화면에 없음. 스크롤 탐색 루프 진입.", Level.Info);
                            scrollActivatedLogged = true;
                        }

                        var heightObj = ((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight");
                        long newHeight = Convert.ToInt64(heightObj);

                        if (newHeight == lastHeight)
                            break;

                        LogMessage($"스크롤 수행 (attempt={attempt}, scrollHeight={newHeight}).", Level.Info);

                        ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, arguments[0]);", newHeight);
                        lastHeight = newHeight;
                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        LogException(ex, Level.Error);
                        break;
                    }
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

                Util.InputKeys(_config.UserID, 200);
                Util.InputKeys("{TAB}", 200);
                Util.InputKeys(_config.UserPW, 200);
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
