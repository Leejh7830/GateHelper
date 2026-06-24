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
            this.olvValidatorRawLog = new BrightIdeasSoftware.FastObjectListView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.materialProgressBar1 = new MaterialSkin.Controls.MaterialProgressBar();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.materialButton1 = new MaterialSkin.Controls.MaterialButton();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.treeScenarioGroup = new System.Windows.Forms.TreeView();
            this.olvScenarioRepository = new BrightIdeasSoftware.ObjectListView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.materialButton2 = new MaterialSkin.Controls.MaterialButton();
            this.olvValidationResult = new BrightIdeasSoftware.TreeListView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.olvValidatorRawLog);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1414, 776);
            this.splitContainer1.SplitterDistance = 385;
            this.splitContainer1.TabIndex = 0;
            // 
            // olvValidatorRawLog
            // 
            this.olvValidatorRawLog.AllowDrop = true;
            this.olvValidatorRawLog.CellEditUseWholeCell = false;
            this.olvValidatorRawLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvValidatorRawLog.FullRowSelect = true;
            this.olvValidatorRawLog.GridLines = true;
            this.olvValidatorRawLog.HideSelection = false;
            this.olvValidatorRawLog.Location = new System.Drawing.Point(0, 0);
            this.olvValidatorRawLog.Name = "olvValidatorRawLog";
            this.olvValidatorRawLog.ShowGroups = false;
            this.olvValidatorRawLog.Size = new System.Drawing.Size(385, 776);
            this.olvValidatorRawLog.TabIndex = 1;
            this.olvValidatorRawLog.UseCompatibleStateImageBehavior = false;
            this.olvValidatorRawLog.View = System.Windows.Forms.View.Details;
            this.olvValidatorRawLog.VirtualMode = true;
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
            this.splitContainer2.Panel2.Controls.Add(this.panel2);
            this.splitContainer2.Panel2.Controls.Add(this.olvValidationResult);
            this.splitContainer2.Size = new System.Drawing.Size(1025, 776);
            this.splitContainer2.SplitterDistance = 265;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.splitContainer3.Size = new System.Drawing.Size(1025, 265);
            this.splitContainer3.SplitterDistance = 63;
            this.splitContainer3.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.materialProgressBar1);
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Controls.Add(this.materialButton1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1025, 63);
            this.panel1.TabIndex = 0;
            // 
            // materialProgressBar1
            // 
            this.materialProgressBar1.Depth = 0;
            this.materialProgressBar1.Location = new System.Drawing.Point(452, 37);
            this.materialProgressBar1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialProgressBar1.Name = "materialProgressBar1";
            this.materialProgressBar1.Size = new System.Drawing.Size(152, 5);
            this.materialProgressBar1.TabIndex = 0;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(303, 19);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(100, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // materialButton1
            // 
            this.materialButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.materialButton1.Depth = 0;
            this.materialButton1.HighEmphasis = true;
            this.materialButton1.Icon = null;
            this.materialButton1.Location = new System.Drawing.Point(12, 12);
            this.materialButton1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialButton1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton1.Name = "materialButton1";
            this.materialButton1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.materialButton1.Size = new System.Drawing.Size(114, 36);
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
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.olvScenarioRepository);
            this.splitContainer4.Size = new System.Drawing.Size(1025, 198);
            this.splitContainer4.SplitterDistance = 448;
            this.splitContainer4.TabIndex = 0;
            // 
            // treeScenarioGroup
            // 
            this.treeScenarioGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeScenarioGroup.Location = new System.Drawing.Point(0, 0);
            this.treeScenarioGroup.Name = "treeScenarioGroup";
            this.treeScenarioGroup.Size = new System.Drawing.Size(448, 198);
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
            this.olvScenarioRepository.Size = new System.Drawing.Size(573, 198);
            this.olvScenarioRepository.TabIndex = 2;
            this.olvScenarioRepository.UseCompatibleStateImageBehavior = false;
            this.olvScenarioRepository.View = System.Windows.Forms.View.Details;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.materialButton2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(875, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(150, 507);
            this.panel2.TabIndex = 4;
            // 
            // materialButton2
            // 
            this.materialButton2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton2.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.materialButton2.Depth = 0;
            this.materialButton2.HighEmphasis = true;
            this.materialButton2.Icon = null;
            this.materialButton2.Location = new System.Drawing.Point(23, 25);
            this.materialButton2.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialButton2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton2.Name = "materialButton2";
            this.materialButton2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.materialButton2.Size = new System.Drawing.Size(98, 36);
            this.materialButton2.TabIndex = 5;
            this.materialButton2.Text = "🔍 Validate";
            this.materialButton2.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.materialButton2.UseAccentColor = false;
            this.materialButton2.UseVisualStyleBackColor = true;
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
            this.olvValidationResult.Size = new System.Drawing.Size(1025, 507);
            this.olvValidationResult.TabIndex = 3;
            this.olvValidationResult.UseCompatibleStateImageBehavior = false;
            this.olvValidationResult.View = System.Windows.Forms.View.Details;
            this.olvValidationResult.VirtualMode = true;
            // 
            // LogValidatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 843);
            this.Controls.Add(this.splitContainer1);
            this.Name = "LogValidatorForm";
            this.Text = "Log Validator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogValidatorForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
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
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvValidationResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private BrightIdeasSoftware.FastObjectListView olvValidatorRawLog;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel panel1;
        private BrightIdeasSoftware.ObjectListView olvScenarioRepository;
        private MaterialSkin.Controls.MaterialProgressBar materialProgressBar1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private MaterialSkin.Controls.MaterialButton materialButton1;
        // private BrightIdeasSoftware.ObjectListView olvValidationResult;
        private BrightIdeasSoftware.TreeListView olvValidationResult;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.TreeView treeScenarioGroup;
        private MaterialSkin.Controls.MaterialButton materialButton2;
    }
}