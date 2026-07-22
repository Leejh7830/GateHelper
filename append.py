import sys

content = """
        public static bool IsAuthInProgress = false;

        public static void AutoConnect_3_Step_FetchOTP(OpenQA.Selenium.IWebDriver driver, Config config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.EnportalURL))
                {
                    GateHelper.LogManager.LogMessage("config.txt에 EnportalURL이 설정되어 있지 않습니다.", GateHelper.LogManager.Level.Warning);
                    return;
                }

                GateHelper.LogManager.LogMessage("메일 사이트(Enportal)로 이동 준비 중...", GateHelper.LogManager.Level.Info);

                var js = (OpenQA.Selenium.IJavaScriptExecutor)driver;
                js.ExecuteScript($"window.open('{config.EnportalURL}', '_blank');");
                System.Threading.Thread.Sleep(500);

                var handles = driver.WindowHandles;
                string newTabHandle = handles[handles.Count - 1];
                driver.SwitchTo().Window(newTabHandle);
                
                GateHelper.LogManager.LogMessage("메일 탭 열기 성공. 웹페이지 로딩 대기 중...", GateHelper.LogManager.Level.Info);

                OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, System.TimeSpan.FromSeconds(15));
                try
                {
                    wait.Until(d => ((OpenQA.Selenium.IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                    GateHelper.LogManager.LogMessage("메일 탭 로딩 완료. 로그인 폼 탐색 중...", GateHelper.LogManager.Level.Info);
                }
                catch (OpenQA.Selenium.WebDriverTimeoutException)
                {
                    GateHelper.LogManager.LogMessage("페이지 로딩 타임아웃 (15초). 그래도 탐색을 시도합니다.", GateHelper.LogManager.Level.Warning);
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
                                    if (attempt == 0) GateHelper.LogManager.LogMessage($"{frameTag} index {i} 에서 메일함(자동로그인) 버튼 발견!", GateHelper.LogManager.Level.Info);
                                    break;
                                }

                                var idElements = driver.FindElements(OpenQA.Selenium.By.Name("userid"));
                                if (idElements.Count > 0)
                                {
                                    idInput = idElements[0];
                                    foundInFrame = true;
                                    if (attempt == 0) GateHelper.LogManager.LogMessage($"{frameTag} index {i} 에서 로그인 폼 발견!", GateHelper.LogManager.Level.Info);
                                    break;
                                }
                            }
                            catch { }
                        }
                        if (foundInFrame) break;
                    }

                    if (idInput != null || isAutoLoggedIn) break;
                    System.Threading.Thread.Sleep(200);
                }

                if (isAutoLoggedIn && mailBoxBtn != null)
                {
                    GateHelper.LogManager.LogMessage("자동 로그인이 감지되었습니다. 로그인 과정을 스킵하고 메일함으로 이동합니다.", GateHelper.LogManager.Level.Info);
                    mailBoxBtn.Click();
                    GateHelper.LogManager.LogMessage("메일함 이동 버튼 클릭 완료.", GateHelper.LogManager.Level.Info);
                }
                else if (idInput != null)
                {
                    var pwInput = driver.FindElement(OpenQA.Selenium.By.Name("password"));

                    idInput.SendKeys(config.GateUserID);
                    pwInput.SendKeys(config.GateUserPW);
                    pwInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                    
                    GateHelper.LogManager.LogMessage("Enportal 메일 로그인 요청 완료. 메일함 버튼 대기 중...", GateHelper.LogManager.Level.Info);

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
                        System.Threading.Thread.Sleep(200);
                    }

                    if (mailBoxBtn != null)
                    {
                        mailBoxBtn.Click();
                        GateHelper.LogManager.LogMessage("로그인 성공 및 메일함 이동 버튼 클릭 완료.", GateHelper.LogManager.Level.Info);
                    }
                    else
                    {
                        GateHelper.LogManager.LogMessage("로그인 후 메일함 버튼을 찾을 수 없습니다.", GateHelper.LogManager.Level.Error);
                        return;
                    }
                }
                else
                {
                    GateHelper.LogManager.LogMessage("10초 대기 후에도 모든 프레임에서 userid 입력창이나 메일함 버튼을 찾을 수 없습니다.", GateHelper.LogManager.Level.Error);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                GateHelper.LogManager.LogException(ex, GateHelper.LogManager.Level.Error, "메일 사이트 로그인 처리 중 오류 발생");
            }
        }

        public static void AutoConnect_4_Step_FindAndClickMail(OpenQA.Selenium.IWebDriver driver)
        {
            try
            {
                GateHelper.LogManager.LogMessage("인증번호 메일 탐색을 시작합니다...", GateHelper.LogManager.Level.Info);

                OpenQA.Selenium.IWebElement targetMail = null;

                // 내부 함수: 메일 탐색 로직 (최대 지정된 초 동안 대기)
                OpenQA.Selenium.IWebElement SearchForMail(int waitSeconds)
                {
                    OpenQA.Selenium.IWebElement foundMail = null;
                    int maxAttempts = waitSeconds * 1000 / 300;

                    for (int attempt = 0; attempt < maxAttempts; attempt++)
                    {
                        // 재귀적으로 모든 프레임을 탐색하는 내부 Action
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

                            // 하위 프레임들 순회
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

                        System.Threading.Thread.Sleep(300);
                    }
                    return foundMail;
                }

                // 1. 처음 진입 후 5초간 탐색
                targetMail = SearchForMail(5);

                // 2. 없으면 새로고침(메일함 버튼 다시 클릭) 시도
                if (targetMail == null)
                {
                    GateHelper.LogManager.LogMessage("5초 대기 후 메일이 없어 새로고침(메일함 버튼 클릭)을 시도합니다.", GateHelper.LogManager.Level.Warning);
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
                        GateHelper.LogManager.LogMessage("새로고침 클릭 성공. 다시 5초간 탐색합니다.", GateHelper.LogManager.Level.Info);
                        System.Threading.Thread.Sleep(1000); // 새로고침 렌더링 대기
                        targetMail = SearchForMail(5);
                    }
                }

                // 3. 최종 결과 처리
                if (targetMail != null)
                {
                    targetMail.Click();
                    GateHelper.LogManager.LogMessage("최신 인증번호 메일을 성공적으로 찾아 클릭했습니다.", GateHelper.LogManager.Level.Info);
                }
                else
                {
                    GateHelper.LogManager.LogMessage("새로고침 후에도 'GATEONE' 키워드가 포함된 메일을 찾을 수 없습니다.", GateHelper.LogManager.Level.Error);
                    System.Windows.Forms.MessageBox.Show("인증번호 메일을 찾지 못했습니다.\n네트워크 지연이나 발송 오류일 수 있습니다.\n잠시 후 다시 시도해 주세요.", "메일 수신 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
            catch (System.Exception ex)
            {
                GateHelper.LogManager.LogException(ex, GateHelper.LogManager.Level.Error, "메일 탐색 중 오류 발생");
            }
        }
"""

with open(r'c:\Users\LeeJH\source\repos\Leejh7830\GateHelper\Utils\Util_Connect.cs', 'r', encoding='utf-8') as f:
    text = f.read()

text = text.rstrip().rstrip('}').rstrip().rstrip('}')

with open(r'c:\Users\LeeJH\source\repos\Leejh7830\GateHelper\Utils\Util_Connect.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text + content + '\n    }\n}\n')
