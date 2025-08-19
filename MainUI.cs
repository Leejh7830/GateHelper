using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Configuration;
using System.IO;
using System.Linq;
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

        private string mainHandle;
        
        /// Option 전용
        private bool disablePopup;
        private int _popupCount = 0; // 팝업 처리 횟수 카운터
        private readonly Timer timer1;
        private bool removeDuplicates = false; // 서버리스트 중복 제거

        // 연결상태 감지용
        private string _lastDriverStatus = "";
        private string _lastInternetStatus = "";
        private string _lastPopupStatus = "";

        // TestMode 확인용
        private bool testMode = false;

        // Control 관리용
        public static readonly Size FormOriginalSize = new Size(400, 700);
        public static readonly Size FormExtendedSize = new Size(550, 700);
        public static readonly Size TestFormExtendedSize = new Size(1100, 700);
        private Size groupConnect1OriginalSize;
        private Size tabSelector1OriginalSize;
        private Size tabControl1OriginalSize;

        private ContextMenuStrip contextMenuStrip;






        public MainUI()
        {
            InitializeLogFile();
            LogMessage("========== Initialize ==========", Level.Info);
            InitializeComponent();

            configManager.ReloadConfig(); // 설정 파일 로드
            Util.CreateFolder_Resource(); // 리소스 폴더 생성
            this.FormClosing += MainUI_FormClosing; // 폼 닫기 이벤트 연결

            // Material Skin 적용
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;

            // ContextMenuStrip 초기화 및 테마 동기화
            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add(new ToolStripMenuItem("Delete", null, MenuItem1_Click));
            // contextMenuStrip.Items.Add(new ToolStripMenuItem("메모 편집", null, EditMemo_Click)); // 새로운 메뉴 아이템 추가 시
            ListViewServer2.ContextMenuStrip = contextMenuStrip;
            materialSkinManager.ThemeChanged += (sender) => ApplyThemeToContextMenuStrip();
            ApplyThemeToContextMenuStrip(); // 초기 테마 적용

            this.MaximizeBox = false;
            this.Size = FormOriginalSize;

            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 5000; // 5초마다 상태 확인
            timer1.Tick += TimerStatusChecker_Tick;
            timer1.Start();

            LogMessage("프로그램 초기화 완료", Level.Info);
        }

        // 25.03.27 Added
        // 25.08.14 Modified - Popup Detector
        private async void TimerStatusChecker_Tick(object sender, EventArgs e)
        {
            Color onColor = ColorTranslator.FromHtml("#4CAF50"); // Green 500
            Color offColor = ColorTranslator.FromHtml("#F44336"); // Red 500
            Color whiteColor = Color.White;

            // 🔍 Driver 상태
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

            // 🔍 Network 상태
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

            // 🔍 팝업 감지 상태 추가
            bool popupFeatureOn = CBox_DisablePopup1.Checked;
            string newPopupStatus = popupFeatureOn ? "ON" : "OFF";
            lblPopupStatus.Text = $"Detect {newPopupStatus} ({_popupCount})";
            lblPopupStatus.BackColor = popupFeatureOn ? onColor : offColor;
            lblPopupStatus.ForeColor = whiteColor;

            if (_lastPopupStatus != newPopupStatus)
            {
                LogMessage($"[Status Change] Popup {newPopupStatus}", Level.Info);
                _lastPopupStatus = newPopupStatus;
            }

            if (driverOn && popupFeatureOn)
            {
                try
                {
                    bool popupHandled = await Util_Option.HandleWindows(_driver, mainHandle, _config);
                    if (popupHandled)
                    {
                        _popupCount++;
                        LogMessage($"팝업 처리 횟수 : {_popupCount}회", Level.Info);
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex, Level.Error);
                }
            }
        }



        protected async void BtnStart1_Click(object sender, EventArgs e)
        {

            LogMessage("BtnStart1 Click", Level.Info);
            try
            {
                BtnReConfig1_Click(sender, e);
                _driver = await Task.Run(() => ChromeDriverManager.InitializeDriver(_config)); // 비동기로 드라이버 초기화


                _driver.Navigate().GoToUrl(_config.Url); // 입력한 사이트로 이동
                mainHandle = _driver.CurrentWindowHandle; // MainHandle 저장
                LogMessage("Start MainHandle: " + mainHandle, Level.Info);

                Util_Control.MoveFormToTop(this);

                if (CBox_AutoLogin1.Checked == true) // Auto Login
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
            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            LogMessage("BtnGateOneLogin1_Click", Level.Info);
            Util_Connect.AutoConnect_2_Step(_driver, _config, mainHandle);
            // Util_Connect.AutoConnect_3_Step(_driver);
            
        }

        // 25.03.27 Modified - Module
        private void BtnSearch1_Click(object sender, EventArgs e)
        {
            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            LogMessage("BtnSearch1 Click", Level.Info);
            Util.SwitchToMainHandle(_driver, mainHandle);

            try
            {
                // 입력된 형식 검사
                Util.ValidateServerInfo(SearchTxt1.Text, out serverName, out serverIP);

                // 필드 채우기
                Util_Control.FillSearchFields(_driver, serverName, serverIP);

                // 검색 버튼 클릭
                Util_Element.ClickElementByXPath(_driver, "//*[@id='access_control']/table/tbody/tr[2]/td/a");
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

        // 25.03.18 Added - Load Server List Button
        // 25.03.19 Modified - Test Mode
        private void BtnLoadServers1_Click(object sender, EventArgs e)
        {
            if (!chromeDriverManager.IsDriverReady(_driver))
                return;

            LogMessage("BtnLoadServers1 Click", Level.Info);

            try
            {
                List<string> serverList = new List<string>();
                int tbodyIndex = 1;

                while (true)
                {
                    string xpath = $"//*[@id=\'seltable\']/tbody[{tbodyIndex}]/tr/td[4]";
                    IReadOnlyCollection<IWebElement> serverListElements = _driver.FindElements(By.XPath(xpath));

                    if (serverListElements == null || serverListElements.Count == 0) // 더 이상 요소가 없으면 루프 종료
                    {
                        break;
                    }

                    foreach (IWebElement element in serverListElements)
                    {
                        serverList.Add(element.Text);
                    }

                    tbodyIndex++; // tbody 증가 (다음 테이블 이동)
                }

                LogMessage($"서버 이름 리스트:\n{string.Join("\n", serverList)}", Level.Info);

                ComboBoxServerList1.Items.Clear();
                foreach (string serverName in serverList) // 서버 이름 드롭다운 박스 매칭
                {
                    ComboBoxServerList1.Items.Add(serverName);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnConnect1_Click(object sender, EventArgs e)
        {
            // ✅ 테스트 모드일 때는 드라이버 체크 건너뜀
            if (testMode)
            {
                Util_Test.SimulateServerConnect(this, ListViewServer2, ComboBoxServerList1, ref testMode, removeDuplicates);
                Util_ServerList.SaveServerDataToFile(ListViewServer2);
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

            Util_Connect.ConnectToServer(_driver, mainHandle, _config, selectedServer, ListViewServer2, removeDuplicates);
        }


        private void BtnFav1_Click(object sender, EventArgs e)
        {
            Util.ClickFavBtn(_driver, _config, 1, () => BtnLoadServers1_Click(null, EventArgs.Empty), chromeDriverManager);
        }

        private void BtnFav2_Click(object sender, EventArgs e)
        {
            Util.ClickFavBtn(_driver, _config, 2, () => BtnLoadServers1_Click(null, EventArgs.Empty), chromeDriverManager);
        }

        private void BtnTFav3_Click(object sender, EventArgs e)
        {
            Util.ClickFavBtn(_driver, _config, 3, () => BtnLoadServers1_Click(null, EventArgs.Empty), chromeDriverManager);
        }

        // 2025.03.17 Added - Config File Reload Button
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
                    BtnFav1.Text = string.IsNullOrEmpty(_config.Fav1) ? "즐겨찾기 1" : _config.Fav1;
                    BtnFav2.Text = string.IsNullOrEmpty(_config.Fav2) ? "즐겨찾기 2" : _config.Fav2;
                    BtnFav3.Text = string.IsNullOrEmpty(_config.Fav3) ? "즐겨찾기 3" : _config.Fav3;
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

        // 2025.03.17 Added - Config File Open Button
        private void BtnOpenConfig1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnOpenConfig1 Click", Level.Info);
            configManager.OpenConfigFile();
        }

        // 2025.03.17 Added - Log File Open Button
        private void BtnOpenLog1_Click(object sender, EventArgs e)
        {
            LogMessage("BtnOpenLog1 Click", Level.Info);
            OpenLogFile();
        }

        private void MainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_driver != null)
            {
                ChromeDriverManager.CloseDriver(_driver);
                _driver = null;  // 드라이버 객체 해제
            }
            // 프로그램 완전 종료'
            LogMessage("프로그램 종료", Level.Info);
            Environment.Exit(0);
        }


        //////////////////////////////////////////////////////////////////////////////// 옵션 전용 시작

        // 25.08.19 Added - Remove Duplicate Server
        private void CBox_RemoveDuplicate_CheckedChanged(object sender, EventArgs e)
        {
            removeDuplicates = CBox_RemoveDuplicate.Checked;
            string status = removeDuplicates ? "Enabled" : "Disabled";
            LogMessage($"Remove Duplicate Option: {status}", Level.Info);
        }

        private void CBox_FavOneClickConnect1_CheckedChanged(object sender, EventArgs e)
        {
            string status = CBox_FavOneClickConnect1.Checked ? "Enabled" : "Disabled";
            LogMessage($"Favorite One-Click Connect Option: {status}", Level.Info);
        }

        // 25.08.18 Added - Disable Popup/Modal
        private void CBox_DisablePopup1_CheckedChanged(object sender, EventArgs e)
        {
            disablePopup = CBox_DisablePopup1.Checked;
            string status = disablePopup ? "Enabled" : "Disabled";
            LogMessage($"Popup Detection Option: {status}", Level.Info);
        }

        
        // 25.03.19 Added - Test Mode Functions
        private void CBox_TestMode1_CheckedChanged(object sender, EventArgs e)
        {
            string status = CBox_TestMode1.Checked ? "Enabled" : "Disabled";
            LogMessage($"Test Mode Option: {status}", Level.Info);

            if (CBox_TestMode1.Checked)
            {
                Util_Test.EnterTestMode(this, TabSelector1, ref testMode);

                if (testMode)
                {
                    
                    LogMessage("Test Mode 진입", Level.Info);
                    Util_Test.LoadTestServers(ComboBoxServerList1);
                }
                else
                {
                    CBox_TestMode1.Checked = false;
                }
            }
            else
            {
                // this.Size = FormOriginalSize;
                ComboBoxServerList1.Items.Clear();
                testMode = false;
            }
        }

        private void CBox_ListViewClickConnect_CheckedChanged(object sender, EventArgs e)
        {
            string status = CBox_ListViewClickConnect.Checked ? "Enabled" : "Disabled";
            LogMessage($"ListView Click Connect Option: {status}", Level.Info);
        }

        // 25.03.27 Added - Double Click Connect
        private void ListViewServer2_DoubleClick(object sender, EventArgs e)
        {
            if (!CBox_ListViewClickConnect.Checked || ListViewServer2.SelectedItems.Count == 0)
                return;

            string serverName = ListViewServer2.SelectedItems[0].SubItems[1].Text;

            LogMessage("ListView 더블클릭 접속 시도: " + serverName, Level.Info);

            // 검색 실행
            SearchTxt1.Text = serverName;
            BtnSearch1_Click(null, null);

            // 검색 결과 로딩 대기 (서버 리스트 안에 해당 서버 이름이 뜰 때까지)
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

            // 접속 시도
            Util_Connect.ConnectToServer(_driver, mainHandle, _config, serverName, ListViewServer2, removeDuplicates);
        }

        private void CBox_AutoLogin1_CheckedChanged(object sender, EventArgs e)
        {
            // AutoLogin CheckBox가 켜져있다면..
            // StartBtn을 눌렀을 때 추가로 진행한다.
            // 진행중 로딩패널 확인..
            string status = CBox_AutoLogin1.Checked ? "Enabled" : "Disabled";
            LogMessage($"Auto Login Option: {status}", Level.Info);
        }


        //////////////////////////////////////////////////////////////////////////////// 옵션 전용 끝

        private bool isDarkMode = true;
        private bool changeArrow = true;

        private void PicBox_Setting_Click(object sender, EventArgs e)
        {
            if (isDarkMode)
            {
                materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
                PicBox_Setting.Image = Properties.Resources.moon;
                isDarkMode = false;
            }
            else
            {
                materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
                PicBox_Setting.Image = Properties.Resources.sun;
                isDarkMode = true;
            }
        }

        private void PicBox_Arrow_Click(object sender, EventArgs e)
        {
            if (changeArrow)
            {
                PicBox_Arrow.Image = Properties.Resources.arrow_left;
                this.Size = FormExtendedSize;

                // 탭 컨트롤 크기를 기준으로 그룹 박스 및 PictureBox 크기 계산
                TabSelector1.Size = new Size(520, 30);
                GroupConnect1.Size = new Size(TabControl1.Width - 10, TabControl1.Height - 10);

                changeArrow = false;

                // PictureBox 아이콘 A, B, C 위치 변경
                Util_Control.MovePictureBoxIcons(this, PicBox_Arrow, PicBox_Setting, PicBox_Question, FormOriginalSize, true);
            }
            else
            {
                PicBox_Arrow.Image = Properties.Resources.arrow_right;
                this.Size = FormOriginalSize;
                TabSelector1.Size = tabSelector1OriginalSize;
                GroupConnect1.Size = groupConnect1OriginalSize;

                changeArrow = true;

                // PictureBox 아이콘 A, B, C 위치 복원
                Util_Control.MovePictureBoxIcons(this, PicBox_Arrow, PicBox_Setting, PicBox_Question, FormOriginalSize, false);
            }
        }

        private void MainUI_Load(object sender, EventArgs e)
        {
            groupConnect1OriginalSize = GroupConnect1.Size;
            tabSelector1OriginalSize = TabSelector1.Size;
            tabControl1OriginalSize = TabControl1.Size;

            Util_ImageLoader.EnsureReferenceImagesFolderExists(); // ReferenceImages Folder Check
            Util_ImageLoader.LoadReferenceImages(flowLayoutPanel1); // Images Load

            Util_ServerList.LoadServerDataFromFile(ListViewServer2); // ServerData Load
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

        private void TestBtn1_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(5000);
            // Util_Element.FindAndAlertAlert(_driver);
            Util_Element.FindAllXPaths(_driver);
        }

        private void MenuItem1_Click(object sender, EventArgs e)
        {
            if (ListViewServer2.SelectedItems.Count > 0)
            {
                // 선택된 항목을 가져옵니다.
                ListViewItem selectedItem = ListViewServer2.SelectedItems[0];
                string serverName = selectedItem.SubItems[1].Text;

                DialogResult dialogResult = MessageBox.Show($"'{serverName}' 항목을 삭제하시겠습니까?", "항목 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    ListViewServer2.Items.Remove(selectedItem); // 삭제

                    Util_ServerList.ReorderListViewItems(ListViewServer2); // 재정렬
                    Util_ServerList.SaveServerDataToFile(ListViewServer2); // 저장

                    LogMessage($"Server entry '{serverName}' has been removed from the list.", Level.Info);
                }
            }
        }

        private void ApplyThemeToContextMenuStrip() // 테마 색상 변경에 따른 컨텍스트메뉴 색상 변경
        {
            if (materialSkinManager.Theme == MaterialSkinManager.Themes.DARK)
            {
                ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new MaterialToolStripColorTable());
                contextMenuStrip.ForeColor = Color.White;
            }
            else
            {
                ToolStripManager.Renderer = null;
                contextMenuStrip.ForeColor = Color.Black;
            }
        }

        public class MaterialToolStripColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => ColorTranslator.FromHtml("#424242");
            public override Color MenuItemSelectedGradientBegin => ColorTranslator.FromHtml("#424242");
            public override Color MenuItemSelectedGradientEnd => ColorTranslator.FromHtml("#424242");
            public override Color MenuItemPressedGradientBegin => ColorTranslator.FromHtml("#212121");
            public override Color MenuItemPressedGradientEnd => ColorTranslator.FromHtml("#212121");
            public override Color MenuItemBorder => ColorTranslator.FromHtml("#424242");
            public override Color ToolStripDropDownBackground => ColorTranslator.FromHtml("#212121");
            public override Color ToolStripBorder => ColorTranslator.FromHtml("#424242");
        }
    }
}