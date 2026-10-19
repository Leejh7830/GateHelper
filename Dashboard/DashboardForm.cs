using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GateHelper.Dashboard
{
    public partial class DashboardForm : MaterialForm
    {
        private readonly MaterialSkinManager _materialSkinManager;

        public DashboardForm()
        {
            InitializeComponent();

            _materialSkinManager = MaterialSkinManager.Instance;
            _materialSkinManager.AddFormToManage(this);
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {
            ApplyThemeColors();
            SetupCardInteractions();
            UpdateAllViews();
            timerRefresh.Start();
        }

        private void SetupCardInteractions()
        {
            // 시스템 헬스 클릭 시 최근 중요 로그 탭으로 즉시 이동
            var tip = new ToolTip();
            string healthTip = "클릭 시 최근 중요 로그(Error/Warning) 탭으로 이동합니다.";
            foreach (Control c in new Control[] { cardHealth, lblHealthTitle, lblHealthMain, lblHealthSub })
            {
                c.Cursor = Cursors.Hand;
                tip.SetToolTip(c, healthTip);
                c.Click += (s, ev) =>
                {
                    tabControlMain.SelectedTab = tpLogs;
                };
            }
        }

        private void DashboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerRefresh.Stop();
        }

        private void TimerRefresh_Tick(object sender, EventArgs e)
        {
            UpdateResourceAndUptimeViews();
        }

        private void ApplyThemeColors()
        {
            bool isDark = _materialSkinManager.Theme == MaterialSkinManager.Themes.DARK;
            Color cardBg = isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(240, 240, 244);
            Color contentBg = isDark ? Color.FromArgb(37, 37, 38) : Color.FromArgb(250, 250, 250);
            Color textMain = isDark ? Color.White : Color.FromArgb(33, 33, 33);
            Color textSub = isDark ? Color.Silver : Color.FromArgb(100, 100, 100);
            Color dgvBg = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
            Color dgvCellText = isDark ? Color.Gainsboro : Color.Black;

            // 상단 카드 테마 적용
            foreach (Panel card in new[] { cardUptime, cardActivity, cardHealth, cardSandbox, pnlBoxBitFlip, pnlBoxSignalLink, pnlBoxMystery, pnlBoxAiTrend, pnlNetworkCard, pnlDriverCard })
            {
                card.BackColor = cardBg;
                card.BorderStyle = BorderStyle.FixedSingle;
            }

            pnlResourceContent.BackColor = contentBg;
            pnlServerContent.BackColor = contentBg;
            pnlSandboxContent.BackColor = contentBg;
            pnlLogContent.BackColor = contentBg;

            // 데이터그리드뷰 테마 스타일
            foreach (DataGridView dgv in new[] { dgvTopServers, dgvRecentLogs })
            {
                dgv.BackgroundColor = dgvBg;
                dgv.DefaultCellStyle.BackColor = dgvBg;
                dgv.DefaultCellStyle.ForeColor = dgvCellText;
                dgv.DefaultCellStyle.Font = new Font("맑은 고딕", 9F, FontStyle.Regular);
                dgv.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(60, 90, 140) : Color.FromArgb(180, 210, 255);
                dgv.DefaultCellStyle.SelectionForeColor = isDark ? Color.White : Color.Black;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(50, 50, 52) : Color.FromArgb(230, 230, 230);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = textMain;
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
                dgv.EnableHeadersVisualStyles = false;
                dgv.GridColor = isDark ? Color.FromArgb(55, 55, 58) : Color.FromArgb(220, 220, 220);
            }

            txtLogDetail.BackColor = isDark ? Color.FromArgb(25, 25, 26) : Color.FromArgb(245, 245, 245);
            txtLogDetail.ForeColor = isDark ? Color.Gainsboro : Color.Black;
        }

        public void UpdateAllViews()
        {
            UpdateTopKpiCards();
            UpdateResourceAndUptimeViews();
            UpdateServerAccessView();
            UpdateSandboxView();
            UpdateRecentLogsView();
        }

        #region [ 1. 상단 KPI 카드 ]

        private void UpdateTopKpiCards()
        {
            var stats = DashboardManager.Stats;
            TimeSpan sessionTime = DashboardManager.GetSessionUptime();
            double totalMinutes = stats.TotalUptimeMinutes + sessionTime.TotalMinutes;

            int totalHours = (int)(totalMinutes / 60);
            int remainMins = (int)(totalMinutes % 60);

            // 가동시간
            lblUptimeMain.Text = $"{totalHours}시간 {remainMins:D2}분";
            lblUptimeSub.Text = $"이번 세션: {sessionTime:hh\\:mm\\:ss}";

            // 활동
            lblActivityMain.Text = $"{stats.TotalLaunchCount:N0}회 실행";
            int totalConnect = stats.TotalConnectAttempts;
            double autoLoginRate = 0.0;
            int totalAuto = stats.AutoLoginSuccessCount + stats.AutoLoginFailureCount;
            if (totalAuto > 0)
            {
                autoLoginRate = Math.Round((double)stats.AutoLoginSuccessCount / totalAuto * 100.0, 1);
            }
            lblActivitySub.Text = $"서버 접속 {totalConnect:N0}회 · 자동 로그인 {autoLoginRate:F1}%";

            // 시스템 헬스
            int errorCount = stats.TotalErrorCount + stats.TotalCriticalCount;
            if (errorCount == 0)
            {
                lblHealthMain.Text = "Clean (정상)";
                lblHealthMain.ForeColor = Color.FromArgb(100, 220, 100);
                lblHealthSub.Text = "최근 발생한 에러가 없습니다.";
            }
            else
            {
                lblHealthMain.Text = $"주의 ({errorCount}건)";
                lblHealthMain.ForeColor = Color.FromArgb(255, 140, 100);
                lblHealthSub.Text = $"에러 {stats.TotalErrorCount}건 · 치명적 {stats.TotalCriticalCount}건";
            }

            // 샌드박스
            int totalClears = stats.BitFlipWins + stats.SignalLinkWins + stats.MysterySolves;
            lblSandboxMain.Text = $"{totalClears:N0}회 달성";
            int scraps = DashboardManager.GetAiTrendScrapCount();
            lblSandboxSub.Text = $"사건 규명 {stats.MysterySolves}건 · 찜 {scraps}개";
        }

        #endregion

        #region [ 2. 리소스 및 실시간 타이머 갱신 ]

        private void UpdateResourceAndUptimeViews()
        {
            // 실시간 세션 시간 갱신
            TimeSpan sessionTime = DashboardManager.GetSessionUptime();
            lblUptimeSub.Text = $"이번 세션: {sessionTime:hh\\:mm\\:ss}";

            var snap = DashboardManager.GetResourceSnapshot();

            // 1. RAM 세그먼트 게이지 (초록/노랑/빨강)
            string ramStatus;
            Color ramColor;
            if (snap.ProcessRamMb <= 150.0)
            {
                ramStatus = "매우 쾌적 (Optimal)";
                ramColor = Color.FromArgb(46, 204, 113);
            }
            else if (snap.ProcessRamMb <= 300.0)
            {
                ramStatus = "보통 (Normal)";
                ramColor = Color.FromArgb(241, 196, 15);
            }
            else
            {
                ramStatus = "주의/정리 권장 (High)";
                ramColor = Color.FromArgb(231, 76, 60);
            }
            gaugeRam.SetGaugeData(snap.ProcessRamMb, 512, $"{snap.ProcessRamMb:F1} MB", ramStatus, ramColor);

            // 2. 실시간 CPU & RAM 꺾은선 트렌드 차트에 샘플 푸시
            chartTrend.AddSample((float)snap.ProcessCpuPercent, (float)snap.ProcessRamMb);

            // 3. 네트워크 핑 카드 갱신
            if (snap.NetworkPingMs >= 0)
            {
                if (snap.NetworkPingMs <= 80)
                {
                    lblNetworkPing.Text = $"{snap.NetworkPingMs} ms · 정상";
                    lblNetworkPing.ForeColor = Color.FromArgb(46, 204, 113);
                }
                else if (snap.NetworkPingMs <= 150)
                {
                    lblNetworkPing.Text = $"{snap.NetworkPingMs} ms · 주의 (지연)";
                    lblNetworkPing.ForeColor = Color.FromArgb(241, 196, 15);
                }
                else
                {
                    lblNetworkPing.Text = $"{snap.NetworkPingMs} ms · 느림 (지연)";
                    lblNetworkPing.ForeColor = Color.FromArgb(231, 76, 60);
                }
            }
            else
            {
                lblNetworkPing.Text = "측정 중...";
                lblNetworkPing.ForeColor = Color.Silver;
            }
            lblNetworkDetails.Text = $"• 게이트웨이 통신 상태: {snap.NetworkStatusText}\n• 실시간 3초주기 핑 패킷 측정 중";

            // 4. 드라이버 상태 & 스레드/핸들 정보
            string driverStr = snap.ChromeDriverRunning ? "🟢 ChromeDriver 활성" : "⚪ ChromeDriver 미실행";
            string selStr = snap.IsSeleniumDriverAlive ? "🟢 Selenium 연결됨" : "⚪ Selenium 미연결";
            lblDriverStatus.Text = $"{driverStr}   {selStr}";
            lblDriverStatus.ForeColor = (snap.ChromeDriverRunning || snap.IsSeleniumDriverAlive) ? Color.LightGreen : Color.Silver;

            lblThreadsHandles.Text = $"스레드 {snap.ThreadCount}개 · 핸들 {snap.HandleCount}개 · 전용메모리 {snap.ProcessPrivateRamMb:F1} MB";
        }

        #endregion

        #region [ 3. 서버 접속 통계 (원형 도넛 차트 & 링 게이지 & 표) ]

        private void UpdateServerAccessView()
        {
            var topServers = DashboardManager.GetTopServers(8);
            int totalCount = topServers.Sum(x => x.Value);

            // 1. 도넛 차트에 데이터 전달
            chartServerDonut.SetData(topServers);

            // 2. DataGridView에 표 바인딩
            dgvTopServers.Rows.Clear();
            int rank = 1;
            foreach (var kv in topServers)
            {
                double pct = totalCount > 0 ? Math.Round((double)kv.Value / totalCount * 100.0, 1) : 0.0;
                dgvTopServers.Rows.Add(rank++, kv.Key, $"{kv.Value:N0}회", $"{pct:F1}%");
            }

            if (dgvTopServers.Rows.Count == 0)
            {
                dgvTopServers.Rows.Add("-", "접속 이력 없음", "0회", "0.0%");
            }

            // 3. 자동 로그인 원형 링 게이지
            var stats = DashboardManager.Stats;
            int totalAuto = stats.AutoLoginSuccessCount + stats.AutoLoginFailureCount;
            double rate = 0.0;
            if (totalAuto > 0)
            {
                rate = Math.Round((double)stats.AutoLoginSuccessCount / totalAuto * 100.0, 1);
            }

            gaugeAutoLogin.Value = rate;
            gaugeAutoLogin.SubTitle = $"자동 로그인 {stats.AutoLoginSuccessCount}/{totalAuto}회";
            gaugeAutoLogin.RingColor = rate >= 90.0 ? Color.FromArgb(46, 204, 113) : (rate >= 70.0 ? Color.FromArgb(241, 196, 15) : Color.FromArgb(231, 76, 60));
        }

        #endregion

        #region [ 4. 샌드박스 & 업적 ]

        private void UpdateSandboxView()
        {
            var stats = DashboardManager.Stats;

            // BitFlip
            lblBitFlipDetails.Text =
                $"• 비트 반전 클리어: {stats.BitFlipWins:N0}회\n" +
                $"• 달성 상태: {(stats.BitFlipWins > 0 ? "🏆 마스터 달성" : "도전 대기 중")}\n" +
                "• 모든 노드를 녹색 1로 동기화 완료";

            // SignalLink
            lblSignalLinkDetails.Text =
                $"• 회로 퍼즐 클리어: {stats.SignalLinkWins:N0}회\n" +
                $"• 달성 상태: {(stats.SignalLinkWins > 0 ? "⚡ 신호망 해독가" : "도전 대기 중")}\n" +
                "• 신호 노드 간 충돌 없는 완벽한 연결";

            // Mystery Time
            double mysteryRate = 0.0;
            if (stats.MysteryAttempts > 0)
            {
                mysteryRate = Math.Round((double)stats.MysterySolves / stats.MysteryAttempts * 100.0, 1);
            }
            lblMysteryDetails.Text =
                $"• 사건 규명 해결: {stats.MysterySolves:N0}건\n" +
                $"• 질문/추리 도전: {stats.MysteryAttempts:N0}회\n" +
                $"• 사건 규명 성공률: {mysteryRate:F1}%\n" +
                $"• 탐정 등급: {(stats.MysterySolves >= 10 ? "🕵️ 명탐정" : (stats.MysterySolves >= 3 ? "🔍 수사관" : "초보 탐정"))}";

            // AI Trend
            int scraps = DashboardManager.GetAiTrendScrapCount();
            int keywords = DashboardManager.GetCustomKeywordCount();
            lblAiTrendDetails.Text =
                $"• 찜(스크랩) 오픈소스: {scraps:N0}개\n" +
                $"• 등록된 맞춤 키워드: {keywords:N0}개\n" +
                "• GitHub 글로벌 인기 저장소 & Gemini 요약 연동\n" +
                "• 5대 실시간 뉴스 & 초경량 카드뉴스 지원";
        }

        #endregion

        #region [ 5. 최근 중요 로그 ]

        private void UpdateRecentLogsView()
        {
            var logs = DashboardManager.GetRecentLogs();
            dgvRecentLogs.Rows.Clear();

            foreach (var log in logs)
            {
                int rowIndex = dgvRecentLogs.Rows.Add(
                    log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    log.Level,
                    log.Caller,
                    log.Message
                );

                var row = dgvRecentLogs.Rows[rowIndex];
                if (log.Level == "Critical")
                {
                    row.DefaultCellStyle.ForeColor = Color.IndianRed;
                    row.DefaultCellStyle.Font = new Font(dgvRecentLogs.Font, FontStyle.Bold);
                }
                else if (log.Level == "Error")
                {
                    row.DefaultCellStyle.ForeColor = Color.Coral;
                }
                else if (log.Level == "Warning")
                {
                    row.DefaultCellStyle.ForeColor = Color.Goldenrod;
                }
            }

            if (dgvRecentLogs.Rows.Count > 0)
            {
                dgvRecentLogs.Rows[0].Selected = true;
                DgvRecentLogs_SelectionChanged(null, null);
            }
            else
            {
                txtLogDetail.Text = "🎉 최근 기록된 에러나 경고 로그가 없습니다. 시스템이 아주 깨끗합니다!";
            }
        }

        private void DgvRecentLogs_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRecentLogs.SelectedRows.Count > 0)
            {
                var row = dgvRecentLogs.SelectedRows[0];
                string time = row.Cells[0].Value?.ToString();
                string level = row.Cells[1].Value?.ToString();
                string caller = row.Cells[2].Value?.ToString();
                string msg = row.Cells[3].Value?.ToString();

                txtLogDetail.Text = $"[{time}] [{level}] {caller}\r\n{msg}";
            }
        }

        #endregion

        #region [ 6. 버튼 이벤트들 ]

        private void BtnGcCollect_Click(object sender, EventArgs e)
        {
            try
            {
                long beforeBytes = GC.GetTotalMemory(false);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                long afterBytes = GC.GetTotalMemory(true);

                double freedMb = Math.Max(0.0, Math.Round((double)(beforeBytes - afterBytes) / (1024 * 1024), 2));
                UpdateResourceAndUptimeViews();

                MessageBox.Show($"메모리 가비지 컬렉션(GC)을 완료했습니다.\n\n• 회수된 메모리: 약 {freedMb} MB", "메모리 정리 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"메모리 정리 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            UpdateAllViews();
        }

        private void BtnOpenLogDir_Click(object sender, EventArgs e)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                Process.Start("explorer.exe", logDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"로그 폴더 열기 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenDashboardDir_Click(object sender, EventArgs e)
        {
            try
            {
                string dir = DashboardManager.DashboardDirectory;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                Process.Start("explorer.exe", dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"대시보드 폴더 열기 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnResetStats_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                "누적 가동 시간, 서버 접속 카운트, 샌드박스 클리어 기록 등 모든 대시보드 통계를 초기화하시겠습니까?\n\n(※ 초기화 후에는 복구할 수 없습니다.)",
                "통계 초기화 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (res == DialogResult.Yes)
            {
                DashboardManager.ResetStats();
                UpdateAllViews();
                MessageBox.Show("대시보드 통계가 성공적으로 초기화되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion
    }
}
