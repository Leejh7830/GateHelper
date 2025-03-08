using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenQA.Selenium.Chrome;

namespace GateBot
{
    public partial class MainUI : MaterialForm
    {
        

        private readonly MaterialSkinManager materialSkinManager;
        public static IWebDriver _driver = null;

        private Config _config;

        private string serverName;
        private string serverIP;

        private string mainHandle;
        private readonly Timer timer1;

        /// Option
        private bool disablePopup;

        public MainUI()
        {
            InitializeComponent();
            

            // 폼 닫기 이벤트 연결
            this.FormClosing += MainUI_FormClosing;

            // Material SKIN
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue700, Accent.LightBlue200, TextShade.WHITE);

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(500, 700);

            Util_Control.MoveControl(TabSelector1, 150, 30);


            // DisablePopupCheckBox1.Checked = true;

            timer1 = new Timer();
            timer1.Interval = 5000; // 5초마다 팝업 탐색

            timer1.Tick += Timer1_Tick;
            timer1.Start();

        }
        private async void Timer1_Tick(object sender, EventArgs e)
        {
            if (_driver != null && !string.IsNullOrEmpty(mainHandle) && disablePopup)
            {
                try
                {
                    int closedPopupCount = await Util_Option.ControlPopupTimerTick(_driver, mainHandle, _config);
                    Util_Control.UpdateCheckBoxText(closedPopupCount, DisablePopupCheckBox1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        


        private async void StartBtn1_Click(object sender, EventArgs e)
        {
            try
            {
                _config = Util.LoadConfig(); // Config 파일 로드

                _driver = await Task.Run(() => Util.InitializeDriver(_config)); // 비동기로 드라이버 초기화


                _driver.Navigate().GoToUrl(_config.Url); // 사용자가 입력한 사이트로 이동

                mainHandle = Util.FindWindowHandleByUrl(_driver, _config.Url);

                Util_Control.MoveFormToTop(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                Util.ClickElementByXPath(_driver, "/html/body/div/div[2]/button[3]"); // 고급
                Util.ClickElementByXPath(_driver, "/html/body/div/div[3]/p[2]/a"); // 안전하지않음으로이동

                Util.InputKeys("{Tab},SPACE,{Tab},SPACE"); // MPO Helper

                Util_Control.MoveFormToTop(this);



            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


         





        private void MainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 드라이버 종료
            if (_driver != null)
            {
                Util.CloseDriver(_driver);
                _driver = null;  // 드라이버 객체 해제
            }

            // 프로그램 완전 종료
            Environment.Exit(0);
        }

        private void LoginBtn1_Click(object sender, EventArgs e)
        {
            string gateID = GateIDTxt1.Text; // ID 값 가져오기
            string gatePW = GatePWTxt1.Text; // PW 값 가져오기

            // Util.FocusMainWindow(MainHandle);

            // iframe으로 이동
            Util.SendKeysToElement(_driver, "//*[@id='USERID']", gateID);
            Util.SendKeysToElement(_driver, "//*[@id='PASSWD']", gatePW);

            Util.ClickElementByXPath(_driver, "//*[@id='login_submit']");
        }

        private void TestBtn1_Click(object sender, EventArgs e)
        {
            Util.FocusMainWindow(mainHandle);
            Util.InvestigateIframesAndCollectClickableElements(_driver);
        }

        private void SearchBtn1_Click(object sender, EventArgs e)
        {
            Util.ValidateServerInfo(SearchTxt1.Text, out serverName, out serverIP);

            if (!string.IsNullOrEmpty(serverIP))
            {
                // IP 주소인 경우
                try
                {
                    Util.SendKeysToElement(_driver, "//*[@id='id_IPADDR']", serverIP);
                    Util.SendKeysToElement(_driver, "//*[@id='id_DEVNAME']", "");
                }
                catch (NoSuchElementException ex)
                {
                    MessageBox.Show($"SERVER IP XPath에 해당하는 요소를 찾을 수 없습니다: {ex.Message}");
                }
            }
            else if (!string.IsNullOrEmpty(serverName))
            {
                // 서버 이름인 경우
                try
                {
                    Util.SendKeysToElement(_driver, "//*[@id='id_DEVNAME']", serverName);
                    Util.SendKeysToElement(_driver, "//*[@id='id_IPADDR']", "");
                }
                catch (NoSuchElementException ex)
                {
                    MessageBox.Show($"SERVER NAME XPath에 해당하는 요소를 찾을 수 없습니다: {ex.Message}");
                }
            }
            Util.ClickElementByXPath(_driver, "//*[@id='access_control']/table/tbody/tr[2]/td/a");
        }

        private void DisablePopupCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            disablePopup = DisablePopupCheckBox1.Checked;
        }
    }
}