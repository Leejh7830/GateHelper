namespace GateHelper.Dashboard
{
    partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.pnlTopCards = new System.Windows.Forms.Panel();
            this.cardUptime = new System.Windows.Forms.Panel();
            this.lblUptimeSub = new System.Windows.Forms.Label();
            this.lblUptimeMain = new System.Windows.Forms.Label();
            this.lblUptimeTitle = new System.Windows.Forms.Label();
            this.cardActivity = new System.Windows.Forms.Panel();
            this.lblActivitySub = new System.Windows.Forms.Label();
            this.lblActivityMain = new System.Windows.Forms.Label();
            this.lblActivityTitle = new System.Windows.Forms.Label();
            this.cardHealth = new System.Windows.Forms.Panel();
            this.lblHealthSub = new System.Windows.Forms.Label();
            this.lblHealthMain = new System.Windows.Forms.Label();
            this.lblHealthTitle = new System.Windows.Forms.Label();
            this.cardSandbox = new System.Windows.Forms.Panel();
            this.lblSandboxSub = new System.Windows.Forms.Label();
            this.lblSandboxMain = new System.Windows.Forms.Label();
            this.lblSandboxTitle = new System.Windows.Forms.Label();
            this.tabControlMain = new MaterialSkin.Controls.MaterialTabControl();
            this.tpResource = new System.Windows.Forms.TabPage();
            this.pnlResourceContent = new System.Windows.Forms.Panel();
            this.pnlNetworkCard = new System.Windows.Forms.Panel();
            this.lblNetworkDetails = new System.Windows.Forms.Label();
            this.lblNetworkPing = new System.Windows.Forms.Label();
            this.lblNetworkTitle = new System.Windows.Forms.Label();
            this.pnlDriverCard = new System.Windows.Forms.Panel();
            this.btnGcCollect = new MaterialSkin.Controls.MaterialButton();
            this.lblThreadsHandles = new System.Windows.Forms.Label();
            this.lblDriverStatus = new System.Windows.Forms.Label();
            this.lblDriverTitle = new System.Windows.Forms.Label();
            this.chartTrend = new GateHelper.Dashboard.RealtimeTrendChartControl();
            this.gaugeRam = new GateHelper.Dashboard.SegmentedGaugeControl();
            this.tpServer = new System.Windows.Forms.TabPage();
            this.pnlServerContent = new System.Windows.Forms.Panel();
            this.dgvTopServers = new System.Windows.Forms.DataGridView();
            this.colRank = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAccessCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastConnected = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gaugeAutoLogin = new GateHelper.Dashboard.CircularProgressControl();
            this.chartServerDonut = new GateHelper.Dashboard.DonutChartControl();
            this.heatmapHourly = new GateHelper.Dashboard.HourlyHeatmapControl();
            this.tpSandbox = new System.Windows.Forms.TabPage();
            this.pnlSandboxContent = new System.Windows.Forms.Panel();
            this.bannerMastery = new GateHelper.Dashboard.MasteryBannerControl();
            this.dgvAchievements = new System.Windows.Forms.DataGridView();
            this.colAchStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAchCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAchTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAchDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAchProgress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAchPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tpLogs = new System.Windows.Forms.TabPage();
            this.pnlLogContent = new System.Windows.Forms.Panel();
            this.lblLogTabGuide = new System.Windows.Forms.Label();
            this.btnFilterAlerts = new MaterialSkin.Controls.MaterialButton();
            this.btnAckLogs = new MaterialSkin.Controls.MaterialButton();
            this.btnOpenLogDir = new MaterialSkin.Controls.MaterialButton();
            this.txtLogDetail = new System.Windows.Forms.TextBox();
            this.dgvRecentLogs = new System.Windows.Forms.DataGridView();
            this.colLogTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogCaller = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabSelectorMain = new MaterialSkin.Controls.MaterialTabSelector();
            this.pnlBottomBar = new System.Windows.Forms.Panel();
            this.lblStoragePath = new System.Windows.Forms.Label();
            this.btnRefresh = new MaterialSkin.Controls.MaterialButton();
            this.btnResetStats = new MaterialSkin.Controls.MaterialButton();
            this.pnlTopCards.SuspendLayout();
            this.cardUptime.SuspendLayout();
            this.cardActivity.SuspendLayout();
            this.cardHealth.SuspendLayout();
            this.cardSandbox.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.tpResource.SuspendLayout();
            this.pnlResourceContent.SuspendLayout();
            this.pnlNetworkCard.SuspendLayout();
            this.pnlDriverCard.SuspendLayout();
            this.tpServer.SuspendLayout();
            this.pnlServerContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTopServers)).BeginInit();
            this.tpSandbox.SuspendLayout();
            this.pnlSandboxContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAchievements)).BeginInit();
            this.tpLogs.SuspendLayout();
            this.pnlLogContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecentLogs)).BeginInit();
            this.pnlBottomBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerRefresh
            // 
            this.timerRefresh.Interval = 1200;
            this.timerRefresh.Tick += new System.EventHandler(this.TimerRefresh_Tick);
            // 
            // pnlTopCards
            // 
            this.pnlTopCards.Controls.Add(this.cardUptime);
            this.pnlTopCards.Controls.Add(this.cardActivity);
            this.pnlTopCards.Controls.Add(this.cardHealth);
            this.pnlTopCards.Controls.Add(this.cardSandbox);
            this.pnlTopCards.Location = new System.Drawing.Point(12, 72);
            this.pnlTopCards.Name = "pnlTopCards";
            this.pnlTopCards.Size = new System.Drawing.Size(796, 92);
            this.pnlTopCards.TabIndex = 0;
            // 
            // cardUptime
            // 
            this.cardUptime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.cardUptime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardUptime.Controls.Add(this.lblUptimeSub);
            this.cardUptime.Controls.Add(this.lblUptimeMain);
            this.cardUptime.Controls.Add(this.lblUptimeTitle);
            this.cardUptime.Location = new System.Drawing.Point(3, 4);
            this.cardUptime.Name = "cardUptime";
            this.cardUptime.Size = new System.Drawing.Size(192, 82);
            this.cardUptime.TabIndex = 0;
            // 
            // lblUptimeSub
            // 
            this.lblUptimeSub.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblUptimeSub.ForeColor = System.Drawing.Color.Silver;
            this.lblUptimeSub.Location = new System.Drawing.Point(6, 57);
            this.lblUptimeSub.Name = "lblUptimeSub";
            this.lblUptimeSub.Size = new System.Drawing.Size(178, 20);
            this.lblUptimeSub.TabIndex = 2;
            this.lblUptimeSub.Text = "이번 세션: 00:00:00";
            // 
            // lblUptimeMain
            // 
            this.lblUptimeMain.Font = new System.Drawing.Font("맑은 고딕", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblUptimeMain.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.lblUptimeMain.Location = new System.Drawing.Point(6, 26);
            this.lblUptimeMain.Name = "lblUptimeMain";
            this.lblUptimeMain.Size = new System.Drawing.Size(178, 28);
            this.lblUptimeMain.TabIndex = 1;
            this.lblUptimeMain.Text = "0시간 0분";
            // 
            // lblUptimeTitle
            // 
            this.lblUptimeTitle.AutoSize = true;
            this.lblUptimeTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblUptimeTitle.ForeColor = System.Drawing.Color.White;
            this.lblUptimeTitle.Location = new System.Drawing.Point(6, 6);
            this.lblUptimeTitle.Name = "lblUptimeTitle";
            this.lblUptimeTitle.Size = new System.Drawing.Size(89, 15);
            this.lblUptimeTitle.TabIndex = 0;
            this.lblUptimeTitle.Text = "⏱️ 총 가동 시간";
            // 
            // cardActivity
            // 
            this.cardActivity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.cardActivity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardActivity.Controls.Add(this.lblActivitySub);
            this.cardActivity.Controls.Add(this.lblActivityMain);
            this.cardActivity.Controls.Add(this.lblActivityTitle);
            this.cardActivity.Location = new System.Drawing.Point(201, 4);
            this.cardActivity.Name = "cardActivity";
            this.cardActivity.Size = new System.Drawing.Size(192, 82);
            this.cardActivity.TabIndex = 1;
            // 
            // lblActivitySub
            // 
            this.lblActivitySub.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblActivitySub.ForeColor = System.Drawing.Color.Silver;
            this.lblActivitySub.Location = new System.Drawing.Point(6, 57);
            this.lblActivitySub.Name = "lblActivitySub";
            this.lblActivitySub.Size = new System.Drawing.Size(178, 20);
            this.lblActivitySub.TabIndex = 2;
            this.lblActivitySub.Text = "서버 접속: 0회";
            // 
            // lblActivityMain
            // 
            this.lblActivityMain.Font = new System.Drawing.Font("맑은 고딕", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblActivityMain.ForeColor = System.Drawing.Color.LightGreen;
            this.lblActivityMain.Location = new System.Drawing.Point(6, 26);
            this.lblActivityMain.Name = "lblActivityMain";
            this.lblActivityMain.Size = new System.Drawing.Size(178, 28);
            this.lblActivityMain.TabIndex = 1;
            this.lblActivityMain.Text = "0 회";
            // 
            // lblActivityTitle
            // 
            this.lblActivityTitle.AutoSize = true;
            this.lblActivityTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblActivityTitle.ForeColor = System.Drawing.Color.White;
            this.lblActivityTitle.Location = new System.Drawing.Point(6, 6);
            this.lblActivityTitle.Name = "lblActivityTitle";
            this.lblActivityTitle.Size = new System.Drawing.Size(89, 15);
            this.lblActivityTitle.TabIndex = 0;
            this.lblActivityTitle.Text = "🚀 총 실행 횟수";
            // 
            // cardHealth
            // 
            this.cardHealth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.cardHealth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardHealth.Controls.Add(this.lblHealthSub);
            this.cardHealth.Controls.Add(this.lblHealthMain);
            this.cardHealth.Controls.Add(this.lblHealthTitle);
            this.cardHealth.Location = new System.Drawing.Point(399, 4);
            this.cardHealth.Name = "cardHealth";
            this.cardHealth.Size = new System.Drawing.Size(192, 82);
            this.cardHealth.TabIndex = 2;
            // 
            // lblHealthSub
            // 
            this.lblHealthSub.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblHealthSub.ForeColor = System.Drawing.Color.Silver;
            this.lblHealthSub.Location = new System.Drawing.Point(6, 57);
            this.lblHealthSub.Name = "lblHealthSub";
            this.lblHealthSub.Size = new System.Drawing.Size(178, 20);
            this.lblHealthSub.TabIndex = 2;
            this.lblHealthSub.Text = "누적 에러: 0건";
            // 
            // lblHealthMain
            // 
            this.lblHealthMain.Font = new System.Drawing.Font("맑은 고딕", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblHealthMain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(220)))), ((int)(((byte)(100)))));
            this.lblHealthMain.Location = new System.Drawing.Point(6, 26);
            this.lblHealthMain.Name = "lblHealthMain";
            this.lblHealthMain.Size = new System.Drawing.Size(178, 28);
            this.lblHealthMain.TabIndex = 1;
            this.lblHealthMain.Text = "Clean ✨";
            // 
            // lblHealthTitle
            // 
            this.lblHealthTitle.AutoSize = true;
            this.lblHealthTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblHealthTitle.ForeColor = System.Drawing.Color.White;
            this.lblHealthTitle.Location = new System.Drawing.Point(6, 6);
            this.lblHealthTitle.Name = "lblHealthTitle";
            this.lblHealthTitle.Size = new System.Drawing.Size(89, 15);
            this.lblHealthTitle.TabIndex = 0;
            this.lblHealthTitle.Text = "🛡️ 시스템 헬스";
            // 
            // cardSandbox
            // 
            this.cardSandbox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.cardSandbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardSandbox.Controls.Add(this.lblSandboxSub);
            this.cardSandbox.Controls.Add(this.lblSandboxMain);
            this.cardSandbox.Controls.Add(this.lblSandboxTitle);
            this.cardSandbox.Location = new System.Drawing.Point(597, 4);
            this.cardSandbox.Name = "cardSandbox";
            this.cardSandbox.Size = new System.Drawing.Size(192, 82);
            this.cardSandbox.TabIndex = 3;
            // 
            // lblSandboxSub
            // 
            this.lblSandboxSub.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSandboxSub.ForeColor = System.Drawing.Color.Silver;
            this.lblSandboxSub.Location = new System.Drawing.Point(6, 57);
            this.lblSandboxSub.Name = "lblSandboxSub";
            this.lblSandboxSub.Size = new System.Drawing.Size(178, 20);
            this.lblSandboxSub.TabIndex = 2;
            this.lblSandboxSub.Text = "미스터리 추리 / 트렌드";
            // 
            // lblSandboxMain
            // 
            this.lblSandboxMain.Font = new System.Drawing.Font("맑은 고딕", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSandboxMain.ForeColor = System.Drawing.Color.Gold;
            this.lblSandboxMain.Location = new System.Drawing.Point(6, 26);
            this.lblSandboxMain.Name = "lblSandboxMain";
            this.lblSandboxMain.Size = new System.Drawing.Size(178, 28);
            this.lblSandboxMain.TabIndex = 1;
            this.lblSandboxMain.Text = "0 회 클리어";
            // 
            // lblSandboxTitle
            // 
            this.lblSandboxTitle.AutoSize = true;
            this.lblSandboxTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSandboxTitle.ForeColor = System.Drawing.Color.White;
            this.lblSandboxTitle.Location = new System.Drawing.Point(6, 6);
            this.lblSandboxTitle.Name = "lblSandboxTitle";
            this.lblSandboxTitle.Size = new System.Drawing.Size(97, 15);
            this.lblSandboxTitle.TabIndex = 0;
            this.lblSandboxTitle.Text = "🏆 샌드박스 정복";
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tpResource);
            this.tabControlMain.Controls.Add(this.tpServer);
            this.tabControlMain.Controls.Add(this.tpSandbox);
            this.tabControlMain.Controls.Add(this.tpLogs);
            this.tabControlMain.Depth = 0;
            this.tabControlMain.Location = new System.Drawing.Point(12, 208);
            this.tabControlMain.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabControlMain.Multiline = true;
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(796, 396);
            this.tabControlMain.TabIndex = 1;
            // 
            // tpResource
            // 
            this.tpResource.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tpResource.Controls.Add(this.pnlResourceContent);
            this.tpResource.Location = new System.Drawing.Point(4, 22);
            this.tpResource.Name = "tpResource";
            this.tpResource.Padding = new System.Windows.Forms.Padding(3);
            this.tpResource.Size = new System.Drawing.Size(788, 364);
            this.tpResource.TabIndex = 0;
            this.tpResource.Text = "💻 리소스 & 헬스 모니터";
            // 
            // pnlResourceContent
            // 
            this.pnlResourceContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlResourceContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlResourceContent.Controls.Add(this.pnlNetworkCard);
            this.pnlResourceContent.Controls.Add(this.pnlDriverCard);
            this.pnlResourceContent.Controls.Add(this.chartTrend);
            this.pnlResourceContent.Controls.Add(this.gaugeRam);
            this.pnlResourceContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlResourceContent.Location = new System.Drawing.Point(3, 3);
            this.pnlResourceContent.Name = "pnlResourceContent";
            this.pnlResourceContent.Size = new System.Drawing.Size(782, 328);
            this.pnlResourceContent.TabIndex = 0;
            // 
            // pnlNetworkCard
            // 
            this.pnlNetworkCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(46)))));
            this.pnlNetworkCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlNetworkCard.Controls.Add(this.lblNetworkDetails);
            this.pnlNetworkCard.Controls.Add(this.lblNetworkPing);
            this.pnlNetworkCard.Controls.Add(this.lblNetworkTitle);
            this.pnlNetworkCard.Location = new System.Drawing.Point(12, 175);
            this.pnlNetworkCard.Name = "pnlNetworkCard";
            this.pnlNetworkCard.Size = new System.Drawing.Size(350, 175);
            this.pnlNetworkCard.TabIndex = 3;
            // 
            // lblNetworkDetails
            // 
            this.lblNetworkDetails.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblNetworkDetails.ForeColor = System.Drawing.Color.Silver;
            this.lblNetworkDetails.Location = new System.Drawing.Point(15, 95);
            this.lblNetworkDetails.Name = "lblNetworkDetails";
            this.lblNetworkDetails.Size = new System.Drawing.Size(320, 60);
            this.lblNetworkDetails.TabIndex = 2;
            this.lblNetworkDetails.Text = "• 사내망/게이트웨이 통신 상태: 매우 양호\n• 실시간 3초 주기 ICMP 핑 패킷 측정";
            // 
            // lblNetworkPing
            // 
            this.lblNetworkPing.Font = new System.Drawing.Font("맑은 고딕", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblNetworkPing.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblNetworkPing.Location = new System.Drawing.Point(15, 42);
            this.lblNetworkPing.Name = "lblNetworkPing";
            this.lblNetworkPing.Size = new System.Drawing.Size(320, 36);
            this.lblNetworkPing.TabIndex = 1;
            this.lblNetworkPing.Text = "14 ms (초고속)";
            // 
            // lblNetworkTitle
            // 
            this.lblNetworkTitle.AutoSize = true;
            this.lblNetworkTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblNetworkTitle.ForeColor = System.Drawing.Color.White;
            this.lblNetworkTitle.Location = new System.Drawing.Point(15, 12);
            this.lblNetworkTitle.Name = "lblNetworkTitle";
            this.lblNetworkTitle.Size = new System.Drawing.Size(211, 19);
            this.lblNetworkTitle.TabIndex = 0;
            this.lblNetworkTitle.Text = "🌐 네트워크 응답 지연 (Latency)";
            // 
            // pnlDriverCard
            // 
            this.pnlDriverCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(46)))));
            this.pnlDriverCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDriverCard.Controls.Add(this.btnGcCollect);
            this.pnlDriverCard.Controls.Add(this.lblThreadsHandles);
            this.pnlDriverCard.Controls.Add(this.lblDriverStatus);
            this.pnlDriverCard.Controls.Add(this.lblDriverTitle);
            this.pnlDriverCard.Location = new System.Drawing.Point(375, 175);
            this.pnlDriverCard.Name = "pnlDriverCard";
            this.pnlDriverCard.Size = new System.Drawing.Size(395, 175);
            this.pnlDriverCard.TabIndex = 2;
            // 
            // btnGcCollect
            // 
            this.btnGcCollect.AutoSize = false;
            this.btnGcCollect.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnGcCollect.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnGcCollect.Depth = 0;
            this.btnGcCollect.HighEmphasis = true;
            this.btnGcCollect.Icon = null;
            this.btnGcCollect.Location = new System.Drawing.Point(18, 118);
            this.btnGcCollect.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnGcCollect.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnGcCollect.Name = "btnGcCollect";
            this.btnGcCollect.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnGcCollect.Size = new System.Drawing.Size(360, 36);
            this.btnGcCollect.TabIndex = 3;
            this.btnGcCollect.Text = "🧹 RAM 즉시 최적화 정리 (GC)";
            this.btnGcCollect.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnGcCollect.UseAccentColor = false;
            this.btnGcCollect.UseVisualStyleBackColor = true;
            this.btnGcCollect.Click += new System.EventHandler(this.BtnGcCollect_Click);
            // 
            // lblThreadsHandles
            // 
            this.lblThreadsHandles.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblThreadsHandles.ForeColor = System.Drawing.Color.DarkGray;
            this.lblThreadsHandles.Location = new System.Drawing.Point(15, 75);
            this.lblThreadsHandles.Name = "lblThreadsHandles";
            this.lblThreadsHandles.Size = new System.Drawing.Size(365, 20);
            this.lblThreadsHandles.TabIndex = 2;
            this.lblThreadsHandles.Text = "스레드: 0개 | 핸들: 0개 | Private: 0MB";
            // 
            // lblDriverStatus
            // 
            this.lblDriverStatus.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDriverStatus.ForeColor = System.Drawing.Color.LightGreen;
            this.lblDriverStatus.Location = new System.Drawing.Point(15, 38);
            this.lblDriverStatus.Name = "lblDriverStatus";
            this.lblDriverStatus.Size = new System.Drawing.Size(365, 22);
            this.lblDriverStatus.TabIndex = 1;
            this.lblDriverStatus.Text = "ChromeDriver: 활성 | Selenium: 연결됨";
            // 
            // lblDriverTitle
            // 
            this.lblDriverTitle.AutoSize = true;
            this.lblDriverTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDriverTitle.ForeColor = System.Drawing.Color.White;
            this.lblDriverTitle.Location = new System.Drawing.Point(15, 12);
            this.lblDriverTitle.Name = "lblDriverTitle";
            this.lblDriverTitle.Size = new System.Drawing.Size(189, 19);
            this.lblDriverTitle.TabIndex = 0;
            this.lblDriverTitle.Text = "⚡ Selenium 드라이버 상태";
            // 
            // chartTrend
            // 
            this.chartTrend.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.chartTrend.Location = new System.Drawing.Point(375, 12);
            this.chartTrend.Name = "chartTrend";
            this.chartTrend.Size = new System.Drawing.Size(395, 150);
            this.chartTrend.TabIndex = 1;
            // 
            // gaugeRam
            // 
            this.gaugeRam.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.gaugeRam.Location = new System.Drawing.Point(12, 12);
            this.gaugeRam.Maximum = 512D;
            this.gaugeRam.Name = "gaugeRam";
            this.gaugeRam.Size = new System.Drawing.Size(350, 150);
            this.gaugeRam.TabIndex = 0;
            this.gaugeRam.Title = "⚡ GateHelper RAM (WorkingSet)";
            this.gaugeRam.Value = 0D;
            this.gaugeRam.ValueText = "0 MB";
            // 
            // tpServer
            // 
            this.tpServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tpServer.Controls.Add(this.pnlServerContent);
            this.tpServer.Location = new System.Drawing.Point(4, 22);
            this.tpServer.Name = "tpServer";
            this.tpServer.Padding = new System.Windows.Forms.Padding(3);
            this.tpServer.Size = new System.Drawing.Size(788, 364);
            this.tpServer.TabIndex = 1;
            this.tpServer.Text = "🚀 서버 접속 통계";
            // 
            // pnlServerContent
            // 
            this.pnlServerContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlServerContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlServerContent.Controls.Add(this.dgvTopServers);
            this.pnlServerContent.Controls.Add(this.heatmapHourly);
            this.pnlServerContent.Controls.Add(this.gaugeAutoLogin);
            this.pnlServerContent.Controls.Add(this.chartServerDonut);
            this.pnlServerContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlServerContent.Location = new System.Drawing.Point(0, 0);
            this.pnlServerContent.Name = "pnlServerContent";
            this.pnlServerContent.Size = new System.Drawing.Size(788, 364);
            this.pnlServerContent.TabIndex = 0;
            // 
            // dgvTopServers
            // 
            this.dgvTopServers.AllowUserToAddRows = false;
            this.dgvTopServers.AllowUserToDeleteRows = false;
            this.dgvTopServers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTopServers.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.dgvTopServers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTopServers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRank,
            this.colServerName,
            this.colAccessCount,
            this.colLastConnected});
            this.dgvTopServers.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.dgvTopServers.Location = new System.Drawing.Point(415, 12);
            this.dgvTopServers.Name = "dgvTopServers";
            this.dgvTopServers.ReadOnly = true;
            this.dgvTopServers.RowHeadersVisible = false;
            this.dgvTopServers.RowTemplate.Height = 27;
            this.dgvTopServers.Size = new System.Drawing.Size(358, 255);
            this.dgvTopServers.TabIndex = 2;
            // 
            // colRank
            // 
            this.colRank.FillWeight = 55F;
            this.colRank.HeaderText = "순위";
            this.colRank.MinimumWidth = 55;
            this.colRank.Name = "colRank";
            this.colRank.ReadOnly = true;
            this.colRank.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colRank.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colServerName
            // 
            this.colServerName.FillWeight = 110F;
            this.colServerName.HeaderText = "서버명";
            this.colServerName.MinimumWidth = 90;
            this.colServerName.Name = "colServerName";
            this.colServerName.ReadOnly = true;
            this.colServerName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colAccessCount
            // 
            this.colAccessCount.FillWeight = 55F;
            this.colAccessCount.HeaderText = "접속수";
            this.colAccessCount.MinimumWidth = 50;
            this.colAccessCount.Name = "colAccessCount";
            this.colAccessCount.ReadOnly = true;
            this.colAccessCount.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colLastConnected
            // 
            this.colLastConnected.FillWeight = 85F;
            this.colLastConnected.HeaderText = "최근 접속";
            this.colLastConnected.MinimumWidth = 75;
            this.colLastConnected.Name = "colLastConnected";
            this.colLastConnected.ReadOnly = true;
            this.colLastConnected.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // gaugeAutoLogin
            // 
            this.gaugeAutoLogin.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.gaugeAutoLogin.Location = new System.Drawing.Point(215, 12);
            this.gaugeAutoLogin.Name = "gaugeAutoLogin";
            this.gaugeAutoLogin.RingColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.gaugeAutoLogin.Size = new System.Drawing.Size(190, 255);
            this.gaugeAutoLogin.SubTitle = "자동 로그인 성공률";
            this.gaugeAutoLogin.TabIndex = 1;
            this.gaugeAutoLogin.Value = 0D;
            // 
            // chartServerDonut
            // 
            this.chartServerDonut.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.chartServerDonut.Location = new System.Drawing.Point(15, 12);
            this.chartServerDonut.Name = "chartServerDonut";
            this.chartServerDonut.Size = new System.Drawing.Size(190, 255);
            this.chartServerDonut.TabIndex = 0;
            // 
            // heatmapHourly
            // 
            this.heatmapHourly.Font = new System.Drawing.Font("맑은 고딕", 8.5F);
            this.heatmapHourly.Location = new System.Drawing.Point(15, 278);
            this.heatmapHourly.Name = "heatmapHourly";
            this.heatmapHourly.Size = new System.Drawing.Size(758, 68);
            this.heatmapHourly.TabIndex = 3;
            // 
            // tpSandbox
            // 
            this.tpSandbox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tpSandbox.Controls.Add(this.pnlSandboxContent);
            this.tpSandbox.Location = new System.Drawing.Point(4, 22);
            this.tpSandbox.Name = "tpSandbox";
            this.tpSandbox.Size = new System.Drawing.Size(788, 364);
            this.tpSandbox.TabIndex = 2;
            this.tpSandbox.Text = "🎮 샌드박스 & 업적";
            // 
            // pnlSandboxContent
            // 
            this.pnlSandboxContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlSandboxContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSandboxContent.Controls.Add(this.dgvAchievements);
            this.pnlSandboxContent.Controls.Add(this.bannerMastery);
            this.pnlSandboxContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSandboxContent.Location = new System.Drawing.Point(0, 0);
            this.pnlSandboxContent.Name = "pnlSandboxContent";
            this.pnlSandboxContent.Size = new System.Drawing.Size(788, 364);
            this.pnlSandboxContent.TabIndex = 0;
            // 
            // bannerMastery
            // 
            this.bannerMastery.Location = new System.Drawing.Point(15, 10);
            this.bannerMastery.Name = "bannerMastery";
            this.bannerMastery.Size = new System.Drawing.Size(755, 116);
            this.bannerMastery.TabIndex = 0;
            // 
            // dgvAchievements
            // 
            this.dgvAchievements.AllowUserToAddRows = false;
            this.dgvAchievements.AllowUserToDeleteRows = false;
            this.dgvAchievements.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAchievements.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.dgvAchievements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAchievements.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAchStatus,
            this.colAchCategory,
            this.colAchTitle,
            this.colAchDesc,
            this.colAchProgress,
            this.colAchPoints});
            this.dgvAchievements.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.dgvAchievements.Location = new System.Drawing.Point(15, 134);
            this.dgvAchievements.MultiSelect = false;
            this.dgvAchievements.Name = "dgvAchievements";
            this.dgvAchievements.ReadOnly = true;
            this.dgvAchievements.RowHeadersVisible = false;
            this.dgvAchievements.RowTemplate.Height = 28;
            this.dgvAchievements.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAchievements.Size = new System.Drawing.Size(755, 218);
            this.dgvAchievements.TabIndex = 1;
            // 
            // colAchStatus
            // 
            this.colAchStatus.FillWeight = 38F;
            this.colAchStatus.HeaderText = "상태";
            this.colAchStatus.Name = "colAchStatus";
            this.colAchStatus.ReadOnly = true;
            // 
            // colAchCategory
            // 
            this.colAchCategory.FillWeight = 55F;
            this.colAchCategory.HeaderText = "분야";
            this.colAchCategory.Name = "colAchCategory";
            this.colAchCategory.ReadOnly = true;
            // 
            // colAchTitle
            // 
            this.colAchTitle.FillWeight = 85F;
            this.colAchTitle.HeaderText = "도전과제 명칭";
            this.colAchTitle.Name = "colAchTitle";
            this.colAchTitle.ReadOnly = true;
            // 
            // colAchDesc
            // 
            this.colAchDesc.FillWeight = 135F;
            this.colAchDesc.HeaderText = "달성 조건";
            this.colAchDesc.Name = "colAchDesc";
            this.colAchDesc.ReadOnly = true;
            // 
            // colAchProgress
            // 
            this.colAchProgress.FillWeight = 65F;
            this.colAchProgress.HeaderText = "진척도";
            this.colAchProgress.Name = "colAchProgress";
            this.colAchProgress.ReadOnly = true;
            // 
            // colAchPoints
            // 
            this.colAchPoints.FillWeight = 42F;
            this.colAchPoints.HeaderText = "보상";
            this.colAchPoints.Name = "colAchPoints";
            this.colAchPoints.ReadOnly = true;
            // 
            // tpLogs
            // 
            this.tpLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tpLogs.Controls.Add(this.pnlLogContent);
            this.tpLogs.Location = new System.Drawing.Point(4, 22);
            this.tpLogs.Name = "tpLogs";
            this.tpLogs.Size = new System.Drawing.Size(788, 364);
            this.tpLogs.TabIndex = 3;
            this.tpLogs.Text = "🚨 최근 중요 로그";
            // 
            // pnlLogContent
            // 
            this.pnlLogContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlLogContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLogContent.Controls.Add(this.lblLogTabGuide);
            this.pnlLogContent.Controls.Add(this.btnFilterAlerts);
            this.pnlLogContent.Controls.Add(this.btnAckLogs);
            this.pnlLogContent.Controls.Add(this.btnOpenLogDir);
            this.pnlLogContent.Controls.Add(this.txtLogDetail);
            this.pnlLogContent.Controls.Add(this.dgvRecentLogs);
            this.pnlLogContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLogContent.Location = new System.Drawing.Point(0, 0);
            this.pnlLogContent.Name = "pnlLogContent";
            this.pnlLogContent.Size = new System.Drawing.Size(788, 364);
            this.pnlLogContent.TabIndex = 0;
            // 
            // lblLogTabGuide
            // 
            this.lblLogTabGuide.AutoSize = true;
            this.lblLogTabGuide.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblLogTabGuide.ForeColor = System.Drawing.Color.DarkGray;
            this.lblLogTabGuide.Location = new System.Drawing.Point(15, 13);
            this.lblLogTabGuide.Name = "lblLogTabGuide";
            this.lblLogTabGuide.Size = new System.Drawing.Size(260, 15);
            this.lblLogTabGuide.TabIndex = 2;
            this.lblLogTabGuide.Text = "💡 시스템 이벤트 & 최근 중요 로그 모니터링";
            // 
            // btnFilterAlerts
            // 
            this.btnFilterAlerts.AutoSize = false;
            this.btnFilterAlerts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnFilterAlerts.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnFilterAlerts.Depth = 0;
            this.btnFilterAlerts.HighEmphasis = false;
            this.btnFilterAlerts.Icon = null;
            this.btnFilterAlerts.Location = new System.Drawing.Point(325, 5);
            this.btnFilterAlerts.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnFilterAlerts.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnFilterAlerts.Name = "btnFilterAlerts";
            this.btnFilterAlerts.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnFilterAlerts.Size = new System.Drawing.Size(140, 32);
            this.btnFilterAlerts.TabIndex = 5;
            this.btnFilterAlerts.Text = "⚠️ 주의/심각만 보기";
            this.btnFilterAlerts.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnFilterAlerts.UseAccentColor = false;
            this.btnFilterAlerts.UseVisualStyleBackColor = true;
            this.btnFilterAlerts.Click += new System.EventHandler(this.BtnFilterAlerts_Click);
            // 
            // btnAckLogs
            // 
            this.btnAckLogs.AutoSize = false;
            this.btnAckLogs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAckLogs.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnAckLogs.Depth = 0;
            this.btnAckLogs.HighEmphasis = true;
            this.btnAckLogs.Icon = null;
            this.btnAckLogs.Location = new System.Drawing.Point(475, 5);
            this.btnAckLogs.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAckLogs.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAckLogs.Name = "btnAckLogs";
            this.btnAckLogs.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnAckLogs.Size = new System.Drawing.Size(160, 32);
            this.btnAckLogs.TabIndex = 4;
            this.btnAckLogs.Text = "✔️ 로그 확인 (클리어)";
            this.btnAckLogs.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAckLogs.UseAccentColor = false;
            this.btnAckLogs.UseVisualStyleBackColor = true;
            this.btnAckLogs.Click += new System.EventHandler(this.BtnAckLogs_Click);
            // 
            // btnOpenLogDir
            // 
            this.btnOpenLogDir.AutoSize = false;
            this.btnOpenLogDir.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOpenLogDir.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnOpenLogDir.Depth = 0;
            this.btnOpenLogDir.HighEmphasis = false;
            this.btnOpenLogDir.Icon = null;
            this.btnOpenLogDir.Location = new System.Drawing.Point(645, 5);
            this.btnOpenLogDir.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOpenLogDir.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnOpenLogDir.Name = "btnOpenLogDir";
            this.btnOpenLogDir.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnOpenLogDir.Size = new System.Drawing.Size(125, 32);
            this.btnOpenLogDir.TabIndex = 3;
            this.btnOpenLogDir.Text = "📂 로그 폴더 열기";
            this.btnOpenLogDir.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnOpenLogDir.UseAccentColor = false;
            this.btnOpenLogDir.UseVisualStyleBackColor = true;
            this.btnOpenLogDir.Click += new System.EventHandler(this.BtnOpenLogDir_Click);
            // 
            // txtLogDetail
            // 
            this.txtLogDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(26)))));
            this.txtLogDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLogDetail.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtLogDetail.ForeColor = System.Drawing.Color.Gainsboro;
            this.txtLogDetail.Location = new System.Drawing.Point(15, 248);
            this.txtLogDetail.Multiline = true;
            this.txtLogDetail.Name = "txtLogDetail";
            this.txtLogDetail.ReadOnly = true;
            this.txtLogDetail.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogDetail.Size = new System.Drawing.Size(755, 102);
            this.txtLogDetail.TabIndex = 1;
            // 
            // dgvRecentLogs
            // 
            this.dgvRecentLogs.AllowUserToAddRows = false;
            this.dgvRecentLogs.AllowUserToDeleteRows = false;
            this.dgvRecentLogs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRecentLogs.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.dgvRecentLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecentLogs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLogTime,
            this.colLogLevel,
            this.colLogCaller,
            this.colLogMsg});
            this.dgvRecentLogs.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.dgvRecentLogs.Location = new System.Drawing.Point(15, 42);
            this.dgvRecentLogs.MultiSelect = false;
            this.dgvRecentLogs.Name = "dgvRecentLogs";
            this.dgvRecentLogs.ReadOnly = true;
            this.dgvRecentLogs.RowHeadersVisible = false;
            this.dgvRecentLogs.RowTemplate.Height = 25;
            this.dgvRecentLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecentLogs.Size = new System.Drawing.Size(755, 198);
            this.dgvRecentLogs.TabIndex = 0;
            this.dgvRecentLogs.SelectionChanged += new System.EventHandler(this.DgvRecentLogs_SelectionChanged);
            // 
            // colLogTime
            // 
            this.colLogTime.FillWeight = 55F;
            this.colLogTime.HeaderText = "발생 시각";
            this.colLogTime.Name = "colLogTime";
            this.colLogTime.ReadOnly = true;
            // 
            // colLogLevel
            // 
            this.colLogLevel.FillWeight = 35F;
            this.colLogLevel.HeaderText = "레벨";
            this.colLogLevel.Name = "colLogLevel";
            this.colLogLevel.ReadOnly = true;
            // 
            // colLogCaller
            // 
            this.colLogCaller.FillWeight = 60F;
            this.colLogCaller.HeaderText = "호출부";
            this.colLogCaller.Name = "colLogCaller";
            this.colLogCaller.ReadOnly = true;
            // 
            // colLogMsg
            // 
            this.colLogMsg.FillWeight = 150F;
            this.colLogMsg.HeaderText = "로그 내용";
            this.colLogMsg.Name = "colLogMsg";
            this.colLogMsg.ReadOnly = true;
            // 
            // tabSelectorMain
            // 
            this.tabSelectorMain.BaseTabControl = this.tabControlMain;
            this.tabSelectorMain.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.tabSelectorMain.Depth = 0;
            this.tabSelectorMain.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabSelectorMain.Location = new System.Drawing.Point(12, 172);
            this.tabSelectorMain.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabSelectorMain.Name = "tabSelectorMain";
            this.tabSelectorMain.Size = new System.Drawing.Size(796, 36);
            this.tabSelectorMain.TabIndex = 2;
            // 
            // pnlBottomBar
            // 
            this.pnlBottomBar.Controls.Add(this.lblStoragePath);
            this.pnlBottomBar.Controls.Add(this.btnRefresh);
            this.pnlBottomBar.Controls.Add(this.btnResetStats);
            this.pnlBottomBar.Location = new System.Drawing.Point(12, 608);
            this.pnlBottomBar.Name = "pnlBottomBar";
            this.pnlBottomBar.Size = new System.Drawing.Size(796, 50);
            this.pnlBottomBar.TabIndex = 3;
            // 
            // lblStoragePath
            // 
            this.lblStoragePath.AutoEllipsis = true;
            this.lblStoragePath.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblStoragePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.lblStoragePath.Location = new System.Drawing.Point(15, 16);
            this.lblStoragePath.Name = "lblStoragePath";
            this.lblStoragePath.Size = new System.Drawing.Size(500, 20);
            this.lblStoragePath.TabIndex = 4;
            this.lblStoragePath.Text = "🕒 마지막 동기화: --:--:-- · 실시간 모니터링 중";
            // 
            // btnRefresh
            // 
            this.btnRefresh.AutoSize = false;
            this.btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRefresh.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRefresh.Depth = 0;
            this.btnRefresh.HighEmphasis = true;
            this.btnRefresh.Icon = null;
            this.btnRefresh.Location = new System.Drawing.Point(545, 7);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRefresh.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRefresh.Size = new System.Drawing.Size(115, 36);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "🔄 새로고침";
            this.btnRefresh.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRefresh.UseAccentColor = false;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // btnResetStats
            // 
            this.btnResetStats.AutoSize = false;
            this.btnResetStats.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnResetStats.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnResetStats.Depth = 0;
            this.btnResetStats.HighEmphasis = false;
            this.btnResetStats.Icon = null;
            this.btnResetStats.Location = new System.Drawing.Point(670, 7);
            this.btnResetStats.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnResetStats.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnResetStats.Name = "btnResetStats";
            this.btnResetStats.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnResetStats.Size = new System.Drawing.Size(115, 36);
            this.btnResetStats.TabIndex = 0;
            this.btnResetStats.Text = "⚠️ 통계 초기화";
            this.btnResetStats.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnResetStats.UseAccentColor = true;
            this.btnResetStats.UseVisualStyleBackColor = true;
            this.btnResetStats.Click += new System.EventHandler(this.BtnResetStats_Click);
            // 
            // DashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 670);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Controls.Add(this.pnlBottomBar);
            this.Controls.Add(this.tabSelectorMain);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.pnlTopCards);
            this.MaximizeBox = false;
            this.Name = "DashboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "📊 GateHelper Analytics & System Dashboard";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DashboardForm_FormClosing);
            this.Load += new System.EventHandler(this.DashboardForm_Load);
            this.pnlTopCards.ResumeLayout(false);
            this.cardUptime.ResumeLayout(false);
            this.cardUptime.PerformLayout();
            this.cardActivity.ResumeLayout(false);
            this.cardActivity.PerformLayout();
            this.cardHealth.ResumeLayout(false);
            this.cardHealth.PerformLayout();
            this.cardSandbox.ResumeLayout(false);
            this.cardSandbox.PerformLayout();
            this.tabControlMain.ResumeLayout(false);
            this.tpResource.ResumeLayout(false);
            this.pnlResourceContent.ResumeLayout(false);
            this.pnlNetworkCard.ResumeLayout(false);
            this.pnlNetworkCard.PerformLayout();
            this.pnlDriverCard.ResumeLayout(false);
            this.pnlDriverCard.PerformLayout();
            this.tpServer.ResumeLayout(false);
            this.pnlServerContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTopServers)).EndInit();
            this.tpSandbox.ResumeLayout(false);
            this.pnlSandboxContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAchievements)).EndInit();
            this.tpLogs.ResumeLayout(false);
            this.pnlLogContent.ResumeLayout(false);
            this.pnlLogContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecentLogs)).EndInit();
            this.pnlBottomBar.ResumeLayout(false);
            this.pnlBottomBar.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.Panel pnlTopCards;
        private System.Windows.Forms.Panel cardUptime;
        private System.Windows.Forms.Label lblUptimeSub;
        private System.Windows.Forms.Label lblUptimeMain;
        private System.Windows.Forms.Label lblUptimeTitle;
        private System.Windows.Forms.Panel cardActivity;
        private System.Windows.Forms.Label lblActivitySub;
        private System.Windows.Forms.Label lblActivityMain;
        private System.Windows.Forms.Label lblActivityTitle;
        private System.Windows.Forms.Panel cardHealth;
        private System.Windows.Forms.Label lblHealthSub;
        private System.Windows.Forms.Label lblHealthMain;
        private System.Windows.Forms.Label lblHealthTitle;
        private System.Windows.Forms.Panel cardSandbox;
        private System.Windows.Forms.Label lblSandboxSub;
        private System.Windows.Forms.Label lblSandboxMain;
        private System.Windows.Forms.Label lblSandboxTitle;
        private MaterialSkin.Controls.MaterialTabControl tabControlMain;
        private System.Windows.Forms.TabPage tpResource;
        private System.Windows.Forms.TabPage tpServer;
        private System.Windows.Forms.TabPage tpSandbox;
        private System.Windows.Forms.TabPage tpLogs;
        private MaterialSkin.Controls.MaterialTabSelector tabSelectorMain;
        private System.Windows.Forms.Panel pnlResourceContent;
        private SegmentedGaugeControl gaugeRam;
        private RealtimeTrendChartControl chartTrend;
        private System.Windows.Forms.Panel pnlNetworkCard;
        private System.Windows.Forms.Label lblNetworkTitle;
        private System.Windows.Forms.Label lblNetworkPing;
        private System.Windows.Forms.Label lblNetworkDetails;
        private System.Windows.Forms.Panel pnlDriverCard;
        private System.Windows.Forms.Label lblDriverTitle;
        private System.Windows.Forms.Label lblDriverStatus;
        private System.Windows.Forms.Label lblThreadsHandles;
        private MaterialSkin.Controls.MaterialButton btnGcCollect;
        private System.Windows.Forms.Panel pnlServerContent;
        private DonutChartControl chartServerDonut;
        private CircularProgressControl gaugeAutoLogin;
        private HourlyHeatmapControl heatmapHourly;
        private System.Windows.Forms.DataGridView dgvTopServers;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRank;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAccessCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastConnected;
        private System.Windows.Forms.Panel pnlSandboxContent;
        private MasteryBannerControl bannerMastery;
        private System.Windows.Forms.DataGridView dgvAchievements;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAchStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAchCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAchTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAchDesc;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAchProgress;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAchPoints;
        private System.Windows.Forms.Panel pnlLogContent;
        private System.Windows.Forms.DataGridView dgvRecentLogs;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogLevel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogCaller;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogMsg;
        private System.Windows.Forms.TextBox txtLogDetail;
        private System.Windows.Forms.Panel pnlBottomBar;
        private System.Windows.Forms.Label lblStoragePath;
        private MaterialSkin.Controls.MaterialButton btnResetStats;
        private MaterialSkin.Controls.MaterialButton btnRefresh;
        private System.Windows.Forms.Label lblLogTabGuide;
        private MaterialSkin.Controls.MaterialButton btnFilterAlerts;
        private MaterialSkin.Controls.MaterialButton btnAckLogs;
        private MaterialSkin.Controls.MaterialButton btnOpenLogDir;
    }
}
