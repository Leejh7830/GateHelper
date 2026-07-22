import sys

content = """
        public static bool IsAuthInProgress = false;

        public static void AutoConnect_3_Step_FetchOTP(OpenQA.Selenium.IWebDriver driver, Config config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.EnportalURL))
                {
                    LogMessage("config.txt에 EnportalURL이 설정되어 있지 않습니다.", Level.Warning);
                    return;
                }

                LogMessage("메일 사이트(Enportal)로 이동 준비 중...", Level.Info);

                var js = (OpenQA.Selenium.IJavaScriptExecutor)driver;
                js.ExecuteScript($"window.open('{config.EnportalURL}', '_blank');");
                System.Threading.Thread.Sleep(500);

                var handles = driver.WindowHandles;
                string newTabHandle = handles[handles.Count - 1];
                driver.SwitchTo().Window(newTabHandle);
                
                LogMessage("메일 탭 열기 성공. 웹페이지 로딩 대기 중...", Level.Info);

                OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, System.TimeSpan.FromSeconds(15));
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
                    System.Threading.Thread.Sleep(200);
                }

                if (isAutoLoggedIn && mailBoxBtn != null)
                {
                    LogMessage("자동 로그인이 감지되었습니다. 로그인 과정을 스킵하고 메일함으로 이동합니다.", Level.Info);
                    mailBoxBtn.Click();
                    LogMessage("메일함 이동 버튼 클릭 완료.", Level.Info);
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
                        System.Threading.Thread.Sleep(200);
                    }

                    if (mailBoxBtn != null)
                    {
                        mailBoxBtn.Click();
                        LogMessage("로그인 성공 및 메일함 이동 버튼 클릭 완료.", Level.Info);
                    }
                    else
                    {
                        LogMessage("로그인 후 메일함 버튼을 찾을 수 없습니다.", Level.Error);
                        return;
                    }
                }
                else
                {
                    LogMessage("10초 대기 후에도 모든 프레임에서 userid 입력창이나 메일함 버튼을 찾을 수 없습니다.", Level.Error);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                LogException(ex, Level.Error, "메일 사이트 로그인 처리 중 오류 발생");
            }
        }

        public static void AutoConnect_4_Step_FindAndClickMail(OpenQA.Selenium.IWebDriver driver)
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

                        System.Threading.Thread.Sleep(300);
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
                        System.Threading.Thread.Sleep(1000); 
                        targetMail = SearchForMail(5);
                    }
                }

                if (targetMail != null)
                {
                    try
                    {
                        ((OpenQA.Selenium.IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", targetMail);
                        System.Threading.Thread.Sleep(200);
                        targetMail.Click();
                    }
                    catch (OpenQA.Selenium.ElementNotInteractableException)
                    {
                        LogMessage("일반 클릭 실패. JS Click으로 재시도합니다.", Level.Warning);
                        ((OpenQA.Selenium.IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", targetMail);
                    }
                    catch (System.Exception ex)
                    {
                        LogMessage($"클릭 중 예외 발생, JS Click 시도: {ex.Message}", Level.Warning);
                        ((OpenQA.Selenium.IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", targetMail);
                    }
                    
                    LogMessage("최신 인증번호 메일을 성공적으로 찾아 클릭했습니다.", Level.Info);
                }
                else
                {
                    LogMessage("새로고침 후에도 'GATEONE' 키워드가 포함된 메일을 찾을 수 없습니다.", Level.Error);
                    System.Windows.Forms.MessageBox.Show("인증번호 메일을 찾지 못했습니다.\n네트워크 지연이나 발송 오류일 수 있습니다.\n잠시 후 다시 시도해 주세요.", "메일 수신 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
            catch (System.Exception ex)
            {
                LogException(ex, Level.Error, "메일 탐색 중 오류 발생");
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

                        System.Threading.Thread.Sleep(300);
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
                    System.Threading.Thread.Sleep(200);
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
                    return otpCode;
                }
                else
                {
                    LogMessage("메일 본문에서 6자리 연속된 숫자를 찾을 수 없습니다.", Level.Error);
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                LogException(ex, Level.Error, "인증번호 추출 중 오류 발생");
                return null;
            }
        }

        public static void AutoConnect_6_Step_EnterOTP(OpenQA.Selenium.IWebDriver driver, string otpCode, string originalHandle)
        {
            try
            {
                LogMessage("메일 탭을 닫고 원래 탭(인증번호 입력창)으로 복귀합니다...", Level.Info);
                driver.Close();

                driver.SwitchTo().Window(originalHandle);
                LogMessage("원래 탭으로 복귀 완료. 인증번호 입력을 시작합니다.", Level.Info);
                
                driver.SwitchTo().DefaultContent();
                try
                {
                    driver.SwitchTo().Frame("main");
                }
                catch { }

                OpenQA.Selenium.Support.UI.WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, System.TimeSpan.FromSeconds(10));
                var otpInput = wait.Until(OpenQA.Selenium.Support.UI.ExpectedConditions.ElementIsVisible(OpenQA.Selenium.By.Id("vali_num")));

                otpInput.Clear();
                otpInput.SendKeys(otpCode);
                otpInput.SendKeys(OpenQA.Selenium.Keys.Enter);

                LogMessage("인증번호 입력 및 엔터 완료.", Level.Info);
            }
            catch (System.Exception ex)
            {
                LogException(ex, Level.Error, "인증번호 입력 중 오류 발생");
            }
        }
"""

path = 'Utils/Util_Connect.cs'
with open(path, 'rb') as f:
    text = f.read().decode('euc-kr', errors='replace')

idx = text.rfind('}')
idx2 = text.rfind('}', 0, idx)

new_text = text[:idx2] + content + '    }\n}\n'

with open(path, 'wb') as f:
    f.write(new_text.encode('euc-kr'))

print('Appended successfully')
