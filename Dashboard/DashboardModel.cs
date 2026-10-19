using System;
using System.Collections.Generic;

namespace GateHelper.Dashboard
{
    /// <summary>
    /// _meta/dashboard/dashboard_stats.json에 영구 보존되는 통계 모델
    /// </summary>
    public class DashboardStats
    {
        // 1. 실행 및 세션
        public int TotalLaunchCount { get; set; } = 0;
        public double TotalUptimeMinutes { get; set; } = 0;
        public DateTime FirstLaunchDate { get; set; } = DateTime.Now;
        public DateTime LastLaunchDate { get; set; } = DateTime.Now;

        // 2. 업무 및 서버 접속
        public int TotalConnectAttempts { get; set; } = 0;
        public int AutoLoginSuccessCount { get; set; } = 0;
        public int AutoLoginFailureCount { get; set; } = 0;
        public Dictionary<string, int> ServerAccessCounts { get; set; } = new Dictionary<string, int>();

        // 3. 샌드박스 클리어 기록
        public int BitFlipWins { get; set; } = 0;
        public int SignalLinkWins { get; set; } = 0;
        public int MysterySolves { get; set; } = 0;
        public int MysteryAttempts { get; set; } = 0;
        public int AiTrendScraps { get; set; } = 0;

        // 4. 에러 카운터
        public int TotalErrorCount { get; set; } = 0;
        public int TotalCriticalCount { get; set; } = 0;
    }

    /// <summary>
    /// 실시간 프로세스 및 하드웨어 점유율 스냅샷
    /// </summary>
    public class SystemResourceSnapshot
    {
        public double ProcessRamMb { get; set; }
        public double ProcessPrivateRamMb { get; set; }
        public double ProcessCpuPercent { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }

        public long NetworkPingMs { get; set; } = -1;
        public string NetworkStatusText { get; set; } = "측정 대기";

        public bool ChromeDriverRunning { get; set; }
        public bool IsSeleniumDriverAlive { get; set; }

        public TimeSpan SessionUptime { get; set; }
    }

    /// <summary>
    /// 최근 크리티컬/에러 로그 항목
    /// </summary>
    public class DashboardLogItem
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Caller { get; set; }
        public string Message { get; set; }
    }
}
