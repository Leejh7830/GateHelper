using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using MaterialSkin;
using static GateHelper.LogManager;

namespace GateHelper
{
    public partial class OptionForm : MaterialForm
    {
        public bool IsRemoveDuplicatesEnabled { get; set; }
        public bool IsAutoLoginEnabled { get; set; }
        public bool IsPopupDisabled { get; set; }
        public bool IsTestModeEnabled { get; set; }
        public bool IsServerClickEnabled { get; set; }


        public OptionForm(bool currentRemoveDuplicatesStatus, bool currentAutoLoginStatus,
                      bool currentPopupStatus, bool currentTestModeStatus, bool currentServerClickStatus)
        {
            InitializeComponent();

            // public 속성 값으로 UI 컨트롤의 초기 상태 설정
            CBox_RemoveDuplicate.Checked = currentRemoveDuplicatesStatus;
            CBox_AutoLogin.Checked = currentAutoLoginStatus;
            CBox_DisablePopup.Checked = currentPopupStatus;
            CBox_TestMode.Checked = currentTestModeStatus;
            CBox_ServerClickConnect.Checked = currentServerClickStatus;
        }

        private void OptionForm_Load(object sender, EventArgs e)
        {
            Focus();
        }

        private void CBox_RemoveDuplicate_CheckedChanged(object sender, EventArgs e)
        {
            IsRemoveDuplicatesEnabled = CBox_RemoveDuplicate.Checked;
        }

        private void CBox_AutoLogin_CheckedChanged(object sender, EventArgs e)
        {
            IsAutoLoginEnabled = CBox_AutoLogin.Checked;
        }

        private void CBox_DisablePopup_CheckedChanged(object sender, EventArgs e)
        {
            IsPopupDisabled = CBox_DisablePopup.Checked;
        }

        private void CBox_TestMode_CheckedChanged(object sender, EventArgs e)
        {
            IsTestModeEnabled = CBox_TestMode.Checked;
        }

        private void CBox_ServerClick_CheckedChanged(object sender, EventArgs e)
        {
            IsServerClickEnabled = CBox_ServerClickConnect.Checked;
        }
    }
}
