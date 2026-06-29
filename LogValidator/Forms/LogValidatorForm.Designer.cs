namespace GateHelper.LogValidator
{
    partial class LogValidatorForm
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
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.olvValidatorRawLog = new BrightIdeasSoftware.FastObjectListView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnReset = new MaterialSkin.Controls.MaterialButton();
            this.lblAnomalyWarning = new MaterialSkin.Controls.MaterialLabel();
            this.materialButton1 = new MaterialSkin.Controls.MaterialButton();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.treeScenarioGroup = new System.Windows.Forms.TreeView();
            this.olvScenarioRepository = new BrightIdeasSoftware.ObjectListView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnOpenFolder = new MaterialSkin.Controls.MaterialButton();
            this.btnOpenScenarioEditor = new MaterialSkin.Controls.MaterialButton();
            this.btnClose = new MaterialSkin.Controls.MaterialButton();
            this.olvValidationResult = new BrightIdeasSoftware.TreeListView();
            this.toolTipAnomaly = new System.Windows.Forms.ToolTip(this.components);
            this.txtLogFilter = new MaterialSkin.Controls.MaterialTextBox2();
            this.dtpTimeJump = new System.Windows.Forms.DateTimePicker();
            this.btnTimeJump = new MaterialSkin.Controls.MaterialButton();
            this.cmbDateFilter = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvValidatorRawLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvScenarioRepository)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvValidationResult)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 64);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2MinSize = 400;
            this.splitContainer1.Size = new System.Drawing.Size(1414, 776);
            this.splitContainer1.SplitterDistance = 744;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(744, 776);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.olvValidatorRawLog);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(736, 750);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // olvValidatorRawLog
            // 
            this.olvValidatorRawLog.AllowDrop = true;
            this.olvValidatorRawLog.CellEditUseWholeCell = false;
            this.olvValidatorRawLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvValidatorRawLog.FullRowSelect = true;
            this.olvValidatorRawLog.GridLines = true;
            this.olvValidatorRawLog.HideSelection = false;
            this.olvValidatorRawLog.Location = new System.Drawing.Point(3, 3);
            this.olvValidatorRawLog.Name = "olvValidatorRawLog";
            this.olvValidatorRawLog.ShowGroups = false;
            this.olvValidatorRawLog.Size = new System.Drawing.Size(730, 744);
            this.olvValidatorRawLog.TabIndex = 1;
            this.olvValidatorRawLog.UseCompatibleStateImageBehavior = false;
            this.olvValidatorRawLog.View = System.Windows.Forms.View.Details;
            this.olvValidatorRawLog.VirtualMode = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(736, 750);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            this.splitContainer2.Panel1MinSize = 150;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panel2);
            this.splitContainer2.Panel2.Controls.Add(this.olvValidationResult);
            this.splitContainer2.Panel2MinSize = 150;
            this.splitContainer2.Size = new System.Drawing.Size(666, 776);
            this.splitContainer2.SplitterDistance = 265;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(666, 265);
            this.splitContainer3.SplitterDistance = 63;
            this.splitContainer3.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.cmbDateFilter);
            this.panel1.Controls.Add(this.btnTimeJump);
            this.panel1.Controls.Add(this.dtpTimeJump);
            this.panel1.Controls.Add(this.lblAnomalyWarning);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(666, 63);
            this.panel1.TabIndex = 0;
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.AutoSize = false;
            this.btnReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnReset.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnReset.Depth = 0;
            this.btnReset.HighEmphasis = true;
            this.btnReset.Icon = null;
            this.btnReset.Location = new System.Drawing.Point(17, 51);
            this.btnReset.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnReset.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnReset.Name = "btnReset";
            this.btnReset.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnReset.Size = new System.Drawing.Size(120, 36);
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "↻ Reset";
            this.btnReset.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnReset.UseAccentColor = true;
            this.btnReset.UseVisualStyleBackColor = true;
            // 
            // lblAnomalyWarning
            // 
            this.lblAnomalyWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAnomalyWarning.AutoSize = true;
            this.lblAnomalyWarning.Depth = 0;
            this.lblAnomalyWarning.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblAnomalyWarning.Location = new System.Drawing.Point(511, 19);
            this.lblAnomalyWarning.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblAnomalyWarning.Name = "lblAnomalyWarning";
            this.lblAnomalyWarning.Size = new System.Drawing.Size(142, 19);
            this.lblAnomalyWarning.TabIndex = 2;
            this.lblAnomalyWarning.Text = "Abnormal Logs: 0    ";
            this.lblAnomalyWarning.Visible = false;
            // 
            // materialButton1
            // 
            this.materialButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.materialButton1.AutoSize = false;
            this.materialButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.materialButton1.Depth = 0;
            this.materialButton1.HighEmphasis = true;
            this.materialButton1.Icon = null;
            this.materialButton1.Location = new System.Drawing.Point(19, 6);
            this.materialButton1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialButton1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton1.Name = "materialButton1";
            this.materialButton1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.materialButton1.Size = new System.Drawing.Size(118, 36);
            this.materialButton1.TabIndex = 0;
            this.materialButton1.Text = "🔍 Validation";
            this.materialButton1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.materialButton1.UseAccentColor = false;
            this.materialButton1.UseVisualStyleBackColor = true;
            this.materialButton1.Click += new System.EventHandler(this.btnStartValidation_Click);
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.treeScenarioGroup);
            this.splitContainer4.Panel1MinSize = 150;
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.olvScenarioRepository);
            this.splitContainer4.Panel2MinSize = 150;
            this.splitContainer4.Size = new System.Drawing.Size(666, 198);
            this.splitContainer4.SplitterDistance = 234;
            this.splitContainer4.TabIndex = 0;
            // 
            // treeScenarioGroup
            // 
            this.treeScenarioGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeScenarioGroup.Location = new System.Drawing.Point(0, 0);
            this.treeScenarioGroup.Name = "treeScenarioGroup";
            this.treeScenarioGroup.Size = new System.Drawing.Size(234, 198);
            this.treeScenarioGroup.TabIndex = 0;
            // 
            // olvScenarioRepository
            // 
            this.olvScenarioRepository.CellEditUseWholeCell = false;
            this.olvScenarioRepository.CheckBoxes = true;
            this.olvScenarioRepository.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvScenarioRepository.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvScenarioRepository.HideSelection = false;
            this.olvScenarioRepository.Location = new System.Drawing.Point(0, 0);
            this.olvScenarioRepository.Name = "olvScenarioRepository";
            this.olvScenarioRepository.Size = new System.Drawing.Size(428, 198);
            this.olvScenarioRepository.TabIndex = 2;
            this.olvScenarioRepository.UseCompatibleStateImageBehavior = false;
            this.olvScenarioRepository.View = System.Windows.Forms.View.Details;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.btnOpenFolder);
            this.panel2.Controls.Add(this.btnOpenScenarioEditor);
            this.panel2.Controls.Add(this.btnClose);
            this.panel2.Controls.Add(this.btnReset);
            this.panel2.Controls.Add(this.materialButton1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(516, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(150, 507);
            this.panel2.TabIndex = 4;
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolder.AutoSize = false;
            this.btnOpenFolder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOpenFolder.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnOpenFolder.Depth = 0;
            this.btnOpenFolder.HighEmphasis = true;
            this.btnOpenFolder.Icon = null;
            this.btnOpenFolder.Location = new System.Drawing.Point(13, 276);
            this.btnOpenFolder.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOpenFolder.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnOpenFolder.Size = new System.Drawing.Size(120, 40);
            this.btnOpenFolder.TabIndex = 11114;
            this.btnOpenFolder.Text = "📂 Open Folder";
            this.btnOpenFolder.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnOpenFolder.UseAccentColor = false;
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // btnOpenScenarioEditor
            // 
            this.btnOpenScenarioEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenScenarioEditor.AutoSize = false;
            this.btnOpenScenarioEditor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOpenScenarioEditor.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnOpenScenarioEditor.Depth = 0;
            this.btnOpenScenarioEditor.HighEmphasis = true;
            this.btnOpenScenarioEditor.Icon = null;
            this.btnOpenScenarioEditor.Location = new System.Drawing.Point(13, 328);
            this.btnOpenScenarioEditor.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOpenScenarioEditor.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnOpenScenarioEditor.Name = "btnOpenScenarioEditor";
            this.btnOpenScenarioEditor.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnOpenScenarioEditor.Size = new System.Drawing.Size(120, 40);
            this.btnOpenScenarioEditor.TabIndex = 11113;
            this.btnOpenScenarioEditor.Text = "🔗 Scenario Editor";
            this.btnOpenScenarioEditor.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnOpenScenarioEditor.UseAccentColor = false;
            this.btnOpenScenarioEditor.UseVisualStyleBackColor = true;
            this.btnOpenScenarioEditor.Click += new System.EventHandler(this.btnOpenScenarioEditor_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.AutoSize = false;
            this.btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnClose.Depth = 0;
            this.btnClose.HighEmphasis = false;
            this.btnClose.Icon = null;
            this.btnClose.Location = new System.Drawing.Point(13, 380);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnClose.Name = "btnClose";
            this.btnClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnClose.Size = new System.Drawing.Size(120, 36);
            this.btnClose.TabIndex = 11112;
            this.btnClose.Text = "✕   Close";
            this.btnClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnClose.UseAccentColor = true;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // olvValidationResult
            // 
            this.olvValidationResult.CellEditUseWholeCell = false;
            this.olvValidationResult.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvValidationResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvValidationResult.HideSelection = false;
            this.olvValidationResult.Location = new System.Drawing.Point(0, 0);
            this.olvValidationResult.Name = "olvValidationResult";
            this.olvValidationResult.ShowGroups = false;
            this.olvValidationResult.Size = new System.Drawing.Size(666, 507);
            this.olvValidationResult.TabIndex = 3;
            this.olvValidationResult.UseCompatibleStateImageBehavior = false;
            this.olvValidationResult.View = System.Windows.Forms.View.Details;
            this.olvValidationResult.VirtualMode = true;
            // 
            // toolTipAnomaly
            // 
            this.toolTipAnomaly.AutoPopDelay = 10000;
            this.toolTipAnomaly.InitialDelay = 200;
            this.toolTipAnomaly.ReshowDelay = 100;
            // 
            // txtLogFilter
            // 
            this.txtLogFilter.AnimateReadOnly = false;
            this.txtLogFilter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.txtLogFilter.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.txtLogFilter.Depth = 0;
            this.txtLogFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtLogFilter.HideSelection = true;
            this.txtLogFilter.LeadingIcon = null;
            this.txtLogFilter.Location = new System.Drawing.Point(144, 10);
            this.txtLogFilter.MaxLength = 32767;
            this.txtLogFilter.MouseState = MaterialSkin.MouseState.OUT;
            this.txtLogFilter.Name = "txtLogFilter";
            this.txtLogFilter.PasswordChar = '\0';
            this.txtLogFilter.PrefixSuffixText = null;
            this.txtLogFilter.ReadOnly = false;
            this.txtLogFilter.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtLogFilter.SelectedText = "";
            this.txtLogFilter.SelectionLength = 0;
            this.txtLogFilter.SelectionStart = 0;
            this.txtLogFilter.ShortcutsEnabled = true;
            this.txtLogFilter.Size = new System.Drawing.Size(153, 48);
            this.txtLogFilter.TabIndex = 1;
            this.txtLogFilter.TabStop = false;
            this.txtLogFilter.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtLogFilter.TrailingIcon = null;
            this.txtLogFilter.UseSystemPasswordChar = false;
            // 
            // dtpTimeJump
            // 
            this.dtpTimeJump.CustomFormat = "HH:mm:ss";
            this.dtpTimeJump.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.dtpTimeJump.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTimeJump.Location = new System.Drawing.Point(15, 29);
            this.dtpTimeJump.Name = "dtpTimeJump";
            this.dtpTimeJump.ShowUpDown = true;
            this.dtpTimeJump.Size = new System.Drawing.Size(137, 23);
            this.dtpTimeJump.TabIndex = 4;
            // 
            // btnTimeJump
            // 
            this.btnTimeJump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTimeJump.AutoSize = false;
            this.btnTimeJump.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnTimeJump.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnTimeJump.Depth = 0;
            this.btnTimeJump.HighEmphasis = true;
            this.btnTimeJump.Icon = null;
            this.btnTimeJump.Location = new System.Drawing.Point(159, 5);
            this.btnTimeJump.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnTimeJump.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnTimeJump.Name = "btnTimeJump";
            this.btnTimeJump.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnTimeJump.Size = new System.Drawing.Size(74, 40);
            this.btnTimeJump.TabIndex = 11115;
            this.btnTimeJump.Text = "▶ Jump";
            this.btnTimeJump.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnTimeJump.UseAccentColor = false;
            this.btnTimeJump.UseVisualStyleBackColor = true;
            // 
            // cmbDateFilter
            // 
            this.cmbDateFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDateFilter.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cmbDateFilter.FormattingEnabled = true;
            this.cmbDateFilter.Location = new System.Drawing.Point(15, 4);
            this.cmbDateFilter.Name = "cmbDateFilter";
            this.cmbDateFilter.Size = new System.Drawing.Size(137, 23);
            this.cmbDateFilter.TabIndex = 11117;
            // 
            // LogValidatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 843);
            this.Controls.Add(this.txtLogFilter);
            this.Controls.Add(this.splitContainer1);
            this.Name = "LogValidatorForm";
            this.Text = "Log Validator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogValidatorForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvValidatorRawLog)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvScenarioRepository)).EndInit();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvValidationResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private BrightIdeasSoftware.FastObjectListView olvValidatorRawLog;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolTip toolTipAnomaly;
        private MaterialSkin.Controls.MaterialTextBox2 txtLogFilter;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Panel panel1;
        private MaterialSkin.Controls.MaterialButton btnReset;
        private MaterialSkin.Controls.MaterialLabel lblAnomalyWarning;
        private MaterialSkin.Controls.MaterialButton materialButton1;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.TreeView treeScenarioGroup;
        private BrightIdeasSoftware.ObjectListView olvScenarioRepository;
        private System.Windows.Forms.Panel panel2;
        private BrightIdeasSoftware.TreeListView olvValidationResult;
        private MaterialSkin.Controls.MaterialButton btnOpenFolder;
        private MaterialSkin.Controls.MaterialButton btnOpenScenarioEditor;
        private MaterialSkin.Controls.MaterialButton btnClose;
        private MaterialSkin.Controls.MaterialButton btnTimeJump;
        private System.Windows.Forms.DateTimePicker dtpTimeJump;
        private System.Windows.Forms.ComboBox cmbDateFilter;
    }
}