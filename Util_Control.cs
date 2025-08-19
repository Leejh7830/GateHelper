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
using static GateHelper.Util_Element;

namespace GateHelper
{
    internal class Util_Control
    {
        // LoadingPanel Instance
        private static PictureBox _spinnerPicture = null;

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

        /// <summary>
        /// 아래는 점검 필요
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="mode"></param>
        /// <param name="isDarkMode"></param>

        // 25.04.10 Added
        public static void ToggleSpinner(Control parent, string mode, bool isDarkMode)
        {
            if (mode.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                if (_spinnerPicture == null)
                {
                    _spinnerPicture = CreateSpinnerPictureBox(parent, isDarkMode); // isDarkMode에 따라 배경색 결정
                    parent.Controls.Add(_spinnerPicture);
                    _spinnerPicture.BringToFront();
                    // 단, DisableInteractiveControls는 spinner는 제외해서 계속 애니메이션되게 처리
                    DisableInteractiveControls(parent);
                }
            }
            else if (mode.Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                if (_spinnerPicture != null)
                {
                    parent.Controls.Remove(_spinnerPicture);
                    _spinnerPicture.Dispose();
                    _spinnerPicture = null;
                    EnableInteractiveControls(parent);
                }
            }
            else
            {
                throw new ArgumentException("mode 파라미터는 'on' 또는 'off'여야 합니다.");
            }
        }

        public static PictureBox CreateSpinnerPictureBox(Control parent, bool DarkMode)
        {
            // TransparentPictureBox를 사용하면 true한 투명 효과를 기대할 수 있음
            TransparentPictureBox spinner = new TransparentPictureBox();
            string imagePath = Path.Combine(Application.StartupPath, "Resources", "Spinner.gif");
            if (System.IO.File.Exists(imagePath))
            {
                spinner.Image = new Bitmap(imagePath); // 새 Bitmap 객체로 로드
            }
            else
            {
                MessageBox.Show("Spinner.gif 파일을 찾을 수 없습니다.\n경로: " + imagePath,
                                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            spinner.SizeMode = PictureBoxSizeMode.Zoom;
            spinner.Size = new Size(100, 100);
            spinner.Location = new Point(
                (parent.ClientSize.Width - spinner.Width) / 2,
                (parent.ClientSize.Height - spinner.Height) / 2);
            // 배경색은 Transparent로 설정 (이제 사용자 정의 컨트롤이 제대로 작동하면 누끼가 보임)
            spinner.BackColor = System.Drawing.Color.Transparent;
            return spinner;
        }

        // 컨트롤 비활성화
        public static void DisableInteractiveControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                // spinner PictureBox는 건너뛰기
                if (control == _spinnerPicture) continue;

                control.Enabled = false;
                if (control.Controls.Count > 0)
                {
                    DisableInteractiveControls(control);
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
