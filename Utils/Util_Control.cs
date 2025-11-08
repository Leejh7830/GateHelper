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

        // Preset 버튼 상태(색상 + 활성/비활성) 적용
        public static void ApplyPresetSelection(Control btnA, Control btnB, bool isASelected, bool isBSelected)
        {
            // 기본 리셋
            try
            {
                btnA.Enabled = true;
                btnB.Enabled = true;
                btnA.BackColor = SystemColors.Control;
                btnB.BackColor = SystemColors.Control;
                btnA.ForeColor = SystemColors.ControlText;
                btnB.ForeColor = SystemColors.ControlText;
            }
            catch { }

            // 선택된 버튼에 하이라이트(녹색) 및 비활성화 표시
            if (isASelected)
            {
                try { btnA.BackColor = ColorTranslator.FromHtml("#4CAF50"); btnA.ForeColor = Color.White; } catch { }
                btnA.Enabled = false;
            }
            else if (isBSelected)
            {
                try { btnB.BackColor = ColorTranslator.FromHtml("#4CAF50"); btnB.ForeColor = Color.White; } catch { }
                btnB.Enabled = false;
            }
        }
    }
}
