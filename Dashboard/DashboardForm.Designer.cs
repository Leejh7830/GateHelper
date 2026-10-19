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
            this.colPercentage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gaugeAutoLogin = new GateHelper.Dashboard.CircularProgressControl();
            this.chartServerDonut = new GateHelper.Dashboard.DonutChartControl();
            this.tpSandbox = new System.Windows.Forms.TabPage();
            this.pnlSandboxContent = new System.Windows.Forms.Panel();
            this.pnlBoxAiTrend = new System.Windows.Forms.Panel();
            this.lblAiTrendDetails = new System.Windows.Forms.Label();
            this.lblAiTrendTitle = new System.Windows.Forms.Label();
            this.pnlBoxMystery = new System.Windows.Forms.Panel();
            this.lblMysteryDetails = new System.Windows.Forms.Label();
            this.lblMysteryTitle = new System.Windows.Forms.Label();
            this.pnlBoxSignalLink = new System.Windows.Forms.Panel();
            this.lblSignalLinkDetails = new System.Windows.Forms.Label();
            this.lblSignalLinkTitle = new System.Windows.Forms.Label();
            this.pnlBoxBitFlip = new System.Windows.Forms.Panel();
            this.lblBitFlipDetails = new System.Windows.Forms.Label();
            this.lblBitFlipTitle = new System.Windows.Forms.Label();
            this.tpLogs = new System.Windows.Forms.TabPage();
            this.pnlLogContent = new System.Windows.Forms.Panel();
            this.txtLogDetail = new System.Windows.Forms.TextBox();
            this.dgvRecentLogs = new System.Windows.Forms.DataGridView();
            this.colLogTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogCaller = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabSelectorMain = new MaterialSkin.Controls.MaterialTabSelector();
            this.pnlBottomBar = new System.Windows.Forms.Panel();
            this.lblStoragePath = new System.Windows.Forms.Label();
            this.btnOpenLogDir = new MaterialSkin.Controls.MaterialButton();
            this.btnOpenDashboardDir = new MaterialSkin.Controls.MaterialButton();
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
            this.pnlBoxAiTrend.SuspendLayout();
            this.pnlBoxMystery.SuspendLayout();
            this.pnlBoxSignalLink.SuspendLayout();
            this.pnlBoxBitFlip.SuspendLayout();
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
            this.tabControlMain.Location = new System.Drawing.Point(12, 212);
            this.tabControlMain.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabControlMain.Multiline = true;
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(796, 360);
            this.tabControlMain.TabIndex = 1;
            // 
            // tpResource
            // 
            this.tpResource.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tpResource.Controls.Add(this.pnlResourceContent);
            this.tpResource.Location = new System.Drawing.Point(4, 22);
            this.tpResource.Name = "tpResource";
            this.tpResource.Padding = new System.Windows.Forms.Padding(3);
            this.tpResource.Size = new System.Drawing.Size(788, 334);
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
            this.pnlNetworkCard.Size = new System.Drawing.Size(350, 140);
            this.pnlNetworkCard.TabIndex = 3;
            // 
            // lblNetworkDetails
            // 
            this.lblNetworkDetails.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblNetworkDetails.ForeColor = System.Drawing.Color.Silver;
            this.lblNetworkDetails.Location = new System.Drawing.Point(15, 88);
            this.lblNetworkDetails.Name = "lblNetworkDetails";
            this.lblNetworkDetails.Size = new System.Drawing.Size(320, 42);
            this.lblNetworkDetails.TabIndex = 2;
            this.lblNetworkDetails.Text = "• 사내망/게이트웨이 통신 상태: 매우 양호\n• 실시간 3초 주기 ICMP 핑 패킷 측정";
            // 
            // lblNetworkPing
            // 
            this.lblNetworkPing.Font = new System.Drawing.Font("맑은 고딕", 13.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblNetworkPing.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.lblNetworkPing.Location = new System.Drawing.Point(15, 38);
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
            this.pnlDriverCard.Size = new System.Drawing.Size(395, 140);
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
            this.btnGcCollect.Location = new System.Drawing.Point(18, 92);
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
            this.lblThreadsHandles.Location = new System.Drawing.Point(15, 65);
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
            this.tpServer.Size = new System.Drawing.Size(788, 334);
            this.tpServer.TabIndex = 1;
            this.tpServer.Text = "🚀 서버 접속 통계";
            // 
            // pnlServerContent
            // 
            this.pnlServerContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlServerContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlServerContent.Controls.Add(this.dgvTopServers);
            this.pnlServerContent.Controls.Add(this.gaugeAutoLogin);
            this.pnlServerContent.Controls.Add(this.chartServerDonut);
            this.pnlServerContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlServerContent.Location = new System.Drawing.Point(3, 3);
            this.pnlServerContent.Name = "pnlServerContent";
            this.pnlServerContent.Size = new System.Drawing.Size(782, 328);
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
            this.colPercentage});
            this.dgvTopServers.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.dgvTopServers.Location = new System.Drawing.Point(448, 15);
            this.dgvTopServers.Name = "dgvTopServers";
            this.dgvTopServers.ReadOnly = true;
            this.dgvTopServers.RowHeadersVisible = false;
            this.dgvTopServers.RowTemplate.Height = 27;
            this.dgvTopServers.Size = new System.Drawing.Size(320, 298);
            this.dgvTopServers.TabIndex = 2;
            // 
            // colRank
            // 
            this.colRank.FillWeight = 38F;
            this.colRank.HeaderText = "순위";
            this.colRank.Name = "colRank";
            this.colRank.ReadOnly = true;
            // 
            // colServerName
            // 
            this.colServerName.FillWeight = 120F;
            this.colServerName.HeaderText = "서버명";
            this.colServerName.Name = "colServerName";
            this.colServerName.ReadOnly = true;
            // 
            // colAccessCount
            // 
            this.colAccessCount.FillWeight = 55F;
            this.colAccessCount.HeaderText = "접속수";
            this.colAccessCount.Name = "colAccessCount";
            this.colAccessCount.ReadOnly = true;
            // 
            // colPercentage
            // 
            this.colPercentage.FillWeight = 55F;
            this.colPercentage.HeaderText = "비율";
            this.colPercentage.Name = "colPercentage";
            this.colPercentage.ReadOnly = true;
            // 
            // gaugeAutoLogin
            // 
            this.gaugeAutoLogin.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.gaugeAutoLogin.Location = new System.Drawing.Point(245, 15);
            this.gaugeAutoLogin.Name = "gaugeAutoLogin";
            this.gaugeAutoLogin.RingColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.gaugeAutoLogin.Size = new System.Drawing.Size(185, 298);
            this.gaugeAutoLogin.SubTitle = "자동 로그인 성공률";
            this.gaugeAutoLogin.TabIndex = 1;
            this.gaugeAutoLogin.Value = 0D;
            // 
            // chartServerDonut
            // 
            this.chartServerDonut.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.chartServerDonut.Location = new System.Drawing.Point(15, 15);
            this.chartServerDonut.Name = "chartServerDonut";
            this.chartServerDonut.Size = new System.Drawing.Size(215, 298);
            this.chartServerDonut.TabIndex = 0;
            // 
            // tpSandbox
            // 
            this.tpSandbox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tpSandbox.Controls.Add(this.pnlSandboxContent);
            this.tpSandbox.Location = new System.Drawing.Point(4, 22);
            this.tpSandbox.Name = "tpSandbox";
            this.tpSandbox.Size = new System.Drawing.Size(788, 334);
            this.tpSandbox.TabIndex = 2;
            this.tpSandbox.Text = "🎮 샌드박스 & 업적";
            // 
            // pnlSandboxContent
            // 
            this.pnlSandboxContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlSandboxContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSandboxContent.Controls.Add(this.pnlBoxAiTrend);
            this.pnlSandboxContent.Controls.Add(this.pnlBoxMystery);
            this.pnlSandboxContent.Controls.Add(this.pnlBoxSignalLink);
            this.pnlSandboxContent.Controls.Add(this.pnlBoxBitFlip);
            this.pnlSandboxContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSandboxContent.Location = new System.Drawing.Point(0, 0);
            this.pnlSandboxContent.Name = "pnlSandboxContent";
            this.pnlSandboxContent.Size = new System.Drawing.Size(788, 334);
            this.pnlSandboxContent.TabIndex = 0;
            // 
            // pnlBoxAiTrend
            // 
            this.pnlBoxAiTrend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.pnlBoxAiTrend.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBoxAiTrend.Controls.Add(this.lblAiTrendDetails);
            this.pnlBoxAiTrend.Controls.Add(this.lblAiTrendTitle);
            this.pnlBoxAiTrend.Location = new System.Drawing.Point(403, 172);
            this.pnlBoxAiTrend.Name = "pnlBoxAiTrend";
            this.pnlBoxAiTrend.Size = new System.Drawing.Size(360, 140);
            this.pnlBoxAiTrend.TabIndex = 3;
            // 
            // lblAiTrendDetails
            // 
            this.lblAiTrendDetails.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblAiTrendDetails.ForeColor = System.Drawing.Color.Silver;
            this.lblAiTrendDetails.Location = new System.Drawing.Point(15, 45);
            this.lblAiTrendDetails.Name = "lblAiTrendDetails";
            this.lblAiTrendDetails.Size = new System.Drawing.Size(325, 80);
            this.lblAiTrendDetails.TabIndex = 1;
            this.lblAiTrendDetails.Text = "• 찜(스크랩)한 오픈소스: 0개\n• 등록된 맞춤 관심 키워드: 0개\n• 실시간 데일리 브리핑 연동";
            // 
            // lblAiTrendTitle
            // 
            this.lblAiTrendTitle.AutoSize = true;
            this.lblAiTrendTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblAiTrendTitle.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.lblAiTrendTitle.Location = new System.Drawing.Point(15, 15);
            this.lblAiTrendTitle.Name = "lblAiTrendTitle";
            this.lblAiTrendTitle.Size = new System.Drawing.Size(183, 20);
            this.lblAiTrendTitle.TabIndex = 0;
            this.lblAiTrendTitle.Text = "📰 AI TREND (트렌드 허브)";
            // 
            // pnlBoxMystery
            // 
            this.pnlBoxMystery.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.pnlBoxMystery.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBoxMystery.Controls.Add(this.lblMysteryDetails);
            this.pnlBoxMystery.Controls.Add(this.lblMysteryTitle);
            this.pnlBoxMystery.Location = new System.Drawing.Point(20, 172);
            this.pnlBoxMystery.Name = "pnlBoxMystery";
            this.pnlBoxMystery.Size = new System.Drawing.Size(360, 140);
            this.pnlBoxMystery.TabIndex = 2;
            // 
            // lblMysteryDetails
            // 
            this.lblMysteryDetails.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblMysteryDetails.ForeColor = System.Drawing.Color.Silver;
            this.lblMysteryDetails.Location = new System.Drawing.Point(15, 45);
            this.lblMysteryDetails.Name = "lblMysteryDetails";
            this.lblMysteryDetails.Size = new System.Drawing.Size(325, 80);
            this.lblMysteryDetails.TabIndex = 1;
            this.lblMysteryDetails.Text = "• 해결한 사건(정답): 0건\n• 질문 및 추리 도전: 0회\n• 사건 규명 정답률: 0.0%";
            // 
            // lblMysteryTitle
            // 
            this.lblMysteryTitle.AutoSize = true;
            this.lblMysteryTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblMysteryTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(165)))), ((int)(((byte)(0)))));
            this.lblMysteryTitle.Location = new System.Drawing.Point(15, 15);
            this.lblMysteryTitle.Name = "lblMysteryTitle";
            this.lblMysteryTitle.Size = new System.Drawing.Size(189, 20);
            this.lblMysteryTitle.TabIndex = 0;
            this.lblMysteryTitle.Text = "🕵️ Mystery Time (AI 추리)";
            // 
            // pnlBoxSignalLink
            // 
            this.pnlBoxSignalLink.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.pnlBoxSignalLink.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBoxSignalLink.Controls.Add(this.lblSignalLinkDetails);
            this.pnlBoxSignalLink.Controls.Add(this.lblSignalLinkTitle);
            this.pnlBoxSignalLink.Location = new System.Drawing.Point(403, 18);
            this.pnlBoxSignalLink.Name = "pnlBoxSignalLink";
            this.pnlBoxSignalLink.Size = new System.Drawing.Size(360, 140);
            this.pnlBoxSignalLink.TabIndex = 1;
            // 
            // lblSignalLinkDetails
            // 
            this.lblSignalLinkDetails.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSignalLinkDetails.ForeColor = System.Drawing.Color.Silver;
            this.lblSignalLinkDetails.Location = new System.Drawing.Point(15, 45);
            this.lblSignalLinkDetails.Name = "lblSignalLinkDetails";
            this.lblSignalLinkDetails.Size = new System.Drawing.Size(325, 80);
            this.lblSignalLinkDetails.TabIndex = 1;
            this.lblSignalLinkDetails.Text = "• 회로 퍼즐 클리어: 0회\n• 신호 노드 암호 해독 완료\n• 완벽한 루트 연결 성공";
            // 
            // lblSignalLinkTitle
            // 
            this.lblSignalLinkTitle.AutoSize = true;
            this.lblSignalLinkTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSignalLinkTitle.ForeColor = System.Drawing.Color.Cyan;
            this.lblSignalLinkTitle.Location = new System.Drawing.Point(15, 15);
            this.lblSignalLinkTitle.Name = "lblSignalLinkTitle";
            this.lblSignalLinkTitle.Size = new System.Drawing.Size(201, 20);
            this.lblSignalLinkTitle.TabIndex = 0;
            this.lblSignalLinkTitle.Text = "⚡ Signal Link (회로 연결)";
            // 
            // pnlBoxBitFlip
            // 
            this.pnlBoxBitFlip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.pnlBoxBitFlip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBoxBitFlip.Controls.Add(this.lblBitFlipDetails);
            this.pnlBoxBitFlip.Controls.Add(this.lblBitFlipTitle);
            this.pnlBoxBitFlip.Location = new System.Drawing.Point(20, 18);
            this.pnlBoxBitFlip.Name = "pnlBoxBitFlip";
            this.pnlBoxBitFlip.Size = new System.Drawing.Size(360, 140);
            this.pnlBoxBitFlip.TabIndex = 0;
            // 
            // lblBitFlipDetails
            // 
            this.lblBitFlipDetails.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblBitFlipDetails.ForeColor = System.Drawing.Color.Silver;
            this.lblBitFlipDetails.Location = new System.Drawing.Point(15, 45);
            this.lblBitFlipDetails.Name = "lblBitFlipDetails";
            this.lblBitFlipDetails.Size = new System.Drawing.Size(325, 80);
            this.lblBitFlipDetails.TabIndex = 1;
            this.lblBitFlipDetails.Text = "• 비트 반전 승리: 0회\n• 모든 노드 동기화 달성\n• 기믹 극복 퍼즐 클리어";
            // 
            // lblBitFlipTitle
            // 
            this.lblBitFlipTitle.AutoSize = true;
            this.lblBitFlipTitle.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblBitFlipTitle.ForeColor = System.Drawing.Color.LightGreen;
            this.lblBitFlipTitle.Location = new System.Drawing.Point(15, 15);
            this.lblBitFlipTitle.Name = "lblBitFlipTitle";
            this.lblBitFlipTitle.Size = new System.Drawing.Size(183, 20);
            this.lblBitFlipTitle.TabIndex = 0;
            this.lblBitFlipTitle.Text = "🧩 Bit Flip (비트 퍼즐)";
            // 
            // tpLogs
            // 
            this.tpLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.tpLogs.Controls.Add(this.pnlLogContent);
            this.tpLogs.Location = new System.Drawing.Point(4, 22);
            this.tpLogs.Name = "tpLogs";
            this.tpLogs.Size = new System.Drawing.Size(788, 334);
            this.tpLogs.TabIndex = 3;
            this.tpLogs.Text = "🚨 최근 중요 로그";
            // 
            // pnlLogContent
            // 
            this.pnlLogContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.pnlLogContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLogContent.Controls.Add(this.txtLogDetail);
            this.pnlLogContent.Controls.Add(this.dgvRecentLogs);
            this.pnlLogContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLogContent.Location = new System.Drawing.Point(0, 0);
            this.pnlLogContent.Name = "pnlLogContent";
            this.pnlLogContent.Size = new System.Drawing.Size(788, 334);
            this.pnlLogContent.TabIndex = 0;
            // 
            // txtLogDetail
            // 
            this.txtLogDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(26)))));
            this.txtLogDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLogDetail.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtLogDetail.ForeColor = System.Drawing.Color.Gainsboro;
            this.txtLogDetail.Location = new System.Drawing.Point(15, 230);
            this.txtLogDetail.Multiline = true;
            this.txtLogDetail.Name = "txtLogDetail";
            this.txtLogDetail.ReadOnly = true;
            this.txtLogDetail.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogDetail.Size = new System.Drawing.Size(755, 88);
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
            this.dgvRecentLogs.Location = new System.Drawing.Point(15, 15);
            this.dgvRecentLogs.MultiSelect = false;
            this.dgvRecentLogs.Name = "dgvRecentLogs";
            this.dgvRecentLogs.ReadOnly = true;
            this.dgvRecentLogs.RowHeadersVisible = false;
            this.dgvRecentLogs.RowTemplate.Height = 25;
            this.dgvRecentLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecentLogs.Size = new System.Drawing.Size(755, 205);
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
            this.pnlBottomBar.Controls.Add(this.btnOpenLogDir);
            this.pnlBottomBar.Controls.Add(this.btnOpenDashboardDir);
            this.pnlBottomBar.Controls.Add(this.btnRefresh);
            this.pnlBottomBar.Controls.Add(this.btnResetStats);
            this.pnlBottomBar.Location = new System.Drawing.Point(12, 578);
            this.pnlBottomBar.Name = "pnlBottomBar";
            this.pnlBottomBar.Size = new System.Drawing.Size(796, 50);
            this.pnlBottomBar.TabIndex = 3;
            // 
            // lblStoragePath
            // 
            this.lblStoragePath.AutoSize = true;
            this.lblStoragePath.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblStoragePath.ForeColor = System.Drawing.Color.Gray;
            this.lblStoragePath.Location = new System.Drawing.Point(8, 18);
            this.lblStoragePath.Name = "lblStoragePath";
            this.lblStoragePath.Size = new System.Drawing.Size(270, 15);
            this.lblStoragePath.TabIndex = 4;
            this.lblStoragePath.Text = "📁 저장 위치: _meta/dashboard/dashboard_stats.json";
            // 
            // btnOpenLogDir
            // 
            this.btnOpenLogDir.AutoSize = false;
            this.btnOpenLogDir.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOpenLogDir.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnOpenLogDir.Depth = 0;
            this.btnOpenLogDir.HighEmphasis = false;
            this.btnOpenLogDir.Icon = null;
            this.btnOpenLogDir.Location = new System.Drawing.Point(340, 7);
            this.btnOpenLogDir.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOpenLogDir.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnOpenLogDir.Name = "btnOpenLogDir";
            this.btnOpenLogDir.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnOpenLogDir.Size = new System.Drawing.Size(100, 36);
            this.btnOpenLogDir.TabIndex = 3;
            this.btnOpenLogDir.Text = "로그 폴더";
            this.btnOpenLogDir.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnOpenLogDir.UseAccentColor = false;
            this.btnOpenLogDir.UseVisualStyleBackColor = true;
            this.btnOpenLogDir.Click += new System.EventHandler(this.BtnOpenLogDir_Click);
            // 
            // btnOpenDashboardDir
            // 
            this.btnOpenDashboardDir.AutoSize = false;
            this.btnOpenDashboardDir.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOpenDashboardDir.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnOpenDashboardDir.Depth = 0;
            this.btnOpenDashboardDir.HighEmphasis = false;
            this.btnOpenDashboardDir.Icon = null;
            this.btnOpenDashboardDir.Location = new System.Drawing.Point(448, 7);
            this.btnOpenDashboardDir.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOpenDashboardDir.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnOpenDashboardDir.Name = "btnOpenDashboardDir";
            this.btnOpenDashboardDir.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnOpenDashboardDir.Size = new System.Drawing.Size(110, 36);
            this.btnOpenDashboardDir.TabIndex = 2;
            this.btnOpenDashboardDir.Text = "대시보드 폴더";
            this.btnOpenDashboardDir.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnOpenDashboardDir.UseAccentColor = false;
            this.btnOpenDashboardDir.UseVisualStyleBackColor = true;
            this.btnOpenDashboardDir.Click += new System.EventHandler(this.BtnOpenDashboardDir_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.AutoSize = false;
            this.btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRefresh.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRefresh.Depth = 0;
            this.btnRefresh.HighEmphasis = true;
            this.btnRefresh.Icon = null;
            this.btnRefresh.Location = new System.Drawing.Point(566, 7);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRefresh.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRefresh.Size = new System.Drawing.Size(100, 36);
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
            this.btnResetStats.Location = new System.Drawing.Point(674, 7);
            this.btnResetStats.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnResetStats.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnResetStats.Name = "btnResetStats";
            this.btnResetStats.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnResetStats.Size = new System.Drawing.Size(112, 36);
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
            this.ClientSize = new System.Drawing.Size(820, 640);
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
            this.pnlBoxAiTrend.ResumeLayout(false);
            this.pnlBoxAiTrend.PerformLayout();
            this.pnlBoxMystery.ResumeLayout(false);
            this.pnlBoxMystery.PerformLayout();
            this.pnlBoxSignalLink.ResumeLayout(false);
            this.pnlBoxSignalLink.PerformLayout();
            this.pnlBoxBitFlip.ResumeLayout(false);
            this.pnlBoxBitFlip.PerformLayout();
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
        private System.Windows.Forms.DataGridView dgvTopServers;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRank;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAccessCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPercentage;
        private System.Windows.Forms.Panel pnlSandboxContent;
        private System.Windows.Forms.Panel pnlBoxBitFlip;
        private System.Windows.Forms.Label lblBitFlipTitle;
        private System.Windows.Forms.Label lblBitFlipDetails;
        private System.Windows.Forms.Panel pnlBoxSignalLink;
        private System.Windows.Forms.Label lblSignalLinkTitle;
        private System.Windows.Forms.Label lblSignalLinkDetails;
        private System.Windows.Forms.Panel pnlBoxMystery;
        private System.Windows.Forms.Label lblMysteryTitle;
        private System.Windows.Forms.Label lblMysteryDetails;
        private System.Windows.Forms.Panel pnlBoxAiTrend;
        private System.Windows.Forms.Label lblAiTrendTitle;
        private System.Windows.Forms.Label lblAiTrendDetails;
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
        private MaterialSkin.Controls.MaterialButton btnOpenDashboardDir;
        private MaterialSkin.Controls.MaterialButton btnOpenLogDir;
    }
}
