using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using static GateHelper.LogManager;

namespace GateHelper
{
    internal class Util_Control
    {
        public static void MoveFormToTop(Form form)
        {
            Thread.Sleep(500);
            form.TopMost = true;
            form.Activate();
            form.TopMost = false;
        }

        public static void MoveControl(Control control, int x, int y)
        {
            control.Location = new Point(x, y);
        }

        public static void ClickElementByXPath(IWebDriver driver, string xpath)
        {
            try
            {
                // WebDriverWait을 사용하여 요소가 클릭 가능한 상태로 나타날 때까지 기다리기
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                IWebElement element = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(xpath)));

                // 클릭 가능하면 클릭
                element.Click();
            }
            catch (NoSuchElementException ex)
            {
                MessageBox.Show($"XPath에 해당하는 요소를 찾을 수 없습니다.: {ex.Message}", "실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            catch (WebDriverTimeoutException ex)
            {
                MessageBox.Show($"요소가 클릭 가능한 상태가 되지 않았습니다.: {ex.Message}", "실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // 요소에 값을 입력하는 메서드
        public static void SendKeysToElement(IWebDriver driver, string xpath, string value)
        {
            try
            {
                // WebDriverWait을 사용하여 요소가 나타날 때까지 대기
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

                // 요소가 준비될 때까지 기다림
                var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

                if (element != null)
                {
                    element.Clear(); // 빈 값을 입력하여 초기화
                    element.SendKeys(value); // 값을 입력
                }
                else
                {
                    throw new NoSuchElementException($"{xpath} 요소를 찾을 수 없습니다.");
                }
            }
            catch (NoSuchElementException ex)
            {
                throw new Exception($"요소를 찾을 수 없습니다: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"오류 발생: {ex.Message}");
            }
        }

        public static void MovePictureBoxIcons(Form form, PictureBox pictureBoxA, PictureBox pictureBoxB, PictureBox pictureBoxC, Size formOriginalSize, bool isExpanded)
        {
            int iconSpacing = 10;
            int xPos = 0;

            if (isExpanded)
            {
                // 확장된 폼 크기에 맞춰 PictureBox 아이콘 A, B, C 위치 변경
                xPos = 550 - pictureBoxC.Width - iconSpacing;
                pictureBoxC.Location = new Point(xPos, 700 - pictureBoxC.Height - 10);

                xPos -= pictureBoxB.Width + iconSpacing;
                pictureBoxB.Location = new Point(xPos, 700 - pictureBoxB.Height - 10);

                xPos -= pictureBoxA.Width + iconSpacing;
                pictureBoxA.Location = new Point(xPos, 700 - pictureBoxA.Height - 10);
            }
            else
            {
                // 초기 폼 크기에 맞춰 PictureBox 아이콘 A, B, C 위치 복원
                xPos = formOriginalSize.Width - pictureBoxC.Width - iconSpacing;
                pictureBoxC.Location = new Point(xPos, formOriginalSize.Height - pictureBoxC.Height - 10);

                xPos -= pictureBoxB.Width + iconSpacing;
                pictureBoxB.Location = new Point(xPos, formOriginalSize.Height - pictureBoxB.Height - 10);

                xPos -= pictureBoxA.Width + iconSpacing;
                pictureBoxA.Location = new Point(xPos, formOriginalSize.Height - pictureBoxA.Height - 10);
            }
        }

        public static void FillSearchFields(IWebDriver driver, string serverName, string serverIP)
        {
            if (!string.IsNullOrEmpty(serverIP))
            {
                // 입력: IP 주소
                SendKeysToElement(driver, "//*[@id='id_IPADDR']", serverIP);
                SendKeysToElement(driver, "//*[@id='id_DEVNAME']", "");
            }
            else if (!string.IsNullOrEmpty(serverName))
            {
                // 입력: 서버 이름
                SendKeysToElement(driver, "//*[@id='id_DEVNAME']", serverName);
                SendKeysToElement(driver, "//*[@id='id_IPADDR']", "");
            }
        }

        public static Panel CreateLoadingPanel(Control parent)
        {
            Panel loadingPanel = new Panel();
            loadingPanel.Size = new Size(300, 450);
            // 알파값 128을 사용해 반투명 검정색 배경으로 설정
            loadingPanel.BackColor = Color.FromArgb(128, 0, 0, 0);
            loadingPanel.BorderStyle = BorderStyle.FixedSingle;
            // 부모 컨트롤(Client 영역)의 중앙에 배치
            loadingPanel.Location = new Point(
                (parent.ClientSize.Width - loadingPanel.Width) / 2,
                (parent.ClientSize.Height - loadingPanel.Height) / 2);

            // 2. 스피너 GIF를 표시할 PictureBox 생성
            PictureBox pbSpinner = new PictureBox();
            // Zoom 모드로 설정하면 PictureBox 크기에 맞게 이미지가 축소/확대되어 보임
            pbSpinner.SizeMode = PictureBoxSizeMode.Zoom;
            // 스피너 크기 지정 (원하는 크기로 변경 가능)
            pbSpinner.Size = new Size(80, 80);

            // 실행파일 기준으로 "Resource_TEMP" 폴더 내의 Spinner.gif 파일 경로 구성
            string imagePath = Path.Combine(Application.StartupPath, "Resources", "Spinner.gif");
            if (File.Exists(imagePath))
            {
                pbSpinner.Image = Image.FromFile(imagePath);
            }
            else
            {
                // 이미지 파일이 없으면 오류 메시지 출력
                LogMessage("Spinner.gif 파일을 찾을 수 없습니다.\n경로: " + imagePath, Level.Error);
            }
            // 패널 상단 중앙에 PictureBox 배치 (상단 여백 20px)
            pbSpinner.Location = new Point((loadingPanel.Width - pbSpinner.Width) / 2, 20);

            // 3. 상태 메시지 Label 생성
            Label lblMessage = new Label();
            lblMessage.Text = "잠시만 기다려주세요...";
            lblMessage.ForeColor = Color.White;
            lblMessage.AutoSize = true;
            // Label은 PictureBox 아래에 10px 간격으로 배치
            // AutoSize인 경우 PreferredWidth를 이용해 중앙 배치
            lblMessage.Location = new Point((loadingPanel.Width - lblMessage.PreferredWidth) / 2, pbSpinner.Bottom + 10);

            // 4. 패널에 PictureBox와 Label 추가
            loadingPanel.Controls.Add(pbSpinner);
            loadingPanel.Controls.Add(lblMessage);

            return loadingPanel;
        }

        public static Panel ShowLoadingPanel(Control parent)
        {
            Panel panel = CreateLoadingPanel(parent);
            parent.Controls.Add(panel);
            panel.BringToFront();
            return panel;
        }

        public static void HideLoadingPanel(Control parent, Panel panel)
        {
            if (panel != null)
            {
                parent.Controls.Remove(panel);
                panel.Dispose();
            }
        }


    }
}
