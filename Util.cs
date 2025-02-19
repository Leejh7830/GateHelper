using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.IO.Compression;
using System.Diagnostics;
using Newtonsoft.Json;

namespace GateBot
{
    public static class Util
    {
        // ChromeDriver 초기화 메소드
        public static IWebDriver InitializeDriver()
        {
            try
            {
                // ChromeDriver 경로 지정 (실행 파일과 같은 위치의 ChromeDriver 폴더)
                string driverDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChromeDriver");
                string driverPath = Path.Combine(driverDirectory, "chromedriver.exe");

                // 드라이버 존재 여부 확인
                if (!File.Exists(driverPath))
                {
                    throw new Exception($"ChromeDriver가 경로에 존재하지 않습니다: {driverPath}");
                }

                // Chrome 옵션 설정
                var options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-notifications");
                options.AddArgument("--no-sandbox");             // 샌드박스 비활성화 (리눅스 및 일부 환경에서 충돌 방지)
                options.AddArgument("--disable-dev-shm-usage");  // 메모리 부족 이슈 해결 (특히 Docker나 VM 환경에서 유용)
                options.AddArgument("--remote-debugging-port=9222"); // DevToolsActivePort 대체 포트
                options.AddArgument("--disable-extensions");     // 확장 프로그램 비활성화
                options.AddArgument("--disable-gpu");            // GPU 가속 비활성화 (일부 환경에서 충돌 방지)

                // ChromeDriver 초기화
                var service = ChromeDriverService.CreateDefaultService(driverDirectory);
                MessageBox.Show("정상실행");
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

            // 설정 파일이 존재하지 않으면 기본값을 사용하여 생성
            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new Config
                {
                    URL = "https://www.exampleAAA.com",
                    UserID = "your_id",
                    Password = "your_password"
                };

                try
                {
                    // 기본값을 JSON 파일로 저장
                    File.WriteAllText(configFilePath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));

                    // 설정 파일 생성 후 안내 메시지 출력
                    MessageBox.Show($"설정 파일이 생성되었습니다. 정보 입력 후 재실행 해주세요.\n파일 경로: {configFilePath}",
                                    "설정 파일 생성", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();  // 프로그램 종료
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"설정 파일 생성 중 오류 발생: {ex.Message}\n파일 경로: {configFilePath}",
                                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }

            // 설정 파일이 존재하면 읽어오기
            try
            {
                var json = File.ReadAllText(configFilePath);

                // 사용자 입력 데이터를 Config 객체로 역직렬화
                var config = JsonConvert.DeserializeObject<Config>(json);

                // 데이터가 없을 경우 기본값으로 설정
                if (config == null)
                {
                    throw new Exception("설정 파일의 데이터가 올바르지 않습니다.");
                }

                // 정상적으로 로드된 경우
                MessageBox.Show($"설정 파일이 정상적으로 불러와졌습니다.\n파일 경로: {configFilePath}",
                                "설정 파일 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return config;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 파일 로딩 오류: {ex.Message}\n파일 경로: {configFilePath}",
                                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
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
