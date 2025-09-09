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
        private readonly MaterialSkinManager materialSkinManager;

        // ⭐ 외부로 노출할 속성: MainUI가 이 속성을 통해 변경된 설정을 가져갑니다.
        public AppSettings AppSettings { get; private set; }
        public bool IsDarkModeEnabled { get; private set; }


        // ⭐ AppSettings 객체와 isDarkMode 상태를 인자로 받는 새로운 생성자
        public OptionForm(AppSettings appSettings, bool isDarkMode)
        {
            InitializeComponent();

            this.AppSettings = appSettings;
            this.IsDarkModeEnabled = isDarkMode;

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = isDarkMode ?
                                        MaterialSkinManager.Themes.DARK :
                                        MaterialSkinManager.Themes.LIGHT;
        }

        private void OptionForm_Load(object sender, EventArgs e)
        {
            CBox_RemoveDuplicate.Checked = AppSettings.RemoveDuplicates;
            CBox_AutoLogin.Checked = AppSettings.AutoLogin;
            CBox_DisablePopup.Checked = AppSettings.DisablePopup;
            CBox_TestMode.Checked = AppSettings.TestMode;
            CBox_ServerClickConnect.Checked = AppSettings.ServerClickConnect;

            Focus();
        }



        private void BtnOK_Click(object sender, EventArgs e)
        {
            // ⭐ UI의 변경사항을 AppSettings 객체에 다시 저장
            AppSettings.RemoveDuplicates = CBox_RemoveDuplicate.Checked;
            AppSettings.AutoLogin = CBox_AutoLogin.Checked;
            AppSettings.DisablePopup = CBox_DisablePopup.Checked;
            AppSettings.TestMode = CBox_TestMode.Checked;
            AppSettings.ServerClickConnect = CBox_ServerClickConnect.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ComboBoxGraceMs1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}