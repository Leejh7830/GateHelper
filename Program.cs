using GateHelper.LogValidator;
using System;
using System.Threading.Tasks;
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

            // [전역 예외 안전망] 예기치 않은 UI 스레드/도메인 예외 시 무언 종료 방지 및 복구
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                try
                {
                    LogManager.LogException(e.Exception, LogManager.Level.Critical, "[UI Thread Exception]");
                }
                catch { }
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try
                {
                    LogManager.LogMessage($"[Domain Unhandled Exception] {e.ExceptionObject}", LogManager.Level.Critical);
                }
                catch { }
            };

            // 1. 3일 경과 오래된 임시 캐시 파일 자동 청소
            try
            {
                AiTrend.AiTrendService.PurgeStaleCaches(days: 3);
            }
            catch { }

            // 2. [사용자 옵션] 프로그램 시작 시 AI 데일리 브리핑 백그라운드 자동 예열
            try
            {
                var options = Util_Option.LoadUserOptions();
                if (options != null && options.AutoPreloadAiTrend && !string.IsNullOrWhiteSpace(options.GeminiApiKey))
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            // 폼이 화면에 완전히 뜨고 안정화되도록 5초 대기 후 순차 예열
                            await Task.Delay(5000);
                            var svc = new AiTrend.AiTrendService();
                            await svc.PreloadAllCategoriesAsync(
                                System.Threading.CancellationToken.None,
                                canProceed: () => !AiTrend.AiTrendControl.IsUiActive && !Util_Connect.IsAuthInProgress
                            );
                        }
                        catch { }
                    });
                }
            }
            catch { }
            
            // [] 메인 UI 진입점 ]
            Application.Run(new MainUI());
            // Application.Run(new LogValidatorMain());
            // Application.Run(new SandBox("AiTrend"));
        }
    }
}
