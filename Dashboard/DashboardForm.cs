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
        private bool _showOnlyAlerts = false;

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

            // 탭 전환 시 활성 탭 데이터 즉각 자동 동기화 (불필요한 전체 리드로우 방지)
            tabControlMain.SelectedIndexChanged += TabControlMain_SelectedIndexChanged;

            timerRefresh.Start();
        }

        private void SetupCardInteractions()
        {
            var tip = new ToolTip();

            // 1. 가동 시간 카드 클릭 시 리소스 모니터 탭으로 이동
            string uptimeTip = "클릭 시 리소스 & 헬스 모니터 탭으로 이동합니다.";
            foreach (Control c in new Control[] { cardUptime, lblUptimeTitle, lblUptimeMain, lblUptimeSub })
            {
                c.Cursor = Cursors.Hand;
                tip.SetToolTip(c, uptimeTip);
                c.Click += (s, ev) => tabControlMain.SelectedTab = tpResource;
            }

            // 2. 활동 카드 클릭 시 서버 접속 분석 탭으로 이동
            string activityTip = "클릭 시 서버 접속 통계 & 자동 로그인 탭으로 이동합니다.";
            foreach (Control c in new Control[] { cardActivity, lblActivityTitle, lblActivityMain, lblActivitySub })
            {
                c.Cursor = Cursors.Hand;
                tip.SetToolTip(c, activityTip);
                c.Click += (s, ev) => tabControlMain.SelectedTab = tpServer;
            }

            // 3. 시스템 헬스 클릭 시 최근 중요 로그 탭으로 즉시 이동
            string healthTip = "클릭 시 최근 중요 로그(Error/Warning) 탭으로 이동합니다.";
            foreach (Control c in new Control[] { cardHealth, lblHealthTitle, lblHealthMain, lblHealthSub })
            {
                c.Cursor = Cursors.Hand;
                tip.SetToolTip(c, healthTip);
                c.Click += (s, ev) =>
                {
                    _showOnlyAlerts = true;
                    btnFilterAlerts.Text = "📋 전체 로그 보기";
                    btnFilterAlerts.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
                    UpdateRecentLogsView();
                    tabControlMain.SelectedTab = tpLogs;
                };
            }

            // 4. 샌드박스 & 업적 카드 클릭 시 샌드박스 탭으로 즉시 이동
            string sandboxTip = "클릭 시 스팀 스타일 도전과제 & 샌드박스 마스터리 탭으로 이동합니다.";
            foreach (Control c in new Control[] { cardSandbox, lblSandboxTitle, lblSandboxMain, lblSandboxSub })
            {
                c.Cursor = Cursors.Hand;
                tip.SetToolTip(c, sandboxTip);
                c.Click += (s, ev) => tabControlMain.SelectedTab = tpSandbox;
            }
        }

        private void DashboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerRefresh.Stop();
        }

        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 상단 KPI 카드(가동 시간, 헬스 상태 등)는 항상 최신 상태 유지
            UpdateTopKpiCards();

            // 사용자가 선택한 활성 탭만 경량 동기화 (불필요한 타 탭 중복 계산 방지)
            if (tabControlMain.SelectedTab == tpResource)
            {
                UpdateResourceAndUptimeViews();
            }
            else if (tabControlMain.SelectedTab == tpServer)
            {
                UpdateServerAccessView();
            }
            else if (tabControlMain.SelectedTab == tpSandbox)
            {
                UpdateSandboxView();
            }
            else if (tabControlMain.SelectedTab == tpLogs)
            {
                UpdateRecentLogsView();
            }
        }

        private void TimerRefresh_Tick(object sender, EventArgs e)
        {
            // 상단 세션 가동 시간은 1초마다 실시간 반영
            UpdateTopKpiCards();

            // 리소스 탭이 열려 있을 때만 실시간 차트/게이지 리드로우 (다른 탭 열람 중에는 CPU/GDI 부하 차단)
            if (tabControlMain.SelectedTab == tpResource)
            {
                UpdateResourceAndUptimeViews();
            }
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
            foreach (Panel card in new[] { cardUptime, cardActivity, cardHealth, cardSandbox, pnlNetworkCard, pnlDriverCard })
            {
                card.BackColor = cardBg;
                card.BorderStyle = BorderStyle.FixedSingle;
            }

            pnlResourceContent.BackColor = contentBg;
            pnlServerContent.BackColor = contentBg;
            pnlSandboxContent.BackColor = contentBg;
            pnlLogContent.BackColor = contentBg;

            // 데이터그리드뷰 테마 스타일 (도전과제 dgvAchievements 포함)
            foreach (DataGridView dgv in new[] { dgvTopServers, dgvRecentLogs, dgvAchievements })
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
                dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
                dgv.EnableHeadersVisualStyles = false;
                dgv.GridColor = isDark ? Color.FromArgb(55, 55, 58) : Color.FromArgb(220, 220, 220);
            }

            // 서버 통계 그리드 정렬 최적화 (순위 가로 출력 보장 및 줄바꿈 방지, 중앙 정렬)
            colRank.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colRank.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colRank.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            colRank.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            colServerName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colServerName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colServerName.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            colServerName.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            colAccessCount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colAccessCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colAccessCount.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            colAccessCount.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            colLastConnected.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colLastConnected.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colLastConnected.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            colLastConnected.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dgvTopServers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTopServers.MultiSelect = false;

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

            // 시스템 헬스 (확인/클리어 상태 반영)
            int unackedErrors = stats.GetUnacknowledgedErrorCount();
            if (unackedErrors == 0)
            {
                lblHealthMain.Text = "Clean ✨";
                lblHealthMain.ForeColor = Color.FromArgb(100, 220, 100);
                if (stats.LastErrorAcknowledgedTime.HasValue)
                {
                    lblHealthSub.Text = $"확인 완료 ({stats.LastErrorAcknowledgedTime.Value:HH:mm}) · 신규 에러 없음";
                }
                else
                {
                    lblHealthSub.Text = "최근 발생한 에러가 없습니다.";
                }
            }
            else
            {
                lblHealthMain.Text = $"주의 (신규 {unackedErrors}건)";
                lblHealthMain.ForeColor = Color.FromArgb(255, 120, 90);
                lblHealthSub.Text = $"미확인 에러 {unackedErrors}건 · 클릭하여 로그 확인";
            }

            // 샌드박스 & 업적
            var mastery = DashboardManager.GetMasteryTierInfo();
            lblSandboxMain.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblSandboxMain.Text = mastery.TierName;
            lblSandboxSub.Text = $"AP {mastery.TotalEarnedPoints}점 · 달성 {mastery.UnlockedCount}/{mastery.TotalCount}개";
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

        #region [ 3. 서버 접속 통계 (원형 도넛 차트 & 링 게이지 & 표 & 24시간 잔디 히트맵) ]

        private void UpdateServerAccessView()
        {
            var topServers = DashboardManager.GetTopServers(8);

            // 1. 도넛 차트에 데이터 전달
            chartServerDonut.SetData(topServers);

            // 2. DataGridView에 표 바인딩 (순위 색상 도넛 차트 일치 & 최근 접속 일시 연동)
            dgvTopServers.Rows.Clear();
            var lastConnectedMap = Util_ServerList.GetServerLastConnectedMap();
            int rank = 1;
            foreach (var kv in topServers)
            {
                int currentRank = rank++;
                Color rankColor = (currentRank <= 5)
                    ? DonutChartControl.Palette[currentRank - 1]
                    : DonutChartControl.Palette[5];

                string lastConnectedStr = "-";
                if (lastConnectedMap != null && lastConnectedMap.TryGetValue(kv.Key, out DateTime lastDt) && lastDt > DateTime.MinValue && lastDt.Year > 2000)
                {
                    var now = DateTime.Now;
                    if (lastDt.Date == now.Date)
                    {
                        lastConnectedStr = $"오늘 {lastDt:HH:mm}";
                    }
                    else if (lastDt.Date == now.Date.AddDays(-1))
                    {
                        lastConnectedStr = $"어제 {lastDt:HH:mm}";
                    }
                    else if (lastDt.Year == now.Year)
                    {
                        lastConnectedStr = lastDt.ToString("MM-dd HH:mm");
                    }
                    else
                    {
                        lastConnectedStr = lastDt.ToString("yyyy-MM-dd");
                    }
                }

                int rowIdx = dgvTopServers.Rows.Add($"● {currentRank}위", kv.Key, $"{kv.Value:N0}회", lastConnectedStr);
                var row = dgvTopServers.Rows[rowIdx];
                row.Cells[0].Style.ForeColor = rankColor;
                row.Cells[0].Style.SelectionForeColor = rankColor;
                row.Cells[0].Style.Font = new Font(dgvTopServers.Font, FontStyle.Bold);
            }

            if (dgvTopServers.Rows.Count == 0)
            {
                dgvTopServers.Rows.Add("-", "접속 이력 없음", "0회", "-");
            }

            // 3. 자동 로그인 원형 링 게이지 (성공/실패 카운트 분리 & 누적 절약 시간 연동)
            var stats = DashboardManager.Stats;
            int totalAuto = stats.AutoLoginSuccessCount + stats.AutoLoginFailureCount;
            double rate = 0.0;
            if (totalAuto > 0)
            {
                rate = Math.Round((double)stats.AutoLoginSuccessCount / totalAuto * 100.0, 1);
            }

            gaugeAutoLogin.SetProgress(
                rate,
                stats.AutoLoginSuccessCount,
                stats.AutoLoginFailureCount,
                stats.GetEstimatedSavedFormatted()
            );

            // 4. 24시간 접속 활동 잔디 히트맵 (서버 접속 및 화면 잠금 해제 횟수 연동)
            heatmapHourly.SetData(DashboardManager.GetHourlyAccessCounts(), stats.ScreenUnlockCount);
        }

        #endregion

        #region [ 4. 샌드박스 & 업적 (스팀 스타일 도전과제 & 2단 HUD 배너) ]

        private void UpdateSandboxView()
        {
            var mastery = DashboardManager.GetMasteryTierInfo();
            var catTiers = DashboardManager.GetCategoryTiers();
            var achievements = DashboardManager.GetAchievements();

            // 1. 상단 2단 와이드 마스터리 배너 갱신
            bannerMastery.SetData(mastery, catTiers);

            // 2. 스팀 스타일 도전과제 DataGridView 갱신
            dgvAchievements.Rows.Clear();

            bool isDark = _materialSkinManager.Theme == MaterialSkinManager.Themes.DARK;
            Color unlockedFore = isDark ? Color.FromArgb(255, 215, 0) : Color.FromArgb(180, 120, 0); // 골드
            Color lockedFore = isDark ? Color.DarkGray : Color.Gray;

            foreach (var ach in achievements)
            {
                string statusText = ach.IsUnlocked ? "🏆 달성" : "🔒 잠김";
                string progressText = ach.IsUnlocked 
                    ? $"완료 ({ach.CurrentCount}/{ach.RequiredCount})" 
                    : $"{ach.CurrentCount} / {ach.RequiredCount} ({ach.ProgressPercent:F0}%)";

                int rowIndex = dgvAchievements.Rows.Add(
                    statusText,
                    ach.CategoryName,
                    ach.Title,
                    ach.Description,
                    progressText,
                    $"{ach.Points} AP"
                );

                var row = dgvAchievements.Rows[rowIndex];
                if (ach.IsUnlocked)
                {
                    row.DefaultCellStyle.ForeColor = unlockedFore;
                    row.DefaultCellStyle.Font = new Font(dgvAchievements.Font, FontStyle.Bold);
                }
                else
                {
                    row.DefaultCellStyle.ForeColor = lockedFore;
                }
            }

            if (dgvAchievements.Rows.Count > 0)
            {
                dgvAchievements.ClearSelection();
            }
        }

        #endregion

        #region [ 5. 최근 중요 로그 ]

        private void UpdateRecentLogsView()
        {
            var logs = DashboardManager.GetRecentLogs(50, _showOnlyAlerts);
            dgvRecentLogs.Rows.Clear();

            int unacked = DashboardManager.Stats.GetUnacknowledgedErrorCount();
            btnAckLogs.Enabled = unacked > 0;
            btnAckLogs.Text = unacked > 0 ? $"✔️ 로그 확인 ({unacked}건 클리어)" : "✔️ 모든 로그 확인됨";

            // 주의/심각만 보기 필터 적용
            if (_showOnlyAlerts)
            {
                logs = logs.Where(x => x.Level == "Warning" || x.Level == "Error" || x.Level == "Critical").ToList();
            }

            foreach (var log in logs)
            {
                int rowIndex = dgvRecentLogs.Rows.Add(
                    log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    log.Level,
                    log.Caller,
                    log.Message
                );

                var row = dgvRecentLogs.Rows[rowIndex];

                // 확인 완료된 과거 로그는 차분한 톤으로 구별, 신규 미확인 에러는 굵고 선명한 경고색
                if (log.IsAcknowledged)
                {
                    if (log.Level == "Critical")
                    {
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(190, 105, 105);
                    }
                    else if (log.Level == "Error")
                    {
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(195, 125, 105);
                    }
                    else if (log.Level == "Warning")
                    {
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 155, 95);
                    }
                    else
                    {
                        row.DefaultCellStyle.ForeColor = Color.DarkGray;
                    }
                }
                else
                {
                    if (log.Level == "Critical")
                    {
                        row.DefaultCellStyle.ForeColor = Color.IndianRed;
                        row.DefaultCellStyle.Font = new Font(dgvRecentLogs.Font, FontStyle.Bold);
                    }
                    else if (log.Level == "Error")
                    {
                        row.DefaultCellStyle.ForeColor = Color.Coral;
                        row.DefaultCellStyle.Font = new Font(dgvRecentLogs.Font, FontStyle.Bold);
                    }
                    else if (log.Level == "Warning")
                    {
                        row.DefaultCellStyle.ForeColor = Color.Goldenrod;
                    }
                }
            }

            if (dgvRecentLogs.Rows.Count > 0)
            {
                dgvRecentLogs.Rows[0].Selected = true;
                DgvRecentLogs_SelectionChanged(null, null);
            }
            else
            {
                txtLogDetail.Text = _showOnlyAlerts
                    ? "🎉 최근 기록된 주의(Warning)나 심각(Error/Critical) 로그가 없습니다. 시스템이 아주 깨끗합니다!"
                    : "🎉 최근 기록된 에러나 경고 로그가 없습니다. 시스템이 아주 깨끗합니다!";
            }
        }

        private void DgvRecentLogs_SelectionChanged(object sender, EventArgs e)
        {
            var row = dgvRecentLogs.CurrentRow ?? (dgvRecentLogs.SelectedRows.Count > 0 ? dgvRecentLogs.SelectedRows[0] : null);
            if (row != null && row.Index >= 0)
            {
                string time = row.Cells[0].Value?.ToString();
                string level = row.Cells[1].Value?.ToString();
                string caller = row.Cells[2].Value?.ToString();
                string msg = row.Cells[3].Value?.ToString();

                txtLogDetail.Text = $"[{time}] [{level}] {caller}\r\n{msg}";
            }
        }

        #endregion

        #region [ 6. 버튼 이벤트들 ]

        private void BtnFilterAlerts_Click(object sender, EventArgs e)
        {
            _showOnlyAlerts = !_showOnlyAlerts;
            btnFilterAlerts.Text = _showOnlyAlerts ? "📋 전체 로그 보기" : "⚠️ 주의/심각만 보기";
            btnFilterAlerts.Type = _showOnlyAlerts ? MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained : MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            UpdateRecentLogsView();
        }

        private void BtnAckLogs_Click(object sender, EventArgs e)
        {
            // 1. 에러 확인(클리어) 처리
            DashboardManager.AcknowledgeErrors();

            // 2. 메시지박스 없이, 사용자가 즉시 주의/심각 로그를 모아볼 수 있도록 자동 전환
            _showOnlyAlerts = true;
            btnFilterAlerts.Text = "📋 전체 로그 보기";
            btnFilterAlerts.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;

            // 3. 전체 뷰 갱신 (상단 헬스 Clean 복구 + 주의/심각 로그 모아서 표시)
            UpdateAllViews();
        }

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
