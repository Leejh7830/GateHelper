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
            this.olvRawLog = new BrightIdeasSoftware.FastObjectListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rtbMaskedPreview = new System.Windows.Forms.RichTextBox();
            this.txtEventName = new MaterialSkin.Controls.MaterialTextBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.btnRegisterUnit = new MaterialSkin.Controls.MaterialButton();
            this.olvUnitRepository = new BrightIdeasSoftware.ObjectListView();
            this.olvScenarioLadder = new BrightIdeasSoftware.ObjectListView();
            this.pnlControlButtons = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlDropZone.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvRawLog)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvUnitRepository)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvScenarioLadder)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.pnlDropZone);
            this.splitContainer1.Panel1.Controls.Add(this.olvRawLog);
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
            // 
            // lblRawLogDrag
            // 
            this.lblRawLogDrag.Depth = 0;
            this.lblRawLogDrag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRawLogDrag.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblRawLogDrag.Location = new System.Drawing.Point(0, 0);
            this.lblRawLogDrag.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblRawLogDrag.Name = "lblRawLogDrag";
            this.lblRawLogDrag.Size = new System.Drawing.Size(219, 295);
            this.lblRawLogDrag.TabIndex = 0;
            this.lblRawLogDrag.Text = "Raw Log Drag";
            this.lblRawLogDrag.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // olvRawLog
            // 
            this.olvRawLog.CellEditUseWholeCell = false;
            this.olvRawLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvRawLog.FullRowSelect = true;
            this.olvRawLog.GridLines = true;
            this.olvRawLog.HideSelection = false;
            this.olvRawLog.Location = new System.Drawing.Point(0, 0);
            this.olvRawLog.Name = "olvRawLog";
            this.olvRawLog.ShowGroups = false;
            this.olvRawLog.Size = new System.Drawing.Size(385, 776);
            this.olvRawLog.TabIndex = 0;
            this.olvRawLog.UseCompatibleStateImageBehavior = false;
            this.olvRawLog.View = System.Windows.Forms.View.Details;
            this.olvRawLog.VirtualMode = true;
            this.olvRawLog.SelectedIndexChanged += new System.EventHandler(this.olvRawLog_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rtbMaskedPreview);
            this.groupBox1.Controls.Add(this.txtEventName);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(319, 180);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Event Builder";
            // 
            // rtbMaskedPreview
            // 
            this.rtbMaskedPreview.Location = new System.Drawing.Point(6, 85);
            this.rtbMaskedPreview.Name = "rtbMaskedPreview";
            this.rtbMaskedPreview.Size = new System.Drawing.Size(307, 89);
            this.rtbMaskedPreview.TabIndex = 2;
            this.rtbMaskedPreview.Text = "";
            // 
            // txtEventName
            // 
            this.txtEventName.AnimateReadOnly = false;
            this.txtEventName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEventName.Depth = 0;
            this.txtEventName.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.txtEventName.LeadingIcon = null;
            this.txtEventName.Location = new System.Drawing.Point(6, 29);
            this.txtEventName.MaxLength = 50;
            this.txtEventName.MouseState = MaterialSkin.MouseState.OUT;
            this.txtEventName.Multiline = false;
            this.txtEventName.Name = "txtEventName";
            this.txtEventName.Size = new System.Drawing.Size(307, 50);
            this.txtEventName.TabIndex = 0;
            this.txtEventName.Text = "";
            this.txtEventName.TrailingIcon = null;
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
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.btnRegisterUnit.Text = "Register";
            this.btnRegisterUnit.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnRegisterUnit.UseAccentColor = false;
            this.btnRegisterUnit.UseVisualStyleBackColor = true;
            // 
            // olvUnitRepository
            // 
            this.olvUnitRepository.CellEditUseWholeCell = false;
            this.olvUnitRepository.HideSelection = false;
            this.olvUnitRepository.Location = new System.Drawing.Point(37, 41);
            this.olvUnitRepository.Name = "olvUnitRepository";
            this.olvUnitRepository.Size = new System.Drawing.Size(149, 145);
            this.olvUnitRepository.TabIndex = 0;
            this.olvUnitRepository.UseCompatibleStateImageBehavior = false;
            this.olvUnitRepository.View = System.Windows.Forms.View.Details;
            // 
            // olvScenarioLadder
            // 
            this.olvScenarioLadder.AllowDrop = true;
            this.olvScenarioLadder.CellEditUseWholeCell = false;
            this.olvScenarioLadder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvScenarioLadder.FullRowSelect = true;
            this.olvScenarioLadder.GridLines = true;
            this.olvScenarioLadder.HideSelection = false;
            this.olvScenarioLadder.Location = new System.Drawing.Point(0, 0);
            this.olvScenarioLadder.Name = "olvScenarioLadder";
            this.olvScenarioLadder.Size = new System.Drawing.Size(1025, 555);
            this.olvScenarioLadder.TabIndex = 0;
            this.olvScenarioLadder.UseCompatibleStateImageBehavior = false;
            this.olvScenarioLadder.View = System.Windows.Forms.View.Details;
            // 
            // pnlControlButtons
            // 
            this.pnlControlButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlControlButtons.Location = new System.Drawing.Point(825, 0);
            this.pnlControlButtons.Name = "pnlControlButtons";
            this.pnlControlButtons.Size = new System.Drawing.Size(200, 555);
            this.pnlControlButtons.TabIndex = 1;
            // 
            // LogScenarioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 843);
            this.Controls.Add(this.splitContainer1);
            this.Name = "LogScenarioForm";
            this.Text = "Scenario Editor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlDropZone.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvRawLog)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvUnitRepository)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvScenarioLadder)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private BrightIdeasSoftware.FastObjectListView olvRawLog;
        private System.Windows.Forms.Panel pnlDropZone;
        private MaterialSkin.Controls.MaterialLabel lblRawLogDrag;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox rtbMaskedPreview;
        private MaterialSkin.Controls.MaterialTextBox txtEventName;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private MaterialSkin.Controls.MaterialButton btnRegisterUnit;
        private BrightIdeasSoftware.ObjectListView olvUnitRepository;
        private System.Windows.Forms.Panel pnlControlButtons;
        private BrightIdeasSoftware.ObjectListView olvScenarioLadder;
    }
}