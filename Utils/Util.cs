using ClosedXML.Excel;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static GateHelper.LogManager;
using static GateHelper.Util_Element;

namespace GateHelper
{
    public static class Util // 공통 유틸리티
    {
        #region [Chrome Protocol Popup Control (Keyboard Injection)]

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private static class NativeMethods
        {
            public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        }

        /// <summary>
        /// 실행 중인 Chrome 창 중 첫 번째 창을 찾아 포그라운드로 활성화합니다.
        /// </summary>
        /// <returns>Chrome 창을 찾아 활성화했으면 true, 아니면 false</returns>
        public static bool SetChromeForeground()
        {
            IntPtr chromeHwnd = IntPtr.Zero;
            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                var sb = new StringBuilder(512);
                GetWindowText(hWnd, sb, 512);
                string title = sb.ToString();
                if (title.IndexOf("Chrome", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    chromeHwnd = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            if (chromeHwnd != IntPtr.Zero)
            {
                SetForegroundWindow(chromeHwnd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Chrome 브라우저 창에 포커스를 맞추고 키보드 입력을 주입하여
        /// "MDOHelper을(를) 여시겠습니까?" 다이얼로그를 자동 처리합니다.
        /// 페이지 로드 완료를 감지한 뒤 bufferMs 만큼만 기다리고 즉시 처리합니다.
        /// </summary>
        /// <param name="driver">Selenium IWebDriver (페이지 로드 감지용)</param>
        /// <param name="bufferMs">페이지 로드 완료 후 팝업 뜰 때까지 추가 대기 (기본 800ms)</param>
        /// <param name="pageLoadTimeoutMs">페이지 로드 최대 대기시간 (기본 15초)</param>
        public static bool HandleMDOHelperDialog(IWebDriver driver, int bufferMs = 800, int pageLoadTimeoutMs = 15000)
        {
            try
            {
                // 1. 페이지 로드 완료까지 대기 (document.readyState == 'complete')
                LogMessage("[MDOHelper] 페이지 로드 완료 감지 대기 중...", Level.Info);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                bool loaded = false;
                while (sw.ElapsedMilliseconds < pageLoadTimeoutMs)
                {
                    try
                    {
                        var js = driver as OpenQA.Selenium.IJavaScriptExecutor;
                        string state = js?.ExecuteScript("return document.readyState")?.ToString();
                        if (state == "complete")
                        {
                            loaded = true;
                            break;
                        }
                    }
                    catch { /* 드라이버 전환 중 예외 무시 */ }
                    Thread.Sleep(200);
                }

                if (!loaded)
                    LogMessage($"[MDOHelper] 페이지 로드 타임아웃 ({pageLoadTimeoutMs}ms), 그래도 진행합니다.", Level.Warning);
                else
                    LogMessage($"[MDOHelper] 페이지 로드 완료 감지 ({sw.ElapsedMilliseconds}ms 소요)", Level.Info);

                // 2. 팝업이 뜰 여유 시간
                Thread.Sleep(bufferMs);

                // 3. Chrome 창 활성화
                if (!SetChromeForeground())
                {
                    LogMessage("[MDOHelper] Chrome 창을 찾지 못했습니다.", Level.Warning);
                    return false;
                }

                LogMessage($"[MDOHelper] Chrome 창 발견, 키보드 주입 시작", Level.Info);
                Thread.Sleep(400);

                SendKeys.SendWait("{TAB}");  // 체크박스 포커스
                Thread.Sleep(300);
                SendKeys.SendWait(" ");      // 체크박스 체크
                Thread.Sleep(300);
                SendKeys.SendWait("{TAB}");  // 열기 버튼 포커스
                Thread.Sleep(300);
                SendKeys.SendWait(" ");      // 열기 버튼 클릭

                LogMessage("[MDOHelper] 키보드 처리 완료", Level.Info);
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "[MDOHelper] 팝업 처리 중 오류 발생");
                return false;
            }
        }

        #endregion Chrome 프로토콜 팝업 제어 (키보드 주입 방식)


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
v2.4.5 / 26.07.29 / Test Version
- leejh7830@lgespartner.com
- 비영리 목적으로 제작한 유틸리티입니다.


[ 신규 사항 ] 진행중 : 
1. 신규 / 오류(알람) UI 창
5. 신규 / Error,Critical 로그 카운트
6. 신규 / 전체 서버를 리스트에 저장하는 기능
7. 신규 / 마우스 감지 및 일정 시간(사용자 비활성) 후 자동 마우스 움직임 기능 추가
8. 신규 / 레이아웃 폼 생성, 설비 레이아웃 클릭 시 바로 해당 AP로 이동
9. MGMT로 데이터 수집 후 통계 및 비교 기능 추가

[ 개선 사항 ] 진행중 : 
1. 개선 / FAV One Click Connect 오류 (검색은 되지만 접속 시 찾을 수 없음)
2. 개선 / 드라이버종료되면 팝업옵션 끄기
3. 개선 / 날짜별로그확인 기능
4. 개선 / 접속할때 웹페이지가 접속목록이 아닌 다른 페이지에 있으면 접속목록 페이지로 이동
5. 개선 / [UDP] 접속 Port 변경 옵션 추가
6. 개선 / [UDP] RDP 감지 추가
7. 개선 / [UDP] 현재 접속자 확인 기능 추가
8. 개선 / [UDP] 접속자 목록 UI 추가
9. 개선 / ExcelAnalysis 기능 개선 (엑셀파일이 열려있으면 어떻게 처리할지)
10. 개선 / ExcelAnalysis 기능 개선 (대표기준이 되는 호기를 사용자가 선정)
11. 개선 / ExcelAnalysis 기능 개선 (Dgv 가로스크롤 추가, HeaderColumn 길이 조정)
12. 개선 / ExcelAnalysis 기능 개선 (변수명이 중복됨. 한번만 올라가도록, SEM에서 등록 한 변수가 Port에서 보임)
13. 개선 / ExcelAnalysis 기능 개선 (엑셀드랍후 바로 분석 할 때, Rule은 어떤걸로 하게 되는지?)


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
25.08.18 OP) Popup/Modal 해제 기능 (30분마다 Gateone 비밀번호 알림창 해제)
25.08.19 OP) ListView 중복 제거 기능
25.08.20 OP) 옵션전용폼 추가 및 옵션변수 이동
25.09.04 ListView 메모 기능
25.09.25 OP) GraceTime 기능 (해당시간마다 팝업창 체크, 기본 5초 인터넷환경 고려)
25.10.20 OP) 옵션설명 라벨 추가 / _meta 폴더 이동 기능 강화 / OP) FAV OneClick 접속 기능 추가
25.10.21 Preset 기능 추가

