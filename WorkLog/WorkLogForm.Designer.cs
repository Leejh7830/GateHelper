namespace GateHelper
{
    partial class WorkLogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTopHud = new System.Windows.Forms.Panel();
            this.lblStatsTotal = new System.Windows.Forms.Label();
            this.lblStatsIng = new System.Windows.Forms.Label();
            this.lblStatsOpen = new System.Windows.Forms.Label();
            this.lblStatsDone = new System.Windows.Forms.Label();
            this.btnDailyReport = new System.Windows.Forms.Button();
            this.btnAddNew = new System.Windows.Forms.Button();
            this.btnZoomIn = new System.Windows.Forms.Button();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.OlvWorkLog = new BrightIdeasSoftware.ObjectListView();
            this.colNo = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.colStatus = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.colPriority = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.colTitle = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.colImages = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.colDate = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.colTags = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.pnlLeftTop = new System.Windows.Forms.Panel();
            this.flowQuickFilter = new System.Windows.Forms.FlowLayoutPanel();
            this.btnFilterAll = new System.Windows.Forms.Button();
            this.btnFilterIng = new System.Windows.Forms.Button();
            this.btnFilterOpen = new System.Windows.Forms.Button();
            this.btnFilterToday = new System.Windows.Forms.Button();
            this.btnFilterHigh = new System.Windows.Forms.Button();
            this.chkHideDone = new System.Windows.Forms.CheckBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.splitDetail = new System.Windows.Forms.SplitContainer();
            this.tabDetail = new System.Windows.Forms.TabControl();
            this.tabContent = new System.Windows.Forms.TabPage();
            this.txtContent = new System.Windows.Forms.TextBox();
            this.tabMemo = new System.Windows.Forms.TabPage();
            this.txtMemo = new System.Windows.Forms.TextBox();
            this.zoomViewer = new GateHelper.WorkLogDeepZoomViewer();
            this.flowThumbnails = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlImageBar = new System.Windows.Forms.Panel();
            this.btnToggleText = new System.Windows.Forms.Button();
            this.btnOpenExternal = new System.Windows.Forms.Button();
            this.btnPasteImage = new System.Windows.Forms.Button();
            this.btnDeleteImage = new System.Windows.Forms.Button();
            this.btnCopyImage = new System.Windows.Forms.Button();
            this.btnPopout = new System.Windows.Forms.Button();
            this.btnFitScreen = new System.Windows.Forms.Button();
            this.btnActualSize = new System.Windows.Forms.Button();
            this.lblImageTitle = new System.Windows.Forms.Label();
            this.pnlTitleTag = new System.Windows.Forms.Panel();
            this.txtTags = new System.Windows.Forms.TextBox();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.pnlDetailHeader = new System.Windows.Forms.Panel();
            this.btnSaveEntry = new System.Windows.Forms.Button();
            this.btnDeleteEntry = new System.Windows.Forms.Button();
            this.dtpDate = new System.Windows.Forms.DateTimePicker();
            this.btnPriorityToggle = new System.Windows.Forms.Button();
            this.btnStatusToggle = new System.Windows.Forms.Button();
            this.lblDetailNo = new System.Windows.Forms.Label();
            this.pnlTopHud.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OlvWorkLog)).BeginInit();
            this.pnlLeftTop.SuspendLayout();
            this.flowQuickFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDetail)).BeginInit();
            this.splitDetail.Panel1.SuspendLayout();
            this.splitDetail.Panel2.SuspendLayout();
            this.splitDetail.SuspendLayout();
            this.tabDetail.SuspendLayout();
            this.tabContent.SuspendLayout();
            this.tabMemo.SuspendLayout();
            this.pnlImageBar.SuspendLayout();
            this.pnlTitleTag.SuspendLayout();
            this.pnlDetailHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTopHud
            // 
            this.pnlTopHud.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(27)))), ((int)(((byte)(34)))));
            this.pnlTopHud.Controls.Add(this.btnZoomOut);
            this.pnlTopHud.Controls.Add(this.btnZoomIn);
            this.pnlTopHud.Controls.Add(this.btnDailyReport);
            this.pnlTopHud.Controls.Add(this.btnAddNew);
            this.pnlTopHud.Controls.Add(this.lblStatsDone);
            this.pnlTopHud.Controls.Add(this.lblStatsIng);
            this.pnlTopHud.Controls.Add(this.lblStatsOpen);
            this.pnlTopHud.Controls.Add(this.lblStatsTotal);
            this.pnlTopHud.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopHud.Location = new System.Drawing.Point(3, 64);
            this.pnlTopHud.Name = "pnlTopHud";
            this.pnlTopHud.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlTopHud.Size = new System.Drawing.Size(1274, 46);
            this.pnlTopHud.TabIndex = 0;
            // 
            // lblStatsTotal
            // 
            this.lblStatsTotal.AutoSize = true;
            this.lblStatsTotal.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatsTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(237)))), ((int)(((byte)(243)))));
            this.lblStatsTotal.Location = new System.Drawing.Point(10, 14);
            this.lblStatsTotal.Name = "lblStatsTotal";
            this.lblStatsTotal.Size = new System.Drawing.Size(65, 17);
            this.lblStatsTotal.TabIndex = 0;
            this.lblStatsTotal.Text = "전체: 0건";
            // 
            // lblStatsIng
            // 
            this.lblStatsIng.AutoSize = true;
            this.lblStatsIng.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatsIng.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.lblStatsIng.Location = new System.Drawing.Point(95, 14);
            this.lblStatsIng.Name = "lblStatsIng";
            this.lblStatsIng.Size = new System.Drawing.Size(81, 17);
            this.lblStatsIng.TabIndex = 1;
            this.lblStatsIng.Text = "🔥 진행: 0건";
            // 
            // lblStatsOpen
            // 
            this.lblStatsOpen.AutoSize = true;
            this.lblStatsOpen.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatsOpen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.lblStatsOpen.Location = new System.Drawing.Point(195, 14);
            this.lblStatsOpen.Name = "lblStatsOpen";
            this.lblStatsOpen.Size = new System.Drawing.Size(81, 17);
            this.lblStatsOpen.TabIndex = 2;
            this.lblStatsOpen.Text = "⏳ 대기: 0건";
            // 
            // lblStatsDone
            // 
            this.lblStatsDone.AutoSize = true;
            this.lblStatsDone.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatsDone.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(230)))), ((int)(((byte)(118)))));
            this.lblStatsDone.Location = new System.Drawing.Point(295, 14);
            this.lblStatsDone.Name = "lblStatsDone";
            this.lblStatsDone.Size = new System.Drawing.Size(122, 17);
            this.lblStatsDone.TabIndex = 3;
            this.lblStatsDone.Text = "✅ 완료: 0건 (0%)";
            // 
            // btnDailyReport
            // 
            this.btnDailyReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDailyReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(60)))));
            this.btnDailyReport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDailyReport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnDailyReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDailyReport.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnDailyReport.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnDailyReport.Location = new System.Drawing.Point(960, 8);
            this.btnDailyReport.Name = "btnDailyReport";
            this.btnDailyReport.Size = new System.Drawing.Size(140, 30);
            this.btnDailyReport.TabIndex = 4;
            this.btnDailyReport.Text = "📋 일일보고 복사";
            this.btnDailyReport.UseVisualStyleBackColor = false;
            this.btnDailyReport.Click += new System.EventHandler(this.btnDailyReport_Click);
            // 
            // btnAddNew
            // 
            this.btnAddNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddNew.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnAddNew.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddNew.FlatAppearance.BorderSize = 0;
            this.btnAddNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddNew.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddNew.ForeColor = System.Drawing.Color.White;
            this.btnAddNew.Location = new System.Drawing.Point(1108, 8);
            this.btnAddNew.Name = "btnAddNew";
            this.btnAddNew.Size = new System.Drawing.Size(95, 30);
            this.btnAddNew.TabIndex = 5;
            this.btnAddNew.Text = "➕ 새 업무";
            this.btnAddNew.UseVisualStyleBackColor = false;
            this.btnAddNew.Click += new System.EventHandler(this.btnAddNew_Click);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZoomIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnZoomIn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnZoomIn.FlatAppearance.BorderSize = 0;
            this.btnZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomIn.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnZoomIn.ForeColor = System.Drawing.Color.White;
            this.btnZoomIn.Location = new System.Drawing.Point(1210, 8);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(26, 30);
            this.btnZoomIn.TabIndex = 6;
            this.btnZoomIn.Text = "+";
            this.btnZoomIn.UseVisualStyleBackColor = false;
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZoomOut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnZoomOut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnZoomOut.FlatAppearance.BorderSize = 0;
            this.btnZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomOut.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnZoomOut.ForeColor = System.Drawing.Color.White;
            this.btnZoomOut.Location = new System.Drawing.Point(1240, 8);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(26, 30);
            this.btnZoomOut.TabIndex = 7;
            this.btnZoomOut.Text = "ㅡ";
            this.btnZoomOut.UseVisualStyleBackColor = false;
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(54)))), ((int)(((byte)(61)))));
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.Location = new System.Drawing.Point(3, 110);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.splitMain.Panel1.Controls.Add(this.OlvWorkLog);
            this.splitMain.Panel1.Controls.Add(this.pnlLeftTop);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.splitMain.Panel2.Controls.Add(this.splitDetail);
            this.splitMain.Panel2.Controls.Add(this.pnlTitleTag);
            this.splitMain.Panel2.Controls.Add(this.pnlDetailHeader);
            this.splitMain.Size = new System.Drawing.Size(1274, 747);
            this.splitMain.SplitterDistance = 480;
            this.splitMain.SplitterWidth = 5;
            this.splitMain.TabIndex = 1;
            // 
            // OlvWorkLog
            // 
            this.OlvWorkLog.AllColumns.Add(this.colNo);
            this.OlvWorkLog.AllColumns.Add(this.colStatus);
            this.OlvWorkLog.AllColumns.Add(this.colPriority);
            this.OlvWorkLog.AllColumns.Add(this.colTitle);
            this.OlvWorkLog.AllColumns.Add(this.colImages);
            this.OlvWorkLog.AllColumns.Add(this.colDate);
            this.OlvWorkLog.AllColumns.Add(this.colTags);
            this.OlvWorkLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.OlvWorkLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.OlvWorkLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colNo,
            this.colStatus,
            this.colPriority,
            this.colTitle,
            this.colImages,
            this.colDate});
            this.OlvWorkLog.Cursor = System.Windows.Forms.Cursors.Default;
            this.OlvWorkLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OlvWorkLog.Font = new System.Drawing.Font("맑은 고딕", 9.5F);
            this.OlvWorkLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(237)))), ((int)(((byte)(243)))));
            this.OlvWorkLog.FullRowSelect = true;
            this.OlvWorkLog.HideSelection = false;
            this.OlvWorkLog.Location = new System.Drawing.Point(0, 78);
            this.OlvWorkLog.Name = "OlvWorkLog";
            this.OlvWorkLog.ShowGroups = false;
            this.OlvWorkLog.Size = new System.Drawing.Size(480, 669);
            this.OlvWorkLog.TabIndex = 1;
            this.OlvWorkLog.UseCompatibleStateImageBehavior = false;
            this.OlvWorkLog.View = System.Windows.Forms.View.Details;
            // 
            // colNo
            // 
            this.colNo.AspectName = "No";
            this.colNo.Text = "#";
            this.colNo.Width = 36;
            // 
            // colStatus
            // 
            this.colStatus.AspectName = "Status";
            this.colStatus.Text = "상태";
            this.colStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colStatus.Width = 60;
            // 
            // colPriority
            // 
            this.colPriority.AspectName = "Priority";
            this.colPriority.Text = "중요도";
            this.colPriority.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colPriority.Width = 68;
            // 
            // colTitle
            // 
            this.colTitle.AspectName = "Title";
            this.colTitle.FillsFreeSpace = true;
            this.colTitle.MinimumWidth = 180;
            this.colTitle.Text = "제목";
            this.colTitle.Width = 220;
            // 
            // colImages
            // 
            this.colImages.AspectName = "ImagePaths";
            this.colImages.Text = "📸";
            this.colImages.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colImages.Width = 38;
            // 
            // colDate
            // 
            this.colDate.AspectName = "Date";
            this.colDate.AspectToStringFormat = "{0:MM-dd}";
            this.colDate.Text = "일자";
            this.colDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colDate.Width = 46;
            // 
            // colTags
            // 
            this.colTags.AspectName = "Tags";
            this.colTags.Text = "태그";
            this.colTags.Width = 60;
            // 
            // pnlLeftTop
            // 
            this.pnlLeftTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.pnlLeftTop.Controls.Add(this.flowQuickFilter);
            this.pnlLeftTop.Controls.Add(this.txtSearch);
            this.pnlLeftTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftTop.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftTop.Name = "pnlLeftTop";
            this.pnlLeftTop.Padding = new System.Windows.Forms.Padding(6);
            this.pnlLeftTop.Size = new System.Drawing.Size(480, 78);
            this.pnlLeftTop.TabIndex = 0;
            // 
            // txtSearch
            // 
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtSearch.Font = new System.Drawing.Font("맑은 고딕", 9.5F);
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(237)))), ((int)(((byte)(243)))));
            this.txtSearch.Location = new System.Drawing.Point(6, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(468, 24);
            this.txtSearch.TabIndex = 0;
            // 
            // flowQuickFilter
            // 
            this.flowQuickFilter.Controls.Add(this.btnFilterAll);
            this.flowQuickFilter.Controls.Add(this.btnFilterIng);
            this.flowQuickFilter.Controls.Add(this.btnFilterOpen);
            this.flowQuickFilter.Controls.Add(this.btnFilterToday);
            this.flowQuickFilter.Controls.Add(this.btnFilterHigh);
            this.flowQuickFilter.Controls.Add(this.chkHideDone);
            this.flowQuickFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowQuickFilter.Location = new System.Drawing.Point(6, 30);
            this.flowQuickFilter.Name = "flowQuickFilter";
            this.flowQuickFilter.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowQuickFilter.Size = new System.Drawing.Size(468, 42);
            this.flowQuickFilter.TabIndex = 1;
            // 
            // btnFilterAll
            // 
            this.btnFilterAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(52)))), ((int)(((byte)(64)))));
            this.btnFilterAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterAll.FlatAppearance.BorderSize = 0;
            this.btnFilterAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterAll.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnFilterAll.ForeColor = System.Drawing.Color.White;
            this.btnFilterAll.Location = new System.Drawing.Point(2, 6);
            this.btnFilterAll.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnFilterAll.Name = "btnFilterAll";
            this.btnFilterAll.Size = new System.Drawing.Size(46, 26);
            this.btnFilterAll.TabIndex = 0;
            this.btnFilterAll.Text = "전체";
            this.btnFilterAll.UseVisualStyleBackColor = false;
            this.btnFilterAll.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // btnFilterIng
            // 
            this.btnFilterIng.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(36)))), ((int)(((byte)(44)))));
            this.btnFilterIng.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterIng.FlatAppearance.BorderSize = 0;
            this.btnFilterIng.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterIng.Font = new System.Drawing.Font("맑은 고딕", 8.25F);
            this.btnFilterIng.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.btnFilterIng.Location = new System.Drawing.Point(52, 6);
            this.btnFilterIng.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnFilterIng.Name = "btnFilterIng";
            this.btnFilterIng.Size = new System.Drawing.Size(56, 26);
            this.btnFilterIng.TabIndex = 1;
            this.btnFilterIng.Text = "🔥 진행";
            this.btnFilterIng.UseVisualStyleBackColor = false;
            this.btnFilterIng.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // btnFilterOpen
            // 
            this.btnFilterOpen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(36)))), ((int)(((byte)(44)))));
            this.btnFilterOpen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterOpen.FlatAppearance.BorderSize = 0;
            this.btnFilterOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterOpen.Font = new System.Drawing.Font("맑은 고딕", 8.25F);
            this.btnFilterOpen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnFilterOpen.Location = new System.Drawing.Point(112, 6);
            this.btnFilterOpen.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnFilterOpen.Name = "btnFilterOpen";
            this.btnFilterOpen.Size = new System.Drawing.Size(56, 26);
            this.btnFilterOpen.TabIndex = 2;
            this.btnFilterOpen.Text = "⏳ 대기";
            this.btnFilterOpen.UseVisualStyleBackColor = false;
            this.btnFilterOpen.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // btnFilterToday
            // 
            this.btnFilterToday.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(36)))), ((int)(((byte)(44)))));
            this.btnFilterToday.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterToday.FlatAppearance.BorderSize = 0;
            this.btnFilterToday.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterToday.Font = new System.Drawing.Font("맑은 고딕", 8.25F);
            this.btnFilterToday.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(230)))), ((int)(((byte)(118)))));
            this.btnFilterToday.Location = new System.Drawing.Point(172, 6);
            this.btnFilterToday.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnFilterToday.Name = "btnFilterToday";
            this.btnFilterToday.Size = new System.Drawing.Size(56, 26);
            this.btnFilterToday.TabIndex = 3;
            this.btnFilterToday.Text = "📅 오늘";
            this.btnFilterToday.UseVisualStyleBackColor = false;
            this.btnFilterToday.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // btnFilterHigh
            // 
            this.btnFilterHigh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(36)))), ((int)(((byte)(44)))));
            this.btnFilterHigh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilterHigh.FlatAppearance.BorderSize = 0;
            this.btnFilterHigh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilterHigh.Font = new System.Drawing.Font("맑은 고딕", 8.25F);
            this.btnFilterHigh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(82)))), ((int)(((byte)(82)))));
            this.btnFilterHigh.Location = new System.Drawing.Point(232, 6);
            this.btnFilterHigh.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.btnFilterHigh.Name = "btnFilterHigh";
            this.btnFilterHigh.Size = new System.Drawing.Size(56, 26);
            this.btnFilterHigh.TabIndex = 4;
            this.btnFilterHigh.Text = "⭐ 긴급";
            this.btnFilterHigh.UseVisualStyleBackColor = false;
            this.btnFilterHigh.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // chkHideDone
            // 
            this.chkHideDone.AutoSize = true;
            this.chkHideDone.Font = new System.Drawing.Font("맑은 고딕", 8F);
            this.chkHideDone.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(170)))), ((int)(((byte)(185)))));
            this.chkHideDone.Location = new System.Drawing.Point(294, 9);
            this.chkHideDone.Margin = new System.Windows.Forms.Padding(4, 3, 2, 0);
            this.chkHideDone.Name = "chkHideDone";
            this.chkHideDone.Size = new System.Drawing.Size(74, 17);
            this.chkHideDone.TabIndex = 5;
            this.chkHideDone.Text = "완료 숨김";
            this.chkHideDone.UseVisualStyleBackColor = true;
            this.chkHideDone.CheckedChanged += new System.EventHandler(this.chkHideDone_CheckedChanged);
            // 
            // splitDetail
            // 
            this.splitDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(54)))), ((int)(((byte)(61)))));
            this.splitDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDetail.Location = new System.Drawing.Point(0, 92);
            this.splitDetail.Name = "splitDetail";
            this.splitDetail.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitDetail.Panel1
            // 
            this.splitDetail.Panel1.Controls.Add(this.tabDetail);
            // 
            // splitDetail.Panel2
            // 
            this.splitDetail.Panel2.Controls.Add(this.zoomViewer);
            this.splitDetail.Panel2.Controls.Add(this.flowThumbnails);
            this.splitDetail.Panel2.Controls.Add(this.pnlImageBar);
            this.splitDetail.Size = new System.Drawing.Size(789, 655);
            this.splitDetail.SplitterDistance = 130;
            this.splitDetail.SplitterWidth = 5;
            this.splitDetail.TabIndex = 2;
            // 
            // tabDetail
            // 
            this.tabDetail.Controls.Add(this.tabContent);
            this.tabDetail.Controls.Add(this.tabMemo);
            this.tabDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabDetail.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.tabDetail.Location = new System.Drawing.Point(0, 0);
            this.tabDetail.Name = "tabDetail";
            this.tabDetail.SelectedIndex = 0;
            this.tabDetail.Size = new System.Drawing.Size(789, 130);
            this.tabDetail.TabIndex = 0;
            // 
            // tabContent
            // 
            this.tabContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.tabContent.Controls.Add(this.txtContent);
            this.tabContent.Location = new System.Drawing.Point(4, 24);
            this.tabContent.Name = "tabContent";
            this.tabContent.Padding = new System.Windows.Forms.Padding(4);
            this.tabContent.Size = new System.Drawing.Size(781, 102);
            this.tabContent.TabIndex = 0;
            this.tabContent.Text = "📝 상세 내용 / 업무 본문";
            // 
            // txtContent
            // 
            this.txtContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.txtContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtContent.Font = new System.Drawing.Font("맑은 고딕", 9.5F);
            this.txtContent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(237)))), ((int)(((byte)(243)))));
            this.txtContent.Location = new System.Drawing.Point(4, 4);
            this.txtContent.Multiline = true;
            this.txtContent.Name = "txtContent";
            this.txtContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtContent.Size = new System.Drawing.Size(773, 94);
            this.txtContent.TabIndex = 0;
            this.txtContent.TextChanged += new System.EventHandler(this.txtDetail_TextChanged);
            // 
            // tabMemo
            // 
            this.tabMemo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.tabMemo.Controls.Add(this.txtMemo);
            this.tabMemo.Location = new System.Drawing.Point(4, 24);
            this.tabMemo.Name = "tabMemo";
            this.tabMemo.Padding = new System.Windows.Forms.Padding(4);
            this.tabMemo.Size = new System.Drawing.Size(781, 102);
            this.tabMemo.TabIndex = 1;
            this.tabMemo.Text = "💡 후속 조치 / 메모";
            // 
            // txtMemo
            // 
            this.txtMemo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.txtMemo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMemo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMemo.Font = new System.Drawing.Font("맑은 고딕", 9.5F);
            this.txtMemo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(237)))), ((int)(((byte)(243)))));
            this.txtMemo.Location = new System.Drawing.Point(4, 4);
            this.txtMemo.Multiline = true;
            this.txtMemo.Name = "txtMemo";
            this.txtMemo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMemo.Size = new System.Drawing.Size(773, 94);
            this.txtMemo.TabIndex = 0;
            this.txtMemo.TextChanged += new System.EventHandler(this.txtDetail_TextChanged);
            // 
            // zoomViewer
            // 
            this.zoomViewer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(24)))), ((int)(((byte)(30)))));
            this.zoomViewer.Cursor = System.Windows.Forms.Cursors.Default;
            this.zoomViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoomViewer.Location = new System.Drawing.Point(0, 36);
            this.zoomViewer.Name = "zoomViewer";
            this.zoomViewer.Size = new System.Drawing.Size(789, 484);
            this.zoomViewer.TabIndex = 0;
            // 
            // flowThumbnails
            // 
            this.flowThumbnails.AutoScroll = true;
            this.flowThumbnails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.flowThumbnails.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowThumbnails.Location = new System.Drawing.Point(0, 464);
            this.flowThumbnails.Name = "flowThumbnails";
            this.flowThumbnails.Padding = new System.Windows.Forms.Padding(4);
            this.flowThumbnails.Size = new System.Drawing.Size(789, 56);
            this.flowThumbnails.TabIndex = 1;
            this.flowThumbnails.Visible = false;
            this.flowThumbnails.WrapContents = false;
            // 
            // pnlImageBar
            // 
            this.pnlImageBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(27)))), ((int)(((byte)(34)))));
            this.pnlImageBar.Controls.Add(this.btnToggleText);
            this.pnlImageBar.Controls.Add(this.btnOpenExternal);
            this.pnlImageBar.Controls.Add(this.btnPasteImage);
            this.pnlImageBar.Controls.Add(this.btnDeleteImage);
            this.pnlImageBar.Controls.Add(this.btnCopyImage);
            this.pnlImageBar.Controls.Add(this.btnPopout);
            this.pnlImageBar.Controls.Add(this.btnFitScreen);
            this.pnlImageBar.Controls.Add(this.btnActualSize);
            this.pnlImageBar.Controls.Add(this.lblImageTitle);
            this.pnlImageBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlImageBar.Location = new System.Drawing.Point(0, 0);
            this.pnlImageBar.Name = "pnlImageBar";
            this.pnlImageBar.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlImageBar.Size = new System.Drawing.Size(789, 36);
            this.pnlImageBar.TabIndex = 2;
            // 
            // btnToggleText
            // 
            this.btnToggleText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnToggleText.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnToggleText.FlatAppearance.BorderSize = 0;
            this.btnToggleText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleText.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnToggleText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnToggleText.Location = new System.Drawing.Point(145, 5);
            this.btnToggleText.Name = "btnToggleText";
            this.btnToggleText.Size = new System.Drawing.Size(86, 26);
            this.btnToggleText.TabIndex = 8;
            this.btnToggleText.Text = "▲ 본문 접기";
            this.btnToggleText.UseVisualStyleBackColor = false;
            this.btnToggleText.Click += new System.EventHandler(this.btnToggleText_Click);
            // 
            // btnOpenExternal
            // 
            this.btnOpenExternal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenExternal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnOpenExternal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpenExternal.FlatAppearance.BorderSize = 0;
            this.btnOpenExternal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenExternal.Font = new System.Drawing.Font("맑은 고딕", 8.25F);
            this.btnOpenExternal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(210)))), ((int)(((byte)(225)))));
            this.btnOpenExternal.Location = new System.Drawing.Point(600, 5);
            this.btnOpenExternal.Name = "btnOpenExternal";
            this.btnOpenExternal.Size = new System.Drawing.Size(64, 26);
            this.btnOpenExternal.TabIndex = 6;
            this.btnOpenExternal.Text = "📂 원본";
            this.btnOpenExternal.UseVisualStyleBackColor = false;
            this.btnOpenExternal.Click += new System.EventHandler(this.btnOpenExternal_Click);
            // 
            // btnPasteImage
            // 
            this.btnPasteImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPasteImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(45)))), ((int)(((byte)(60)))));
            this.btnPasteImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPasteImage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnPasteImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPasteImage.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnPasteImage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnPasteImage.Location = new System.Drawing.Point(670, 5);
            this.btnPasteImage.Name = "btnPasteImage";
            this.btnPasteImage.Size = new System.Drawing.Size(112, 26);
            this.btnPasteImage.TabIndex = 7;
            this.btnPasteImage.Text = "📋 스크린샷 추가";
            this.btnPasteImage.UseVisualStyleBackColor = false;
            this.btnPasteImage.Click += new System.EventHandler(this.btnPasteImage_Click);
            // 
            // btnDeleteImage
            // 
            this.btnDeleteImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnDeleteImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteImage.FlatAppearance.BorderSize = 0;
            this.btnDeleteImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteImage.Font = new System.Drawing.Font("맑은 고딕", 8.25F);
            this.btnDeleteImage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnDeleteImage.Location = new System.Drawing.Point(525, 5);
            this.btnDeleteImage.Name = "btnDeleteImage";
            this.btnDeleteImage.Size = new System.Drawing.Size(56, 26);
            this.btnDeleteImage.TabIndex = 5;
            this.btnDeleteImage.Text = "🗑️ 삭제";
            this.btnDeleteImage.UseVisualStyleBackColor = false;
            this.btnDeleteImage.Click += new System.EventHandler(this.btnDeleteImage_Click);
            // 
            // btnCopyImage
            // 
            this.btnCopyImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnCopyImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCopyImage.FlatAppearance.BorderSize = 0;
            this.btnCopyImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopyImage.Font = new System.Drawing.Font("맑은 고딕", 8.25F);
            this.btnCopyImage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(210)))), ((int)(((byte)(225)))));
            this.btnCopyImage.Location = new System.Drawing.Point(463, 5);
            this.btnCopyImage.Name = "btnCopyImage";
            this.btnCopyImage.Size = new System.Drawing.Size(58, 26);
            this.btnCopyImage.TabIndex = 4;
            this.btnCopyImage.Text = "📋 복사";
            this.btnCopyImage.UseVisualStyleBackColor = false;
            this.btnCopyImage.Click += new System.EventHandler(this.btnCopyImage_Click);
            // 
            // btnPopout
            // 
            this.btnPopout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnPopout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPopout.FlatAppearance.BorderSize = 0;
            this.btnPopout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPopout.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnPopout.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnPopout.Location = new System.Drawing.Point(375, 5);
            this.btnPopout.Name = "btnPopout";
            this.btnPopout.Size = new System.Drawing.Size(84, 26);
            this.btnPopout.TabIndex = 3;
            this.btnPopout.Text = "⤢ 팝아웃 새창";
            this.btnPopout.UseVisualStyleBackColor = false;
            this.btnPopout.Click += new System.EventHandler(this.btnPopout_Click);
            // 
            // btnFitScreen
            // 
            this.btnFitScreen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnFitScreen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFitScreen.FlatAppearance.BorderSize = 0;
            this.btnFitScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFitScreen.Font = new System.Drawing.Font("맑은 고딕", 8.25F);
            this.btnFitScreen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(210)))), ((int)(((byte)(225)))));
            this.btnFitScreen.Location = new System.Drawing.Point(307, 5);
            this.btnFitScreen.Name = "btnFitScreen";
            this.btnFitScreen.Size = new System.Drawing.Size(64, 26);
            this.btnFitScreen.TabIndex = 2;
            this.btnFitScreen.Text = "화면 맞춤";
            this.btnFitScreen.UseVisualStyleBackColor = false;
            this.btnFitScreen.Click += new System.EventHandler(this.btnFitScreen_Click);
            // 
            // btnActualSize
            // 
            this.btnActualSize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnActualSize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnActualSize.FlatAppearance.BorderSize = 0;
            this.btnActualSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActualSize.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnActualSize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(215)))), ((int)(((byte)(0)))));
            this.btnActualSize.Location = new System.Drawing.Point(234, 5);
            this.btnActualSize.Name = "btnActualSize";
            this.btnActualSize.Size = new System.Drawing.Size(70, 26);
            this.btnActualSize.TabIndex = 1;
            this.btnActualSize.Text = "1:1 원본";
            this.btnActualSize.UseVisualStyleBackColor = false;
            this.btnActualSize.Click += new System.EventHandler(this.btnActualSize_Click);
            // 
            // lblImageTitle
            // 
            this.lblImageTitle.AutoSize = true;
            this.lblImageTitle.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblImageTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(237)))), ((int)(((byte)(243)))));
            this.lblImageTitle.Location = new System.Drawing.Point(8, 10);
            this.lblImageTitle.Name = "lblImageTitle";
            this.lblImageTitle.Size = new System.Drawing.Size(127, 15);
            this.lblImageTitle.TabIndex = 0;
            this.lblImageTitle.Text = "🖼️ 스크린샷 뷰어 (0장)";
            // 
            // pnlTitleTag
            // 
            this.pnlTitleTag.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.pnlTitleTag.Controls.Add(this.txtTags);
            this.pnlTitleTag.Controls.Add(this.txtTitle);
            this.pnlTitleTag.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleTag.Location = new System.Drawing.Point(0, 38);
            this.pnlTitleTag.Name = "pnlTitleTag";
            this.pnlTitleTag.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlTitleTag.Size = new System.Drawing.Size(789, 54);
            this.pnlTitleTag.TabIndex = 1;
            // 
            // txtTags
            // 
            this.txtTags.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.txtTags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTags.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtTags.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.txtTags.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(166)))), ((int)(((byte)(255)))));
            this.txtTags.Location = new System.Drawing.Point(8, 27);
            this.txtTags.Name = "txtTags";
            this.txtTags.Size = new System.Drawing.Size(773, 23);
            this.txtTags.TabIndex = 1;
            this.txtTags.TextChanged += new System.EventHandler(this.txtDetail_TextChanged);
            // 
            // txtTitle
            // 
            this.txtTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtTitle.Font = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Bold);
            this.txtTitle.ForeColor = System.Drawing.Color.White;
            this.txtTitle.Location = new System.Drawing.Point(8, 4);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(773, 26);
            this.txtTitle.TabIndex = 0;
            this.txtTitle.TextChanged += new System.EventHandler(this.txtDetail_TextChanged);
            // 
            // pnlDetailHeader
            // 
            this.pnlDetailHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(27)))), ((int)(((byte)(34)))));
            this.pnlDetailHeader.Controls.Add(this.btnSaveEntry);
            this.pnlDetailHeader.Controls.Add(this.btnDeleteEntry);
            this.pnlDetailHeader.Controls.Add(this.dtpDate);
            this.pnlDetailHeader.Controls.Add(this.btnPriorityToggle);
            this.pnlDetailHeader.Controls.Add(this.btnStatusToggle);
            this.pnlDetailHeader.Controls.Add(this.lblDetailNo);
            this.pnlDetailHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetailHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlDetailHeader.Name = "pnlDetailHeader";
            this.pnlDetailHeader.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlDetailHeader.Size = new System.Drawing.Size(789, 38);
            this.pnlDetailHeader.TabIndex = 0;
            // 
            // btnSaveEntry
            // 
            this.btnSaveEntry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(134)))), ((int)(((byte)(54)))));
            this.btnSaveEntry.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveEntry.FlatAppearance.BorderSize = 0;
            this.btnSaveEntry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveEntry.Font = new System.Drawing.Font("맑은 고딕", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnSaveEntry.ForeColor = System.Drawing.Color.White;
            this.btnSaveEntry.Location = new System.Drawing.Point(709, 5);
            this.btnSaveEntry.Name = "btnSaveEntry";
            this.btnSaveEntry.Size = new System.Drawing.Size(72, 28);
            this.btnSaveEntry.TabIndex = 5;
            this.btnSaveEntry.Text = "💾 저장";
            this.btnSaveEntry.UseVisualStyleBackColor = false;
            this.btnSaveEntry.Click += new System.EventHandler(this.btnSaveEntry_Click);
            // 
            // btnDeleteEntry
            // 
            this.btnDeleteEntry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteEntry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(20)))), ((int)(((byte)(24)))));
            this.btnDeleteEntry.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteEntry.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(81)))), ((int)(((byte)(73)))));
            this.btnDeleteEntry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteEntry.Font = new System.Drawing.Font("맑은 고딕", 8.5F);
            this.btnDeleteEntry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(81)))), ((int)(((byte)(73)))));
            this.btnDeleteEntry.Location = new System.Drawing.Point(633, 5);
            this.btnDeleteEntry.Name = "btnDeleteEntry";
            this.btnDeleteEntry.Size = new System.Drawing.Size(72, 28);
            this.btnDeleteEntry.TabIndex = 4;
            this.btnDeleteEntry.Text = "🗑️ 삭제";
            this.btnDeleteEntry.UseVisualStyleBackColor = false;
            this.btnDeleteEntry.Click += new System.EventHandler(this.btnDeleteEntry_Click);
            // 
            // dtpDate
            // 
            this.dtpDate.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpDate.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.dtpDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDate.Location = new System.Drawing.Point(245, 7);
            this.dtpDate.Name = "dtpDate";
            this.dtpDate.Size = new System.Drawing.Size(145, 23);
            this.dtpDate.TabIndex = 3;
            this.dtpDate.ValueChanged += new System.EventHandler(this.dtpDate_ValueChanged);
            // 
            // btnPriorityToggle
            // 
            this.btnPriorityToggle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnPriorityToggle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPriorityToggle.FlatAppearance.BorderSize = 0;
            this.btnPriorityToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPriorityToggle.Font = new System.Drawing.Font("맑은 고딕", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnPriorityToggle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(237)))), ((int)(((byte)(243)))));
            this.btnPriorityToggle.Location = new System.Drawing.Point(160, 5);
            this.btnPriorityToggle.Name = "btnPriorityToggle";
            this.btnPriorityToggle.Size = new System.Drawing.Size(78, 28);
            this.btnPriorityToggle.TabIndex = 2;
            this.btnPriorityToggle.Text = "⚡ 보통";
            this.btnPriorityToggle.UseVisualStyleBackColor = false;
            this.btnPriorityToggle.Click += new System.EventHandler(this.btnPriorityToggle_Click);
            // 
            // btnStatusToggle
            // 
            this.btnStatusToggle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(38)))), ((int)(((byte)(45)))));
            this.btnStatusToggle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStatusToggle.FlatAppearance.BorderSize = 0;
            this.btnStatusToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStatusToggle.Font = new System.Drawing.Font("맑은 고딕", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnStatusToggle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnStatusToggle.Location = new System.Drawing.Point(75, 5);
            this.btnStatusToggle.Name = "btnStatusToggle";
            this.btnStatusToggle.Size = new System.Drawing.Size(80, 28);
            this.btnStatusToggle.TabIndex = 1;
            this.btnStatusToggle.Text = "[ OPEN ]";
            this.btnStatusToggle.UseVisualStyleBackColor = false;
            this.btnStatusToggle.Click += new System.EventHandler(this.btnStatusToggle_Click);
            // 
            // lblDetailNo
            // 
            this.lblDetailNo.AutoSize = true;
            this.lblDetailNo.Font = new System.Drawing.Font("Consolas", 13F, System.Drawing.FontStyle.Bold);
            this.lblDetailNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.lblDetailNo.Location = new System.Drawing.Point(8, 8);
            this.lblDetailNo.Name = "lblDetailNo";
            this.lblDetailNo.Size = new System.Drawing.Size(37, 21);
            this.lblDetailNo.TabIndex = 0;
            this.lblDetailNo.Text = "#--";
            // 
            // WorkLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.ClientSize = new System.Drawing.Size(1280, 860);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlTopHud);
            this.MinimumSize = new System.Drawing.Size(960, 680);
            this.Name = "WorkLogForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Work Log Studio";
            this.pnlTopHud.ResumeLayout(false);
            this.pnlTopHud.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OlvWorkLog)).EndInit();
            this.pnlLeftTop.ResumeLayout(false);
            this.pnlLeftTop.PerformLayout();
            this.flowQuickFilter.ResumeLayout(false);
            this.flowQuickFilter.PerformLayout();
            this.splitDetail.Panel1.ResumeLayout(false);
            this.splitDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDetail)).EndInit();
            this.splitDetail.ResumeLayout(false);
            this.tabDetail.ResumeLayout(false);
            this.tabContent.ResumeLayout(false);
            this.tabContent.PerformLayout();
            this.tabMemo.ResumeLayout(false);
            this.tabMemo.PerformLayout();
            this.pnlImageBar.ResumeLayout(false);
            this.pnlImageBar.PerformLayout();
            this.pnlTitleTag.ResumeLayout(false);
            this.pnlTitleTag.PerformLayout();
            this.pnlDetailHeader.ResumeLayout(false);
            this.pnlDetailHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTopHud;
        private System.Windows.Forms.Label lblStatsTotal;
        private System.Windows.Forms.Label lblStatsIng;
        private System.Windows.Forms.Label lblStatsOpen;
        private System.Windows.Forms.Label lblStatsDone;
        private System.Windows.Forms.Button btnDailyReport;
        private System.Windows.Forms.Button btnAddNew;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.SplitContainer splitMain;
        private BrightIdeasSoftware.ObjectListView OlvWorkLog;
        private BrightIdeasSoftware.OLVColumn colNo;
        private BrightIdeasSoftware.OLVColumn colStatus;
        private BrightIdeasSoftware.OLVColumn colPriority;
        private BrightIdeasSoftware.OLVColumn colTitle;
        private BrightIdeasSoftware.OLVColumn colImages;
        private BrightIdeasSoftware.OLVColumn colDate;
        private BrightIdeasSoftware.OLVColumn colTags;
        private System.Windows.Forms.Panel pnlLeftTop;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.FlowLayoutPanel flowQuickFilter;
        private System.Windows.Forms.Button btnFilterAll;
        private System.Windows.Forms.Button btnFilterIng;
        private System.Windows.Forms.Button btnFilterOpen;
        private System.Windows.Forms.Button btnFilterToday;
        private System.Windows.Forms.Button btnFilterHigh;
        private System.Windows.Forms.CheckBox chkHideDone;
        private System.Windows.Forms.Panel pnlDetailHeader;
        private System.Windows.Forms.Label lblDetailNo;
        private System.Windows.Forms.Button btnStatusToggle;
        private System.Windows.Forms.Button btnPriorityToggle;
        private System.Windows.Forms.DateTimePicker dtpDate;
        private System.Windows.Forms.Button btnDeleteEntry;
        private System.Windows.Forms.Button btnSaveEntry;
        private System.Windows.Forms.Panel pnlTitleTag;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtTags;
        private System.Windows.Forms.SplitContainer splitDetail;
        private System.Windows.Forms.TabControl tabDetail;
        private System.Windows.Forms.TabPage tabContent;
        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.TabPage tabMemo;
        private System.Windows.Forms.TextBox txtMemo;
        private System.Windows.Forms.Panel pnlImageBar;
        private System.Windows.Forms.Label lblImageTitle;
        private System.Windows.Forms.Button btnToggleText;
        private System.Windows.Forms.Button btnActualSize;
        private System.Windows.Forms.Button btnFitScreen;
        private System.Windows.Forms.Button btnPopout;
        private System.Windows.Forms.Button btnCopyImage;
        private System.Windows.Forms.Button btnDeleteImage;
        private System.Windows.Forms.Button btnPasteImage;
        private System.Windows.Forms.Button btnOpenExternal;
        private GateHelper.WorkLogDeepZoomViewer zoomViewer;
        private System.Windows.Forms.FlowLayoutPanel flowThumbnails;
    }
}