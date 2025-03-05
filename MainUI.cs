using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GateBot
{
    public partial class MainUI : MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;
        public static IWebDriver _driver = null;
        private Config _config;

        // private IntPtr mainChromeHandle; // 크롬 창 핸들 저장
        private string MainHandle;

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
            this.Size = new Size(400, 600);

        }



        private async void StartBtn1_Click(object sender, EventArgs e)
        {
            try
            {
                // Config 파일 로드
                _config = Util.LoadConfig();

                // 비동기로 드라이버 초기화
                _driver = await Task.Run(() => Util.InitializeDriver(_config));

                // 사용자가 입력한 사이트로 이동
                _driver.Navigate().GoToUrl(_config.URL);

                MainHandle = Util.FindWindowHandleByUrl(_driver, _config.URL);

                Util.MoveToTop(this);
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

                Util.MoveToTop(this);



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
            string gateID = GateIDBox1.Text; // ID텍스트 박스 값 가져오기
            string gatePW = GatePWBox1.Text; // PW텍스트 박스 값 가져오기

            // Util.FocusMainWindow(MainHandle);

            // iframe으로 이동
            Util.SendKeysToElement(_driver, "//*[@id='USERID']", gateID);
            Util.SendKeysToElement(_driver, "//*[@id='PASSWD']", gatePW);

            Util.ClickElementByXPath(_driver, "//*[@id='login_submit']");
        }

        private void TestBtn1_Click(object sender, EventArgs e)
        {

        }
    }
}