v2.1.0 / 25.10.29 DriverManager 개선 및 ChromeDriver 자동업데이트 기능 추가 (ChromeDriverManager.cs)
v2.1.1 / 25.11.05 GetServerListFromWebPage 추가 (웹페이지에 접속서버가 있으면 재검색 안하고 바로 접속)
v2.1.2 / 25.11.10 [UDP] BroadCast 수신 기능 추가 (Util_RDP.cs) - 접속서버 공유 시스템
v2.1.3 / 25.11.15 [UDP] 접속/종료 시 송신
v2.1.4 / 25.12.16 개선 - 현재 화면의 서버목록에 접속하려는 서버가 없으면 스크롤하여 검색
v2.1.5 / 25.12.17 개선 - Listview 컨텍스트 메뉴 색상 반전 (현재 색상모드와 불일치)

v2.2.0 / 25.12.22 신규 - WorkLog 추가 (작업 메모용, Reference 탭)
v2.2.1 / 26.01.07 개선 - WorkLog 기능추가 (이미지붙여넣기)
v2.2.2 / 26.01.13 신규 - SandBox 추가
v2.2.3 / 26.01.17 개선 - SandBox - BitFlip
v2.2.4 / 26.01.29 개선 - SandBox - SignalLink
v2.2.5 / 26.03.30 개선 - 프로그램 종료(X) 클릭 시 확인 창 띄우기 / 리스트뷰에서 바로 접속 시 검색 텍스트 박스 비우기
v2.2.6 / 26.04.08 개선 - Disable Pop up -> Auto Screen Unlock 으로 변경, 통합관리시스템(Manufacturing Management) 연결

v2.3.0 / 26.05.12 신규 - MGMT 사이트 오픈 및 이동
v2.3.1 / 26.05.14 개선 - MGMT 자동로그인 기능 구현 / Main과 Management Handle 관리 구분 및 인터락 구현
v2.3.2 / 26.06.10 신규 - (안정화버전) 통합모니터링(MGMT) STO 데이터 수집 / 신규 - ServerMapping 기능 추가 (호기명 입력 시 서버 매핑)
v2.3.3 / 26.06.19 신규 - Drag & Drop 기능 추가 (파일 즐겨찾기 등록)

