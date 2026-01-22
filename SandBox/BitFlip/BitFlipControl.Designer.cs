namespace GateHelper
{
    partial class BitFlipControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.materialCard1 = new MaterialSkin.Controls.MaterialCard();
            this.lblGimmickNotify = new System.Windows.Forms.Label();
            this.lblTimer = new MaterialSkin.Controls.MaterialLabel();
            this.btnReset = new MaterialSkin.Controls.MaterialButton();
            this.btnBack = new MaterialSkin.Controls.MaterialButton();
            this.cmbDifficulty = new MaterialSkin.Controls.MaterialComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.materialCard1.SuspendLayout();
            this.SuspendLayout();
            // 
            // materialCard1
            // 
            this.materialCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.materialCard1.Controls.Add(this.lblGimmickNotify);
            this.materialCard1.Controls.Add(this.lblTimer);
            this.materialCard1.Controls.Add(this.btnReset);
            this.materialCard1.Controls.Add(this.btnBack);
            this.materialCard1.Controls.Add(this.cmbDifficulty);
            this.materialCard1.Controls.Add(this.tableLayoutPanel1);
            this.materialCard1.Depth = 0;
            this.materialCard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialCard1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialCard1.Location = new System.Drawing.Point(0, 0);
            this.materialCard1.Margin = new System.Windows.Forms.Padding(14);
            this.materialCard1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCard1.Name = "materialCard1";
            this.materialCard1.Padding = new System.Windows.Forms.Padding(14);
            this.materialCard1.Size = new System.Drawing.Size(600, 650);
            this.materialCard1.TabIndex = 0;
            // 
            // lblGimmickNotify
            // 
            this.lblGimmickNotify.AutoSize = true;
            this.lblGimmickNotify.Location = new System.Drawing.Point(360, 12);
            this.lblGimmickNotify.Name = "lblGimmickNotify";
            this.lblGimmickNotify.Size = new System.Drawing.Size(38, 12);
            this.lblGimmickNotify.TabIndex = 5;
            this.lblGimmickNotify.Text = "label1";
            this.lblGimmickNotify.Visible = false;
            // 
            // lblTimer
            // 
            this.lblTimer.AutoSize = true;
            this.lblTimer.Depth = 0;
            this.lblTimer.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblTimer.Location = new System.Drawing.Point(312, 9);
            this.lblTimer.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(42, 19);
            this.lblTimer.TabIndex = 3;
            this.lblTimer.Text = "Timer";
            // 
            // btnReset
            // 
            this.btnReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnReset.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnReset.Depth = 0;
            this.btnReset.HighEmphasis = true;
            this.btnReset.Icon = null;
            this.btnReset.Location = new System.Drawing.Point(137, 6);
            this.btnReset.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnReset.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnReset.Name = "btnReset";
            this.btnReset.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnReset.Size = new System.Drawing.Size(65, 36);
            this.btnReset.TabIndex = 2;
            this.btnReset.Text = "Reset";
            this.btnReset.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnReset.UseAccentColor = false;
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnBack
            // 
            this.btnBack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBack.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnBack.Depth = 0;
            this.btnBack.HighEmphasis = true;
            this.btnBack.Icon = null;
            this.btnBack.Location = new System.Drawing.Point(210, 6);
            this.btnBack.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnBack.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnBack.Name = "btnBack";
            this.btnBack.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnBack.Size = new System.Drawing.Size(64, 36);
            this.btnBack.TabIndex = 2;
            this.btnBack.Text = "Back";
            this.btnBack.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnBack.UseAccentColor = false;
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // cmbDifficulty
            // 
            this.cmbDifficulty.AutoResize = false;
            this.cmbDifficulty.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cmbDifficulty.Depth = 0;
            this.cmbDifficulty.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cmbDifficulty.DropDownHeight = 174;
            this.cmbDifficulty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDifficulty.DropDownWidth = 121;
            this.cmbDifficulty.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.cmbDifficulty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.cmbDifficulty.FormattingEnabled = true;
            this.cmbDifficulty.IntegralHeight = false;
            this.cmbDifficulty.ItemHeight = 43;
            this.cmbDifficulty.Items.AddRange(new object[] {
            "Easy",
            "Normal",
            "Hard",
            "Insane",
            "Extreme"});
            this.cmbDifficulty.Location = new System.Drawing.Point(9, -4);
            this.cmbDifficulty.MaxDropDownItems = 4;
            this.cmbDifficulty.MouseState = MaterialSkin.MouseState.OUT;
            this.cmbDifficulty.Name = "cmbDifficulty";
            this.cmbDifficulty.Size = new System.Drawing.Size(121, 49);
            this.cmbDifficulty.StartIndex = 0;
            this.cmbDifficulty.TabIndex = 1;
            this.cmbDifficulty.SelectedIndexChanged += new System.EventHandler(this.cmbDifficulty_SelectedIndexChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(14, 64);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(572, 572);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // BitFlipControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.materialCard1);
            this.Name = "BitFlipControl";
            this.Size = new System.Drawing.Size(600, 650);
            this.materialCard1.ResumeLayout(false);
            this.materialCard1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialCard materialCard1;
        internal MaterialSkin.Controls.MaterialButton btnBack;
        internal MaterialSkin.Controls.MaterialComboBox cmbDifficulty;
        internal MaterialSkin.Controls.MaterialButton btnReset;
        internal MaterialSkin.Controls.MaterialLabel lblTimer;
        internal System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblGimmickNotify;
    }
}
