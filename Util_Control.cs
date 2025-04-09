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
        // LoadingPanel Instance
        private static Panel _messagePanel = null;

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



        public static void ToggleMessagePanel(Control parent, string mode)
        {
            if (mode.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                if (_messagePanel == null)
                {
                    _messagePanel = CreateMessagePanel(parent); // 패널 생성
                    parent.Controls.Add(_messagePanel);           // 패널 추가
                    _messagePanel.BringToFront();                   // 최상단으로 이동
                    DisableInteractiveControls(parent);           // 버튼 등 비활성화
                }
            }
            else if (mode.Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                if (_messagePanel != null)
                {
                    parent.Controls.Remove(_messagePanel);        // 패널 제거
                    _messagePanel.Dispose();                      // 자원 해제
                    _messagePanel = null;
                    EnableInteractiveControls(parent);            // 버튼 등 활성화
                }
            }
            else
            {
                throw new ArgumentException("mode 파라미터는 'on' 또는 'off'여야 합니다.");
            }
        }

        public static Panel CreateMessagePanel(Control parent)
        {
            Panel messagePanel = new Panel();
            messagePanel.Size = new Size(300, 150);
            // 어두운 배경 (ARGB: alpha 220)
            messagePanel.BackColor = Color.FromArgb(220, 0, 0, 0);
            messagePanel.BorderStyle = BorderStyle.FixedSingle;
            messagePanel.Location = new Point(
                (parent.ClientSize.Width - messagePanel.Width) / 2,
                (parent.ClientSize.Height - messagePanel.Height) / 2);

            Label lblMessage = new Label();
            lblMessage.Text = "잠시만 기다려주세요...";
            lblMessage.ForeColor = Color.White;
            lblMessage.Font = new Font("Segoe UI", 24, FontStyle.Bold);

            lblMessage.AutoSize = false;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.Dock = DockStyle.Fill;

            messagePanel.Controls.Add(lblMessage);
            return messagePanel;
        }

        // 컨트롤 비활성화
        public static void DisableInteractiveControls(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button)
                {
                    ctrl.Enabled = false;
                }
            }
        }

        // 컨트롤 활성화
        public static void EnableInteractiveControls(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button)
                {
                    ctrl.Enabled = true;
                }
            }
        }

        


    }
}
