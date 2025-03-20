using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;

namespace GateHelper
{
    class ChromeDriverManager
    {
        // ChromeDriver 초기화 메소드
        public static IWebDriver InitializeDriver()
        {
            try
            {
                // 1. Chrome 버전 확인 (메시지 박스로 사용자에게 알림)
                string chromeVersion = GetChromeVersion();

                if (string.IsNullOrEmpty(chromeVersion))
                {
                    MessageBox.Show("Chrome 브라우저를 찾을 수 없습니다!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                // ✅ 크롬 버전 메시지 박스 출력
                MessageBox.Show($"현재 사용 중인 Chrome 버전: {chromeVersion}", "Chrome 버전 확인", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 2. ChromeDriver 다운로드
                string driverDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChromeDriver");
                string driverPath = Path.Combine(driverDirectory, "chromedriver.exe");

                // ✅ 드라이버가 없으면 다운로드 시도
                if (!File.Exists(driverPath))
                {
                    DownloadChromeDriver(chromeVersion, driverDirectory);
                }

                // 3. Chrome 옵션 설정
                var options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-notifications");

                // 4. ChromeDriver 초기화
                var service = ChromeDriverService.CreateDefaultService(driverDirectory);
                return new ChromeDriver(service, options);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"드라이버 초기화 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // Chrome 버전 확인 메소드
        private static string GetChromeVersion()
        {
            try
            {
                string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                if (!File.Exists(chromePath))
                {
                    chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                    if (!File.Exists(chromePath))
                    {
                        return null;
                    }
                }

                var fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);
                return fileVersionInfo.FileVersion;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ChromeDriver 다운로드 메소드
        private static void DownloadChromeDriver(string chromeVersion, string driverDirectory)
        {
            try
            {
                // 1️⃣ 메이저 버전 추출
                string majorVersion = chromeVersion.Split('.')[0];

                // 2️⃣ ChromeDriver 버전 매칭 URL (구글 API 활용)
                string versionUrl = $"https://chromedriver.storage.googleapis.com/LATEST_RELEASE_{majorVersion}";

                string driverVersion;
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        // ChromeDriver의 최신 버전 가져오기
                        driverVersion = client.DownloadString(versionUrl).Trim();
                    }
                    catch (WebException webEx)
                    {
                        MessageBox.Show($"ChromeDriver 버전을 확인할 수 없습니다.\n{webEx.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw;
                    }
                }

                // 3️⃣ ChromeDriver 다운로드 URL 생성
                string driverDownloadUrl = $"https://chromedriver.storage.googleapis.com/{driverVersion}/chromedriver_win32.zip";

                // 4️⃣ 드라이버 디렉토리 생성
                if (!Directory.Exists(driverDirectory))
                {
                    Directory.CreateDirectory(driverDirectory);
                }

                // 5️⃣ 드라이버 ZIP 파일 경로
                string zipPath = Path.Combine(driverDirectory, "chromedriver.zip");

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        // ChromeDriver ZIP 파일 다운로드
                        client.DownloadFile(driverDownloadUrl, zipPath);
                    }
                    catch (WebException webEx)
                    {
                        MessageBox.Show($"ChromeDriver 다운로드 실패!\nURL: {driverDownloadUrl}\n오류: {webEx.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw;
                    }
                }

                // 6️⃣ ZIP 파일 해제
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, driverDirectory);

                // 7️⃣ ZIP 파일 삭제
                File.Delete(zipPath);

                MessageBox.Show("ChromeDriver가 성공적으로 다운로드 및 설치되었습니다!", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ChromeDriver 다운로드 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }



        public static ChromeOptions ChromeDriverOptionSet(string chromePath)
        {
            // Chrome 옵션 설정
            var options = new ChromeOptions();
            options.BinaryLocation = chromePath;
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            return options;
        }



    }
}
