﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using SeleniumExtras.WaitHelpers;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Level = GateHelper.LogManager.Level;
using System.Diagnostics;

namespace GateHelper
{
    public static class Util
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        // ChromeDriver 초기화 메소드
        public static IWebDriver InitializeDriver(Config config)
        {
            try
            {
                string driverDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string driverPath = Path.Combine(driverDirectory, "chromedriver.exe");

                // ChromeDriver 존재 확인
                if (!File.Exists(driverPath))
                {
                    MessageBox.Show("ChromeDriver가 존재하지 않습니다");
                    throw new Exception($"ChromeDriver가 존재하지 않습니다: {driverPath}");
                }

                // Chrome 실행 경로 확인
                string chromePath = config.ChromePath; // 설정 파일 경로 사용
                if (string.IsNullOrEmpty(chromePath) || !File.Exists(chromePath))
                {
                    chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"; // 기본 경로 (64비트)
                    if (!File.Exists(chromePath))
                    {
                        chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"; // 32비트 경로
                    }
                    if (!File.Exists(chromePath))
                    {
                        chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                    @"Google\Chrome\Application\chrome.exe"); // 사용자 폴더 경로
                    }
                }

                // Chrome 실행 파일 존재 확인
                if (!File.Exists(chromePath))
                {
                    throw new Exception($"Chrome 실행 파일을 찾을 수 없습니다.\n경로: {chromePath}\n설정 파일에서 지정한 경로를 확인해 주세요.");
                }

                // Chrome 옵션 설정
                ChromeOptions options = ChromeDriverManager.ChromeDriverOptionSet(chromePath);

                // ChromeDriver 실행
                var service = ChromeDriverService.CreateDefaultService(driverDirectory);
                service.HideCommandPromptWindow = true; // cmd 창 숨김

                return new ChromeDriver(service, options);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                throw;
            }
        }



        public static void ClickFavBtn(IWebDriver _driver, Config config, int favIndex, Action serverListLoadAction)
        {
            try
            {
                string favKey = $"Fav{favIndex}";
                LogManager.LogMessage($"BtnFav{favIndex} Click", Level.Info);
                string serverName, serverIP;
                ValidateServerInfo(config.GetType().GetProperty(favKey).GetValue(config).ToString(), out serverName, out serverIP);

                if (!string.IsNullOrEmpty(serverIP))
                {
                    // IP 주소인 경우
                    Util_Control.SendKeysToElement(_driver, "//*[@id='id_IPADDR']", serverIP);
                    Util_Control.SendKeysToElement(_driver, "//*[@id='id_DEVNAME']", "");
                }
                else if (!string.IsNullOrEmpty(serverName))
                {
                    // 서버 이름인 경우
                    Util_Control.SendKeysToElement(_driver, "//*[@id='id_DEVNAME']", serverName);
                    Util_Control.SendKeysToElement(_driver, "//*[@id='id_IPADDR']", "");
                }

                Util_Control.ClickElementByXPath(_driver, "//*[@id='access_control']/table/tbody/tr[2]/td/a");

                WaitForElementLoadByXPath(_driver, "//*[@id=\'seltable\']/tbody[1]/tr/td[4]", 10);

                serverListLoadAction?.Invoke(); // 서버리스트 로드
            }
            catch (ArgumentException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show(ex.Message, "알림");
            }
            catch (NoSuchElementException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show("요소를 찾을 수 없습니다.", "오류");
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Critical);
                MessageBox.Show("예상치 못한 오류가 발생했습니다.", "오류");
            }
        }


        public static void WaitForElementLoadByXPath(IWebDriver driver, string xpath, int timeoutSeconds = 30)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));
            }
            catch (WebDriverTimeoutException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"XPath 요소 로딩 시간 초과 (제한 시간: {timeoutSeconds}초)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // 예외 다시 던지기
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"XPath 요소 로딩 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // 예외 다시 던지기
            }
        }


        public static void InputKeys(string keys, int intervalMilliseconds = 1000) // 메서드 이름 변경
        {
            try
            {
                Thread.Sleep(1000);
                string[] keyArray = keys.Split(',');

                foreach (string key in keyArray)
                {
                    string trimmedKey = key.Trim();

                    if (trimmedKey.StartsWith("{") && trimmedKey.EndsWith("}"))
                    {
                                SendKeys.SendWait(trimmedKey);
                    }
                    else if (trimmedKey.ToUpper() == "SPACE")
                    {
                         SendKeys.SendWait(" ");
                    }
                    else
                    {
                        SendKeys.SendWait(trimmedKey);
                    }

                    Thread.Sleep(intervalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"키 입력 오류: {ex.Message}");
                LogManager.LogException(ex, Level.Error);
            }
        }




        public static string FindWindowHandleByUrl(IWebDriver _driver, string url)
        {
            ReadOnlyCollection<string> windowHandles = _driver.WindowHandles;

            foreach (string handle in windowHandles)
            {
                _driver.SwitchTo().Window(handle);
                if (_driver.Url == url)
                {
                    return handle;
                }
            }
            return null;
        }

        public static void FocusMainWindow(string chromeHandleString)
        {
            try
            {
                if (!string.IsNullOrEmpty(chromeHandleString))
                {
                    ulong chromeHandleULong = Convert.ToUInt64(chromeHandleString, 16);
                    IntPtr chromeHandle = new IntPtr((long)chromeHandleULong);

                    if (chromeHandle != IntPtr.Zero)
                    {
                        if (!SetForegroundWindow(chromeHandle))
                        {
                            MessageBox.Show("크롬 창으로 포커스 이동 실패.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("크롬 창 핸들을 찾을 수 없습니다.");
                    }
                }
                else
                {
                    MessageBox.Show("크롬 창 핸들을 찾을 수 없습니다.");
                }
            }
            catch (FormatException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"잘못된 핸들 문자열 형식: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OverflowException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"핸들 문자열 값이 범위를 벗어남: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"크롬 창 포커스 이동 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void SwitchToMainHandle(IWebDriver _driver, string mainHandle)
        {
            _driver.SwitchTo().Window(mainHandle);
        }

        // iframe순회, 클릭가능한 Element 조사
        public static void InvestigateIframesAndCollectClickableElements(IWebDriver _driver)
        {
            try
            {
                StringBuilder elementInfo = new StringBuilder();

                // 기본 문서의 클릭 가능한 요소 조사
                CollectClickableElements(_driver, elementInfo, "기본 문서");

                // 모든 iframe 찾기
                IReadOnlyCollection<IWebElement> iframes = _driver.FindElements(By.TagName("iframe"));

                if (iframes.Count > 0)
                {
                    foreach (IWebElement iframe in iframes)
                    {
                        // iframe으로 전환
                        _driver.SwitchTo().Frame(iframe);

                        // iframe 내부의 클릭 가능한 요소 조사
                        CollectClickableElements(_driver, elementInfo, $"iframe (이름: {iframe.GetAttribute("name")}, ID: {iframe.GetAttribute("id")})");

                        // 기본 문서로 전환
                        _driver.SwitchTo().DefaultContent();
                    }
                }

                // 메시지 박스에 요소 정보 표시
                MessageBox.Show(elementInfo.ToString(), "클릭 가능한 요소 정보");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"iframe 조사 및 요소 수집 중 오류 발생: {ex.Message}", "오류");
                LogManager.LogException(ex, Level.Error);
            }
        }

        private static void CollectClickableElements(IWebDriver driver, StringBuilder elementInfo, string location)
        {
            // 클릭 가능한 요소 찾기 (button, a, input[type=button], input[type=submit])
            IReadOnlyCollection<IWebElement> clickableElements = driver.FindElements(By.XPath("//button | //a | //input[@type='button'] | //input[@type='submit']"));

            if (clickableElements.Count > 0)
            {
                elementInfo.AppendLine($"\n{location}의 클릭 가능한 요소:");
                foreach (IWebElement element in clickableElements)
                {
                    elementInfo.AppendLine($"XPath: {GetXPath(driver ,element)}");
                }
            }
            else
            {
                elementInfo.AppendLine($"\n{location}에 클릭 가능한 요소가 없습니다.");
            }
        }

        private static string GetXPath(IWebDriver driver, IWebElement element)
        {
            try
            {
                return (string)((IJavaScriptExecutor)driver).ExecuteScript(
                    "gPt=function(c){if(c.id!==''){return'id(\"'+c.id+'\")'}if(c===document.body){return'html/'+c.tagName}var a=0;var e=c.parentNode.childNodes;for(var b=0;b<e.length;b++){var d=e[b];if(d===c){return gPt(c.parentNode)+'/'+c.tagName+'['+(a+1)+']'}if(d.nodeType===1&&d.tagName===c.tagName){a++}}};return gPt(arguments[0]).toLowerCase();",
                    element);
            }
            catch (Exception)
            {
                return "XPath를 가져올 수 없습니다.";
            }
        }

        public static void ValidateServerInfo(string inputText, out string serverName, out string serverIP)
        {
            serverName = null;
            serverIP = null;

            if (string.IsNullOrEmpty(inputText))
            {
                throw new ArgumentException("검색어 입력 값 없음.");
            }

            // IP 주소 형식 검사 (정규식 사용, 부분 입력 허용)
            string ipPattern = @"^([0-9]{1,3}\.){0,3}[0-9]{1,3}$";
            if (Regex.IsMatch(inputText, ipPattern))
            {
                // IP 주소 형식인 경우 (부분 입력 허용)
                serverIP = inputText;
            }
            else
            {
                // IP 주소 형식이 아닌 경우 (서버 이름으로 간주)
                serverName = inputText;
            }
        }

        public static bool CreateFolder_Resource()
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource");
            try
            {
                // 폴더가 없으면 생성
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    LogManager.LogMessage($"{Path.GetFileName(folderPath)} 폴더 생성", Level.Info);
                    return true;
                }
                else
                {
                    // 이미 존재함
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폴더 생성 오류: {ex.Message}");
                LogManager.LogException(ex, Level.Error);
                return false; // 실패
            }
        }

        // ChromeDriver 종료 메소드
        public static void CloseDriver(IWebDriver driver)
        {
            try
            {
                driver.Quit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"드라이버 종료 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogManager.LogException(ex, Level.Error);
            }
            finally
            {
                LogManager.LogMessage("ChromeDriver 종료", Level.Info);
            }
        }


    }
}