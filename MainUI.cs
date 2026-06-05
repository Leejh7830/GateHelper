using GateHelper.Utils;
using MaterialSkin;
using MaterialSkin.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private string mainHandle; // GateOne Main
        private bool _isManagementActive; // Manufacturing Management 통합관리 플래그
        private string managementHandle;  // Manufacturing Management 통합관리
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
            timer1.Stop();

            // 💡 1. [동기화 레이더망] 보조 사이트 탭 감지 및 포커스 인터락 (잠금)
            try
            {
                if (_driver != null)
                {
                    var currentHandles = _driver.WindowHandles;
                    
                    // [탭 열림 자동 감지]
                    if (currentHandles.Count > 1)
                    {

                        if (string.IsNullOrEmpty(managementHandle) || !currentHandles.Contains(managementHandle))
                        {
                            bool isLoading = false; // 💡 비정상 인터락 방어용 상태값

                            foreach (var handle in currentHandles)
                            {
                                if (handle != mainHandle)
                                {
                                    _driver.SwitchTo().Window(handle);

                                    // IP 주소, 포트 번호, 서브 도메인 파편화 방지를 위한 완벽한 정제
                                    string targetKeyword = _config.ManagementUrl
                                        .Replace("http://", "")
                                        .Replace("https://", "")
                                        .Replace("www.", "")
                                        .TrimEnd('/');

                                    // '/'를 기준으로 잘라내어 순수 도메인 또는 IP:Port만 추출 (예: 192.168.0.1:8080)
                                    if (targetKeyword.Contains("/"))
                                    {
                                        targetKeyword = targetKeyword.Split('/')[0];
                                    }

                                    string currentUrl = _driver.Url.ToLower();

                                    // 매칭 성공
                                    if (currentUrl.Contains(targetKeyword.ToLower()))
                                    {
                                        managementHandle = handle;
                                        _isManagementActive = true;
                                        LogMessage($"[플래그 ON] MGMT탭({targetKeyword}) 열림 감지", Level.Info);
                                        break;
                                    }
                                    // 💡 [수정 2] 페이지가 열리는 중(about:blank)일 때 메인 로직 침범 차단
                                    else if (currentUrl.Contains("about:blank") || string.IsNullOrEmpty(currentUrl))
                                    {
                                        LogMessage("[로딩 대기] 새 탭이 아직 로딩 중입니다.", Level.Info);
                                        isLoading = true;
                                    }
                                    // 완전한 다른 사이트 (구글 등)
                                    else
                                    {
                                        LogMessage($"[플래그 실패] 타겟 키워드: '{targetKeyword}', 실제 URL: '{currentUrl}'", Level.Error);
                                    }
                                }
                            }

                            // 💡 [핵심 방어] 탭이 로딩 중이라면 메인 팝업 감지(포커스 강탈)를 스킵하여 방해하지 않음
                            if (isLoading && !_isManagementActive)
                            {
                                timer1.Start();
                                return;
                            }
                        }
                    }

                    // [탭 닫힘 자동 감지]
                    if (_isManagementActive && !string.IsNullOrEmpty(managementHandle))
                    {
                        if (!currentHandles.Contains(managementHandle))
                        {
                            _isManagementActive = false; // 플래그 OFF
                            managementHandle = null;

                            if (currentHandles.Contains(mainHandle))
                            {
                                _driver.SwitchTo().Window(mainHandle);
                            }
                            LogMessage("[플래그 OFF] MGMT탭 종료 감지 -> 팝업감지 재개", Level.Info);
                        }
                        else
                        {
                            // 보조 사이트가 아직 살아있으므로 하위 로직 올스톱
                            timer1.Start();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _isManagementActive = false;
                managementHandle = null;
                LogMessage($"[인터락 예외] 탭 검사 중 브라우저 예외 발생: {ex.Message}", Level.Error);
            }

            // --- (이 아래부터는 평상시 5초마다 도는 메인 전용 로직) ---

            if (_isStatusTickRunning)
            {
                timer1.Start();
                return;
            }

            _isStatusTickRunning = true;

            try
            {
                UpdateConnectionStatus();
                bool popupFeatureOn = _appSettings.AutoScreenUnlock;
                Util_Option.UpdatePopupStatus(lblPopupStatus, popupFeatureOn, Util_Option.GetLockHandledCount());

                bool driverOn = (_driver != null && chromeDriverManager.IsDriverAlive(_driver));

                if (driverOn && popupFeatureOn)
                {
                    await HandleScreenLockAutomation(popupFeatureOn);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "TimerStatusChecker_Tick 메인 로직 오류");
            }
            finally
            {
                _isStatusTickRunning = false;
                timer1.Start();
            }
        }

        private void UpdateConnectionStatus()
        {
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
        }

        private async Task HandleScreenLockAutomation(bool popupFeatureOn)
        {
            string currentHandle = "";
            try
            {
                // 1. 현재 핸들 저장 시도 (실패 시 복구 불가하므로 무시)
                try { currentHandle = _driver.CurrentWindowHandle; } catch { }

                // 2. 메인 핸들로 전환 (이게 실패하면 검사 자체가 불가능)
                if (currentHandle != mainHandle)
                {
                    _driver.SwitchTo().Window(mainHandle);
                }

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

                // [핵심 수정] 성공/실패 여부와 상관없이, 원래 메인이 아닌 다른 탭을 보고 있었다면 복구
                if (!string.IsNullOrEmpty(currentHandle) && currentHandle != mainHandle)
                {
                    if (_driver.WindowHandles.Contains(currentHandle))
                    {
                        _driver.SwitchTo().Window(currentHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"HandleWindows 실행 중 예외: {ex.Message}", Level.Error);

                // 예외 발생 시에도 시선이 꼬였다면 메인으로라도 돌려놓는 보험
                try { _driver.SwitchTo().Window(mainHandle); } catch { }
            }
        }


        protected async void BtnStart1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnStart1 Click", Level.Info);

            // 1. 진입 시 버튼 비활성화 (중복 클릭 방지)
            BtnStart1.Enabled = false;

            try
            {
                // 2. 이미 브라우저가 실행 중인지 체크
                if (_driver != null)
                {
                    try
                    {
                        var title = _driver.Title; // 생존 확인
                        MessageBox.Show("이미 브라우저가 실행 중입니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Util_Control.MoveFormToTop(this);
                        return;
                    }
                    catch
                    {
                        // 드라이버가 죽었다면 새로 시작
                        _driver = null;
                    }
                }

                // 3. 기존 실행 로직 진행
                BtnReConfig1_Click(sender, e);

                if (PresetManager.TryApplyPreset(_config, PresetManager.Preset.A, BtnPreset1, BtnPreset2, out var id, out var pw))
                {
                    GateID = id;
                    GatePW = pw;
                }

                // 드라이버 비동기 초기화
                _driver = await Task.Run(() => ChromeDriverManager.InitializeDriverWithSeleniumManager(_config));

                _driver.Navigate().GoToUrl(_config.Url);
                mainHandle = _driver.CurrentWindowHandle;
                LogMessage("Start MainHandle: " + mainHandle, Level.Info);

                Util_Control.MoveFormToTop(this);

                if (_appSettings.AutoLogin)
                {
                    BtnStart2_Click(sender, e);
                    BtnGateOneLogin1_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show("브라우저 시작 중 오류가 발생했습니다.");
            }
            finally
            {
                // 4. 어떤 이유로든 메서드가 끝날 때 버튼을 다시 활성화
                // 조기 리턴(return)이나 에러(catch) 상황 모두 여기서 처리
                BtnStart1.Enabled = true;
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
                return; // ChromeDriver 없음

            if (!Util.SwitchToMainHandle(_driver, mainHandle))
            {
                return; // MainHandle 없음
            }

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

        // 서버 목록을 로드하여 콤보박스에 채우기
        private async void BtnLoadServers1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnLoadServers1 Click", Level.Info);

            if (!chromeDriverManager.IsDriverReady(_driver))
                return; // ChromeDriver 없음

            if (!Util.SwitchToMainHandle(_driver, mainHandle))
            {
                return; // MainHandle 없음
            }

            await LoadServersIntoComboBoxAsync();
        }

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
                return; // ChromeDriver 없음

            if (!Util.SwitchToMainHandle(_driver, mainHandle))
            {
                return; // MainHandle 없음
            }

            if (ComboBoxServerList1.SelectedItem == null)
            {
                MessageBox.Show("서버를 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedServer = ComboBoxServerList1.SelectedItem.ToString();
            LogMessage("접속 서버 명: " + selectedServer, Level.Info);

            // UDP 접속 정보 송신
            StartRdpDetect(serverName);

            Util_Connect.ConnectToServer(_driver, mainHandle, managementHandle, GateID, GatePW, selectedServer, OlvServerList, _appSettings.RemoveDuplicates);
        }


        private async void BtnFav1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnFav1 Click", Level.Info);

            if (!chromeDriverManager.IsDriverReady(_driver))
                return; // ChromeDriver 없음

            if (!Util.SwitchToMainHandle(_driver, mainHandle))
            {
                return; // MainHandle 없음
            }

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
                    Util_Connect.ConnectToServer(_driver, mainHandle, managementHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
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
                return; // ChromeDriver 없음

            if (!Util.SwitchToMainHandle(_driver, mainHandle))
            {
                return; // MainHandle 없음
            }

            string serverName = Util.ClickFavBtnAndGetServerName(_driver, _config, 2, chromeDriverManager);

            await LoadServersIntoComboBoxAsync();

            if (!string.IsNullOrEmpty(serverName) && _appSettings.FavOneClickConnect)
            {
                try
                {
                    StartRdpDetect(serverName);
                    Util_Connect.ConnectToServer(_driver, mainHandle, managementHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
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
                    Util_Connect.ConnectToServer(_driver, mainHandle, managementHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
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
            bool oldAutoScreenUnlock = _appSettings.AutoScreenUnlock;
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

                if (oldAutoScreenUnlock != _appSettings.AutoScreenUnlock)
                {
                    Util_Option.UpdatePopupStatus(lblPopupStatus, _appSettings.AutoScreenUnlock, _popupCount);
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

            if (hit.Column == Memo)  // 더블클릭이 메모열이면
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

                Util_Connect.ConnectToServer(_driver, mainHandle, managementHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
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

                Util_Connect.ConnectToServer(_driver, mainHandle, managementHandle, GateID, GatePW, serverName, OlvServerList, _appSettings.RemoveDuplicates);
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
            // 1. 사용자에게 종료 여부 확인
            DialogResult result = MessageBox.Show(this,
                "Are you sure you want to exit?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            // 2. '아니요'를 선택하면 종료 이벤트 취소
            if (result == DialogResult.No)
            {
                e.Cancel = true; // 이 줄이 실행되면 폼이 닫히지 않습니다.
                return;
            }

            Util_Rdp.SendExitMessage(_config); // 프로그램 종료 시 메시지 송신
            Util_Rdp.StopBroadcastReceiveLoop(); // RDP 수신 루프 종료

            if (_driver != null)
            {
                ChromeDriverManager.CloseDriver(_driver);
                _driver = null;  // 드라이버 객체 해제
            }
            // 프로그램 완전 종료'
            LogMessage("프로그램 종료", Level.Info);

            // Environment.Exit(0);
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

        // 통합관리시스템 자동오픈
        private async void BtnStartManagement_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "보조 사이트(Mgmt)를 사용하는 동안 드라이버 충돌 방지를 위해" +
                "\n메인 사이트의 '팝업 자동 감지'기능이 일시적으로 비활성화됩니다." +
                "\n\n작업을 마치고 Mgmt 탭을 닫으면 팝업 감지가 재개됩니다.",
                "시스템 안내",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // 1. 드라이버 상태 체크
            if (!chromeDriverManager.IsDriverReady(_driver)) return;

            // 2. 포커스 복구 로직을 최상단으로 이동 (안전한 명령 수행을 위한 기초 공사)
            try
            {
                var current = _driver.CurrentWindowHandle;
            }
            catch
            {
                // 포커스를 잃었다면 무조건 메인으로 복귀시켜 이후 명령(WindowHandles 조회 등)이 가능하게 함
                _driver.SwitchTo().Window(mainHandle);
            }

            // 3. [인터락] 이미 보조 사이트가 열려있는지 확인
            if (!string.IsNullOrEmpty(managementHandle) && _driver.WindowHandles.Contains(managementHandle))
            {
                LogMessage("보조 사이트가 이미 열려 있습니다. 해당 탭으로 이동합니다.", Level.Info);

                // [추가] 사용자에게 상황을 명확히 알림
                MessageBox.Show("보조 사이트가 이미 실행 중입니다.\n기존에 열려 있는 탭으로 이동합니다.",
                                "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 기존 탭으로 포커스 이동
                _driver.SwitchTo().Window(managementHandle);

                // 프로그램 폼을 맨 위로 올려 시인성 확보
                Util_Control.MoveFormToTop(this);
                return;
            }

            // 4. URL 유효성 검사
            if (string.IsNullOrWhiteSpace(_config.ManagementUrl))
            {
                MessageBox.Show("ManagementUrl 설정이 비어있습니다.");
                return;
            }

            try
            {
                // 5. 새 탭 오픈 및 로그인 시도
                _driver.SwitchTo().NewWindow(WindowType.Tab);
                _driver.Navigate().GoToUrl(_config.ManagementUrl);

                // 핸들 저장과 동시에 인터락 플래그ON
                managementHandle = _driver.CurrentWindowHandle;
                _isManagementActive = true;

                LogMessage("[플래그 ON] MGMT탭 열림 감지 -> 팝업감지 중지", Level.Info);

                await Task.Run(() => PerformManagementLogin());
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "보조 사이트 오픈 중 오류");
            }
        }

        private void PerformManagementLogin()
        {
            bool idResult = Util_Element.SendKeysToElement(_driver, "//*[@id=\"validationCustom01\"]", _config.ManagementUserID);
            bool pwResult = Util_Element.SendKeysToElement(_driver, "//*[@id=\"LoginContainer\"]/section/div/form/div[1]/div[2]/input", _config.ManagementUserPW);

            // 3. ID와 PW 입력이 모두 성공했을 때만 로그인 버튼 클릭
            if (idResult && pwResult)
            {
                bool clickResult = Util_Element.ClickElementByXPath(_driver, "//*[@id=\"loginButton\"]");

                if (clickResult)
                    LogMessage("MGMT 자동 로그인 완료", Level.Info);
            }
            else
            {
                LogMessage("MGMT 로그인 필드를 찾지 못해 자동 로그인을 중단합니다.", Level.Error);
            }
        }

        private async void BtnMoveVariable_Click(object sender, EventArgs e)
        {
            // 1. 유효성 검사
            if (_driver == null || string.IsNullOrEmpty(managementHandle))
            {
                LogMessage("MGMT가 열려있지 않습니다.", Level.Error);
                return;
            }

            try
            {
                LogMessage("BtnMoveVariable_Click", Level.Info);

                // 2. 보조 사이트로 시선 전환
                _driver.SwitchTo().Window(managementHandle);

                // 3. 상단 메뉴 클릭 (Util_Element 메서드 활용)
                bool step1 = await Task.Run(() => Util_Element.ClickElementByXPath(_driver, "//*[@id=\"topNavArea\"]/nav/div[2]/a[3]"));
                if (!step1) return;

                // 4. 메뉴가 열리는 물리적인 시간을 위한 아주 짧은 대기
                await Task.Delay(500);

                // 5. 왼쪽 내비게이션 메뉴 클릭
                bool step2Result = await Task.Run(() => Util_Element.ClickElementByXPath(_driver, "//*[@id=\"mes_container\"]/div/div[1]/div[2]/div[1]/div[2]/div/a[2]/span/span"));
                if (step2Result)
                {
                    LogMessage("Variable Search 메뉴 이동 완료", Level.Info);
                }



            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "메뉴 이동 중 치명적 오류 발생");
            }
        }

        private async void BtnStoCollect_Click(object sender, EventArgs e)
        {
            // 1. 사전 체크
            if (_driver == null || string.IsNullOrEmpty(managementHandle))
            {
                LogMessage("MGMT가 열려있지 않습니다.", Level.Error);
                return;
            }

            // 2. Util_Mgmt의 동적 다이얼로그 호출
            var (isSemChecked, isPortChecked) = Util_Mgmt.ShowCollectionSelectDialog();

            if (!isSemChecked && !isPortChecked)
            {
                LogMessage("선택된 수집 항목이 없거나 사용자에 의해 작업이 취소되었습니다.", Level.Info);
                return;
            }

            LogMessage($"[수집 옵션] SEM 수집: {isSemChecked}, Port 수집: {isPortChecked}", Level.Info);
            BtnStoCollect.Enabled = false;

            try
            {
                LogMessage("STO 설비 다중 순회 수집 루틴 시작", Level.Info);
                _driver.SwitchTo().Window(managementHandle);

                // 전체 STO 설비 탐색
                string stoXPath = "//span[contains(@class, 'wj-node-text') and contains(text(), 'STO')]";
                var initialStoList = _driver.FindElements(By.XPath(stoXPath));
                int machineCount = initialStoList.Count;

                if (machineCount == 0)
                {
                    LogMessage("화면에서 STO 설비를 찾을 수 없습니다. 트리를 확장해 주세요.", Level.Error);
                    return;
                }

                LogMessage($"총 {machineCount}대의 STO 설비 순회를 시작합니다.", Level.Info);

                // 3. [Master Loop] 설비 순회 시작
                for (int i = 0; i < machineCount; i++)
                {
                    // Stale Element Reference 방어: 매 턴마다 설비 목록 재확보
                    var currentMachines = _driver.FindElements(By.XPath(stoXPath));
                    if (i >= currentMachines.Count) break;

                    var targetMachine = currentMachines[i];
                    string currentMachineName = targetMachine.Text;

                    LogMessage($"\n▶ [{i + 1}/{machineCount}] {currentMachineName} 진입", Level.Info);

                    targetMachine.Click();
                    await Task.Delay(1000); // 하위 노드 전개 애니메이션 대기

                    // ----------------------------------------------------------
                    // 3-1. StockerSEM 수집
                    // ----------------------------------------------------------
                    if (isSemChecked)
                    {
                        string semXPath = "//span[contains(@class, 'wj-node-text') and text()='StockerSEM']";
                        // 💡 [수정] e -> el로 변경하여 변수명 충돌 해결
                        var semElement = _driver.FindElements(By.XPath(semXPath)).FirstOrDefault(el => el.Displayed);

                        if (semElement != null)
                        {
                            LogMessage($"  - {currentMachineName} : StockerSEM 수집 중...", Level.Info);
                            semElement.Click();
                            await Task.Delay(1500);

                            // 💡 Util_Element.cs의 클립보드 초고속 추출 메서드 적극 활용
                            var tableData = await Task.Run(() => Util_Element.GetTableDataByClipboardFast(_driver));

                            if (tableData != null && tableData.Count > 0)
                            {
                                await Task.Run(() => SaveDataToCSV($"{currentMachineName}_SEM", tableData));
                            }
                            else
                            {
                                LogMessage($"  - {currentMachineName}_SEM : 표 데이터가 비어있습니다.", Level.Error);
                            }
                        }
                        else
                        {
                            LogMessage($"  - {currentMachineName} : StockerSEM (표시됨) 항목을 찾을 수 없습니다.", Level.Error);
                        }
                    }

                    // ----------------------------------------------------------
                    // 3-2. StockerPort 가변 다중 수집
                    // ----------------------------------------------------------
                    if (isPortChecked)
                    {
                        string portParentXPath = "//span[contains(@class, 'wj-node-text') and text()='StockerPort']";
                        // 💡 [수정] e -> el로 변경하여 변수명 충돌 해결
                        var portParentElement = _driver.FindElements(By.XPath(portParentXPath)).FirstOrDefault(el => el.Displayed);

                        if (portParentElement != null)
                        {
                            portParentElement.Click();
                            await Task.Delay(1000);

                            string childPortXPath = "//span[contains(@class, 'wj-node-text') and contains(text(), 'Port') and not(contains(text(), 'Stocker'))]";
                            // 💡 [수정] e -> el로 변경하여 변수명 충돌 해결
                            var visibleChildPorts = _driver.FindElements(By.XPath(childPortXPath)).Where(el => el.Displayed).ToList();
                            int childPortCount = visibleChildPorts.Count;

                            LogMessage($"  - {currentMachineName} : 총 {childPortCount}개의 하위 Port 발견. 순회 시작.", Level.Info);

                            for (int j = 0; j < childPortCount; j++)
                            {
                                // 💡 [수정] e -> el로 변경하여 변수명 충돌 해결
                                var refreshedPorts = _driver.FindElements(By.XPath(childPortXPath)).Where(el => el.Displayed).ToList();
                                if (j >= refreshedPorts.Count) break;

                                var targetPort = refreshedPorts[j];
                                string portName = targetPort.Text;

                                LogMessage($"    -> {portName} 수집 중...", Level.Info);
                                targetPort.Click();
                                await Task.Delay(1500);

                                // 💡 Util_Element.cs의 클립보드 초고속 추출 메서드 적극 활용
                                var tableData = await Task.Run(() => Util_Element.GetTableDataByClipboardFast(_driver));

                                if (tableData != null && tableData.Count > 0)
                                {
                                    await Task.Run(() => SaveDataToCSV($"{currentMachineName}_{portName}", tableData));
                                }
                            }
                        }
                        else
                        {
                            LogMessage($"  - {currentMachineName} : StockerPort 하위 폴더가 없습니다.", Level.Error);
                        }
                    }
                }

                LogMessage("모든 설비의 자동화 수집 루프가 완료되었습니다.", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "STO 다중 수집 루프 실행 중 오류 발생");
            }
            finally
            {
                BtnStoCollect.Enabled = true;
            }
        }

        private string SaveDataToCSV(string machineName, List<string[]> tableData)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // 💡 [핵심 1] 확장자를 .csv로 변경하여 엑셀의 '형식 불일치' 경고창을 원천 차단합니다.
                string fileName = $"호기결과_{machineName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = Path.Combine(desktopPath, fileName);

                // 한글 깨짐 방지를 위해 EUC-KR 인코딩은 그대로 유지합니다.
                using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.GetEncoding("euc-kr")))
                {
                    // (주의: CSV 포맷이므로 sep=\t 구문은 삭제했습니다)

                    // 1. [공통 헤더 주입] 쉼표(,)로 구분
                    string[] headers = { "Name", "Access", "Type", "Value", "Description" };
                    sw.WriteLine(string.Join(",", headers));

                    // 2. [알맹이 데이터 적재] 표준 CSV 쌍따옴표 래핑 규격 적용
                    foreach (var rowData in tableData)
                    {
                        var sanitizedRow = rowData.Select(cell =>
                        {
                            // 줄바꿈 제거 및 데이터 내부의 쌍따옴표 이스케이프 처리
                            string clean = cell.Replace("\r", "").Replace("\n", " ").Replace("\"", "\"\"");

                            // 💡 [핵심 2] 데이터 안에 쉼표(,)가 있어도 열이 밀리지 않도록 양끝을 쌍따옴표로 감쌉니다.
                            return $"\"{clean}\"";
                        });

                        sw.WriteLine(string.Join(",", sanitizedRow));
                    }
                }

                return filePath;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "CSV 파일 생성 중 예외 발생");
                return string.Empty;
            }
        }

        private async void BtnTestStoSearch_Click(object sender, EventArgs e)
        {
            if (_driver == null || string.IsNullOrEmpty(managementHandle))
            {
                LogMessage("MGMT 탭이 열려있지 않습니다.", Level.Error);
                return;
            }

            try
            {
                LogMessage("=== [테스트] 3-Depth 하위 노드(SEM, Ports) 탐색 시작 ===", Level.Info);
                _driver.SwitchTo().Window(managementHandle);

                // 1. [Depth 1] StockerSEM 탐색 및 강제 확장
                string semXPath = "//span[contains(@class, 'wj-node-text') and text()='StockerSEM']";
                var semElement = _driver.FindElements(By.XPath(semXPath)).FirstOrDefault(el => el.Displayed);

                if (semElement != null)
                {
                    LogMessage($"- [Depth 1] StockerSEM 발견됨. 하위 항목을 열기 위해 클릭합니다.", Level.Info);
                    semElement.Click();
                    await Task.Delay(1000); // 트리 확장 대기
                }
                else
                {
                    LogMessage("- [Depth 1] StockerSEM 항목이 화면에 보이지 않습니다. 탐색을 중단합니다.", Level.Error);
                    return; // SEM이 안 열리면 Port도 열 수 없으므로 즉시 종료
                }

                // 2. [Depth 2] StockerPorts 부모 폴더 탐색 및 강제 확장 (이미지 팩트: 끝에 's'가 붙음)
                string portParentXPath = "//span[contains(@class, 'wj-node-text') and text()='StockerPorts']";
                var portParentElement = _driver.FindElements(By.XPath(portParentXPath)).FirstOrDefault(el => el.Displayed);

                if (portParentElement != null)
                {
                    LogMessage($"- [Depth 2] StockerPorts 부모 폴더 발견됨. 자식 포트를 열기 위해 클릭합니다.", Level.Info);
                    portParentElement.Click();
                    await Task.Delay(1000); // 트리 확장 대기

                    // 3. [Depth 3] 전개된 하위 STOCKERPORT:n 개수 및 이름 파악 (이미지 팩트: 대문자 및 콜론 포함)
                    string childPortXPath = "//span[contains(@class, 'wj-node-text') and contains(text(), 'STOCKERPORT:')]";

                    var visibleChildPorts = _driver.FindElements(By.XPath(childPortXPath)).Where(el => el.Displayed).ToList();

                    LogMessage($"- [결과] 총 {visibleChildPorts.Count}개의 하위 세부 포트가 감지되었습니다.", Level.Info);

                    for (int i = 0; i < visibleChildPorts.Count; i++)
                    {
                        LogMessage($"  -> 자식 항목 [{i + 1}]: {visibleChildPorts[i].Text}", Level.Info);
                    }
                }
                else
                {
                    LogMessage("- [Depth 2] StockerPorts 부모 폴더를 찾을 수 없습니다.", Level.Error);
                }

                LogMessage("=== [테스트] 3-Depth 하위 노드 탐색 종료 ===", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "하위 노드 탐색 테스트 중 오류 발생");
            }
        }
    }
}