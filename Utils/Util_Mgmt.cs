using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;


namespace GateHelper.Utils
{
    public static class Util_Mgmt
    {
        /// <summary>
        /// 수집 항목을 선택받는 동적 팝업을 띄우고 결과를 반환합니다.
        /// 반환값: (SEM 선택 여부, Port 선택 여부)
        /// </summary>
        public static (bool isSem, bool isPort) ShowCollectionSelectDialog()
        {
            // 1. 메모리 상에서 즉석으로 폼(창) 생성
            Form prompt = new Form()
            {
                Width = 320,
                Height = 220,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "데이터 수집 옵션",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // 2. 체크박스 컨트롤 동적 생성
            CheckBox chkSem = new CheckBox()
            {
                Left = 40,
                Top = 30,
                Text = "StockerSEM 수집",
                // Checked = true,
                Width = 220
            };
            CheckBox chkPort = new CheckBox()
            {
                Left = 40,
                Top = 60,
                Text = "StockerPort 수집 (하위 항목 포함)",
                // Checked = true,
                Width = 220
            };

            // 3. 버튼 컨트롤 동적 생성
            Button btnOk = new Button()
            {
                Text = "수집 시작",
                Left = 40,
                Top = 120,
                Width = 100,
                DialogResult = DialogResult.OK
            };
            Button btnCancel = new Button()
            {
                Text = "취소",
                Left = 150,
                Top = 120,
                Width = 100,
                DialogResult = DialogResult.Cancel
            };

            // 4. 생성한 컨트롤들을 폼에 부착
            prompt.Controls.Add(chkSem);
            prompt.Controls.Add(chkPort);
            prompt.Controls.Add(btnOk);
            prompt.Controls.Add(btnCancel);

            // 엔터 키 누르면 확인, ESC 키 누르면 취소 작동
            prompt.AcceptButton = btnOk;
            prompt.CancelButton = btnCancel;

            // 5. 창을 띄우고(ShowDialog) 사용자의 응답 대기 및 결과 반환
            if (prompt.ShowDialog() == DialogResult.OK)
            {
                return (chkSem.Checked, chkPort.Checked);
            }

            // 취소 버튼을 누르거나 창을 그냥 끄면 둘 다 false 반환
            return (false, false);
        }
    }
}
