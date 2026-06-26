using GateHelper.LogValidator;
using GateHelper.LogValidator.Core;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateHelper.LogValidator
{
    public partial class LogValidatorMain : MaterialForm
    {
        // 메인 프레임워크의 테마 매니저 인스턴스를 동기화
        private readonly MaterialSkinManager _skinManager = MaterialSkinManager.Instance;

        public LogValidatorMain()
        {
            InitializeComponent();

            // [테마 동기화 엔진] 메인 프로그램의 다크/라이트 상태를 그대로 복제
            _skinManager.AddFormToManage(this);
        }

        private void LogValidatorMain_Load(object sender, EventArgs e)
        {

        }

        // 분석창 (LVForm) 호출
        private void btnOpenValidator_Click(object sender, EventArgs e)
        {
            using (LogValidatorForm lvForm = new LogValidatorForm())
            {
                lvForm.StartPosition = FormStartPosition.CenterParent;
                lvForm.ShowDialog(this);
            }
        }

        // 시나리오 편집기 (LSForm) 호출
        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            using (LogScenarioForm lsForm = new LogScenarioForm())
            {
                lsForm.StartPosition = FormStartPosition.CenterParent;
                lsForm.ShowDialog(this);
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            // 💡 [입출력 동기화 가드] 설정 창을 열기 직전, 디스크에서 최신 JSON 설정을 불러와 동기화합니다.
            LogValidatorConfigManager.Load();

            // 설정 폼 인스턴스 생성 (MaterialSkin 폼이든 일반 폼이든 동일하게 작동)
            using (LogValidatorSettingForm settingForm = new LogValidatorSettingForm())
            {
                // 💡 ShowDialog()로 띄워 설정 창이 닫히기 전까지 메인 화면 제어를 인터락(잠금)합니다.
                if (settingForm.ShowDialog() == DialogResult.OK)
                {
                    // 사용자가 SAVE 버튼을 눌러 정상적으로 닫힌 경우
                    // 💡 [정규식 재조립 시그널] 파서나 메인 뷰어에서 바뀐 설정을 즉시 반영하도록 리프레시 로직 유도 가능
                    MessageBox.Show("설정이 안전하게 저장되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
