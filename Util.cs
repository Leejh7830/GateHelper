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
using static GateHelper.LogManager;
using System.Diagnostics;

namespace GateHelper
{
    public static class Util // 공통 유틸리티
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
                LogException(ex, Level.Error);
                throw;
            }
        }

        public static string CreateMetaFolderAndGetPath()
        {
            string metaPath = Path.Combine(Application.StartupPath, "_meta");

            if (!Directory.Exists(metaPath))
            {
                Directory.CreateDirectory(metaPath);
                LogMessage("Create Meta Folder", Level.Info);
            }

            return metaPath;
        }

        public static void ClickFavBtn(IWebDriver _driver, Config config, int favIndex, Action serverListLoadAction)
        {
            if (!CheckDriverExists(_driver))
                return;

            try
            {
                string favKey = $"Fav{favIndex}";
                LogMessage($"BtnFav{favIndex} Click", Level.Info);
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
                LogException(ex, Level.Error);
                MessageBox.Show(ex.Message, "알림");
            }
            catch (NoSuchElementException ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show("요소를 찾을 수 없습니다.", "오류");
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Critical);
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
                LogException(ex, Level.Error);
                MessageBox.Show($"XPath 요소 로딩 시간 초과 (제한 시간: {timeoutSeconds}초)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // 예외 다시 던지기
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
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
                LogException(ex, Level.Error);
            }
        }




        public static bool CheckDriverExists(IWebDriver driver)
        {
            if (driver == null)
            {
                MessageBox.Show("ChromeDriver가 실행 중이 아닙니다.\n먼저 [Start] 버튼을 눌러주세요.", "드라이버 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        

        public static void SwitchToMainHandle(IWebDriver _driver, string mainHandle)
        {
            _driver.SwitchTo().Window(mainHandle);
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
                    LogMessage($"{Path.GetFileName(folderPath)} 폴더 생성", Level.Info);
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
                LogException(ex, Level.Error);
                return false; // 실패
            }
        }

        // ChromeDriver 종료
        public static void CloseDriver(IWebDriver driver)
        {
            // 드라이버 종료 시도
            if (driver != null)
            {
                try
                {
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"드라이버 종료 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogException(ex, Level.Error);
                }
                finally
                {
                    LogMessage("ChromeDriver 종료", Level.Info);
                }
            }

            try // 25.03.27 Added - 백그라운드 chromedriver 프로세스 강제 종료
            {
                foreach (var process in Process.GetProcessesByName("chromedriver"))
                {
                    process.Kill();
                }
                LogMessage("남아있는 ChromeDriver 프로세스 강제 종료", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
        }



    }
}