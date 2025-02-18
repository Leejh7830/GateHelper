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

            // 각 사이트를 변수로 할당
            string site1 = "https://naver.com";
            string site2 = "https://google.com";
            string site3 = "https://example.com";

            // 확인할 URL 목록
            string[] targetUrls = { site1, site2, site3 };

            try
            {
                // 열려 있는 브라우저 창 확인
                driver.Navigate().GoToUrl(site1);  // 예시로 site1을 열기

                // 현재 URL 확인
                string currentUrl = driver.Url;

                // 대상 URL 중 하나와 일치하는지 확인
                bool isTargetUrl = false;
                foreach (string url in targetUrls)
                {
                    if (currentUrl.Contains(url))
                    {
                        isTargetUrl = true;
                        break;
                    }
                }

                if (isTargetUrl)
                {
                    Console.WriteLine("열려 있는 URL이 대상 목록에 있습니다!");
                }
                else
                {
                    Console.WriteLine("열려 있는 URL이 대상 목록에 없습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류 발생: {ex.Message}");
            }
            finally
            {
                // 드라이버 종료
                driver.Quit();
            }
        }
    }
}