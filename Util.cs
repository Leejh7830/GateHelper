using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.IO.Compression;
using System.Diagnostics;

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

        // 열려 있는 탭에서 URL 확인하는 메소드
        public static void CheckOpenUrls(IWebDriver driver)
        {
            bool isTargetUrl = false;

            // 확인할 URL 목록을 메서드 내부에서 정의
            string[] targetUrls =
            {
                "https://naver.com",
                "https://google.com",
                "https://example.com"
            };

            try
            {
                foreach (var handle in driver.WindowHandles)
                {
                    // 각 창/탭으로 전환
                    driver.SwitchTo().Window(handle);

                    // 현재 URL 확인
                    string currentUrl = driver.Url;

                    // 대상 URL 중 하나와 일치하는지 확인
                    foreach (string url in targetUrls)
                    {
                        if (currentUrl.StartsWith(url, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show($"열려 있는 탭에 {url}가 있습니다!", "URL 확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            isTargetUrl = true;
                            break;
                        }
                    }
                }

                // 대상 URL이 하나도 열려 있지 않은 경우
                if (!isTargetUrl)
                {
                    MessageBox.Show("열려 있는 탭에 대상 URL이 없습니다.", "URL 확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                // URL 확인 중 오류 발생 시
                MessageBox.Show($"URL 확인 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
