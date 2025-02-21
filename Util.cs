using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using SeleniumExtras.WaitHelpers;

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


        public static void FindElementWithWait(IWebDriver driver, string xpath)
        {
            try
            {
                // WebDriverWait을 사용하여 요소가 나타날 때까지 대기
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

                // 요소가 나타났다면 그 요소에 대해 처리
                if (element != null)
                {
                    Console.WriteLine("요소를 찾았습니다!");
                    element.SendKeys("test");
                }
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"요소를 찾을 수 없습니다: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"기타 오류: {ex.Message}");
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