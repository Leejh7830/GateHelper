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
            this.OlvWorkLog = new BrightIdeasSoftware.ObjectListView();
            this.No = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Date = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Title = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Content = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Status = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Tags = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Memo = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.LastUpdated = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Images = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.TxtWorkLog = new MaterialSkin.Controls.MaterialTextBox();
            this.BtnTemp1 = new MaterialSkin.Controls.MaterialButton();
            this.btnZoomIn = new MaterialSkin.Controls.MaterialButton();
            this.btnZoomOut = new MaterialSkin.Controls.MaterialButton();
            this.chkHideDone = new MaterialSkin.Controls.MaterialCheckbox();
            ((System.ComponentModel.ISupportInitialize)(this.OlvWorkLog)).BeginInit();
            this.SuspendLayout();
            // 
            // OlvWorkLog
            // 
            this.OlvWorkLog.AllColumns.Add(this.No);
            this.OlvWorkLog.AllColumns.Add(this.Date);
            this.OlvWorkLog.AllColumns.Add(this.Title);
            this.OlvWorkLog.AllColumns.Add(this.Content);
            this.OlvWorkLog.AllColumns.Add(this.Status);
            this.OlvWorkLog.AllColumns.Add(this.Tags);
            this.OlvWorkLog.AllColumns.Add(this.Memo);
            this.OlvWorkLog.AllColumns.Add(this.LastUpdated);
            this.OlvWorkLog.AllColumns.Add(this.Images);
            this.OlvWorkLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OlvWorkLog.CellEditUseWholeCell = false;
            this.OlvWorkLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.No,
            this.Date,
            this.Title,
            this.Content,
            this.Status,
            this.Tags,
            this.Memo,
            this.LastUpdated,
            this.Images});
            this.OlvWorkLog.Cursor = System.Windows.Forms.Cursors.Default;
            this.OlvWorkLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OlvWorkLog.HideSelection = false;
            this.OlvWorkLog.Location = new System.Drawing.Point(12, 144);
            this.OlvWorkLog.Name = "OlvWorkLog";
            this.OlvWorkLog.Size = new System.Drawing.Size(1120, 476);
            this.OlvWorkLog.TabIndex = 0;
            this.OlvWorkLog.UseCompatibleStateImageBehavior = false;
            this.OlvWorkLog.View = System.Windows.Forms.View.Details;
            this.OlvWorkLog.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OlvWorkLog_KeyDown);
            // 
            // No
            // 
            this.No.AspectName = "No";
            this.No.Text = "No";
            // 
            // Date
            // 
            this.Date.AspectName = "Date";
            this.Date.Text = "Date";
            this.Date.Width = 100;
            // 
            // Title
            // 
            this.Title.AspectName = "Title";
            this.Title.Text = "Title";
            // 
            // Content
            // 
            this.Content.AspectName = "Content";
            this.Content.Text = "Content";
            this.Content.Width = 400;
            // 
            // Status
            // 
            this.Status.AspectName = "Status";
            this.Status.Text = "Status";
            this.Status.Width = 80;
            // 
            // Tags
            // 
            this.Tags.AspectName = "Tags";
            this.Tags.Text = "Tags";
            // 
            // Memo
            // 
            this.Memo.AspectName = "Memo";
            this.Memo.Text = "Memo";
            this.Memo.Width = 120;
            // 
            // LastUpdated
            // 
            this.LastUpdated.AspectName = "LastUpdated";
            this.LastUpdated.Text = "LastUpdated";
            this.LastUpdated.Width = 150;
            // 
            // Images
            // 
            this.Images.AspectName = "Images";
            this.Images.Text = "Images";
            this.Images.Width = 80;
            // 
            // TxtWorkLog
            // 
            this.TxtWorkLog.AnimateReadOnly = false;
            this.TxtWorkLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TxtWorkLog.Depth = 0;
            this.TxtWorkLog.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TxtWorkLog.LeadingIcon = null;
            this.TxtWorkLog.Location = new System.Drawing.Point(12, 88);
            this.TxtWorkLog.MaxLength = 50;
            this.TxtWorkLog.MouseState = MaterialSkin.MouseState.OUT;
            this.TxtWorkLog.Multiline = false;
            this.TxtWorkLog.Name = "TxtWorkLog";
            this.TxtWorkLog.Size = new System.Drawing.Size(203, 50);
            this.TxtWorkLog.TabIndex = 1;
            this.TxtWorkLog.Text = "";
            this.TxtWorkLog.TrailingIcon = null;
            // 
            // BtnTemp1
            // 
            this.BtnTemp1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnTemp1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnTemp1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnTemp1.Depth = 0;
            this.BtnTemp1.HighEmphasis = true;
            this.BtnTemp1.Icon = null;
            this.BtnTemp1.Location = new System.Drawing.Point(12, 629);
            this.BtnTemp1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnTemp1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnTemp1.Name = "BtnTemp1";
            this.BtnTemp1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnTemp1.Size = new System.Drawing.Size(64, 36);
            this.BtnTemp1.TabIndex = 2;
            this.BtnTemp1.Text = "Temp";
            this.BtnTemp1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnTemp1.UseAccentColor = false;
            this.BtnTemp1.UseVisualStyleBackColor = true;
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZoomIn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnZoomIn.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnZoomIn.Depth = 0;
            this.btnZoomIn.HighEmphasis = true;
            this.btnZoomIn.Icon = null;
            this.btnZoomIn.Location = new System.Drawing.Point(995, 102);
            this.btnZoomIn.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnZoomIn.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnZoomIn.Size = new System.Drawing.Size(64, 36);
            this.btnZoomIn.TabIndex = 3;
            this.btnZoomIn.Text = "+";
            this.btnZoomIn.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnZoomIn.UseAccentColor = false;
            this.btnZoomIn.UseVisualStyleBackColor = true;
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZoomOut.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnZoomOut.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnZoomOut.Depth = 0;
            this.btnZoomOut.HighEmphasis = true;
            this.btnZoomOut.Icon = null;
            this.btnZoomOut.Location = new System.Drawing.Point(1067, 102);
            this.btnZoomOut.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnZoomOut.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnZoomOut.Size = new System.Drawing.Size(64, 36);
            this.btnZoomOut.TabIndex = 4;
            this.btnZoomOut.Text = "-";
            this.btnZoomOut.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnZoomOut.UseAccentColor = true;
            this.btnZoomOut.UseVisualStyleBackColor = true;
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // chkHideDone
            // 
            this.chkHideDone.AutoSize = true;
            this.chkHideDone.Depth = 0;
            this.chkHideDone.Location = new System.Drawing.Point(231, 88);
            this.chkHideDone.Margin = new System.Windows.Forms.Padding(0);
            this.chkHideDone.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkHideDone.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkHideDone.Name = "chkHideDone";
            this.chkHideDone.ReadOnly = false;
            this.chkHideDone.Ripple = true;
            this.chkHideDone.Size = new System.Drawing.Size(116, 37);
            this.chkHideDone.TabIndex = 5;
            this.chkHideDone.Text = "HIDE DONE";
            this.chkHideDone.UseVisualStyleBackColor = true;
            this.chkHideDone.CheckedChanged += new System.EventHandler(this.chkHideDone_CheckedChanged);
            // 
            // WorkLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1138, 674);
            this.Controls.Add(this.chkHideDone);
            this.Controls.Add(this.btnZoomOut);
            this.Controls.Add(this.btnZoomIn);
            this.Controls.Add(this.BtnTemp1);
            this.Controls.Add(this.TxtWorkLog);
            this.Controls.Add(this.OlvWorkLog);
            this.Name = "WorkLogForm";
            this.Text = "Work Log";
            ((System.ComponentModel.ISupportInitialize)(this.OlvWorkLog)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.ObjectListView OlvWorkLog;
        private BrightIdeasSoftware.OLVColumn Title;
        private BrightIdeasSoftware.OLVColumn Content;
        private BrightIdeasSoftware.OLVColumn Date;
        private BrightIdeasSoftware.OLVColumn Tags;
        private BrightIdeasSoftware.OLVColumn Memo;
        private MaterialSkin.Controls.MaterialTextBox TxtWorkLog;
        private MaterialSkin.Controls.MaterialButton BtnTemp1;
        private BrightIdeasSoftware.OLVColumn No;
        private BrightIdeasSoftware.OLVColumn Status;
        private BrightIdeasSoftware.OLVColumn LastUpdated;
        private MaterialSkin.Controls.MaterialButton btnZoomIn;
        private MaterialSkin.Controls.MaterialButton btnZoomOut;
        private BrightIdeasSoftware.OLVColumn Images;
        private MaterialSkin.Controls.MaterialCheckbox chkHideDone;
    }
}