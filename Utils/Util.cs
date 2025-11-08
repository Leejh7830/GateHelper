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

        public static string EnsureReleaseNotesInMeta()
        {
            string metaNotesPath = GetMetaPath("ReleaseNotes.txt");
            string content =
@"
v2.1.1 / 25.11.05
- Release
- leejh7830@lgespartner.com
- 본 프로그램은 비영리 목적으로 제작한 유틸리티입니다.


[ 신규 사항 ] 진행중 : 7
1. 신규 / 오류(알람) UI 창
2. 신규 / 옵션 상태 저장하기 (프로그램을 껐다가 다시 켜도 기존 옵션 상태 유지)
3. 신규 / ListView 고정 옵션 추가 (사용자가 원하는 리스트를 고정하여 항상 상단에 표시)
4. 신규 / 통합모니터링 웹 접속 및 제어
5. 신규 / Error,Critical 로그 카운트
6. 신규 / Excel, PPT 파일 바로가기 기능
7. 신규 / 현재접속서버 사용자 공유 시스템 (RDP감지/GateOne Log API 활용/UDP BroadCast) / 안되면 접속했던 기록이라도 공유
 - OBJ No열 수정 / 송신시 로그 / 누군가 접속하면 현재 열려있는 프로그램에서 현황 공유 / 모든 수신메시지를 볼 수 있는 로그 관리
8. 신규 / 전체 서버를 리스트에 저장하는 기능
9. 신규 / 마우스 감지 및 일정 시간(사용자 비활성) 후 자동 마우스 움직임 기능 추가


[ 개선 사항 ] 진행중 : 
1. 개선 / Listview 컨텍스트 메뉴 색상 반전 (현재 색상모드와 불일치)
2. 개선 / FAV One Click Connect 오류 (검색은 되지만 접속 시 찾을 수 없음)
3. 개선 / 드라이버종료되면 팝업옵션 끄기
4. 개선 / 날짜별로그확인 기능
5. 개선 / 접속할때 웹페이지가 접속목록이 아닌 다른 페이지에 있으면 접속목록 페이지로 이동


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


v2.1.0 / 25.10.29 DriverManager 개선 및 ChromeDriver 자동업데이트 기능 추가 (ChromeDriverManager.cs)
v2.1.1 / 25.11.05 GetServerListFromWebPage 추가 (웹페이지에 접속서버가 있으면 재검색 안하고 바로 접속)
         
";

            try
            {
                // 항상 덮어쓰기
                File.WriteAllText(metaNotesPath, content, System.Text.Encoding.UTF8);
                LogMessage("Updated _meta/ReleaseNotes.txt content", Level.Info);
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

        // 릴리즈노트 버전 당겨오기
        public static string GetCurrentVersionFromReleaseNotes()
        {
            string metaNotesPath = Util.GetMetaPath("ReleaseNotes.txt");
            if (!System.IO.File.Exists(metaNotesPath))
                return "Unknown";

            try
            {
                using (var reader = new System.IO.StreamReader(metaNotesPath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            // 첫 번째 비어있지 않은 줄을 버전으로 간주
                            return line.Trim();
                        }
                    }
                }
            }
            catch
            {
                // 예외 발생 시 Unknown 반환
            }
            return "Unknown";
        }




    }
}