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

                            // --- 클릭 전 현재 열려있는 모든 탭/창 목록 캡처 ---
                            var oldHandles = driver.WindowHandles;

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

                            // 💡 기존의 단순 개수(Count) 비교 로직에서 '새로운 핸들' 식별 로직으로 변경 (공지사항 팝업 등 간섭 방지)
                            string popupHandle = "";
                            try
                            {
                                var handleWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                                handleWait.Until(d => {
                                    var newHandles = d.WindowHandles;
                                    foreach(var h in newHandles) {
                                        if (!oldHandles.Contains(h)) {
                                            popupHandle = h;
                                            return true;
                                        }
                                    }
                                    return false;
                                });
                                LogMessage($"새로운 팝업 감지 완료 (popupHandle: {popupHandle})", Level.Info);
                            }
                            catch (WebDriverTimeoutException)
                            {
                                LogMessage("10초 동안 대기했으나 새 팝업창이 생성되지 않았습니다.", Level.Error);
                                return false; // 창이 안 떴으므로 중단
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


        public static bool AutoConnect_1_Step_IDPWInput(IWebDriver driver, Config _config, string mainHandle)
        {
            try
            {
                if (!Util.SwitchToMainHandle(driver, mainHandle))
                {
                    return false; 
                }

                LogMessage("ID/PW 입력(백그라운드 제어)을 시작합니다...", Level.Info);

                OpenQA.Selenium.IWebElement FindElementSafely(string id)
                {
                    OpenQA.Selenium.IWebElement found = null;
                    bool SearchInCurrentFrame()
                    {
                        try
                        {
                            var elements = driver.FindElements(OpenQA.Selenium.By.Id(id));
                            foreach (var el in elements)
                            {
                                if (el.Displayed)
                                {
                                    found = el;
                                    return true;
                                }
                            }
                        }
                        catch { }

                        var iframesChild = driver.FindElements(OpenQA.Selenium.By.TagName("iframe"));
                        var framesChild = driver.FindElements(OpenQA.Selenium.By.TagName("frame"));
                        int totalFrames = iframesChild.Count + framesChild.Count;

                        for (int i = 0; i < totalFrames; i++)
                        {
                            try
                            {
                                driver.SwitchTo().Frame(i);
                                if (SearchInCurrentFrame()) return true;
                                driver.SwitchTo().ParentFrame();
                            }
                            catch { }
                        }
                        return false;
                    }

                    driver.SwitchTo().DefaultContent();
                    if (SearchInCurrentFrame()) return found;
                    return null;
                }

                OpenQA.Selenium.IWebElement idInput = null;
                OpenQA.Selenium.IWebElement pwInput = null;

                for (int i = 0; i < 15; i++)
                {
                    idInput = FindElementSafely("USERID");
                    if (idInput != null) break;
                    Thread.Sleep(500);
                }

                if (idInput == null)
                {
                    LogMessage("10초 대기 후에도 USERID 입력칸을 찾을 수 없습니다.", Level.Error);
                    return false;
                }

                pwInput = FindElementSafely("PASSWD");
                if (pwInput == null)
                {
                    LogMessage("PASSWD 입력칸을 찾을 수 없습니다.", Level.Error);
                    return false;
                }

                idInput.Clear();
                idInput.SendKeys(_config.GateUserID);
                
                pwInput.Clear();
                pwInput.SendKeys(_config.GateUserPW);
                pwInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                
                LogMessage("ID/PW 백그라운드 입력 및 엔터 전송 완료", Level.Info);
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "1단계 ID/PW 입력 중 오류");
                return false;
            }
        }

        public static bool AutoConnect_2_Step_RequestOTPClick(IWebDriver driver)
        {
            try
            {
                LogMessage("인증 팝업 대기 중...", Level.Info);

                LogMessage("인증 팝업 및 버튼 로딩 대기 중...", Level.Info);

                OpenQA.Selenium.IWebElement btn = null;

                bool SearchInCurrentFrame()
                {
                    try
                    {
                        var elements = driver.FindElements(OpenQA.Selenium.By.Id("login_return"));
                        foreach (var el in elements)
                        {
                            if (el.Displayed)
                            {
                                btn = el;
                                return true;
                            }
                        }
                    }
                    catch { }

                    var iframesChild = driver.FindElements(OpenQA.Selenium.By.TagName("iframe"));
                    var framesChild = driver.FindElements(OpenQA.Selenium.By.TagName("frame"));
                    int totalFrames = iframesChild.Count + framesChild.Count;

                    for (int i = 0; i < totalFrames; i++)
                    {
                        try
                        {
                            driver.SwitchTo().Frame(i);
                            if (SearchInCurrentFrame()) return true;
                            driver.SwitchTo().ParentFrame();
                        }
                        catch { }
                    }
                    return false;
                }

                for (int attempt = 0; attempt < 50; attempt++)
                {
                    driver.SwitchTo().DefaultContent();
                    if (SearchInCurrentFrame()) break;
                    Thread.Sleep(200);
                }

                if (btn == null)
                {
                    LogMessage("10초가 지나도 '인증번호 받기' 팝업 버튼을 찾을 수 없습니다.", Level.Error);
                    return false;
                }

                var js = (OpenQA.Selenium.IJavaScriptExecutor)driver;
                try 
                {
                    js.ExecuteScript("arguments[0].click();", btn);
                    LogMessage("'인증번호 받기' JS 클릭 성공", Level.Info);
                } 
                catch 
                {
                    js.ExecuteScript("otp_login();");
                    LogMessage("'인증번호 받기' 함수 직접 호출(otp_login) 성공", Level.Info);
                }

                LogMessage("인증번호 요청 Alert 창 대기 중...", Level.Info);
                bool alertAccepted = false;
                for (int i = 0; i < 50; i++) 
                {
                    try
                    {
                        var alert = driver.SwitchTo().Alert();
                        alert.Accept();
                        alertAccepted = true;
                        LogMessage("Alert 창 '확인' 클릭 완료", Level.Info);
                        break;
                    }
                    catch (OpenQA.Selenium.NoAlertPresentException)
                    {
                        Thread.Sleep(200);
                    }
                }

                if (!alertAccepted)
                {
                    LogMessage("10초 동안 Alert 창이 뜨지 않았습니다.", Level.Warning);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "인증번호 받기 버튼 클릭 실패");
                return false;
            }
        }

        public static bool IsAuthInProgress = false;

        public static bool AutoConnect_3_Step_FetchOTP(OpenQA.Selenium.IWebDriver driver, Config config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.EnportalURL))
                {
                    LogMessage("config.txt에 EnportalURL이 설정되어 있지 않습니다.", Level.Warning);
                    return false;
                }

                LogMessage("메일 사이트(Enportal)로 이동 준비 중...", Level.Info);

                var js = (OpenQA.Selenium.IJavaScriptExecutor)driver;
                js.ExecuteScript($"window.open('{config.EnportalURL}', '_blank');");
                Thread.Sleep(500);

                var handles = driver.WindowHandles;
                string newTabHandle = handles[handles.Count - 1];
                driver.SwitchTo().Window(newTabHandle);
                
                LogMessage("메일 탭 열기 성공. 웹페이지 로딩 대기 중...", Level.Info);

                OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(15));
                try
                {
                    wait.Until(d => ((OpenQA.Selenium.IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                    LogMessage("메일 탭 로딩 완료. 로그인 폼 탐색 중...", Level.Info);
                }
                catch (OpenQA.Selenium.WebDriverTimeoutException)
                {
                    LogMessage("페이지 로딩 타임아웃 (15초). 그래도 탐색을 시도합니다.", Level.Warning);
                }

                OpenQA.Selenium.IWebElement idInput = null;
                OpenQA.Selenium.IWebElement mailBoxBtn = null;
                bool isAutoLoggedIn = false;

                for (int attempt = 0; attempt < 50; attempt++)
                {
                    driver.SwitchTo().DefaultContent();
                    try
                    {
                        var mailElements = driver.FindElements(OpenQA.Selenium.By.CssSelector("#headerCountWrap a.mail-cnt"));
                        if (mailElements.Count > 0)
                        {
                            mailBoxBtn = mailElements[0];
                            isAutoLoggedIn = true;
                            break;
                        }

                        var idElements = driver.FindElements(OpenQA.Selenium.By.Name("userid"));
                        if (idElements.Count > 0)
                        {
                            idInput = idElements[0];
                            break;
                        }
                    }
                    catch { }

                    bool foundInFrame = false;
                    foreach (var frameTag in new[] { "iframe", "frame" })
                    {
                        var frames = driver.FindElements(OpenQA.Selenium.By.TagName(frameTag));
                        for (int i = 0; i < frames.Count; i++)
                        {
                            driver.SwitchTo().DefaultContent();
                            driver.SwitchTo().Frame(i);
                            try
                            {
                                var mailElements = driver.FindElements(OpenQA.Selenium.By.CssSelector("#headerCountWrap a.mail-cnt"));
                                if (mailElements.Count > 0)
                                {
                                    mailBoxBtn = mailElements[0];
                                    isAutoLoggedIn = true;
                                    foundInFrame = true;
                                    if (attempt == 0) LogMessage($"{frameTag} index {i} 에서 메일함(자동로그인) 버튼 발견!", Level.Info);
                                    break;
                                }

                                var idElements = driver.FindElements(OpenQA.Selenium.By.Name("userid"));
                                if (idElements.Count > 0)
                                {
                                    idInput = idElements[0];
                                    foundInFrame = true;
                                    if (attempt == 0) LogMessage($"{frameTag} index {i} 에서 로그인 폼 발견!", Level.Info);
                                    break;
                                }
                            }
                            catch { }
                        }
                        if (foundInFrame) break;
                    }

                    if (idInput != null || isAutoLoggedIn) break;
                    Thread.Sleep(200);
                }

                if (isAutoLoggedIn && mailBoxBtn != null)
                {
                    LogMessage("자동 로그인이 감지되었습니다. 로그인 과정을 스킵하고 메일함으로 이동합니다.", Level.Info);
                    mailBoxBtn.Click();
                    LogMessage("메일함 이동 버튼 클릭 완료.", Level.Info);
                    return true;
                }
                else if (idInput != null)
                {
                    var pwInput = driver.FindElement(OpenQA.Selenium.By.Name("password"));

                    idInput.SendKeys(config.GateUserID);
                    pwInput.SendKeys(config.GateUserPW);
                    pwInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                    
                    LogMessage("Enportal 메일 로그인 요청 완료. 메일함 버튼 대기 중...", Level.Info);

                    mailBoxBtn = null;
                    for (int attempt = 0; attempt < 50; attempt++)
                    {
                        driver.SwitchTo().DefaultContent();
                        try
                        {
                            var mailElements = driver.FindElements(OpenQA.Selenium.By.CssSelector("#headerCountWrap a.mail-cnt"));
                            if (mailElements.Count > 0)
                            {
                                mailBoxBtn = mailElements[0];
                                break;
                            }
                        }
                        catch { }

                        bool foundInFrame = false;
                        foreach (var frameTag in new[] { "iframe", "frame" })
                        {
                            var frames = driver.FindElements(OpenQA.Selenium.By.TagName(frameTag));
                            for (int i = 0; i < frames.Count; i++)
                            {
                                driver.SwitchTo().DefaultContent();
                                driver.SwitchTo().Frame(i);
                                try
                                {
                                    var mailElements = driver.FindElements(OpenQA.Selenium.By.CssSelector("#headerCountWrap a.mail-cnt"));
                                    if (mailElements.Count > 0)
                                    {
                                        mailBoxBtn = mailElements[0];
                                        foundInFrame = true;
                                        break;
                                    }
                                }
                                catch { }
                            }
                            if (foundInFrame) break;
                        }

                        if (mailBoxBtn != null) break;
                        Thread.Sleep(200);
                    }

                    if (mailBoxBtn != null)
                    {
                        mailBoxBtn.Click();
                        LogMessage("로그인 성공 및 메일함 이동 버튼 클릭 완료.", Level.Info);
                        return true;
                    }
                    else
                    {
                        LogMessage("로그인 후 메일함 버튼을 찾을 수 없습니다.", Level.Error);
                        return false;
                    }
                }
                else
                {
                    LogMessage("10초 대기 후에도 모든 프레임에서 userid 입력창이나 메일함 버튼을 찾을 수 없습니다.", Level.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "메일 사이트 로그인 처리 중 오류 발생");
                return false;
            }
        }

        public static bool AutoConnect_4_Step_FindAndClickMail(OpenQA.Selenium.IWebDriver driver)
        {
            try
            {
                LogMessage("인증번호 메일 탐색을 시작합니다...", Level.Info);

                OpenQA.Selenium.IWebElement targetMail = null;

                OpenQA.Selenium.IWebElement SearchForMail(int waitSeconds)
                {
                    OpenQA.Selenium.IWebElement foundMail = null;
                    int maxAttempts = waitSeconds * 1000 / 300; 

                    for (int attempt = 0; attempt < maxAttempts; attempt++)
                    {
                        bool SearchInCurrentFrame()
                        {
                            try
                            {
                                var mailItems = driver.FindElements(OpenQA.Selenium.By.CssSelector("li p[title*='GATEONE']"));
                                if (mailItems.Count > 0)
                                {
                                    var liElement = mailItems[0].FindElement(OpenQA.Selenium.By.XPath(".."));
                                    var clickableAreas = liElement.FindElements(OpenQA.Selenium.By.CssSelector("a.title-block"));
                                    foundMail = clickableAreas.Count > 0 ? clickableAreas[0] : mailItems[0];
                                    return true;
                                }
                            }
                            catch { }

                            var iframes = driver.FindElements(OpenQA.Selenium.By.TagName("iframe"));
                            var frames = driver.FindElements(OpenQA.Selenium.By.TagName("frame"));
                            int totalFrames = iframes.Count + frames.Count;

                            for (int i = 0; i < totalFrames; i++)
                            {
                                try
                                {
                                    driver.SwitchTo().Frame(i);
                                    if (SearchInCurrentFrame()) return true;
                                    driver.SwitchTo().ParentFrame();
                                }
                                catch { }
                            }
                            return false;
                        }

                        driver.SwitchTo().DefaultContent();
                        if (SearchInCurrentFrame()) break;

                        Thread.Sleep(300);
                    }
                    return foundMail;
                }

                targetMail = SearchForMail(5);

                if (targetMail == null)
                {
                    LogMessage("5초 대기 후 메일이 없어 새로고침(메일함 버튼 클릭)을 시도합니다.", Level.Warning);
                    bool refreshClicked = false;
                    
                    driver.SwitchTo().DefaultContent();
                    try
                    {
                        var refreshBtns = driver.FindElements(OpenQA.Selenium.By.CssSelector("#headerCountWrap a.mail-cnt"));
                        if (refreshBtns.Count > 0) { refreshBtns[0].Click(); refreshClicked = true; }
                    }
                    catch { }

                    if (!refreshClicked)
                    {
                        foreach (var frameTag in new[] { "iframe", "frame" })
                        {
                            var frames = driver.FindElements(OpenQA.Selenium.By.TagName(frameTag));
                            for (int i = 0; i < frames.Count; i++)
                            {
                                driver.SwitchTo().DefaultContent();
                                driver.SwitchTo().Frame(i);
                                try
                                {
                                    var refreshBtns = driver.FindElements(OpenQA.Selenium.By.CssSelector("#headerCountWrap a.mail-cnt"));
                                    if (refreshBtns.Count > 0)
                                    {
                                        refreshBtns[0].Click();
                                        refreshClicked = true;
                                        break;
                                    }
                                }
                                catch { }
                            }
                            if (refreshClicked) break;
                        }
                    }

                    if (refreshClicked)
                    {
                        LogMessage("새로고침 클릭 성공. 다시 5초간 탐색합니다.", Level.Info);
                        Thread.Sleep(1000); 
                        targetMail = SearchForMail(5);
                    }
                }

                if (targetMail != null)
                {
                    try
                    {
                        ((OpenQA.Selenium.IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", targetMail);
                        Thread.Sleep(200);
                        targetMail.Click();
                    }
                    catch (OpenQA.Selenium.ElementNotInteractableException)
                    {
                        LogMessage("일반 클릭 실패. JS Click으로 재시도합니다.", Level.Warning);
                        ((OpenQA.Selenium.IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", targetMail);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"클릭 중 예외 발생, JS Click 시도: {ex.Message}", Level.Warning);
                        ((OpenQA.Selenium.IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", targetMail);
                    }
                    
                    LogMessage("최신 인증번호 메일을 성공적으로 찾아 클릭했습니다.", Level.Info);
                    return true;
                }
                else
                {
                    LogMessage("새로고침 후에도 'GATEONE' 키워드가 포함된 메일을 찾을 수 없습니다.", Level.Error);
                    MessageBox.Show("인증번호 메일을 찾지 못했습니다.\n네트워크 지연이나 발송 오류일 수 있습니다.\n잠시 후 다시 시도해 주세요.", "메일 수신 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "메일 탐색 중 오류 발생");
                return false;
            }
        }

        public static string AutoConnect_5_Step_ExtractOTP(OpenQA.Selenium.IWebDriver driver)
        {
            try
            {
                LogMessage("메일 본문에서 인증번호 추출을 시도합니다...", Level.Info);

                OpenQA.Selenium.IWebElement SearchForIframe(int waitSeconds)
                {
                    OpenQA.Selenium.IWebElement foundIframe = null;
                    int maxAttempts = waitSeconds * 1000 / 300; 

                    for (int attempt = 0; attempt < maxAttempts; attempt++)
                    {
                        bool SearchInCurrentFrame()
                        {
                            try
                            {
                                var iframes = driver.FindElements(OpenQA.Selenium.By.Id("ctl00_CotPlaceContent_hifrMailContent"));
                                if (iframes.Count > 0)
                                {
                                    foundIframe = iframes[0];
                                    return true;
                                }
                            }
                            catch { }

                            var iframesChild = driver.FindElements(OpenQA.Selenium.By.TagName("iframe"));
                            var framesChild = driver.FindElements(OpenQA.Selenium.By.TagName("frame"));
                            int totalFrames = iframesChild.Count + framesChild.Count;

                            for (int i = 0; i < totalFrames; i++)
                            {
                                try
                                {
                                    driver.SwitchTo().Frame(i);
                                    if (SearchInCurrentFrame()) return true;
                                    driver.SwitchTo().ParentFrame();
                                }
                                catch { }
                            }
                            return false;
                        }

                        driver.SwitchTo().DefaultContent();
                        if (SearchInCurrentFrame()) break;

                        Thread.Sleep(300);
                    }
                    return foundIframe;
                }

                OpenQA.Selenium.IWebElement iframe = SearchForIframe(15);

                if (iframe == null)
                {
                    LogMessage("메일 본문을 담고 있는 iframe을 찾을 수 없습니다.", Level.Error);
                    return null;
                }

                driver.SwitchTo().Frame(iframe);
                LogMessage("메일 본문 iframe 전환 성공.", Level.Info);

                OpenQA.Selenium.IWebElement divBody = null;
                for (int i = 0; i < 50; i++)
                {
                    try
                    {
                        var bodies = driver.FindElements(OpenQA.Selenium.By.Id("divBody"));
                        if (bodies.Count > 0)
                        {
                            divBody = bodies[0];
                            break;
                        }
                    }
                    catch { }
                    Thread.Sleep(200);
                }

                if (divBody == null)
                {
                    LogMessage("인증번호 텍스트가 있는 divBody 영역을 찾을 수 없습니다.", Level.Error);
                    return null;
                }

                string mailText = divBody.Text;
                LogMessage($"추출된 메일 텍스트: {mailText}", Level.Info);

                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(mailText, @"\d{6}");
                if (match.Success)
                {
                    string otpCode = match.Value;
                    LogMessage($"인증번호 추출 완료: [{otpCode}]", Level.Info);

                    try
                    {
                        LogMessage("추출 완료된 인증번호 메일의 완전삭제를 시도합니다...", Level.Info);
                        
                        OpenQA.Selenium.IWebElement FindDeleteBtn()
                        {
                            OpenQA.Selenium.IWebElement foundBtn = null;
                            bool SearchInCurrentFrame()
                            {
                                try
                                {
                                    // 완전삭제 a 태그 또는 span 태그 ID를 찾습니다.
                                    var btns = driver.FindElements(OpenQA.Selenium.By.Id("ctl00_CotPlaceButton_hbtnHardDelete"));
                                    if (btns.Count == 0)
                                        btns = driver.FindElements(OpenQA.Selenium.By.Id("ctl00_CotPlaceButton_hspanHardDelete"));

                                    foreach (var btn in btns)
                                    {
                                        if (btn.Displayed)
                                        {
                                            foundBtn = btn;
                                            return true;
                                        }
                                    }
                                }
                                catch { }

                                var iframesChild = driver.FindElements(OpenQA.Selenium.By.TagName("iframe"));
                                var framesChild = driver.FindElements(OpenQA.Selenium.By.TagName("frame"));
                                int totalFrames = iframesChild.Count + framesChild.Count;

                                for (int i = 0; i < totalFrames; i++)
                                {
                                    try
                                    {
                                        driver.SwitchTo().Frame(i);
                                        if (SearchInCurrentFrame()) return true;
                                        driver.SwitchTo().ParentFrame();
                                    }
                                    catch { }
                                }
                                return false;
                            }

                            driver.SwitchTo().DefaultContent();
                            if (SearchInCurrentFrame()) return foundBtn;
                            return null;
                        }

                        var deleteBtn = FindDeleteBtn();
                        if (deleteBtn != null)
                        {
                            deleteBtn.Click();
                            LogMessage("완전삭제 버튼 클릭 완료. 알럿(Alert) 발생 대기 중...", Level.Info);
                            Thread.Sleep(500); // 자바스크립트 알럿(확인 창) 대기

                            try
                            {
                                var alert = driver.SwitchTo().Alert();
                                alert.Accept();
                                LogMessage("메일 완전삭제 알림창(Alert) 자동 승인 완료.", Level.Info);
                                Thread.Sleep(500); // 삭제 완료까지 잠시 대기
                            }
                            catch
                            {
                                LogMessage("알림창(Alert)이 발생하지 않았거나 자동으로 처리되었습니다.", Level.Info);
                            }
                            LogMessage("사용된 메일 완전삭제 처리가 완료되었습니다.", Level.Info);
                        }
                        else
                        {
                            LogMessage("완전삭제 버튼을 찾지 못해 삭제 과정을 건너뜁니다.", Level.Warning);
                        }
                    }
                    catch (Exception exDel)
                    {
                        LogMessage($"메일 완전삭제 중 오류 발생(무시됨): {exDel.Message}", Level.Warning);
                    }

                    return otpCode;
                }
                else
                {
                    LogMessage("메일 본문에서 6자리 연속된 숫자를 찾을 수 없습니다.", Level.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "인증번호 추출 중 오류 발생");
                return null;
            }
        }

        public static bool AutoConnect_6_Step_EnterOTP(OpenQA.Selenium.IWebDriver driver, string otpCode, string originalHandle)
        {
            try
            {
                LogMessage("메일 탭을 닫고 원래 탭(인증번호 입력창)으로 복귀합니다...", Level.Info);
                driver.Close();

                driver.SwitchTo().Window(originalHandle);
                LogMessage("원래 탭으로 복귀 완료. 인증번호 입력을 시작합니다.", Level.Info);
                
                OpenQA.Selenium.IWebElement SearchForOtpInput(int waitSeconds)
                {
                    OpenQA.Selenium.IWebElement foundInput = null;
                    int maxAttempts = waitSeconds * 1000 / 300; 

                    for (int attempt = 0; attempt < maxAttempts; attempt++)
                    {
                        bool SearchInCurrentFrame()
                        {
                            try
                            {
                                var inputs = driver.FindElements(OpenQA.Selenium.By.Id("vali_num"));
                                // 여러 개가 존재할 수 있으므로(숨겨진 과거 팝업 등), 실제로 화면에 보이는 녀석을 우선 찾습니다.
                                foreach (var input in inputs)
                                {
                                    if (input.Displayed)
                                    {
                                        foundInput = input;
                                        return true;
                                    }
                                }
                                
                                // 만약 화면에 보이는게 당장 없더라도, 혹시 모르니 마지막 요소를 보험으로 저장해둡니다 (하지만 계속 탐색 진행)
                                if (inputs.Count > 0 && foundInput == null)
                                {
                                    foundInput = inputs[inputs.Count - 1];
                                }
                            }
                            catch { }

                            var iframesChild = driver.FindElements(OpenQA.Selenium.By.TagName("iframe"));
                            var framesChild = driver.FindElements(OpenQA.Selenium.By.TagName("frame"));
                            int totalFrames = iframesChild.Count + framesChild.Count;

                            for (int i = 0; i < totalFrames; i++)
                            {
                                try
                                {
                                    driver.SwitchTo().Frame(i);
                                    if (SearchInCurrentFrame()) return true;
                                    driver.SwitchTo().ParentFrame();
                                }
                                catch { }
                            }
                            return false;
                        }

                        driver.SwitchTo().DefaultContent();
                        if (SearchInCurrentFrame()) break;

                        Thread.Sleep(300);
                    }
                    return foundInput;
                }

                OpenQA.Selenium.IWebElement otpInput = SearchForOtpInput(15);

                if (otpInput == null)
                {
                    LogMessage("15초 대기 후에도 인증번호 입력창(vali_num)을 찾을 수 없습니다.", Level.Error);
                    return false;
                }

                try
                {
                    LogMessage("[입력 1단계] 일반 SendKeys 시도...", Level.Info);
                    otpInput.Clear();
                    otpInput.SendKeys(otpCode);
                    otpInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                    LogMessage("[입력 1단계] 일반 SendKeys 성공!", Level.Info);
                }
                catch (Exception ex1)
                {
                    LogMessage($"[입력 1단계] 일반 SendKeys 실패: {ex1.Message}", Level.Warning);
                    
                    var js = (OpenQA.Selenium.IJavaScriptExecutor)driver;
                    try
                    {
                        LogMessage("[입력 2단계] 스크롤 이동 후 일반 SendKeys 시도...", Level.Info);
                        js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", otpInput);
                        Thread.Sleep(200);
                        otpInput.SendKeys(otpCode);
                        otpInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                        LogMessage("[입력 2단계] 스크롤 후 일반 SendKeys 성공!", Level.Info);
                    }
                    catch (Exception ex2)
                    {
                        LogMessage($"[입력 2단계] 스크롤 후 일반 SendKeys 실패: {ex2.Message}", Level.Warning);
                        try
                        {
                            LogMessage("[입력 3단계] JS 값 강제 주입 및 엔터키 전송 시도...", Level.Info);
                            // form.submit()은 페이지 전체 새로고침을 유발할 수 있으므로 제거!
                            // 1. 값 강제 주입
                            js.ExecuteScript("arguments[0].value = arguments[1];", otpInput, otpCode);
                            // 2. React/Vue 등 프론트엔드 프레임워크가 값을 인식하도록 이벤트 발생
                            js.ExecuteScript("arguments[0].dispatchEvent(new Event('input', { bubbles: true })); arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", otpInput);
                            
                            // 3. 해당 엘리먼트에 강제로 포커스를 맞춤
                            js.ExecuteScript("arguments[0].focus();", otpInput);
                            Thread.Sleep(200);
                            
                            // 4. 포커스된 엘리먼트(ActiveElement)에 엔터키만 전송하여 정상적인 자체 JS 검증을 타도록 유도
                            driver.SwitchTo().ActiveElement().SendKeys(OpenQA.Selenium.Keys.Enter);
                            
                            LogMessage("[입력 3단계] JS 강제 주입 및 포커스 엔터 전송 성공!", Level.Info);
                        }
                        catch (Exception ex3)
                        {
                            LogMessage($"[입력 3단계] JS 주입/엔터 전송 실패: {ex3.Message}", Level.Warning);
                            try
                            {
                                LogMessage("[입력 4단계] 화면 전체를 덮는 강제 Action 키보드 타이핑 시도...", Level.Info);
                                // 포커스마저 안 먹힐 경우 최후의 수단으로 화면 자체에 키보드를 쏴버림
                                new OpenQA.Selenium.Interactions.Actions(driver)
                                    .SendKeys(otpCode)
                                    .SendKeys(OpenQA.Selenium.Keys.Enter)
                                    .Perform();
                                LogMessage("[입력 4단계] Action 강제 키보드 타이핑 성공!", Level.Info);
                            }
                            catch (Exception ex4)
                            {
                                LogMessage($"[입력 4단계] Action 키보드 타이핑마저 실패: {ex4.Message}", Level.Error);
                            }
                        }
                    }
                }

                LogMessage("인증번호 입력 및 엔터 절차 완료.", Level.Info);
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "인증번호 입력 중 오류 발생");
                return false;
            }
        }
    }
}
