using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    public partial class MainUI : MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;
        public static IWebDriver _driver = null;

        // Instance 등록
        private Config _config;
        private ConfigManager configManager = new ConfigManager();
        private ChromeDriverManager chromeDriverManager = new ChromeDriverManager();

        private string serverName;
        private string serverIP;

        private string GateID;
        private string GatePW;

        private string mainHandle;
        private ThemeManager _themeManager;
        private bool changeArrow = true;

        /// Option 전용
        private int _popupCount = 0; // 팝업 처리 횟수 카운터
        private readonly Timer timer1;

        private AppSettings _appSettings;


        // 연결상태 감지용
        private string _lastDriverStatus = "";
        private string _lastInternetStatus = "";
        private bool _isStatusTickRunning = false; // 타이머 겹침 방지, 진행중인지
        private DateTime _lastTickAtUtc = DateTime.MinValue; // 타이머 겹침 방지, 시간체크용

        // Control 관리용
        public static readonly Size FormOriginalSize = new Size(405, 700);
        public static readonly Size FormExtendedSize = new Size(590, 700);
        public static readonly Size TestFormExtendedSize = new Size(1100, 700);
        private Size groupConnect1OriginalSize;
        private Size tabSelector1OriginalSize;
        private Size tabControl1OriginalSize;

        // 컨텍스트메뉴 (우클릭)
        private ContextMenuStrip contextMenuStrip;

        // Work Log 관리용
        private WorkLogForm _workLogForm;

        // SandBox 관리용
        private SandBox _sandbox = null;


        private enum PresetSelection { None, A, B }

        public MainUI()
        {
            // 릴리즈노트 파일만 생성(열지 않음)
            GateHelper.Util.EnsureReleaseNotesInMeta();

            // 이하 기존 코드
            InitializeLogFile();
            LogMessage("========== Initialize ==========", Level.Info);
            InitializeComponent();

            _config = configManager.LoadedConfig;

            this.FormClosing += MainUI_FormClosing; // 폼 닫기 이벤트 연결

            // Material Skin 적용
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;

            // ContextMenuStrip 초기화 및 테마 동기화
            contextMenuStrip = OlvServerList.ContextMenuStrip ?? new ContextMenuStrip();
            OlvServerList.ContextMenuStrip = contextMenuStrip;

            // 필요 시 항목이 없을 때만 추가(디자이너에서 이미 있으면 추가하지 않음)
            if (contextMenuStrip.Items.Count == 0)
            {
                contextMenuStrip.Items.Add(new ToolStripMenuItem("Delete", null, MenuItem1_Delete_Click));
                contextMenuStrip.Items.Add(new ToolStripMenuItem("Favorite", null, MenuItem2_Favorite_Click));
            }

            // 실제 표시되는 메뉴 인스턴스에 테마 적용
            _themeManager = new ThemeManager(materialSkinManager, OlvServerList.ContextMenuStrip);
            _themeManager.ApplyContextMenuStripTheme(OlvServerList.ContextMenuStrip);
            materialSkinManager.ThemeChanged += (sender) => _themeManager.ApplyContextMenuStripTheme(OlvServerList.ContextMenuStrip);

            this.MaximizeBox = false;
            this.Size = FormOriginalSize;

            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 5000; // 5초마다 상태 확인
            timer1.Tick += TimerStatusChecker_Tick;
            timer1.Start();

            _appSettings = new AppSettings(); // 옵션 변수
            Util_Option.SetPopupGraceMs(_appSettings.PopupGraceMs);

            // UDP 기능이 처음 켜질 때 접속 송신
            if (_appSettings.UseUDP)
            {
                Util_Rdp.SendInitialConnect(_config);
            }

            LogMessage("프로그램 초기화 완료", Level.Info);
        }

        // ✦ TimerTick : Driver/Network/Popup/UDP Status Check
        private async void TimerStatusChecker_Tick(object sender, EventArgs e)
        {
            // 1. 밀린 틱 방지 및 재진입 가드
            timer1.Stop();

            if (_isStatusTickRunning)
            {
                LogMessage("[Tick] Re-entrancy blocked", Level.Info);
                timer1.Start();
                return;
            }
            _isStatusTickRunning = true;

            try
            {
                // --- 틱 간격 로깅 (디버그용) ---
                var now = DateTime.UtcNow;
                if (_lastTickAtUtc != DateTime.MinValue)
                {
                    var deltaMs = (now - _lastTickAtUtc).TotalMilliseconds;
                    // LogMessage($"[Tick Δ] {deltaMs:F0} ms", Level.Info);
                }
                _lastTickAtUtc = now;

                // 공통 색상 설정
                Color onColor = ColorTranslator.FromHtml("#4CAF50"); // Green 500
                Color offColor = ColorTranslator.FromHtml("#F44336"); // Red 500

                // 🔍 1. Driver 상태 체크
                bool driverOn = (_driver != null && chromeDriverManager.IsDriverAlive(_driver));
                string newDriverStatus = driverOn ? "ON" : "OFF";
                lblDriverStatus.Text = $"Driver {newDriverStatus}";
                lblDriverStatus.BackColor = driverOn ? onColor : offColor;
                lblDriverStatus.ForeColor = Color.White;

                if (_lastDriverStatus != newDriverStatus)
                {
                    LogMessage($"[Status Change] Driver {newDriverStatus}", driverOn ? Level.Info : Level.Error);
                    _lastDriverStatus = newDriverStatus;
                }

                // Driver OFF 감지 시 UDP 수신 중지 로직
                if (!driverOn && Util_Rdp.IsUdpReceiving)
                {
                    Util_Rdp.StopBroadcastReceiveLoop();
                    Util_Rdp.UpdateUDPStatusLabel(false);
                    LogMessage("[자동] 드라이버 OFF 감지, UDP 수신 중지", Level.Info);
                }

                // 🔍 2. Network 상태 체크
                bool netOn = chromeDriverManager.IsInternetAvailable();
                string newNetStatus = netOn ? "ON" : "OFF";
                lblInternetStatus.Text = $"Network {newNetStatus}";
                lblInternetStatus.BackColor = netOn ? onColor : offColor;
                lblInternetStatus.ForeColor = Color.White;

                if (_lastInternetStatus != newNetStatus)
                {
                    LogMessage($"[Status Change] Network {newNetStatus}", netOn ? Level.Info : Level.Error);
                    _lastInternetStatus = newNetStatus;
                }

                // 🔍 3. UDP 상태 업데이트
                Util_Rdp.UpdateUDPStatusLabel(Util_Rdp.IsUdpReceiving);


                // 🔍 4. 팝업 감지 상태 업데이트 (설정값 기반)
                // DisablePopup 변수 명칭에 따라 true일 때 OFF, false일 때 ON으로 처리
                bool popupFeatureOn = _appSettings.DisablePopup;

                // 타이머마다 UI를 갱신해주는 이유는 카운트 숫자를 최신화하기 위함입니다.
                Util_Option.UpdatePopupStatus(lblPopupStatus, popupFeatureOn, Util_Option.GetLockHandledCount());

                if (driverOn && popupFeatureOn)
                {
                    try
                    {
                        // HandleWindows 호출 (라벨과 활성화 여부 전달)
                        bool popupHandled = await Util_Option.HandleWindows(
                            _driver,
                            mainHandle,
                            _config,
                            lblPopupStatus,
                            popupFeatureOn
                        );

                        if (popupHandled)
                        {
                            _popupCount = Util_Option.GetLockHandledCount();
                            LogMessage($"[자동화] 잠금 화면 해제 완료 (누적: {_popupCount})", Level.Info);
                        }

                        try { mainHandle = _driver.CurrentWindowHandle; } catch { }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"HandleWindows 실행 중 예외: {ex.Message}", Level.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "TimerStatusChecker_Tick 전체 오류");
            }
            finally
            {
                // 5. 가드 해제 및 타이머 재시작
                _isStatusTickRunning = false;
                timer1.Start();
            }
        }



        protected async void BtnStart1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnStart1 Click", Level.Info);
            try
            {
                BtnReConfig1_Click(sender, e);

                // Start 경로에서는 드라이버 아직 없음 -> preset 클릭 핸들러 호출하지 말고
                // PresetManager에 skipDriverCheck=true로 직접 적용(알림 발생 방지)
                if (PresetManager.TryApplyPreset(_config, PresetManager.Preset.A, BtnPreset1, BtnPreset2, out var id, out var pw))
                {
                    GateID = id;
                    GatePW = pw;
                }

                ////////////////////////WebDriverManager 사용여부//////////////////////////////// 
                // _driver = await Task.Run(() => ChromeDriverManager.InitializeDriver(_config)); // 비동기로 드라이버 초기화
                _driver = await Task.Run(() => ChromeDriverManager.InitializeDriverWithSeleniumManager(_config)); // 비동기로 드라이버 초기화

                _driver.Navigate().GoToUrl(_config.Url); // 입력한 사이트로 이동
                mainHandle = _driver.CurrentWindowHandle; // MainHandle 저장
                LogMessage("Start MainHandle: " + mainHandle, Level.Info);

                Util_Control.MoveFormToTop(this);

                if (_appSettings.AutoLogin == true) // Auto Login
                {
                    BtnStart2_Click(sender, e);
                    BtnGateOneLogin1_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
        }

        private void BtnStart2_Click(object sender, EventArgs e)
        {
            LogMessage("BtnStart2 Click", Level.Info);
            Util_Connect.AutoConnect_1_Step(_driver, this); // 고급 - MPOHelper 클릭
        }

        private void BtnGateOneLogin1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnGateOneLogin1_Click", Level.Info);

            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            Util_Connect.AutoConnect_2_Step(_driver, _config, mainHandle);
            // Util_Connect.AutoConnect_3_Step(_driver);
            
        }

        private async void BtnSearch1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnSearch1 Click", Level.Info);

            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            Util.SwitchToMainHandle(_driver, mainHandle);

            try
            {
                // 입력된 형식 검사
                Util.ValidateServerInfo(TxtSearch1.Text, out serverName, out serverIP);

                // 필드 채우기
                Util_Control.FillSearchFields(_driver, serverName, serverIP);

                // 검색 버튼 클릭
                Util_Element.ClickElementByXPath(_driver, "//*[@id='access_control']/table/tbody/tr[2]/td/a");

                await LoadServersIntoComboBoxAsync(); // 서버목록 갱신
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

        private async void BtnLoadServers1_Click(object sender, EventArgs e)
        {
            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            LogMessage("BtnLoadServers1 Click", Level.Info);

            await LoadServersIntoComboBoxAsync();
        }

        // 서버 목록을 로드하여 콤보박스에 채우기
        private async Task LoadServersIntoComboBoxAsync()
        {
            try
            {
                // 서버 추출은 별도 유틸에서 수행 (UI 스레드 차단 방지)
                var serverList = await Task.Run(() => ServerLoader.GetServers(_driver));

                // UI 업데이트는 메인 스레드에서
                Invoke(new Action(() =>
                {
                    ComboBoxServerList1.Items.Clear();
                    foreach (string serverName in serverList)
                        ComboBoxServerList1.Items.Add(serverName);
                }));

                this.Activate();

                if (serverList.Count == 0)
                {
                    MessageBox.Show(this, "검색 결과가 없습니다.\n입력하신 내용을 다시 확인해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LogMessage("검색 결과 없음.", Level.Info);
                }
                else
                {
                    LogMessage("서버 목록 로딩 완료.", Level.Info);
                }
            }
            catch (WebDriverTimeoutException)
            {
                this.Activate();
                MessageBox.Show(this, "서버 목록을 불러오는 데 실패했습니다.\n(Timeout Exception)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage("WebDriverTimeoutException: ", Level.Error);
            }
            catch (Exception ex)
            {
                this.Activate();
                MessageBox.Show($"서버 목록 로딩 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogException(ex, Level.Error);
            }
        }

        private void BtnConnect1_Click(object sender, EventArgs e)
        {
            // ✅ 테스트 모드일 때는 드라이버 체크 건너뜀
            if (_appSettings.TestMode)
            {
                Util_Test.SimulateServerConnect(this, OlvServerList, ComboBoxServerList1, _appSettings.TestMode, _appSettings.RemoveDuplicates);
                Util_ServerList.SaveServerDataToFile(OlvServerList);
                return;
            }

            LogMessage("BtnConnect1 Click", Level.Info);
            LogMessage("Connect MainHandle : " + mainHandle, Level.Info);

            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            if (ComboBoxServerList1.SelectedItem == null)
            {
                MessageBox.Show("서버를 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedServer = ComboBoxServerList1.SelectedItem.ToString();
            LogMessage("접속 서버 명: " + selectedServer, Level.Info);

            // UDP 접속 정보 송신
            StartRdpDetect(serverName);

            Util_Connect.ConnectToServer(_driver, mainHandle, GateID, GatePW, selectedServer, OlvServerList, _appSettings.RemoveDuplicates);
        }


        private async void BtnFav1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnFav1 Click", Level.Info);

            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            // 1) 검색 수행하고 서버이름을 받음
            string serverName = Util.ClickFavBtnAndGetServerName(_driver, _config, 1, chromeDriverManager);

            // 2) UI(콤보박스) 갱신: 기존 동작 유지(비동기 로드)
            await LoadServersIntoComboBoxAsync();

            // 3) 옵션이 켜져 있으면 바로 연결
            if (!string.IsNullOrEmpty(serverName) && _appSettings.FavOneClickConnect)
            {
                try
                {
                    StartRdpDetect(serverName);
                    Util_Connect.ConnectToServer(_driver, mainHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
                }
                catch (Exception ex)
                {
                    LogException(ex, Level.Error);
                    MessageBox.Show("즐겨찾기 바로접속 중 오류가 발생했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnFav2_Click(object sender, EventArgs e)
        {
            LogMessage("BtnFav2 Click", Level.Info);

            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            string serverName = Util.ClickFavBtnAndGetServerName(_driver, _config, 2, chromeDriverManager);

            await LoadServersIntoComboBoxAsync();

            if (!string.IsNullOrEmpty(serverName) && _appSettings.FavOneClickConnect)
            {
                try
                {
                    StartRdpDetect(serverName);
                    Util_Connect.ConnectToServer(_driver, mainHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
                }
                catch (Exception ex)
                {
                    LogException(ex, Level.Error);
                    MessageBox.Show("즐겨찾기 바로접속 중 오류가 발생했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnTFav3_Click(object sender, EventArgs e)
        {
            LogMessage("BtnFav3 Click", Level.Info);

            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            string serverName = Util.ClickFavBtnAndGetServerName(_driver, _config, 3, chromeDriverManager);

            await LoadServersIntoComboBoxAsync();

            if (!string.IsNullOrEmpty(serverName) && _appSettings.FavOneClickConnect)
            {
                try
                {
                    StartRdpDetect(serverName);
                    Util_Connect.ConnectToServer(_driver, mainHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
                }
                catch (Exception ex)
                {
                    LogException(ex, Level.Error);
                    MessageBox.Show("즐겨찾기 바로접속 중 오류가 발생했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnReConfig1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnConfig1 Click", Level.Info);
            try
            {
                configManager.ReloadConfig();
                _config = configManager.LoadedConfig;

                if (_config != null)
                {
                    // 즐겨찾기 버튼 텍스트 설정
                    BtnFav1.Text = string.IsNullOrEmpty(_config.Fav1) ? "Fav1" : _config.Fav1;
                    BtnFav2.Text = string.IsNullOrEmpty(_config.Fav2) ? "Fav2" : _config.Fav2;
                    BtnFav3.Text = string.IsNullOrEmpty(_config.Fav3) ? "Fav3" : _config.Fav3;

                    // 프리셋 버튼 텍스트 설정
                    BtnPreset1.Text = string.IsNullOrEmpty(_config.GateName_A) ? "Preset1" : _config.GateName_A;
                    BtnPreset2.Text = string.IsNullOrEmpty(_config.GateName_B) ? "Preset2" : _config.GateName_B;
                }
                else
                {
                    LogMessage("Fail Config Re-Load", Level.Info);
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"설정 파일 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"설정 파일을 찾을 수 없습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"설정 로드 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenConfig1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnOpenConfig1 Click", Level.Info);
            configManager.OpenConfigFile();
        }

        private void BtnOpenLog1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnOpenLog1 Click", Level.Info);
            OpenLogFile();
        }






        //////////////////////////////////////////////////////////////////////////////// 옵션 전용 시작

        private void BtnOption1_Click(object sender, EventArgs e)
        {
            bool oldTestMode = _appSettings.TestMode; // 기존 값 저장
            bool oldDisablePopup = _appSettings.DisablePopup;
            bool oldUseUdpReceive = _appSettings.UseUDP;

            OptionForm optionForm = new OptionForm(_appSettings, _themeManager.IsDarkMode);
            DialogResult result = optionForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                _appSettings = optionForm.AppSettings; // 새로운 값 업데이트
                bool newIsDarkMode = optionForm.IsDarkModeEnabled;
                _themeManager.SetTheme(newIsDarkMode, PicBox_Setting, OlvServerList);
                Util_Option.SetPopupGraceMs(_appSettings.PopupGraceMs);

                if (oldTestMode != _appSettings.TestMode)
                {
                    ApplyTestMode(_appSettings.TestMode);
                }

                if (oldDisablePopup != _appSettings.DisablePopup)
                {
                    Util_Option.UpdatePopupStatus(lblPopupStatus, _appSettings.DisablePopup, _popupCount);
                }

                if (oldUseUdpReceive != _appSettings.UseUDP)
                {
                    if (_appSettings.UseUDP)
                    {
                        // ✅ 드라이버 가드: 드라이버가 OFF면 UDP를 켤 수 없도록 안내
                        bool driverOn = (_driver != null && chromeDriverManager.IsDriverAlive(_driver));
                        if (!driverOn)
                        {
                            _appSettings.UseUDP = false; // 되돌림
                            Util_Rdp.UpdateUDPStatusLabel(false);
                            LogMessage("[UDP] 드라이버 OFF 상태에서 UDP ON 시도 차단", Level.Error);
                            MessageBox.Show(
                                "ChromeDriver가 실행 중이 아닙니다.\nUDP 기능을 사용하려면 먼저 드라이버를 시작하세요.",
                                "UDP 알림",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                        else
                        {
                            Util_Rdp.SendInitialConnect(_config);
                            Util_Rdp.StartBroadcastReceiveLoop(OnUdpMessageReceived);
                            Util_Rdp.UpdateUDPStatusLabel(true);
                            LogMessage("[UDP] 수신 시작", Level.Info);
                        }
                    }
                    else
                    {
                        Util_Rdp.SendExitMessage(_config); // UDP 기능을 끌 때 EXIT 송신
                        Util_Rdp.StopBroadcastReceiveLoop();
                        Util_Rdp.UpdateUDPStatusLabel(false);
                        LogMessage("[UDP] 수신 종료", Level.Info);
                    }
                }

                LogMessage("Options Save Click", Level.Info);
            }
            else
            {
                LogMessage("Options Cancel Click", Level.Info);
            }
        }

        private void ApplyTestMode(bool isEnabled)
        {
            if (isEnabled)
            {
                _appSettings.TestMode = Util_Test.EnterTestMode(this, TabSelector1);

                if (_appSettings.TestMode)
                {
                    LogMessage("Test Mode 진입", Level.Info);
                    Util_Test.LoadTestServers(ComboBoxServerList1);
                }
                else
                {
                    LogMessage("Test Mode 진입 실패", Level.Critical);
                }
            }
            else // isEnabled가 false일 때
            {
                LogMessage("Test Mode 종료", Level.Info);
                ComboBoxServerList1.Items.Clear();
                _appSettings.TestMode = false;
            }
        }


        // ✦ ListView 더블클릭 접속
        private void ObjectListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            var hit = OlvServerList.OlvHitTest(e.X, e.Y);

            if (hit.Column == Memo)  // 메모 열이면
            {
                if (hit.Item != null) 
                    OlvServerList.StartCellEdit(hit.Item, hit.ColumnIndex);
                return;
            }

            if (!_appSettings.ServerClickConnect || OlvServerList.SelectedObjects.Count == 0)
                return;

            var selectedServerInfo = OlvServerList.SelectedObject as Util_ServerList.ServerInfo;
            string serverName = selectedServerInfo?.ServerName;
            if (string.IsNullOrEmpty(serverName))
                return;

            LogMessage($"ListView 더블클릭 접속 시도: {serverName}", Level.Info);

            // 현재 웹페이지에서 서버목록 가져오기
            var serverList = Util_ServerList.GetServerListFromWebPage(_driver);
            bool exists = serverList.Any(s =>
            string.Equals(s.ServerName, serverName, StringComparison.OrdinalIgnoreCase));

            if (exists) // 서버가 현재 페이지에 있으면
            {
                LogMessage($"현재 화면에 [{serverName}] 존재 - 바로 접속 시도", Level.Info);

                // UDP 접속 정보 송신
                StartRdpDetect(serverName);

                Util_Connect.ConnectToServer(_driver, mainHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
            }
            else // 없으면 검색 후 접속
            {
                LogMessage($"현재 화면에 [{serverName}] 없음 - 검색 후 접속 시도", Level.Info);

                // 검색 동작 수행
                TxtSearch1.Text = serverName;
                BtnSearch1_Click(null, null);

                try
                {
                    WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    wait.Until(driver =>
                    {
                        var elements = driver.FindElements(By.XPath("//*[@id='seltable']//td[4]"));
                        return elements.Any(el => el.Text == serverName);
                    });
                }
                catch (WebDriverTimeoutException)
                {
                    MessageBox.Show("검색 결과가 로딩되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // UDP 접속 정보 송신
                StartRdpDetect(serverName);

                Util_Connect.ConnectToServer(_driver, mainHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
            }
        }

        //////////////////////////////////////////////////////////////////////////////// 옵션 전용 끝



        private void PicBox_Setting_Click(object sender, EventArgs e)
        {
            _themeManager.SetTheme(!_themeManager.IsDarkMode, PicBox_Setting, OlvServerList);
        }

        private void PicBox_Arrow_Click(object sender, EventArgs e)
        {
            Util_Control.ToggleFormLayout(
                this, PicBox_Arrow, PicBox_Setting, PicBox_Question, BtnOption1, FormOriginalSize, FormExtendedSize, TabSelector1,
                tabSelector1OriginalSize, GroupConnect1, groupConnect1OriginalSize, TabControl1.Size, ref changeArrow);
        }

        private void PicBox_Question_Click(object sender, EventArgs e)
        {
            Util.OpenReleaseNotes();
        }


        private void MainUI_Load(object sender, EventArgs e)
        {
            groupConnect1OriginalSize = GroupConnect1.Size;
            tabSelector1OriginalSize = TabSelector1.Size;
            tabControl1OriginalSize = TabControl1.Size;

            Util_ImageLoader.EnsureReferenceImagesFolderExists(); // ReferenceImages Folder Check
            Util_ImageLoader.LoadReferenceImages(flowLayoutPanel1); // Images Load

            Util_ServerList.LoadServerDataFromFile(OlvServerList); // ServerData Load

            // [UDP] OBJ 컬럼 이모지 설정
            IsInUse.AspectGetter = rowObj => "🔍";
            this.IsInUse.Name = "IsInUse";

            string version = Util.GetCurrentVersionFromReleaseNotes();
            lblVersion.Text = $"{version}";
        }

        private void MainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Util_Rdp.SendExitMessage(_config); // 프로그램 종료 시 메시지 송신

            Util_Rdp.StopBroadcastReceiveLoop(); // RDP 수신 루프 종료

            if (_driver != null)
            {
                ChromeDriverManager.CloseDriver(_driver);
                _driver = null;  // 드라이버 객체 해제
            }

            // 프로그램 완전 종료'
            LogMessage("프로그램 종료", Level.Info);
            Environment.Exit(0);
        }


        private void SearchTxt1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                BtnSearch1.PerformClick();
            }
        }

        private void BtnOpenImages1_Click(object sender, EventArgs e)
        {
            Util_ImageLoader.OpenReferenceImagesFolder();
        }

        private void BtnReloadImages1_Click(object sender, EventArgs e)
        {
            Util_ImageLoader.LoadReferenceImages(flowLayoutPanel1);
        }



        private void MenuItem1_Delete_Click(object sender, EventArgs e)
        {
            var selectedObject = this.OlvServerList.SelectedObject;

            if (selectedObject != null)
            {
                // ServerInfo 객체에서 서버 이름을 직접 가져옵니다.
                string serverName = (selectedObject as Util_ServerList.ServerInfo)?.ServerName;

                DialogResult dialogResult = MessageBox.Show($"'{serverName}' 항목을 삭제하시겠습니까?", "항목 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    // ObjectListView의 RemoveObject 메서드를 사용하여 객체를 직접 삭제합니다.
                    this.OlvServerList.RemoveObject(selectedObject);

                    // ObjectListView는 객체가 제거되면 자동으로 재정렬되므로, 
                    // ReorderListViewItems 호출은 필요 없습니다.

                    Util_ServerList.SaveServerDataToFile(OlvServerList);

                    LogMessage($"Server entry '{serverName}' has been removed from the list.", Level.Info);
                }
            }
        }

        private void MenuItem2_Favorite_Click(object sender, EventArgs e)
        {
            var selectedObject = this.OlvServerList.SelectedObject as Util_ServerList.ServerInfo;

            if (selectedObject != null)
            {
                selectedObject.IsFavorite = !selectedObject.IsFavorite;

                // ⭐ 이 메서드가 FormatRow 이벤트를 자동으로 발생시킵니다.
                this.OlvServerList.RefreshObject(selectedObject);

                Util_ServerList.SaveServerDataToFile(OlvServerList);

                string logMessage;
                if (selectedObject.IsFavorite)
                {
                    logMessage = $"Server '{selectedObject.ServerName}' Favorited!";
                }
                else
                {
                    logMessage = $"Server '{selectedObject.ServerName}' Unfavorited!";
                }

                LogMessage(logMessage, Level.Info);
            }
        }

        private void ObjectListView1_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            var serverInfo = e.Model as Util_ServerList.ServerInfo;
            if (serverInfo != null && serverInfo.IsFavorite)
            {
                // 즐겨찾기 상태일 때만 폰트를 굵게 만듭니다.
                e.Item.Font = new Font(this.Font, FontStyle.Bold);
            }
            else
            {
                // 즐겨찾기 상태가 아니면 폰트를 원래대로 되돌립니다.
                e.Item.Font = new Font(this.Font, FontStyle.Regular);
            }
        }
        
        private void ObjectListView1_
            (object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            if (e.Column != Memo) return;

            // (선택) 자동반영이 안 되는 설정이라면 수동 반영
            if (e.RowObject is Util_ServerList.ServerInfo si && e.NewValue != null)
            {
                var newText = e.NewValue.ToString();
                if (!string.Equals(si.Memo ?? "", newText ?? "", StringComparison.Ordinal))
                {
                    si.Memo = newText;
                    OlvServerList.RefreshObject(si); // 화면 즉시 반영
                }
            }

            Util_ServerList.SaveServerDataToFile(OlvServerList);
        }

        private void BtnPreset1_Click(object sender, EventArgs e)
        {
            if (PresetManager.TryApplyPreset(_config, PresetManager.Preset.A, BtnPreset1, BtnPreset2, out var id, out var pw))
            {
                GateID = id;
                GatePW = pw;
            }
        }

        private void BtnPreset2_Click(object sender, EventArgs e)
        {
            if (PresetManager.TryApplyPreset(_config, PresetManager.Preset.B, BtnPreset1, BtnPreset2, out var id, out var pw))
            {
                GateID = id;
                GatePW = pw;
            }
        }

        // [UDP] 메시지 수신
        private void OnUdpMessageReceived(string msg)
        {
            Util_Rdp.WriteUdpReceiveLog(msg); // UDP 수신 로그 기록

            // 메시지 파싱
            var parts = msg.Split(new[] { " / " }, StringSplitOptions.None);
            if (parts.Length < 2) return;

            string userId = parts[0].Trim();
            string serverNameOrToken = parts[1].Trim();

            if (OlvServerList.InvokeRequired)
            {
                OlvServerList.Invoke(new Action(() => OnUdpMessageReceived(msg)));
                return;
            }

            foreach (var item in OlvServerList.Objects)
            {
                var serverInfo = item as Util_ServerList.ServerInfo;
                if (serverInfo == null) continue;

                // INITIAL_CONNECT/EXIT는 서버명이 아니므로 제외, 서버명인 경우에만 반영
                bool isServerEvent = 
                    !string.Equals(serverNameOrToken, "INITIAL_CONNECT", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(serverNameOrToken, "EXIT", StringComparison.OrdinalIgnoreCase);

                if (isServerEvent && string.Equals(serverInfo.ServerName, serverNameOrToken, StringComparison.OrdinalIgnoreCase))
                {
                    serverInfo.IsInUse = true;
                    serverInfo.LastBroadcastMessage = msg;

                    // ✦ 통일 기준: 로컬 수신 시각(DateTime.Now)을 LastConnected에 기록
                    DateTime receivedLocal = DateTime.Now;
                    if (serverInfo.LastConnected == null || receivedLocal > serverInfo.LastConnected)
                        serverInfo.LastConnected = receivedLocal;

                    OlvServerList.RefreshObject(serverInfo);
                }
            }
        }

        // [UDP] 툴팁 표시, 사용자의 로컬 시간대로 변환하여 표시
        private void ObjectListView1_CellToolTipShowing(object sender, BrightIdeasSoftware.ToolTipShowingEventArgs e)
        {
            if (e.Column != null && e.Column.Name == "IsInUse" && e.Model is Util_ServerList.ServerInfo info)
            {
                if (string.IsNullOrEmpty(info.LastBroadcastMessage))
                {
                    e.Text = "최근 접속 이력 없음";
                    return;
                }

                string msg = info.LastBroadcastMessage;

                try
                {
                    // 예상 포맷: "userId / serverName / yyyy-MM-dd HH:mm:ss [UTC]"
                    var parts = msg.Split(new[] { " / " }, StringSplitOptions.None);
                    if (parts.Length >= 3)
                    {
                        string userId = parts[0].Trim();
                        string serverName = parts[1].Trim();
                        string timeToken = parts[2].Trim(); // "yyyy-MM-dd HH:mm:ss [UTC]"
                        string timeStr = timeToken.Replace(" [UTC]", "");

                        DateTime utc;
                        // UTC 가정하여 엄격 파싱
                        if (!DateTime.TryParseExact(
                                timeStr,
                                "yyyy-MM-dd HH:mm:ss",
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                                out utc))
                        {
                            // 폴백: 일반 파싱 후 UTC Kind 지정
                            if (!DateTime.TryParse(timeStr, out utc))
                            {
                                e.Text = msg; // 파싱 실패 시 원문 표시
                                return;
                            }
                            if (utc.Kind != DateTimeKind.Utc)
                                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
                        }

                        // 클라이언트(실행 PC)의 시간대로 변환
                        var localTz = TimeZoneInfo.Local;
                        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utc, localTz);

                        // 시간대 약어 생성 (예: "Korea Standard Time" -> KST, "Eastern Daylight Time" -> EDDT -> EDT)
                        string tzBaseName = localTz.IsDaylightSavingTime(localTime) ? localTz.DaylightName : localTz.StandardName;
                        string tzAbbrev = new string(tzBaseName
                            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(w => char.ToUpperInvariant(w[0]))
                            .ToArray());

                        e.Text = $"{userId} / {serverName} / {localTime:yyyy-MM-dd HH:mm:ss} ({tzAbbrev})";
                    }
                    else
                    {
                        e.Text = msg; // 포맷 불일치 시 원문
                    }
                }
                catch
                {
                    e.Text = msg; // 예외 시 원문
                }
            }
            else
            {
                e.Text = null; // 툴팁 없음
            }
        }

        #region Notify Icon
        public void ShowTrayNotification(string title, string message, ToolTipIcon iconType)
        {
            if (this.notifyIcon1 != null)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => ShowTrayNotification(title, message, iconType)));
                    return;
                }

                // 1. 아이콘을 트레이에 보이게 설정
                this.notifyIcon1.Icon = this.Icon ?? SystemIcons.Information;
                this.notifyIcon1.Visible = true;

                this.notifyIcon1.BalloonTipTitle = title;
                this.notifyIcon1.BalloonTipText = message;
                this.notifyIcon1.BalloonTipIcon = iconType;

                this.notifyIcon1.BalloonTipClosed -= HideTrayIcon;
                this.notifyIcon1.BalloonTipClicked -= HideTrayIcon;

                // 3. 이벤트 연결: 사용자가 X를 누르거나(Closed), 클릭했을 때(Clicked)만 실행
                this.notifyIcon1.BalloonTipClosed += HideTrayIcon;
                this.notifyIcon1.BalloonTipClicked += HideTrayIcon;

                this.notifyIcon1.ShowBalloonTip(30000); // 30초 설정 (시스템에 따라 다름)
            }
        }

        // 아이콘을 트레이에서 사라지게 하는 메서드
        private void HideTrayIcon(object sender, EventArgs e)
        {
            if (this.notifyIcon1 != null)
            {
                this.notifyIcon1.Visible = false;
            }
        }
        #endregion

        private void StartRdpDetect(string serverName)
        {
            Util_Rdp.BroadcastSend(_config, serverName, false);
        }

        private void TestBtn1_Click(object sender, EventArgs e)
        {

        }

        // Open UDP Receive Log
        private void BtnOpenLog2_Click(object sender, EventArgs e)
        {
            try
            {
                string logDir = Path.Combine(Application.StartupPath, "Log");
                string logPath = Path.Combine(logDir, $"UdpReceiveLog_{DateTime.Now:yyyyMMdd}.txt");

                if (!File.Exists(logPath))
                {
                    MessageBox.Show("Today's UDP receive log file does not exist.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Process.Start(new ProcessStartInfo(logPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"An error occurred while opening the log file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Open Work Log
        private void BtnWorkLog1_Click(object sender, EventArgs e)
        {
            try
            {
                if (_workLogForm == null || _workLogForm.IsDisposed)
                {
                    _workLogForm = new WorkLogForm();
                    _workLogForm.StartPosition = FormStartPosition.CenterParent;
                    _workLogForm.FormClosed += (s, args) =>
                    {
                        try { _workLogForm.Dispose(); } catch { }
                        _workLogForm = null;
                    };
                    _workLogForm.Show(this); // Non-modal
                }
                else
                {
                    if (_workLogForm.WindowState == FormWindowState.Minimized)
                        _workLogForm.WindowState = FormWindowState.Normal;

                    _workLogForm.BringToFront();
                    _workLogForm.Activate();
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show(this, "An error occurred while opening the Work Log window.", "Error",
                  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSandBox_Click(object sender, EventArgs e)
        {
            // 창이 없거나 닫혀서 메모리에서 해제된 경우 새로 생성
            if (_sandbox == null || _sandbox.IsDisposed)
            {
                _sandbox = new SandBox();
                _sandbox.Show();
            }
            else
            {
                // 이미 떠 있다면 앞으로 가져오기
                _sandbox.BringToFront();
            }
        }
    }
}