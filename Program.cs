using GateHelper.LogValidator;
using System;
using System.Windows.Forms;

namespace GateHelper
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // [바로가기] 당분간 AI 트렌드(SandBox) 기능 집중 테스트를 위해 바로 실행
            Application.Run(new SandBox("AiTrend"));

            // [기존 메인 UI 진입점 - 필요 시 위 줄을 주석하고 아래 줄 활성화]
            // Application.Run(new MainUI());
            // Application.Run(new LogValidatorMain());
        }
    }
}
