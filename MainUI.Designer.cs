namespace GateBot
{
    partial class MainUI
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

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.StartBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.StartBtn2 = new MaterialSkin.Controls.MaterialButton();
            this.LoginBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.TestBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.SearchTxt1 = new MaterialSkin.Controls.MaterialTextBox();
            this.SearchBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.SearchGroup1 = new System.Windows.Forms.GroupBox();
            this.LoginGroup1 = new System.Windows.Forms.GroupBox();
            this.ConnectBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.GatePWTxt1 = new MaterialSkin.Controls.MaterialTextBox2();
            this.GateIDTxt1 = new MaterialSkin.Controls.MaterialTextBox2();
            this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.TabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
            this.materialListView1 = new MaterialSkin.Controls.MaterialListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DisablePopupCheckBox1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.SearchGroup1.SuspendLayout();
            this.LoginGroup1.SuspendLayout();
            this.materialTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartBtn1
            // 
            this.StartBtn1.AutoSize = false;
            this.StartBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.StartBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.StartBtn1.Depth = 0;
            this.StartBtn1.Font = new System.Drawing.Font("돋움", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartBtn1.HighEmphasis = true;
            this.StartBtn1.Icon = null;
            this.StartBtn1.Location = new System.Drawing.Point(19, 110);
            this.StartBtn1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.StartBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.StartBtn1.Name = "StartBtn1";
            this.StartBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.StartBtn1.Size = new System.Drawing.Size(300, 60);
            this.StartBtn1.TabIndex = 0;
            this.StartBtn1.Text = "Start Operation";
            this.StartBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.StartBtn1.UseAccentColor = false;
            this.StartBtn1.UseVisualStyleBackColor = true;
            this.StartBtn1.Click += new System.EventHandler(this.StartBtn1_Click);
            // 
            // StartBtn2
            // 
            this.StartBtn2.AutoSize = false;
            this.StartBtn2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.StartBtn2.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.StartBtn2.Depth = 0;
            this.StartBtn2.Enabled = false;
            this.StartBtn2.HighEmphasis = true;
            this.StartBtn2.Icon = null;
            this.StartBtn2.Location = new System.Drawing.Point(329, 110);
            this.StartBtn2.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.StartBtn2.MouseState = MaterialSkin.MouseState.HOVER;
            this.StartBtn2.Name = "StartBtn2";
            this.StartBtn2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.StartBtn2.Size = new System.Drawing.Size(300, 60);
            this.StartBtn2.TabIndex = 1;
            this.StartBtn2.Text = "Start2";
            this.StartBtn2.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.StartBtn2.UseAccentColor = false;
            this.StartBtn2.UseVisualStyleBackColor = true;
            this.StartBtn2.Click += new System.EventHandler(this.ConnectBtn1_Click);
            // 
            // LoginBtn1
            // 
            this.LoginBtn1.AutoSize = false;
            this.LoginBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.LoginBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.LoginBtn1.Depth = 0;
            this.LoginBtn1.HighEmphasis = true;
            this.LoginBtn1.Icon = null;
            this.LoginBtn1.Location = new System.Drawing.Point(847, 153);
            this.LoginBtn1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.LoginBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.LoginBtn1.Name = "LoginBtn1";
            this.LoginBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.LoginBtn1.Size = new System.Drawing.Size(167, 159);
            this.LoginBtn1.TabIndex = 4;
            this.LoginBtn1.Text = "GATEONE LOGIN";
            this.LoginBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.LoginBtn1.UseAccentColor = false;
            this.LoginBtn1.UseVisualStyleBackColor = true;
            this.LoginBtn1.Click += new System.EventHandler(this.LoginBtn1_Click);
            // 
            // TestBtn1
            // 
            this.TestBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TestBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.TestBtn1.Depth = 0;
            this.TestBtn1.HighEmphasis = true;
            this.TestBtn1.Icon = null;
            this.TestBtn1.Location = new System.Drawing.Point(1233, 105);
            this.TestBtn1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.TestBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.TestBtn1.Name = "TestBtn1";
            this.TestBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.TestBtn1.Size = new System.Drawing.Size(98, 36);
            this.TestBtn1.TabIndex = 5;
            this.TestBtn1.Text = "Func Test";
            this.TestBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.TestBtn1.UseAccentColor = false;
            this.TestBtn1.UseVisualStyleBackColor = true;
            this.TestBtn1.Click += new System.EventHandler(this.TestBtn1_Click);
            // 
            // SearchTxt1
            // 
            this.SearchTxt1.AnimateReadOnly = false;
            this.SearchTxt1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SearchTxt1.Depth = 0;
            this.SearchTxt1.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.SearchTxt1.LeadingIcon = null;
            this.SearchTxt1.Location = new System.Drawing.Point(9, 30);
            this.SearchTxt1.Margin = new System.Windows.Forms.Padding(4);
            this.SearchTxt1.MaxLength = 50;
            this.SearchTxt1.MouseState = MaterialSkin.MouseState.OUT;
            this.SearchTxt1.Multiline = false;
            this.SearchTxt1.Name = "SearchTxt1";
            this.SearchTxt1.Size = new System.Drawing.Size(344, 50);
            this.SearchTxt1.TabIndex = 7;
            this.SearchTxt1.Text = "";
            this.SearchTxt1.TrailingIcon = null;
            // 
            // SearchBtn1
            // 
            this.SearchBtn1.AutoSize = false;
            this.SearchBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SearchBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.SearchBtn1.Depth = 0;
            this.SearchBtn1.HighEmphasis = true;
            this.SearchBtn1.Icon = null;
            this.SearchBtn1.Location = new System.Drawing.Point(364, 30);
            this.SearchBtn1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.SearchBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.SearchBtn1.Name = "SearchBtn1";
            this.SearchBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.SearchBtn1.Size = new System.Drawing.Size(159, 70);
            this.SearchBtn1.TabIndex = 7;
            this.SearchBtn1.Text = "Search";
            this.SearchBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.SearchBtn1.UseAccentColor = false;
            this.SearchBtn1.UseVisualStyleBackColor = true;
            this.SearchBtn1.Click += new System.EventHandler(this.SearchBtn1_Click);
            // 
            // SearchGroup1
            // 
            this.SearchGroup1.Controls.Add(this.SearchBtn1);
            this.SearchGroup1.Controls.Add(this.SearchTxt1);
            this.SearchGroup1.Location = new System.Drawing.Point(7, 8);
            this.SearchGroup1.Margin = new System.Windows.Forms.Padding(4);
            this.SearchGroup1.Name = "SearchGroup1";
            this.SearchGroup1.Padding = new System.Windows.Forms.Padding(4);
            this.SearchGroup1.Size = new System.Drawing.Size(557, 114);
            this.SearchGroup1.TabIndex = 6;
            this.SearchGroup1.TabStop = false;
            this.SearchGroup1.Text = "Search";
            // 
            // LoginGroup1
            // 
            this.LoginGroup1.Controls.Add(this.ConnectBtn1);
            this.LoginGroup1.Controls.Add(this.GatePWTxt1);
            this.LoginGroup1.Controls.Add(this.GateIDTxt1);
            this.LoginGroup1.Location = new System.Drawing.Point(7, 8);
            this.LoginGroup1.Margin = new System.Windows.Forms.Padding(4);
            this.LoginGroup1.Name = "LoginGroup1";
            this.LoginGroup1.Padding = new System.Windows.Forms.Padding(4);
            this.LoginGroup1.Size = new System.Drawing.Size(512, 207);
            this.LoginGroup1.TabIndex = 7;
            this.LoginGroup1.TabStop = false;
            this.LoginGroup1.Text = "Login Info";
            // 
            // ConnectBtn1
            // 
            this.ConnectBtn1.AutoSize = false;
            this.ConnectBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ConnectBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.ConnectBtn1.Depth = 0;
            this.ConnectBtn1.HighEmphasis = true;
            this.ConnectBtn1.Icon = null;
            this.ConnectBtn1.Location = new System.Drawing.Point(268, 28);
            this.ConnectBtn1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.ConnectBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.ConnectBtn1.Name = "ConnectBtn1";
            this.ConnectBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.ConnectBtn1.Size = new System.Drawing.Size(118, 133);
            this.ConnectBtn1.TabIndex = 12;
            this.ConnectBtn1.Text = "Connect";
            this.ConnectBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.ConnectBtn1.UseAccentColor = false;
            this.ConnectBtn1.UseVisualStyleBackColor = true;
            // 
            // GatePWTxt1
            // 
            this.GatePWTxt1.AnimateReadOnly = false;
            this.GatePWTxt1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.GatePWTxt1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.GatePWTxt1.Depth = 0;
            this.GatePWTxt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.GatePWTxt1.HideSelection = true;
            this.GatePWTxt1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.GatePWTxt1.LeadingIcon = null;
            this.GatePWTxt1.Location = new System.Drawing.Point(9, 113);
            this.GatePWTxt1.MaxLength = 32767;
            this.GatePWTxt1.MouseState = MaterialSkin.MouseState.OUT;
            this.GatePWTxt1.Name = "GatePWTxt1";
            this.GatePWTxt1.PasswordChar = '\0';
            this.GatePWTxt1.PrefixSuffixText = null;
            this.GatePWTxt1.ReadOnly = false;
            this.GatePWTxt1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.GatePWTxt1.SelectedText = "";
            this.GatePWTxt1.SelectionLength = 0;
            this.GatePWTxt1.SelectionStart = 0;
            this.GatePWTxt1.ShortcutsEnabled = true;
            this.GatePWTxt1.Size = new System.Drawing.Size(250, 48);
            this.GatePWTxt1.TabIndex = 13;
            this.GatePWTxt1.TabStop = false;
            this.GatePWTxt1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.GatePWTxt1.TrailingIcon = null;
            this.GatePWTxt1.UseSystemPasswordChar = false;
            // 
            // GateIDTxt1
            // 
            this.GateIDTxt1.AnimateReadOnly = false;
            this.GateIDTxt1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.GateIDTxt1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.GateIDTxt1.Depth = 0;
            this.GateIDTxt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.GateIDTxt1.HideSelection = true;
            this.GateIDTxt1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.GateIDTxt1.LeadingIcon = null;
            this.GateIDTxt1.Location = new System.Drawing.Point(9, 28);
            this.GateIDTxt1.MaxLength = 32767;
            this.GateIDTxt1.MouseState = MaterialSkin.MouseState.OUT;
            this.GateIDTxt1.Name = "GateIDTxt1";
            this.GateIDTxt1.PasswordChar = '\0';
            this.GateIDTxt1.PrefixSuffixText = null;
            this.GateIDTxt1.ReadOnly = false;
            this.GateIDTxt1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.GateIDTxt1.SelectedText = "";
            this.GateIDTxt1.SelectionLength = 0;
            this.GateIDTxt1.SelectionStart = 0;
            this.GateIDTxt1.ShortcutsEnabled = true;
            this.GateIDTxt1.Size = new System.Drawing.Size(250, 48);
            this.GateIDTxt1.TabIndex = 12;
            this.GateIDTxt1.TabStop = false;
            this.GateIDTxt1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.GateIDTxt1.TrailingIcon = null;
            this.GateIDTxt1.UseSystemPasswordChar = false;
            // 
            // materialTabControl1
            // 
            this.materialTabControl1.Controls.Add(this.tabPage1);
            this.materialTabControl1.Controls.Add(this.tabPage2);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.Location = new System.Drawing.Point(26, 293);
            this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabControl1.Multiline = true;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 0;
            this.materialTabControl1.Size = new System.Drawing.Size(534, 254);
            this.materialTabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.LoginGroup1);
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(526, 222);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "LOGIN";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.SearchGroup1);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(526, 222);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "SEARCH";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // TabSelector1
            // 
            this.TabSelector1.BaseTabControl = this.materialTabControl1;
            this.TabSelector1.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.TabSelector1.Depth = 0;
            this.TabSelector1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TabSelector1.ForeColor = System.Drawing.SystemColors.Control;
            this.TabSelector1.Location = new System.Drawing.Point(11, 229);
            this.TabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
            this.TabSelector1.Name = "TabSelector1";
            this.TabSelector1.Size = new System.Drawing.Size(521, 48);
            this.TabSelector1.TabIndex = 5;
            this.TabSelector1.TabIndicatorHeight = 3;
            this.TabSelector1.Text = "materialTabSelector1";
            // 
            // materialListView1
            // 
            this.materialListView1.AutoSizeTable = false;
            this.materialListView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.materialListView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.materialListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.materialListView1.Depth = 0;
            this.materialListView1.FullRowSelect = true;
            this.materialListView1.HideSelection = false;
            this.materialListView1.Location = new System.Drawing.Point(11, 693);
            this.materialListView1.MinimumSize = new System.Drawing.Size(200, 100);
            this.materialListView1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialListView1.MouseState = MaterialSkin.MouseState.OUT;
            this.materialListView1.Name = "materialListView1";
            this.materialListView1.OwnerDraw = true;
            this.materialListView1.Size = new System.Drawing.Size(549, 294);
            this.materialListView1.TabIndex = 10;
            this.materialListView1.UseCompatibleStateImageBehavior = false;
            this.materialListView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 91;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 131;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Width = 126;
            // 
            // DisablePopupCheckBox1
            // 
            this.DisablePopupCheckBox1.AutoSize = true;
            this.DisablePopupCheckBox1.Depth = 0;
            this.DisablePopupCheckBox1.Location = new System.Drawing.Point(19, 990);
            this.DisablePopupCheckBox1.Margin = new System.Windows.Forms.Padding(0);
            this.DisablePopupCheckBox1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.DisablePopupCheckBox1.MouseState = MaterialSkin.MouseState.HOVER;
            this.DisablePopupCheckBox1.Name = "DisablePopupCheckBox1";
            this.DisablePopupCheckBox1.ReadOnly = false;
            this.DisablePopupCheckBox1.Ripple = true;
            this.DisablePopupCheckBox1.Size = new System.Drawing.Size(157, 37);
            this.DisablePopupCheckBox1.TabIndex = 11;
            this.DisablePopupCheckBox1.Text = "DISABLE POP-UP";
            this.DisablePopupCheckBox1.UseVisualStyleBackColor = true;
            this.DisablePopupCheckBox1.CheckedChanged += new System.EventHandler(this.DisablePopupCheckBox1_CheckedChanged);
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1140, 1048);
            this.Controls.Add(this.DisablePopupCheckBox1);
            this.Controls.Add(this.materialListView1);
            this.Controls.Add(this.TabSelector1);
            this.Controls.Add(this.materialTabControl1);
            this.Controls.Add(this.LoginBtn1);
            this.Controls.Add(this.TestBtn1);
            this.Controls.Add(this.StartBtn2);
            this.Controls.Add(this.StartBtn1);
            this.Name = "MainUI";
            this.Sizable = false;
            this.Text = "GATE HELPER";
            this.SearchGroup1.ResumeLayout(false);
            this.LoginGroup1.ResumeLayout(false);
            this.materialTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton StartBtn1;
        private MaterialSkin.Controls.MaterialButton StartBtn2;
        private MaterialSkin.Controls.MaterialButton LoginBtn1;
        private MaterialSkin.Controls.MaterialButton TestBtn1;
        private MaterialSkin.Controls.MaterialTextBox SearchTxt1;
        private MaterialSkin.Controls.MaterialButton SearchBtn1;
        private System.Windows.Forms.GroupBox SearchGroup1;
        private System.Windows.Forms.GroupBox LoginGroup1;
        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private MaterialSkin.Controls.MaterialTabSelector TabSelector1;
        private MaterialSkin.Controls.MaterialListView materialListView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private MaterialSkin.Controls.MaterialCheckbox DisablePopupCheckBox1;
        private MaterialSkin.Controls.MaterialTextBox2 GateIDTxt1;
        private MaterialSkin.Controls.MaterialTextBox2 GatePWTxt1;
        private MaterialSkin.Controls.MaterialButton ConnectBtn1;
    }
}

