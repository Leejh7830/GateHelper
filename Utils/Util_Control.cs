using OpenQA.Selenium;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
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

        public static void MovePictureBoxIcons(Form form, PictureBox pictureBoxA, PictureBox pictureBoxB, PictureBox pictureBoxC, Control pictureBoxOption, Control btnLogValidator, Control txtQuickSearch, Control btnQuickConnect, Size formOriginalSize, bool isExpanded)
        {
            int iconSpacing = 10;
            int xPos;
            int yPos;

            int formWidth = isExpanded ? form.ClientSize.Width : formOriginalSize.Width;
            int formHeight = isExpanded ? form.ClientSize.Height : formOriginalSize.Height;

            xPos = formWidth - pictureBoxC.Width - iconSpacing;
            yPos = formHeight - pictureBoxC.Height - 10;
            pictureBoxC.Location = new Point(xPos, yPos);

            xPos -= pictureBoxB.Width + iconSpacing;
            pictureBoxB.Location = new Point(xPos, yPos);

            xPos -= pictureBoxA.Width + iconSpacing;
            pictureBoxA.Location = new Point(xPos, yPos);

            pictureBoxOption.Location = new Point(pictureBoxB.Location.X + 10, pictureBoxB.Location.Y - pictureBoxOption.Height - iconSpacing);

            btnLogValidator.Location = new Point(pictureBoxOption.Location.X - btnLogValidator.Width - iconSpacing, pictureBoxOption.Location.Y);

            int quickY = pictureBoxOption.Location.Y - btnQuickConnect.Height - 8;
            btnQuickConnect.Location = new Point(pictureBoxOption.Location.X + pictureBoxOption.Width - btnQuickConnect.Width, quickY);
            txtQuickSearch.Location = new Point(btnQuickConnect.Location.X - txtQuickSearch.Width - iconSpacing, quickY + (btnQuickConnect.Height - txtQuickSearch.Height) / 2);
        }

        public static void FillSearchFields(IWebDriver driver, string serverName, string serverIP)
        {
            string[] fieldsToClear = {
                "//*[@id='id_IPADDR']",
                "//*[@id='id_DEVNAME']",
                "//*[@id='id_HOSTNAME']",
                "//*[@id='id_GNAME']",
                "//*[@id='id_ACCESS_AUTH_GROUP']"
            };

            foreach (var xpath in fieldsToClear)
            {
                SendKeysToElement(driver, xpath, "");
            }

            if (!string.IsNullOrEmpty(serverIP))
            {
                SendKeysToElement(driver, "//*[@id='id_IPADDR']", serverIP);
            }
            else if (!string.IsNullOrEmpty(serverName))
            {
                SendKeysToElement(driver, "//*[@id='id_DEVNAME']", serverName);
            }
        }

        public static void ToggleFormLayout(
            Form form,
            PictureBox arrowPicBox,
            PictureBox settingPicBox,
            PictureBox questionPicBox,
            Control BtnOption1,
            Control BtnLogValidator,
            Control TxtQuickSearch,
            Control BtnQuickConnect,
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

                tabSelector.Size = new Size(tabControlSize.Width - 40, 30);
                groupConnect.Size = new Size(tabControlSize.Width - 10, tabControlSize.Height - 10);

                changeArrow = false;

                MovePictureBoxIcons(form, arrowPicBox, settingPicBox, questionPicBox, BtnOption1, BtnLogValidator, TxtQuickSearch, BtnQuickConnect, formOriginalSize, true);
            }
            else
            {
                arrowPicBox.Image = Properties.Resources.arrow_right;
                form.Size = formOriginalSize;
                tabSelector.Size = tabSelectorOriginalSize;
                groupConnect.Size = groupConnectOriginalSize;

                changeArrow = true;

                MovePictureBoxIcons(form, arrowPicBox, settingPicBox, questionPicBox, BtnOption1, BtnLogValidator, TxtQuickSearch, BtnQuickConnect, formOriginalSize, false);
            }
        }

        public static void ApplyPresetSelection(Control btnA, Control btnB, bool isASelected, bool isBSelected)
        {
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