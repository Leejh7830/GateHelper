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
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Configuration;
using System.IO;

namespace GateBot
{
    public partial class MainUI : MaterialForm
    {
        

        private readonly MaterialSkinManager materialSkinManager;
        public static IWebDriver _driver = null;

        private Config _config;
        readonly ConfigManager configManager = new ConfigManager();

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
            this.Size = new Size(400, 700);

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




        private async void BtnStart1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("BtnStart1 Click", Level.Info);
            try
            {
                _driver = await Task.Run(() => Util.InitializeDriver(_config)); // 비동기로 드라이버 초기화


                _driver.Navigate().GoToUrl(_config.Url); // 사용자가 입력한 사이트로 이동

                mainHandle = Util.FindWindowHandleByUrl(_driver, _config.Url); // MainHandle 저장

                Util_Control.MoveFormToTop(this);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
            }
        }

        private void BtnStart2_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("BtnStart2 Click", Level.Info);
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
            LogManager.LogMessage("SearchBtn Click", Level.Info);
            try
            {
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



        private void BtnLoadServers1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("BtnLoadServers Click", Level.Info);

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
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void BtnConnect1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("BtnConnect1 Click", Level.Info);

            mainHandle = _driver.CurrentWindowHandle;

            try
            {
                // 선택된 드롭다운 항목 확인
                if (ComboBoxServerList1.SelectedItem == null)
                {
                    MessageBox.Show("서버를 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 선택된 서버 이름 가져오기
                string selectedServer = ComboBoxServerList1.SelectedItem.ToString();
                LogManager.LogMessage("접속 서버 명 :" + selectedServer, Level.Info);

                // 선택된 서버에 해당하는 "rdp" 문자열을 포함하는 span 태그 클릭
                int tbodyIndex = 1;

                while (true)
                {
                    string serverNameXpath = $"//*[@id=\'seltable\']/tbody[{tbodyIndex}]/tr/td[4]";
                    IReadOnlyCollection<IWebElement> serverNameElements = _driver.FindElements(By.XPath(serverNameXpath));

                    if (serverNameElements == null || serverNameElements.Count == 0)
                    {
                        break; // 더 이상 요소가 없으면 루프 종료
                    }

                    foreach (IWebElement element in serverNameElements)
                    {
                        if (element.Text == selectedServer)
                        {
                            // 해당 서버를 찾았으므로 "rdp" 문자열을 포함하는 span 태그 클릭
                            string spanXpath = $"//*[@id=\'seltable\']/tbody[{tbodyIndex}]/tr/td[5]/span[contains(@id, 'rdp')]";
                            IWebElement spanElement = _driver.FindElement(By.XPath(spanXpath));
                            IWebElement aElement = spanElement.FindElement(By.TagName("a")); // span 태그 안의 a 태그 찾기
                            aElement.Click();

                            System.Threading.Thread.Sleep(1000);

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
                            EnterCredentials(_config.GateID, _config.GatePW);

                            Util.FocusMainWindow(mainHandle);

                            return; // 서버를 찾았으므로 메서드 종료
                        }
                    }

                    tbodyIndex++; // 다음 tbody로 이동
                }

                MessageBox.Show($"서버 '{selectedServer}'를 찾을 수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnterCredentials(string gataID, string gatePW)
        {
            try
            {
                // 새 팝업창으로 전환
                SwitchToPopup();

                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10)); // 최대 10초 대기

                // ID 입력
                IWebElement idInput = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='userid']")));
                idInput.SendKeys(gataID);

                // 비밀번호 입력
                IWebElement pwInput = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='passwd']")));
                pwInput.SendKeys(gatePW);

                // 접속하기 버튼 클릭
                IWebElement loginButton = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='pop_container']/div[2]/a")));
                loginButton.Click();
            }
            catch (WebDriverTimeoutException)
            {
                MessageBox.Show("ID/PW 입력 필드 또는 접속하기 버튼을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"ID/PW 입력 및 접속 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SwitchToPopup()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10)); // 최대 10초 대기
                wait.Until(driver => driver.WindowHandles.Count > 1); // 창 핸들 수가 2개 이상이 될 때까지 대기

                string originalWindow = _driver.CurrentWindowHandle; // 원래 창 핸들 저장
                foreach (string windowHandle in _driver.WindowHandles)
                {
                    if (windowHandle != originalWindow)
                    {
                        _driver.SwitchTo().Window(windowHandle); // 새 창으로 전환
                        break;
                    }
                }
            }
            catch (WebDriverTimeoutException)
            {
                MessageBox.Show("팝업창이 열리지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 필요한 경우 원래 창으로 전환
                // _driver.SwitchTo().Window(originalWindow);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"팝업창 전환 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void BtnClient1_Click(object sender, EventArgs e)
        {
            try
            {
                LogManager.LogMessage("BtnClient1 Click", Level.Info);
                Util.ValidateServerInfo(_config.Favorite1, out serverName, out serverIP);

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

        private void BtnAP1_Click(object sender, EventArgs e)
        {
            try
            {
                LogManager.LogMessage("BtnAP1 Click", Level.Info);
                Util.ValidateServerInfo("MIL CIM AP - CNVC", out serverName, out serverIP);

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

        private void BtnTMP1_Click(object sender, EventArgs e)
        {
            try
            {
                LogManager.LogMessage("BtnTMP1 Click", Level.Info);
                Util.ValidateServerInfo("MIL TMP", out serverName, out serverIP);

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

        void BtnConfig1_Click(object sender, EventArgs e)
        {
            LogManager.LogMessage("BtnConfig1 Click", Level.Info);
            try
            {
                _config = configManager.LoadedConfig;

                if (_config != null)
                {
                    LogManager.LogMessage("Config Load", Level.Info);

                    // 즐겨찾기 버튼 텍스트 설정 (null 또는 빈 문자열 처리)
                    BtnFav1.Text = string.IsNullOrEmpty(_config.Favorite1) ? "즐겨찾기 1" : _config.Favorite1;
                    BtnFav2.Text = string.IsNullOrEmpty(_config.Favorite2) ? "즐겨찾기 2" : _config.Favorite2;
                    BtnFav3.Text = string.IsNullOrEmpty(_config.Favorite3) ? "즐겨찾기 3" : _config.Favorite3;

                    // UI 업데이트 (예시)
                    // textBoxUrl.Text = _config.Url;
                    // ...
                }
                else
                {
                    LogManager.LogMessage("Fail Config Load", Level.Info);
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"설정 파일 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FileNotFoundException ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"설정 파일을 찾을 수 없습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                MessageBox.Show($"설정 로드 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}