using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using static GateHelper.LogManager;
using static GateHelper.Util_Element;
using System.Diagnostics;

namespace GateHelper
{
    public static class Util // 공통 유틸리티
    {

        

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

        public static void OpenReleaseNotes()
        {
            string metaFolderPath = CreateMetaFolderAndGetPath();
            string releaseNotesPath = Path.Combine(Application.StartupPath, "ReleaseNotes.txt");

            try
            {
                Process.Start(new ProcessStartInfo(releaseNotesPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening the release notes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void ClickFavBtn(IWebDriver _driver, Config config, int favIndex, Action serverListLoadAction, ChromeDriverManager chromeDriverManager)
        {
            if (!chromeDriverManager.IsDriverReady(_driver))
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
                    SendKeysToElement(_driver, "//*[@id='id_IPADDR']", serverIP);
                    SendKeysToElement(_driver, "//*[@id='id_DEVNAME']", "");
                }
                else if (!string.IsNullOrEmpty(serverName))
                {
                    // 서버 이름인 경우
                    SendKeysToElement(_driver, "//*[@id='id_DEVNAME']", serverName);
                    SendKeysToElement(_driver, "//*[@id='id_IPADDR']", "");
                }

                ClickElementByXPath(_driver, "//*[@id='access_control']/table/tbody/tr[2]/td/a");

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


        public static void InputKeys(string keys, int intervalMilliseconds = 1000)
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
                    return true; // 이미 존재함
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폴더 생성 오류: {ex.Message}");
                LogException(ex, Level.Error);
                return false; // 실패
            }
        }

        



    }
}