v2.4.0 / 26.06.24 신규 - LogValidator 기능 추가 (로그 분석 및 통계, 필터링)
v2.4.1 / 26.07.05 개선 - LogValidator 기능 개선 (UI/UX 배치, 스텝 하이라이팅, 스텝 OR/AND/TimeOut 추가)
v2.4.2 / 26.07.20 개선 - DisplayChange Detect 시, 가장자리 잘림 현상 수정
v2.4.3 / 26.07.23 개선 - Auto Login 기능 추가 / BackgroundMonitoring 추가 / toast 알림 추가
v2.4.4 / 26.07.27 신규 - Variable Excel 분석 탭 추가
v2.4.5 / 26.07.29 개선 - UI 화면크기 윈도우 배율에 따라 화면 밖으로 밀려나가는 현상 수정 (화면 원복) / Region 포맷 통일
         
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


        public static bool SwitchToMainHandle(IWebDriver driver, string mainHandle)
        {
            try
            {
                // 1. 핸들 정보 자체가 없거나 드라이버가 닫힌 경우
                if (string.IsNullOrEmpty(mainHandle) || driver == null)
                {
                    throw new Exception("메인 창 핸들 정보가 존재하지 않습니다.");
                }

                // 2. 현재 포커스된 창이 닫혔을 때 WindowHandles 자체가 예외를 던지는 경우 방어
                System.Collections.ObjectModel.ReadOnlyCollection<string> handles;
                try
                {
                    handles = driver.WindowHandles;
                }
                catch (OpenQA.Selenium.NoSuchWindowException)
                {
                    // 닫힌 팝업에 포커스가 남아있는 상태 → mainHandle로 직접 전환
                    LogManager.LogMessage("현재 창이 닫혀있어 mainHandle로 직접 전환 시도", LogManager.Level.Warning);
                    driver.SwitchTo().Window(mainHandle);
                    return true;
                }

                // 3. 현재 열린 창 목록에 메인 핸들이 있는지 확인
                if (!handles.Contains(mainHandle))
                {
                    throw new OpenQA.Selenium.NoSuchWindowException("사용자가 메인 페이지 창을 닫았거나 유실되었습니다.");
                }

                // 4. 정상적으로 탭 전환
                driver.SwitchTo().Window(mainHandle);
                return true;
            }
            catch (Exception ex)
            {
                // [크리티컬 로그 기록]
                LogManager.LogException(ex, LogManager.Level.Critical, "Main Handle 유실 감지");

                // [사용자에게 명확한 가이드 제공]
                MessageBox.Show(
                    "메인 제어 창을 찾을 수 없는 크리티컬한 문제가 발생했습니다.\n\n" +
                    "1. 사용자가 메인 브라우저 탭을 직접 닫았을 수 있습니다.\n" +
                    "2. 브라우저 연결이 예기치 않게 끊어졌을 수 있습니다.\n\n" +
                    "[Start Operation] 버튼을 눌러 처음부터 다시 시작해주세요.",
                    "시스템 중단",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );

                return false; // 실패 반환
            }
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




        ////////////////////////////////////////////////////////// Server Keyword Mapping START /////////////////////////////////////////////////////////////////////////////

        private static Dictionary<string, string> _serverMappingCache =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 매핑 마스터 엑셀 파일의 존재 여부를 확인하고 없으면 자동 생성합니다.
        /// </summary>
        public static string EnsureMappingFileExists()
        {
            // 이미 존재하는 Util의 폴더 생성/경로 반환 메서드 재사용
            string metaPath = CreateMetaFolderAndGetPath();
            string filePath = Path.Combine(metaPath, "ServerMappingMaster.xlsx");

            try
            {
                // 엑셀 파일이 없으면 새 템플릿 생성
                if (!File.Exists(filePath))
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("MappingMaster");

                        // 표준 가이드 컬럼 작성
                        worksheet.Cell(1, 1).Value = "TargetServer";
                        worksheet.Cell(1, 2).Value = "Keywords";

                        // 샘플 데이터 주입
                        worksheet.Cell(2, 1).Value = "Server_C";
                        worksheet.Cell(2, 2).Value = "사과, apple, FSTO_01";

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(filePath);
                    }
                    LogMessage("[생성] 매핑 마스터 기본 템플릿 파일이 _meta에 생성되었습니다.", Level.Info);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "매핑 마스터 파일 무결성 체크 중 예외 발생");
            }

            return filePath;
        }

        /// <summary>
        /// 매핑 엑셀 파일을 읽어와 N:1 구조를 메모리(Dictionary)에 실시간 캐싱합니다.
        /// 파일이 엑셀에 의해 열려 있어도 FileShare.ReadWrite 권한으로 강제 복사하여 읽어옵니다.
        /// </summary>
        public static void LoadServerMappingCache()
        {
            string filePath = EnsureMappingFileExists();

            if (!File.Exists(filePath))
            {
                LogMessage("[캐시 로드 실패] 매핑 파일이 존재하지 않습니다.", Level.Error);
                return;
            }

            // 중복 발생 내역을 메모리에 임시 수집할 리스트 생성
            List<string> conflictLogs = new List<string>();

            try
            {
                // 딕셔너리 초기화 (새로고침 대응)
                _serverMappingCache.Clear();

                // 💡 [핵심 방어] 사용자가 엑셀을 열어두었어도 ReadWrite 공유 모드로 스트림을 열어 락 붕괴 방지
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1); // 첫 번째 시트 접근
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // 헤더행(1행) 제외

                        foreach (var row in rows)
                        {
                            string targetServer = row.Cell(1).GetValue<string>().Trim();
                            string keywordsToken = row.Cell(2).GetValue<string>().Trim();

                            if (string.IsNullOrEmpty(targetServer) || string.IsNullOrEmpty(keywordsToken))
                                continue;

                            // 콤마(,)를 기준으로 키워드 분리 및 공백 정제
                            string[] keywords = keywordsToken.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string kw in keywords)
                            {
                                // 대소문자 차이로 인한 검색 실패 및 중복을 막기 위해 무조건 소문자로 통일
                                string cleanKeyword = kw.Trim().ToLower();
                                if (string.IsNullOrEmpty(cleanKeyword)) continue;

                                if (!_serverMappingCache.ContainsKey(cleanKeyword))
                                {
                                    _serverMappingCache.Add(cleanKeyword, targetServer);
                                }
                                else
                                {
                                    // 이미 등록된 키워드일 경우, 동일한 서버를 가리키는지 확인
                                    string existingServer = _serverMappingCache[cleanKeyword];

                                    if (existingServer != targetServer)
                                    {
                                        string conflictInfo = $"'{cleanKeyword}' ➔ [{existingServer}] vs [{targetServer}]";
                                        LogMessage($"[캐시 경고] 중복 키워드 충돌: {conflictInfo} (우선 적용: {existingServer})", Level.Warning);
                                        conflictLogs.Add(conflictInfo);
                                    }
                                }
                            }
                        }
                    }
                }
                // 루프가 종료된 후, 수집된 충돌 내역이 있다면 사용자에게 1회 경고 팝업
                if (conflictLogs.Count > 0)
                {
                    // 화면 밖으로 창이 길어지는 것을 방지하기 위해 최대 10건만 팝업에 표시
                    var displayConflicts = conflictLogs.Take(10).ToList();
                    string alertMessage = $"엑셀 매핑 데이터에서 {conflictLogs.Count}건의 중복 서버가 발견되었습니다.\n" +
                                          "시스템 보호를 위해 상단에 먼저 기록된 서버가 유지됩니다.\n\n" +
                                          "[중복 충돌 상세 내역]\n" +
                                          string.Join("\n", displayConflicts);

                    if (conflictLogs.Count > 10)
                    {
                        alertMessage += $"\n...외 {conflictLogs.Count - 10}건 (상세 내역은 로그 참조)";
                    }

                    alertMessage += "\n\n사용자가 직접 엑셀 파일을 열어 중복을 정리해 주십시오.";

                    MessageBox.Show(alertMessage, "매핑 캐시 중복 경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    LogMessage("[캐시 로드 완료] 매핑 파일이 정상적으로 메모리에 적재되었습니다.", Level.Info);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "서버 매핑 엑셀 파일 캐싱 중 치명적 오류 발생");
            }
        }

        /// <summary>
        /// 입력된 호기명 또는 키워드를 기반으로 메모리에서 타겟 서버명을 찾아 반환합니다.
        /// </summary>
        public static string SearchServerByKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return string.Empty;

            string cleanKeyword = keyword.Trim();
            if (_serverMappingCache.TryGetValue(cleanKeyword, out string targetServer))
            {
                return targetServer;
            }

            return string.Empty; // 검색 결과 없음
        }

        ////////////////////////////////////////////////////////// Server Keyword Mapping END /////////////////////////////////////////////////////////////////////////////


    } // Util.cs END
}