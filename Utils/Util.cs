using OpenQA.Selenium;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static GateHelper.LogManager;
using static GateHelper.Util_Element;

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

        // 중앙화된 _meta 경로 접근
        public static string MetaFolder => CreateMetaFolderAndGetPath();

        public static string GetMetaPath(params string[] parts)
        {
            string basePath = MetaFolder;
            if (parts == null || parts.Length == 0) return basePath;
            return Path.Combine(new[] { basePath }.Concat(parts).ToArray());
        }

        private static string EnsureReleaseNotesInMeta()
        {
            string metaNotesPath = GetMetaPath("ReleaseNotes.txt");
            string rootNotesPath = Path.Combine(Application.StartupPath, "ReleaseNotes.txt");

            try
            {
                if (File.Exists(metaNotesPath))
                    return metaNotesPath;

                if (File.Exists(rootNotesPath))
                {
                    try
                    {
                        File.Move(rootNotesPath, metaNotesPath);
                        LogMessage("Moved ReleaseNotes.txt to _meta folder", Level.Info);
                        return metaNotesPath;
                    }
                    catch (Exception exMove)
                    {
                        LogException(exMove, Level.Error);
                        try
                        {
                            File.Copy(rootNotesPath, metaNotesPath);
                            try { File.Delete(rootNotesPath); } catch { }
                            return metaNotesPath;
                        }
                        catch (Exception exCopy)
                        {
                            LogException(exCopy, Level.Error);
                        }
                    }
                }

                // 제공된 릴리즈 노트 내용을 여기서 직접 작성
                string content =
@"v2.0.2
- Initial Release


[기능추가 및 개선 예정]

오류(알람) UI 창
옵션 상태 저장하기 (프로그램을 껐다가 다시 켜도 기존 옵션 상태 유지)
ListView 고정 옵션 추가 (사용자가 원하는 리스트를 고정하여 항상 상단에 표시)
Tick Time 로그에 남기기 (옵션으로 On/Off)
Listview 컨텍스트 메뉴 색상 반전
통합모니터링 웹 접속 및 제어
FAV One Click Connect 오류 개선 (검색은 되지만 접속 시 찾을 수 없음)


[완료]

v1.0.0
25.03.04 서버자동접속(ID/PW 입력) 기능
25.03.06 즐겨찾기 버튼 및 클릭 기능
25.03.09 서버리스트 기능
25.03.14 DriverManager.cs 추가
25.03.17 Log(읽기/쓰기) 기능
25.03.20 ListView(서버목록) 기능
25.03.24 FlowLayoutList(이미지 보기/저장) 기능
25.03.27 Driver/Network 상태 표시, Search Server 기능
25.03.28 ListView 클릭 접속 기능(Option)

v2.0.0
25.08.18 OP) Popup/Modal 해제 기능 (30분마다 Gateone 비밀번호 알림창)
25.08.19 OP) ListView 중복 제거 기능
25.08.20 OP) 옵션전용폼 추가 및 옵션변수 이동
25.09.04 ListView 메모 기능
25.09.25 OP) GraceTime 기능 (해당시간마다 팝업창 체크, 기본 5초 인터넷환경 고려)
25.10.20 OP) 옵션설명 라벨 추가 / _meta 폴더 이동 기능 강화 / OP) FAV OneClick 접속 기능 추가
25.10.21 Preset 기능 추가
";

                File.WriteAllText(metaNotesPath, content, System.Text.Encoding.UTF8);
                LogMessage("Created _meta/ReleaseNotes.txt content", Level.Info);
                return metaNotesPath;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                return metaNotesPath;
            }
        }

        public static void OpenReleaseNotes()
        {
            // 먼저 _meta내 파일을 보장하도록 처리
            string metaNotesPath = EnsureReleaseNotesInMeta();

            try
            {
                Process.Start(new ProcessStartInfo(metaNotesPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"An error occurred while opening the release notes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // 현재 미사용
        public static bool CreateFolder_Resource()
        {
            string metaFolder = CreateMetaFolderAndGetPath();
            string folderPath = Path.Combine(metaFolder, "Resource");
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    LogMessage($"{Path.GetFileName(folderPath)} 폴더 생성", Level.Info);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폴더 생성 오류: {ex.Message}");
                LogException(ex, Level.Error);
                return false;
            }
        }

        // 새로운 FAV : 검색 동작 수행하고 서버 이름(또는 빈 문자열)을 반환
        public static string ClickFavBtnAndGetServerName(IWebDriver _driver, Config config, int favIndex, ChromeDriverManager chromeDriverManager)
        {
            if (!chromeDriverManager.IsDriverReady(_driver))
                return string.Empty;

            try
            {
                string favKey = $"Fav{favIndex}";
                LogMessage($"BtnFav{favIndex} Click (search-only)", Level.Info);

                string favValue = config.GetType().GetProperty(favKey).GetValue(config)?.ToString() ?? string.Empty;
                string serverName, serverIP;
                ValidateServerInfo(favValue, out serverName, out serverIP);

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

                // 페이지상에 결과가 로드될 때까지 대기
                WaitForElementLoadByXPath(_driver, "//*[@id='seltable']/tbody[1]/tr/td[4]", 10);

                // 반환: 서버 이름(연결 시 사용)
                return serverName;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                return string.Empty;
            }
        }

        // 이전 FAV
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





    }
}