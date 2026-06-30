using BrightIdeasSoftware;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;
using System.Windows.Forms;
using static GateHelper.LogManager;
using static GateHelper.Util_Element;

namespace GateHelper
{
    public static class Util_Connect
    {
        // ★ 접속 진행 중 플래그 (타이머 인터락용)
        public static volatile bool IsConnecting = false;

        public static bool ConnectToServer(
            IWebDriver driver,
            string mainHandle,
            string managementHandle,
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

            IsConnecting = true; // 진입 시 접속진행중 플래그 ON
            try
            {
                try
                {
                    driver.SwitchTo().Window(mainHandle);
                }
                catch
                {
                    LogMessage("Critical: 메인 핸들 복구 실패.", Level.Critical);
                    return false;
                }

                // [중요] 보조 사이트가 수동으로 닫혔을 경우를 대비해 managementHandle 상태 확인
                // 현재 열린 창 목록에 없다면 변수를 비워버려야 팝업 찾기 로직에서 꼬이지 않음
                string actualManagementHandle = managementHandle;
                if (!string.IsNullOrEmpty(managementHandle) && !driver.WindowHandles.Contains(managementHandle)) // > Handle값은 있지만 WindowTab에 없을 때 = 닫혔다고 판단
                {
                    actualManagementHandle = ""; // 닫혔으므로 값 초기화
                    LogMessage("보조 사이트가 수동으로 닫힌 것을 감지했습니다.", Level.Info);
                }

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

                            Thread.Sleep(1000); // Alert 대기 시간

                            try
                            {
                                var alert = driver.SwitchTo().Alert();
                                alert.Accept();
                            }
                            catch (NoAlertPresentException)
                            {
                                SendKeys.SendWait(" ");
                            }

                            // 창의 개수가 현재(메인+보조 = 2개)보다 많아질 때까지 최대 10초 대기
                            try
                            {
                                // 현재 열려있어야 할 기본 창 개수 계산 (메인 1개 + 보조가 있다면 1개)
                                int baseCount = string.IsNullOrEmpty(actualManagementHandle) ? 1 : 2;

                                var handleWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                                handleWait.Until(d => d.WindowHandles.Count > baseCount);

                                LogMessage($"팝업 감지 완료 (현재 창 개수: {driver.WindowHandles.Count})", Level.Info);
                            }
                            catch (WebDriverTimeoutException)
                            {
                                LogMessage("10초 동안 대기했으나 새 팝업창이 생성되지 않았습니다.", Level.Error);
                                return false; // 창이 안 떴으므로 중단
                            }

                            // 현재 모든 핸들을 가져와서 '팝업' 찾기
                            string popupHandle = "";
                            var allHandles = driver.WindowHandles;

                            foreach (var handle in allHandles)
                            {
                                // 1. 메인 핸들은 무조건 제외
                                if (handle == mainHandle) continue;

                                // 2. 보조 사이트 핸들이 유효할 때만 제외 (값이 없으면 이 조건은 무시됨)
                                if (!string.IsNullOrEmpty(actualManagementHandle) && handle == actualManagementHandle)
                                    continue;

                                // 3. 위 두 조건에 걸리지 않았다면 그것이 진짜 서버 접속 팝업
                                popupHandle = handle;
                                LogMessage($"popupHandle = {popupHandle}", Level.Info);
                                break;
                            }

                            if (!string.IsNullOrEmpty(popupHandle))
                            {
                                driver.SwitchTo().Window(popupHandle);
                                LogMessage("서버 접속 팝업으로 포커스 이동 완료", Level.Info);

                                // 포커스 이동 성공 시에만 로그인 정보 입력 시도
                                EnterCredentials(driver, popupHandle, GateID, GatePW);
                            }
                            else
                            {
                                LogMessage("서버 접속 팝업을 찾을 수 없습니다.", Level.Error);
                                return false; 
                            }

                            if (!Util.SwitchToMainHandle(driver, mainHandle)) return false; // MainHandle 없음

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
            } finally
            {
                IsConnecting = false; // 접속 종료 시 플래그 OFF
            }
        }


        private static void EnterCredentials(IWebDriver driver, string popupHandle, string id, string pw)
        {
            try
            {
                driver.SwitchTo().Window(popupHandle);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var idInput = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='userid']")));
                idInput.SendKeys(id);

                var pwInput = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='passwd']")));
                pwInput.SendKeys(pw);

                var loginBtn = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='pop_container']/div[2]/a")));
                loginBtn.Click();
                LogMessage("서버 접속 정보 입력 완료", Level.Info);
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
                if (!Util.SwitchToMainHandle(driver, mainHandle))
                {
                    return; // MainHandle 없음
                }

                //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                //Util_Control.SendKeysToElement(driver, "//*[@id='USERID']", _config.EnportalID); 
                //Util_Control.SendKeysToElement(driver, "//*[@id='PASSWD']", _config.EnportalPW);
                //Util.InputKeys("{ENTER}", 100);

                Util.InputKeys(_config.GateUserID, 200);
                Util.InputKeys("{TAB}", 200);
                Util.InputKeys(_config.GateUserPW, 200);
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
