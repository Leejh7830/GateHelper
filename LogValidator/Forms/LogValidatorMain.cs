using GateHelper.LogValidator;
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

namespace GateHelper.LogAnalyzer
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














    }
}
