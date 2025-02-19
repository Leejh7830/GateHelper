using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Drawing;
using System.Windows.Forms;

namespace GateBot
{
    public partial class MainUI : MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;
        public static IWebDriver driver = null;

        public MainUI()
        {
            InitializeComponent();
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue700, Accent.LightBlue200, TextShade.WHITE);
            this.Size = new Size(400, 600);

            // 설정 파일 체크 및 로드
            Util.LoadConfig();
        }

        

        private async void StartBtn1_Click(object sender, EventArgs e)
        {
            try
            {
                // 비동기로 드라이버 초기화
                driver = await Task.Run(() => Util.InitializeDriver());

                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 드라이버 종료
                if (driver != null)
                {
                    Util.CloseDriver(driver);
                }
            }
        }
    }
}
