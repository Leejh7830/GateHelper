using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace GateHelper
{
    public static class Util_Rdp
    {
        /// <summary>
        /// 현재 PC에서 RDP 클라이언트(mstsc.exe) 실행 여부 반환
        /// </summary>
        public static bool IsRdpClientRunning()
        {
            return Process.GetProcessesByName("mstsc").Any();
        }

        /// <summary>
        /// FUNC TEST 버튼 클릭 시 호출: RDP 실행 여부 메시지 안내
        /// </summary>
        public static void ShowRdpStatus()
        {
            bool isRunning = IsRdpClientRunning();
            string msg = isRunning ? "RDP(원격 데스크톱)가 실행 중." : "RDP(원격 데스크톱) 없음.";
            MessageBox.Show(msg, "RDP 감지 결과", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 추후 브로드캐스트 기능 등 확장
    }
}