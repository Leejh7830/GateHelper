using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System;
using System.Drawing;
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



        public static void MovePictureBoxIcons(Form form, PictureBox pictureBoxA, PictureBox pictureBoxB, PictureBox pictureBoxC, Control pictureBoxOption, Size formOriginalSize, bool isExpanded)
        {
            int iconSpacing = 10;
            int xPos;
            int yPos;

            if (isExpanded)
            {
                int formWidth = 550;
                int formHeight = 700;

                // C, B, A 순서로 오른쪽에서 왼쪽으로 정렬
                xPos = formWidth - pictureBoxC.Width - iconSpacing;
                yPos = formHeight - pictureBoxC.Height - 10;
                pictureBoxC.Location = new Point(xPos, yPos);

                xPos -= pictureBoxB.Width + iconSpacing;
                pictureBoxB.Location = new Point(xPos, yPos);

                xPos -= pictureBoxA.Width + iconSpacing;
                pictureBoxA.Location = new Point(xPos, yPos);

                // ⭐ B 아이콘 위에 Option 버튼을 정렬
                pictureBoxOption.Location = new Point(pictureBoxB.Location.X + 10, pictureBoxB.Location.Y - pictureBoxOption.Height - iconSpacing);
            }
            else
            {
                xPos = formOriginalSize.Width - pictureBoxC.Width - iconSpacing;
                yPos = formOriginalSize.Height - pictureBoxC.Height - 10;
                pictureBoxC.Location = new Point(xPos, yPos);

                xPos -= pictureBoxB.Width + iconSpacing;
                pictureBoxB.Location = new Point(xPos, yPos);

                xPos -= pictureBoxA.Width + iconSpacing;
                pictureBoxA.Location = new Point(xPos, yPos);

                // ⭐ B 아이콘 위에 Option 버튼을 정렬
                pictureBoxOption.Location = new Point(pictureBoxB.Location.X + 10, pictureBoxB.Location.Y - pictureBoxOption.Height - iconSpacing);
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

        public static void ToggleFormLayout(
            Form form,
            PictureBox arrowPicBox,
            PictureBox settingPicBox,
            PictureBox questionPicBox,
            Control BtnOption1,
            Size formOriginalSize,
            Size formExtendedSize,
            Control tabSelector,
            Size tabSelectorOriginalSize,
            Control groupConnect,
            Size groupConnectOriginalSize,
            Size tabControlSize,
            ref bool changeArrow)
        {
            if (changeArrow)
            {
                arrowPicBox.Image = Properties.Resources.arrow_left;
                form.Size = formExtendedSize;

                // 탭 컨트롤 크기를 기준으로 그룹 박스 및 TabSelector 크기 계산
                tabSelector.Size = new Size(tabControlSize.Width - 40, 30);
                groupConnect.Size = new Size(tabControlSize.Width - 10, tabControlSize.Height - 10);

                changeArrow = false;

                // PictureBox 아이콘 위치 변경
                MovePictureBoxIcons(form, arrowPicBox, settingPicBox, questionPicBox, BtnOption1, formOriginalSize, true);
            }
            else
            {
                arrowPicBox.Image = Properties.Resources.arrow_right;
                form.Size = formOriginalSize;
                tabSelector.Size = tabSelectorOriginalSize;
                groupConnect.Size = groupConnectOriginalSize;

                changeArrow = true;

                // PictureBox 아이콘 위치 복원
                MovePictureBoxIcons(form, arrowPicBox, settingPicBox, questionPicBox, BtnOption1, formOriginalSize, false);
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
