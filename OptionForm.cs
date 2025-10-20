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

        // UI 보조 요소
        private MaterialLabel _descriptionLabel;
        private Dictionary<Control, string> _descriptions;

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

            InitializeTooltipsAndDescription();

            // 폼 레벨 마우스 이벤트: disabled 컨트롤은 자신이 마우스 이벤트를 받지 않으므로 폼에서 검사
            this.MouseMove += OptionForm_MouseMove;
            this.MouseLeave += (s, e) =>
            {
                if (_descriptionLabel != null) _descriptionLabel.Text = string.Empty;
            };
        }

        private void InitializeTooltipsAndDescription()
        {
            _descriptions = new Dictionary<Control, string>
            {
                { CBox_RemoveDuplicate, "서버 목록에서 중복항목을 제거합니다." },
                { CBox_ServerClickConnect, "서버 항목 더블클릭 시 해당 서버로 바로 연결합니다." },
                { CBox_DisablePopup, "팝업을 닫습니다.\n30분 간격으로 팝업되는 비밀번호창 자동 입력." },
                { CBox_TestMode, "테스트 모드" },
                { CBox_AutoLogin, "Config 정보로 자동 로그인합니다. 미구현" },
                { CBox_FavOneClickConnect, "즐겨찾기 클릭 시 해당 서버로 바로 연결합니다. 미구현" }
            };

            // 설명 라벨을 동적으로 생성하여 폼 하단에 표시 (한 번만)
            if (_descriptionLabel == null)
            {
                _descriptionLabel = new MaterialLabel
                {
                    AutoSize = false,
                    Size = new Size(this.ClientSize.Width - 34, 36),
                    Location = new Point(17, this.ClientSize.Height - 80),
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                    Font = new Font("Roboto", 11F, FontStyle.Regular), // 폰트조절안됨
                    Text = string.Empty
                };
                this.Controls.Add(_descriptionLabel);
            }

            // 중앙 업데이트 함수를 모든 대상에 연결: Enabled 컨트롤은 자체 이벤트로, Disabled는 폼 MouseMove로 처리됨
            foreach (var kvp in _descriptions)
            {
                var ctrl = kvp.Key;
                var desc = kvp.Value;

                // 컨트롤 레벨에서 마우스 이동/진입을 감지하면 화면 좌표로 중앙 함수 호출
                ctrl.MouseMove += (s, e) =>
                {
                    UpdateDescriptionFromScreenPoint(((Control)s).PointToScreen(e.Location));
                };
                ctrl.MouseEnter += (s, e) =>
                {
                    UpdateDescriptionFromScreenPoint(Cursor.Position);
                };
                ctrl.MouseLeave += (s, e) =>
                {
                    if (_descriptionLabel != null) _descriptionLabel.Text = string.Empty;
                };

                // 키보드 포커스는 직접 설명 표시(접근성)
                ctrl.GotFocus += (s, e) => { if (_descriptionLabel != null) _descriptionLabel.Text = desc; };
                ctrl.LostFocus += (s, e) => { if (_descriptionLabel != null) _descriptionLabel.Text = string.Empty; };
            }
        }

        // 중앙 처리 함수 추가
        private void UpdateDescriptionFromScreenPoint(Point screenPt)
        {
            if (_descriptionLabel == null || _descriptions == null) return;

            try
            {
                string desc = null;

                // Controls 역순으로 검사 (Z-order 상 위에 있는 컨트롤 우선)
                for (int i = this.Controls.Count - 1; i >= 0; i--)
                {
                    var ctrl = this.Controls[i];
                    if (ctrl == _descriptionLabel) continue;

                    Rectangle rect = ctrl.RectangleToScreen(ctrl.ClientRectangle);
                    if (!rect.Contains(screenPt)) continue;

                    // 해당 컨트롤 또는 부모 체인에서 설명을 찾음
                    Control current = ctrl;
                    while (current != null && current != this)
                    {
                        if (_descriptions.TryGetValue(current, out desc)) break;
                        current = current.Parent;
                    }

                    if (desc != null) break;
                }

                _descriptionLabel.Text = desc ?? string.Empty;
            }
            catch
            {
                _descriptionLabel.Text = string.Empty;
            }
        }

        private void OptionForm_Load(object sender, EventArgs e)
        {
            if (AppSettings != null)
            {
                CBox_RemoveDuplicate.Checked = AppSettings.RemoveDuplicates;
                CBox_AutoLogin.Checked = AppSettings.AutoLogin;
                CBox_DisablePopup.Checked = AppSettings.DisablePopup;
                CBox_TestMode.Checked = AppSettings.TestMode;
                CBox_ServerClickConnect.Checked = AppSettings.ServerClickConnect;
            }

            Focus();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (AppSettings != null)
            {
                // ⭐ UI의 변경사항을 AppSettings 객체에 다시 저장
                AppSettings.RemoveDuplicates = CBox_RemoveDuplicate.Checked;
                AppSettings.AutoLogin = CBox_AutoLogin.Checked;
                AppSettings.DisablePopup = CBox_DisablePopup.Checked;
                AppSettings.TestMode = CBox_TestMode.Checked;
                AppSettings.ServerClickConnect = CBox_ServerClickConnect.Checked;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ComboBoxGraceMs1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItemString = ComboBoxGraceMs1.SelectedItem?.ToString();
            int resultInMs;
            if (int.TryParse(selectedItemString, out resultInMs))
            {
                resultInMs = resultInMs * 1000;
                if (AppSettings != null) AppSettings.PopupGraceMs = resultInMs;
            }
        }

        private void OptionForm_MouseMove(object sender, MouseEventArgs e)
        {
            // 마우스 위치를 화면 좌표로 변환해 중앙 처리 함수 호출
            UpdateDescriptionFromScreenPoint(this.PointToScreen(e.Location));
        }
    }
}