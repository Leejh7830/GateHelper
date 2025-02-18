using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Drawing;
using MaterialSkin;
using MaterialSkin.Controls;

namespace GateBot
{
    public partial class MainUI : MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;

        public MainUI()
        {
            InitializeComponent();
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;

            // 색상 변경
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue700, Accent.LightBlue200, TextShade.WHITE);

            // 폼의 크기를 600 x 900으로 설정
            this.Size = new Size(400, 600);
        }

        private void startBtn1_Click(object sender, EventArgs e)
        {
            // ChromeDriver 경로 지정
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            IWebDriver driver = new ChromeDriver(chromeDriverService);

            // 특정 사이트로 이동
            driver.Navigate().GoToUrl("https://naver.com");

            // 필요한 추가 작업을 여기에 작성
        }
    }
}