using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Drawing;
using System.Windows.Forms;
using Level = GateBot.LogManager.Level;
using System.Net.Security;
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace GateBot
{
    public partial class MainUI : MaterialForm
    {
        

        private readonly MaterialSkinManager materialSkinManager;
        public static IWebDriver _driver = null;

        private readonly Config _config;

        private string serverName;
        private string serverIP;

        private string mainHandle;
        private readonly Timer timer1;

        /// Option
        private bool disablePopup;

        public MainUI()
        {
            LogManager.InitializeLogFile();
            LogManager.LogMessage("프로그램 초기화 시작", Level.Info);
            InitializeComponent();

            ConfigManager configManager = new ConfigManager();
            _config = configManager.LoadedConfig;

            // 폼 닫기 이벤트 연결
            this.FormClosing += MainUI_FormClosing;

            // Material SKIN
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue700, Accent.LightBlue200, TextShade.WHITE);

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(700, 700);

            // Util_Control.MoveControl(TabSelector1, 150, 30);

            // DisablePopupCheckBox1.Checked = true;

            timer1 = new Timer();
            timer1.Interval = 5000; // 5초마다 팝업 탐색

            timer1.Tick += Timer1_Tick;
            timer1.Start();

            LogManager.LogMessage("프로그램 초기화 완료", Level.Info);
        }

        private async void Timer1_Tick(object sender, EventArgs e)
        {
            if (_driver != null && !string.IsNullOrEmpty(mainHandle) && _driver.WindowHandles.Contains(mainHandle) && disablePopup)
            {
                try
                {
                    bool alertHandled = await Util_Option.HandleWindows(_driver, mainHandle, _config);
                    if (alertHandled)
                    {
                        Console.WriteLine("경고창 처리 성공");
                        // 경고창 처리 성공 시 추가 작업 수행
                    }
                    else
                    {
                        Console.WriteLine("경고창 처리 실패 또는 없음");
                        // 경고창 처리 실패 시 추가 작업 수행
                    }
                }
                catch (NoSuchElementException ex)
                {
                    LogManager.LogException(ex, Level.Error);
                }
                catch (NoSuchWindowException ex)
                {
                    LogManager.LogException(ex, Level.Error);
                }
                catch (NoAlertPresentException)
                {
                    //
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex, Level.Error);
                }
            }
        }




        private async void StartBtn1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("StartBtn Click", Level.Info);
            try
            {
                _driver = await Task.Run(() => Util.InitializeDriver(_config)); // 비동기로 드라이버 초기화


                _driver.Navigate().GoToUrl(_config.Url); // 사용자가 입력한 사이트로 이동

                mainHandle = Util.FindWindowHandleByUrl(_driver, _config.Url);

                Util_Control.MoveFormToTop(this);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
            }
        }

        private void ConnectBtn1_Click(object sender, EventArgs e)
        {
            try
            {
                if (_driver == null)
                {
                    MessageBox.Show("드라이버가 초기화되지 않았습니다. 먼저 시작 버튼을 눌러주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Util_Control.ClickElementByXPath(_driver, "/html/body/div/div[2]/button[3]"); // 고급
                Util_Control.ClickElementByXPath(_driver, "/html/body/div/div[3]/p[2]/a"); // 안전하지않음으로이동

                Util.InputKeys("{Tab},SPACE,{Tab},SPACE"); // MPO Helper

                Util_Control.MoveFormToTop(this);

            }
            catch (Exception ex)
            {
                // MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogManager.LogException(ex, Level.Error);
            }
        }



        

        private void LoginBtn1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("LoginBtn Click", Level.Info);
            string gateID = GateIDTxt1.Text; // ID 값 가져오기
            string gatePW = GatePWTxt1.Text; // PW 값 가져오기

            // Util.FocusMainWindow(MainHandle);

            // iframe으로 이동
            Util_Control.SendKeysToElement(_driver, "//*[@id='USERID']", gateID);
            Util_Control.SendKeysToElement(_driver, "//*[@id='PASSWD']", gatePW);

            Util_Control.ClickElementByXPath(_driver, "//*[@id='login_submit']");
        }

        private void TestBtn1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("TestBtn Click", Level.Info);
            Util.FocusMainWindow(mainHandle);
            Util.InvestigateIframesAndCollectClickableElements(_driver);
        }

        private void SearchBtn1_Click(object sender, EventArgs e)
        {
            try
            {
                LogManager.LogMessage("SearchBtn Click", Level.Info);
                Util.ValidateServerInfo(SearchTxt1.Text, out serverName, out serverIP);

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
            }
            catch (ArgumentException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show(ex.Message, "알림");
            }
            catch (NoSuchElementException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show("요소를 찾을 수 없습니다.", "오류");
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Critical);
                MessageBox.Show("예상치 못한 오류가 발생했습니다.", "오류");
            }
        }

        private void DisablePopupCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            LogManager.LogMessage("DisablePopupCheckBox CheckedChanged", Level.Info);
            disablePopup = DisablePopupCheckBox1.Checked;
        }



        private void btnLoadServers1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("btnLoadServers Click", Level.Info);
            try
            {
                List<string> serverNames = new List<string>();
                int tbodyIndex = 1;

                while (true)
                {
                    string xpath = $"//*[@id=\'seltable\']/tbody[{tbodyIndex}]/tr/td[4]";
                    IReadOnlyCollection<IWebElement> serverNameElements = _driver.FindElements(By.XPath(xpath));

                    if (serverNameElements == null || serverNameElements.Count == 0) // 더 이상 요소가 없으면 루프 종료
                    {
                        break;
                    }

                    foreach (IWebElement element in serverNameElements)
                    {
                        serverNames.Add(element.Text);
                    }

                    tbodyIndex++; // tbody 증가 (다음 테이블 이동)
                }

                LogManager.LogMessage($"서버 이름 리스트:\n{string.Join("\n", serverNames)}", Level.Info);

                // 서버 이름 리스트와 드롭다운 박스 매칭 및 값 표시
                ComboBoxServerList1.Items.Clear();
                foreach (string serverName in serverNames)
                {
                    ComboBoxServerList1.Items.Add(serverName);
                }

                LogManager.LogMessage($"콤보박스 Items 속성:\n{string.Join("\n", ComboBoxServerList1.Items.OfType<string>())}", Level.Info);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void BtnConnect1_Click(object sender, EventArgs e)
        {
            private void BtnConnect1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("BtnConnect1 Click", Level.Info);
            try
            {
                if (ComboBoxServerList1.SelectedItem == null)
                {
                    MessageBox.Show("서버를 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string selectedServer = ComboBoxServerList1.SelectedItem.ToString();

                // 선택된 서버에 해당하는 테이블의 A xpath 클릭
                // 예시: 테이블의 A 링크를 클릭하는 XPath (실제 XPath로 변경해야 함)
                string xpath = $"//table//tr/td[text()='{selectedServer}']/../td/a"; // 예시 XPath, 수정 필요

                // XPath를 사용하여 요소 찾기
                IWebElement linkElement = _driver.FindElement(By.XPath(xpath));

                // 링크 클릭
                linkElement.Click();

                // 경고창 처리 (확인 또는 스페이스바)
                Thread.Sleep(1000); // 경고창 뜨는 시간 대기 (필요에 따라 조정)

                try
                {
                    // 경고창 확인 버튼 클릭 시도
                    IAlert alert = _driver.SwitchTo().Alert();
                    alert.Accept(); // 확인 버튼 클릭
                }
                catch (NoAlertPresentException)
                {
                    // 경고창이 없는 경우 (또는 확인 버튼을 찾을 수 없는 경우)
                    // 스페이스바 입력
                    SendKeys.SendWait(" ");
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        }




        private void MainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_driver != null)
            {
                Util.CloseDriver(_driver);
                _driver = null;  // 드라이버 객체 해제
            }
            // 프로그램 완전 종료
            LogManager.LogMessage("프로그램 종료", Level.Info);
            Environment.Exit(0);
        }


    }
}