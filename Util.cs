using OpenQA.Selenium;
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
using System.Diagnostics;
using System.Threading.Tasks;

namespace GateBot
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
                // ChromeDriver 경로 설정
                string driverDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChromeDriver");
                string driverPath = Path.Combine(driverDirectory, "chromedriver.exe");

                if (!File.Exists(driverPath))
                {
                    throw new Exception($"ChromeDriver가 존재하지 않습니다: {driverPath}");
                }

                // Chrome 실행 경로 확인 (기본 경로 -> 사용자 지정 경로 순으로 확인)
                string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"; // 기본 경로 (64비트)
                if (!File.Exists(chromePath))
                {
                    chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"; // 32비트 경로
                }
                if (!File.Exists(chromePath))
                {
                    chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                              @"Google\Chrome\Application\chrome.exe"); // 사용자 폴더 경로
                }

                // 그래도 없으면 사용자가 입력한 경로 사용
                if (!File.Exists(chromePath))
                {
                    chromePath = config.ChromePath;
                }

                // 최종적으로 Chrome 실행 파일을 찾지 못했다면 예외 발생
                if (!File.Exists(chromePath))
                {
                    throw new Exception($"Chrome 실행 파일을 찾을 수 없습니다.\n경로: {chromePath}\n설정 파일에서 지정한 경로를 확인해 주세요.");
                }

                // Chrome 옵션 설정
                var options = new ChromeOptions();
                options.BinaryLocation = chromePath;
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-notifications");

                // ChromeDriver 실행
                var service = ChromeDriverService.CreateDefaultService(driverDirectory);
                return new ChromeDriver(service, options);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"드라이버 초기화 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }



        // 설정 파일을 불러오고 없으면 생성하는 메소드
        public static Config LoadConfig()
        {
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new Config
                {
                    Url = "",
                    GateID = "",
                    GatePW = "",
                    ChromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"
                };

                try
                {
                    File.WriteAllText(configFilePath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
                    MessageBox.Show($"설정 파일이 생성되었습니다. 정보를 입력하고 프로그램을 재실행 해주세요.\n파일 경로: {configFilePath}",
                                    "설정 파일 생성", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"설정 파일 생성 중 오류 발생: {ex.Message}\n파일 경로: {configFilePath}",
                                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            try
            {
                var json = File.ReadAllText(configFilePath);
                var config = JsonConvert.DeserializeObject<Config>(json);

                if (config == null)
                {
                    MessageBox.Show("설정 파일의 형식이 잘못되었습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // 필수 항목 검증
                var missingFields = new System.Text.StringBuilder();

                if (!json.Contains("\"URL\"")) missingFields.AppendLine("URL");
                if (!json.Contains("\"GateID\"")) missingFields.AppendLine("GateID");
                if (!json.Contains("\"GatePW\"")) missingFields.AppendLine("GatePW");
                if (!json.Contains("\"ChromePath\"")) missingFields.AppendLine("ChromePath");

                if (missingFields.Length > 0)
                {
                    MessageBox.Show($"설정 파일에 다음 항목이 누락되었습니다:\n{missingFields}",
                                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                return config;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 파일 로딩 오류: {ex.Message}\n파일 경로: {configFilePath}",
                                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }


        // 요소에 값을 입력하는 메서드
        public static void SendKeysToElement(IWebDriver driver, string xpath, string value)
        {
            try
            {
                // WebDriverWait을 사용하여 요소가 나타날 때까지 대기
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

                // 요소가 준비될 때까지 기다림
                var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

                if (element != null)
                {
                    element.Clear(); // 빈 값을 입력하여 초기화
                    element.SendKeys(value); // 값을 입력
                }
                else
                {
                    throw new NoSuchElementException($"{xpath} 요소를 찾을 수 없습니다.");
                }
            }
            catch (NoSuchElementException ex)
            {
                throw new Exception($"요소를 찾을 수 없습니다: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"오류 발생: {ex.Message}");
            }
        }




        public static void ClickElementByXPath(IWebDriver driver, string xpath)
        {
            try
            {
                // WebDriverWait을 사용하여 요소가 클릭 가능한 상태로 나타날 때까지 기다리기
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                IWebElement element = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(xpath)));

                // 클릭 가능하면 클릭
                element.Click();
            }
            catch (NoSuchElementException)
            {
                MessageBox.Show("XPath에 해당하는 요소를 찾을 수 없습니다.", "실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (WebDriverTimeoutException)
            {
                MessageBox.Show("요소가 클릭 가능한 상태가 되지 않았습니다.", "실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    IntPtr chromeHandle = new IntPtr(long.Parse(chromeHandleString)); // string을 IntPtr로 변환

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
            catch (Exception ex)
            {
                MessageBox.Show($"크롬 창 포커스 이동 중 오류 발생: {ex.Message}");
            }
        }


        // iframe순회, 클릭가능한 Element 조사
        public static void InvestigateIframesAndCollectClickableElements(IWebDriver driver)
        {
            try
            {
                StringBuilder elementInfo = new StringBuilder();

                // 기본 문서의 클릭 가능한 요소 조사
                CollectClickableElements(driver, elementInfo, "기본 문서");

                // 모든 iframe 찾기
                IReadOnlyCollection<IWebElement> iframes = driver.FindElements(By.TagName("iframe"));

                if (iframes.Count > 0)
                {
                    foreach (IWebElement iframe in iframes)
                    {
                        // iframe으로 전환
                        driver.SwitchTo().Frame(iframe);

                        // iframe 내부의 클릭 가능한 요소 조사
                        CollectClickableElements(driver, elementInfo, $"iframe (이름: {iframe.GetAttribute("name")}, ID: {iframe.GetAttribute("id")})");

                        // 기본 문서로 전환
                        driver.SwitchTo().DefaultContent();
                    }
                }

                // 메시지 박스에 요소 정보 표시
                MessageBox.Show(elementInfo.ToString(), "클릭 가능한 요소 정보");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"iframe 조사 및 요소 수집 중 오류 발생: {ex.Message}", "오류");
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
                MessageBox.Show("검색어를 입력하세요.", "알림");
                return;
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


        // ChromeDriver 종료 메소드
        public static void CloseDriver(IWebDriver driver)
        {
            try
            {
                driver.Quit();
                // 종료 메시지 박스
                MessageBox.Show("ChromeDriver가 종료되었습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // 종료 오류 발생 시 메시지 박스 출력
                MessageBox.Show($"드라이버 종료 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}