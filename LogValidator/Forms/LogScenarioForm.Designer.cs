namespace GateHelper.LogValidator
{
    partial class LogScenarioForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pnlDropZone = new System.Windows.Forms.Panel();
            this.lblRawLogDrag = new MaterialSkin.Controls.MaterialLabel();
            this.olvScenarioRawLog = new BrightIdeasSoftware.FastObjectListView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.btnRegisterUnit = new MaterialSkin.Controls.MaterialButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtEventName = new System.Windows.Forms.TextBox();
            this.rtbMaskedPreview = new System.Windows.Forms.RichTextBox();
            this.olvUnitRepository = new BrightIdeasSoftware.ObjectListView();
            this.pnlControlButtons = new System.Windows.Forms.Panel();
            this.txtScenarioName = new System.Windows.Forms.TextBox();
            this.lblCurrentScenario = new MaterialSkin.Controls.MaterialLabel();
            this.btnLoadScenario = new MaterialSkin.Controls.MaterialButton();
            this.btnSaveScenario = new MaterialSkin.Controls.MaterialButton();
            this.btnNewScenario = new MaterialSkin.Controls.MaterialButton();
            this.olvScenarioLadder = new BrightIdeasSoftware.ObjectListView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlDropZone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvScenarioRawLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvUnitRepository)).BeginInit();
            this.pnlControlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvScenarioLadder)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 64);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pnlDropZone);
            this.splitContainer1.Panel1.Controls.Add(this.olvScenarioRawLog);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1414, 776);
            this.splitContainer1.SplitterDistance = 385;
            this.splitContainer1.TabIndex = 0;
            // 
            // pnlDropZone
            // 
            this.pnlDropZone.Controls.Add(this.lblRawLogDrag);
            this.pnlDropZone.Location = new System.Drawing.Point(14, 12);
            this.pnlDropZone.Name = "pnlDropZone";
            this.pnlDropZone.Size = new System.Drawing.Size(219, 295);
            this.pnlDropZone.TabIndex = 1;
            this.pnlDropZone.DragDrop += new System.Windows.Forms.DragEventHandler(this.PnlDropZone_DragDrop);
            this.pnlDropZone.DragEnter += new System.Windows.Forms.DragEventHandler(this.PnlDropZone_DragEnter);
            // 
            // lblRawLogDrag
            // 
            this.lblRawLogDrag.AllowDrop = true;
            this.lblRawLogDrag.Depth = 0;
            this.lblRawLogDrag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRawLogDrag.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblRawLogDrag.Location = new System.Drawing.Point(0, 0);
            this.lblRawLogDrag.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblRawLogDrag.Name = "lblRawLogDrag";
            this.lblRawLogDrag.Size = new System.Drawing.Size(219, 295);
            this.lblRawLogDrag.TabIndex = 0;
            this.lblRawLogDrag.Text = "Raw Log Drag HERE";
            this.lblRawLogDrag.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRawLogDrag.DragDrop += new System.Windows.Forms.DragEventHandler(this.PnlDropZone_DragDrop);
            this.lblRawLogDrag.DragEnter += new System.Windows.Forms.DragEventHandler(this.PnlDropZone_DragEnter);
            // 
            // olvScenarioRawLog
            // 
            this.olvScenarioRawLog.AllowDrop = true;
            this.olvScenarioRawLog.CellEditUseWholeCell = false;
            this.olvScenarioRawLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvScenarioRawLog.FullRowSelect = true;
            this.olvScenarioRawLog.GridLines = true;
            this.olvScenarioRawLog.HideSelection = false;
            this.olvScenarioRawLog.Location = new System.Drawing.Point(0, 0);
            this.olvScenarioRawLog.Name = "olvScenarioRawLog";
            this.olvScenarioRawLog.ShowGroups = false;
            this.olvScenarioRawLog.Size = new System.Drawing.Size(383, 774);
            this.olvScenarioRawLog.TabIndex = 0;
            this.olvScenarioRawLog.UseCompatibleStateImageBehavior = false;
            this.olvScenarioRawLog.View = System.Windows.Forms.View.Details;
            this.olvScenarioRawLog.VirtualMode = true;
            this.olvScenarioRawLog.SelectedIndexChanged += new System.EventHandler(this.olvRawLog_SelectedIndexChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.pnlControlButtons);
            this.splitContainer2.Panel2.Controls.Add(this.olvScenarioLadder);
            this.splitContainer2.Size = new System.Drawing.Size(1025, 776);
            this.splitContainer2.SplitterDistance = 217;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.btnRegisterUnit);
            this.splitContainer3.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.olvUnitRepository);
            this.splitContainer3.Size = new System.Drawing.Size(1025, 217);
            this.splitContainer3.SplitterDistance = 517;
            this.splitContainer3.TabIndex = 0;
            // 
            // btnRegisterUnit
            // 
            this.btnRegisterUnit.AutoSize = false;
            this.btnRegisterUnit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRegisterUnit.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnRegisterUnit.Depth = 0;
            this.btnRegisterUnit.HighEmphasis = true;
            this.btnRegisterUnit.Icon = null;
            this.btnRegisterUnit.Location = new System.Drawing.Point(378, 22);
            this.btnRegisterUnit.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRegisterUnit.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRegisterUnit.Name = "btnRegisterUnit";
            this.btnRegisterUnit.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnRegisterUnit.Size = new System.Drawing.Size(119, 43);
            this.btnRegisterUnit.TabIndex = 108;
            this.btnRegisterUnit.Text = "⚙️ Register";
            this.btnRegisterUnit.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRegisterUnit.UseAccentColor = false;
            this.btnRegisterUnit.UseVisualStyleBackColor = true;
            this.btnRegisterUnit.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtEventName);
            this.groupBox1.Controls.Add(this.rtbMaskedPreview);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(347, 209);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Event Builder";
            // 
            // txtEventName
            // 
            this.txtEventName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtEventName.Location = new System.Drawing.Point(6, 32);
            this.txtEventName.Name = "txtEventName";
            this.txtEventName.Size = new System.Drawing.Size(307, 21);
            this.txtEventName.TabIndex = 109;
            // 
            // rtbMaskedPreview
            // 
            this.rtbMaskedPreview.Location = new System.Drawing.Point(6, 85);
            this.rtbMaskedPreview.Name = "rtbMaskedPreview";
            this.rtbMaskedPreview.Size = new System.Drawing.Size(307, 89);
            this.rtbMaskedPreview.TabIndex = 2;
            this.rtbMaskedPreview.Text = "";
            // 
            // olvUnitRepository
            // 
            this.olvUnitRepository.CellEditUseWholeCell = false;
            this.olvUnitRepository.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvUnitRepository.HideSelection = false;
            this.olvUnitRepository.Location = new System.Drawing.Point(3, 2);
            this.olvUnitRepository.Name = "olvUnitRepository";
            this.olvUnitRepository.Size = new System.Drawing.Size(395, 210);
            this.olvUnitRepository.TabIndex = 0;
            this.olvUnitRepository.UseCompatibleStateImageBehavior = false;
            this.olvUnitRepository.View = System.Windows.Forms.View.Details;
            this.olvUnitRepository.DoubleClick += new System.EventHandler(this.olvUnitRepository_DoubleClick);
            this.olvUnitRepository.MouseDown += new System.Windows.Forms.MouseEventHandler(this.olvUnitRepository_MouseDown);
            // 
            // pnlControlButtons
            // 
            this.pnlControlButtons.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlControlButtons.Controls.Add(this.txtScenarioName);
            this.pnlControlButtons.Controls.Add(this.lblCurrentScenario);
            this.pnlControlButtons.Controls.Add(this.btnLoadScenario);
            this.pnlControlButtons.Controls.Add(this.btnSaveScenario);
            this.pnlControlButtons.Controls.Add(this.btnNewScenario);
            this.pnlControlButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlControlButtons.Location = new System.Drawing.Point(823, 0);
            this.pnlControlButtons.Name = "pnlControlButtons";
            this.pnlControlButtons.Size = new System.Drawing.Size(200, 553);
            this.pnlControlButtons.TabIndex = 1;
            // 
            // txtScenarioName
            // 
            this.txtScenarioName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtScenarioName.Location = new System.Drawing.Point(17, 29);
            this.txtScenarioName.Name = "txtScenarioName";
            this.txtScenarioName.Size = new System.Drawing.Size(168, 21);
            this.txtScenarioName.TabIndex = 114;
            // 
            // lblCurrentScenario
            // 
            this.lblCurrentScenario.AutoSize = true;
            this.lblCurrentScenario.Depth = 0;
            this.lblCurrentScenario.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblCurrentScenario.Location = new System.Drawing.Point(20, 7);
            this.lblCurrentScenario.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblCurrentScenario.Name = "lblCurrentScenario";
            this.lblCurrentScenario.Size = new System.Drawing.Size(118, 19);
            this.lblCurrentScenario.TabIndex = 113;
            this.lblCurrentScenario.Text = "Current Scenario";
            // 
            // btnLoadScenario
            // 
            this.btnLoadScenario.AutoSize = false;
            this.btnLoadScenario.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLoadScenario.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnLoadScenario.Depth = 0;
            this.btnLoadScenario.HighEmphasis = true;
            this.btnLoadScenario.Icon = null;
            this.btnLoadScenario.Location = new System.Drawing.Point(17, 143);
            this.btnLoadScenario.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnLoadScenario.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnLoadScenario.Name = "btnLoadScenario";
            this.btnLoadScenario.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnLoadScenario.Size = new System.Drawing.Size(168, 43);
            this.btnLoadScenario.TabIndex = 111;
            this.btnLoadScenario.Text = "📂 Load";
            this.btnLoadScenario.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnLoadScenario.UseAccentColor = false;
            this.btnLoadScenario.UseVisualStyleBackColor = true;
            this.btnLoadScenario.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSaveScenario
            // 
            this.btnSaveScenario.AutoSize = false;
            this.btnSaveScenario.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaveScenario.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSaveScenario.Depth = 0;
            this.btnSaveScenario.HighEmphasis = true;
            this.btnSaveScenario.Icon = null;
            this.btnSaveScenario.Location = new System.Drawing.Point(17, 198);
            this.btnSaveScenario.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSaveScenario.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSaveScenario.Name = "btnSaveScenario";
            this.btnSaveScenario.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSaveScenario.Size = new System.Drawing.Size(168, 43);
            this.btnSaveScenario.TabIndex = 110;
            this.btnSaveScenario.Text = "💾 Save";
            this.btnSaveScenario.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSaveScenario.UseAccentColor = false;
            this.btnSaveScenario.UseVisualStyleBackColor = true;
            this.btnSaveScenario.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnNewScenario
            // 
            this.btnNewScenario.AutoSize = false;
            this.btnNewScenario.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNewScenario.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnNewScenario.Depth = 0;
            this.btnNewScenario.HighEmphasis = true;
            this.btnNewScenario.Icon = null;
            this.btnNewScenario.Location = new System.Drawing.Point(17, 88);
            this.btnNewScenario.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnNewScenario.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnNewScenario.Name = "btnNewScenario";
            this.btnNewScenario.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnNewScenario.Size = new System.Drawing.Size(168, 43);
            this.btnNewScenario.TabIndex = 109;
            this.btnNewScenario.Text = "➕ New";
            this.btnNewScenario.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnNewScenario.UseAccentColor = false;
            this.btnNewScenario.UseVisualStyleBackColor = true;
            this.btnNewScenario.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // olvScenarioLadder
            // 
            this.olvScenarioLadder.AllowDrop = true;
            this.olvScenarioLadder.CellEditUseWholeCell = false;
            this.olvScenarioLadder.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvScenarioLadder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvScenarioLadder.FullRowSelect = true;
            this.olvScenarioLadder.GridLines = true;
            this.olvScenarioLadder.HideSelection = false;
            this.olvScenarioLadder.Location = new System.Drawing.Point(0, 0);
            this.olvScenarioLadder.Name = "olvScenarioLadder";
            this.olvScenarioLadder.Size = new System.Drawing.Size(1023, 553);
            this.olvScenarioLadder.TabIndex = 0;
            this.olvScenarioLadder.UseCompatibleStateImageBehavior = false;
            this.olvScenarioLadder.View = System.Windows.Forms.View.Details;
            this.olvScenarioLadder.DragDrop += new System.Windows.Forms.DragEventHandler(this.olvScenarioLadder_DragDrop);
            this.olvScenarioLadder.DragEnter += new System.Windows.Forms.DragEventHandler(this.olvScenarioLadder_DragEnter);
            // 
            // LogScenarioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 843);
            this.Controls.Add(this.splitContainer1);
            this.Name = "LogScenarioForm";
            this.Text = "Scenario Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogScenarioForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlDropZone.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvScenarioRawLog)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvUnitRepository)).EndInit();
            this.pnlControlButtons.ResumeLayout(false);
            this.pnlControlButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvScenarioLadder)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private BrightIdeasSoftware.FastObjectListView olvScenarioRawLog;
        private System.Windows.Forms.Panel pnlDropZone;
        private MaterialSkin.Controls.MaterialLabel lblRawLogDrag;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox rtbMaskedPreview;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private MaterialSkin.Controls.MaterialButton btnRegisterUnit;
        private BrightIdeasSoftware.ObjectListView olvUnitRepository;
        private System.Windows.Forms.Panel pnlControlButtons;
        private BrightIdeasSoftware.ObjectListView olvScenarioLadder;
        private MaterialSkin.Controls.MaterialButton btnLoadScenario;
        private MaterialSkin.Controls.MaterialButton btnSaveScenario;
        private MaterialSkin.Controls.MaterialButton btnNewScenario;
        private MaterialSkin.Controls.MaterialLabel lblCurrentScenario;
        private System.Windows.Forms.TextBox txtEventName;
        private System.Windows.Forms.TextBox txtScenarioName;
    }
}