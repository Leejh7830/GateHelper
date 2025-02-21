using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using SeleniumExtras.WaitHelpers;
using System.Text;

namespace GateBot
{
    public static class Util
    {
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
                    URL = "",
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
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                // 요소가 준비될 때까지 기다림
                var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

                if (element != null)
                {
                    element.SendKeys(value);  // 값을 입력
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


        public static void FindElementAndShowMessage(IWebDriver driver, string query)
        {
            try
            {
                // XPath인지 CSS Selector인지 자동 판별
                By by = query.StartsWith("/") || query.StartsWith(".") ? By.XPath(query) : By.CssSelector(query);

                // 현재 페이지에서 검색
                if (FindElementInCurrentContext(driver, by))
                {
                    MessageBox.Show($"요소를 찾았습니다: {query}", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 모든 iframe을 순회하면서 검색
                var allFrames = driver.FindElements(By.TagName("iframe"));
                foreach (var frame in allFrames)
                {
                    try
                    {
                        driver.SwitchTo().Frame(frame);
                        if (FindElementInCurrentContext(driver, by))
                        {
                            MessageBox.Show($"요소를 찾았습니다 (iframe 내부): {query}", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"iframe 전환 실패: {ex.Message}");
                    }
                    finally
                    {
                        driver.SwitchTo().DefaultContent();
                    }
                }

                // 요소를 찾지 못한 경우
                MessageBox.Show($"요소를 찾을 수 없습니다: {query}", "실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 현재 컨텍스트에서 요소를 찾는 함수
        private static bool FindElementInCurrentContext(IWebDriver driver, By by)
        {
            try
            {
                return driver.FindElements(by).Count > 0;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }


        public static void ShowAllElementXpaths(IWebDriver driver)
        {
            try
            {
                // 페이지 내 모든 요소 찾기
                var allElements = driver.FindElements(By.XPath("//*"));

                if (allElements.Count == 0)
                {
                    MessageBox.Show("페이지에 요소가 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // XPath 생성 및 출력 준비
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("페이지의 모든 XPath 목록:");

                foreach (var element in allElements)
                {
                    // 각 요소의 XPath를 생성
                    string xpath = GetElementXPath(driver, element);
                    sb.AppendLine(xpath);
                }

                // 결과 출력
                MessageBox.Show(sb.ToString(), "모든 XPath", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 요소의 XPath를 생성하는 함수
        private static string GetElementXPath(IWebDriver driver, IWebElement element)
        {
            string script = "function getElementXPath(element) {" +
                            "var paths = [];" +
                            "while (element.nodeType === Node.ELEMENT_NODE) {" +
                            "var index = 0;" +
                            "var sibling = element.previousSibling;" +
                            "while (sibling) {" +
                            "if (sibling.nodeType === Node.DOCUMENT_TYPE_NODE) {" +
                            "} else if (sibling.nodeName === element.nodeName) {" +
                            "index++;" +
                            "}" +
                            "sibling = sibling.previousSibling;" +
                            "}" +
                            "var tagName = element.nodeName.toLowerCase();" +
                            "var pathIndex = (index ? '[' + (index + 1) + ']' : '');" +
                            "paths.unshift(tagName + pathIndex);" +
                            "element = element.parentNode;" +
                            "}" +
                            "return paths.length ? '/' + paths.join('/') : null;" +
                            "}" +
                            "return getElementXPath(arguments[0]);";

            var xpath = (string)((IJavaScriptExecutor)driver).ExecuteScript(script, element);
            return xpath;
        }

        public static void ClickElementByXPath(IWebDriver driver, string xpath)
        {
            try
            {
                // WebDriverWait을 사용하여 요소가 클릭 가능한 상태로 나타날 때까지 기다리기
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
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