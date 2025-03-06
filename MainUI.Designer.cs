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
            this.ConnectBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.GateIDTxt1 = new MaterialSkin.Controls.MaterialTextBox();
            this.GatePWTxt1 = new MaterialSkin.Controls.MaterialTextBox();
            this.LoginBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.TestBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.SearchTxt1 = new MaterialSkin.Controls.MaterialTextBox();
            this.SearchBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.SearchBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SearchBox1.SuspendLayout();
            this.groupBox1.SuspendLayout();
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
            this.StartBtn1.Location = new System.Drawing.Point(5, 70);
            this.StartBtn1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.StartBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.StartBtn1.Name = "StartBtn1";
            this.StartBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.StartBtn1.Size = new System.Drawing.Size(374, 40);
            this.StartBtn1.TabIndex = 0;
            this.StartBtn1.Text = "Start Operation";
            this.StartBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.StartBtn1.UseAccentColor = false;
            this.StartBtn1.UseVisualStyleBackColor = true;
            this.StartBtn1.Click += new System.EventHandler(this.StartBtn1_Click);
            // 
            // ConnectBtn1
            // 
            this.ConnectBtn1.AutoSize = false;
            this.ConnectBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ConnectBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.ConnectBtn1.Depth = 0;
            this.ConnectBtn1.HighEmphasis = true;
            this.ConnectBtn1.Icon = null;
            this.ConnectBtn1.Location = new System.Drawing.Point(5, 117);
            this.ConnectBtn1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.ConnectBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.ConnectBtn1.Name = "ConnectBtn1";
            this.ConnectBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.ConnectBtn1.Size = new System.Drawing.Size(374, 39);
            this.ConnectBtn1.TabIndex = 1;
            this.ConnectBtn1.Text = "Connect";
            this.ConnectBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.ConnectBtn1.UseAccentColor = false;
            this.ConnectBtn1.UseVisualStyleBackColor = true;
            this.ConnectBtn1.Click += new System.EventHandler(this.ConnectBtn1_Click);
            // 
            // GateIDTxt1
            // 
            this.GateIDTxt1.AnimateReadOnly = false;
            this.GateIDTxt1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.GateIDTxt1.Depth = 0;
            this.GateIDTxt1.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.GateIDTxt1.LeadingIcon = null;
            this.GateIDTxt1.Location = new System.Drawing.Point(6, 20);
            this.GateIDTxt1.MaxLength = 30;
            this.GateIDTxt1.MouseState = MaterialSkin.MouseState.OUT;
            this.GateIDTxt1.Multiline = false;
            this.GateIDTxt1.Name = "GateIDTxt1";
            this.GateIDTxt1.Size = new System.Drawing.Size(244, 50);
            this.GateIDTxt1.TabIndex = 2;
            this.GateIDTxt1.Text = "";
            this.GateIDTxt1.TrailingIcon = null;
            // 
            // GatePWTxt1
            // 
            this.GatePWTxt1.AnimateReadOnly = false;
            this.GatePWTxt1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.GatePWTxt1.Depth = 0;
            this.GatePWTxt1.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.GatePWTxt1.LeadingIcon = null;
            this.GatePWTxt1.Location = new System.Drawing.Point(6, 76);
            this.GatePWTxt1.MaxLength = 30;
            this.GatePWTxt1.MouseState = MaterialSkin.MouseState.OUT;
            this.GatePWTxt1.Multiline = false;
            this.GatePWTxt1.Name = "GatePWTxt1";
            this.GatePWTxt1.Password = true;
            this.GatePWTxt1.Size = new System.Drawing.Size(244, 50);
            this.GatePWTxt1.TabIndex = 3;
            this.GatePWTxt1.Text = "";
            this.GatePWTxt1.TrailingIcon = null;
            // 
            // LoginBtn1
            // 
            this.LoginBtn1.AutoSize = false;
            this.LoginBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.LoginBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.LoginBtn1.Depth = 0;
            this.LoginBtn1.HighEmphasis = true;
            this.LoginBtn1.Icon = null;
            this.LoginBtn1.Location = new System.Drawing.Point(257, 20);
            this.LoginBtn1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.LoginBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.LoginBtn1.Name = "LoginBtn1";
            this.LoginBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.LoginBtn1.Size = new System.Drawing.Size(117, 106);
            this.LoginBtn1.TabIndex = 4;
            this.LoginBtn1.Text = "LOGIN";
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
            this.TestBtn1.Location = new System.Drawing.Point(413, 70);
            this.TestBtn1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.TestBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.TestBtn1.Name = "TestBtn1";
            this.TestBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.TestBtn1.Size = new System.Drawing.Size(94, 36);
            this.TestBtn1.TabIndex = 5;
            this.TestBtn1.Text = "FuncTest";
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
            this.SearchTxt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.SearchTxt1.LeadingIcon = null;
            this.SearchTxt1.Location = new System.Drawing.Point(6, 20);
            this.SearchTxt1.MaxLength = 50;
            this.SearchTxt1.MouseState = MaterialSkin.MouseState.OUT;
            this.SearchTxt1.Multiline = false;
            this.SearchTxt1.Name = "SearchTxt1";
            this.SearchTxt1.Size = new System.Drawing.Size(244, 50);
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
            this.SearchBtn1.Location = new System.Drawing.Point(257, 20);
            this.SearchBtn1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.SearchBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.SearchBtn1.Name = "SearchBtn1";
            this.SearchBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.SearchBtn1.Size = new System.Drawing.Size(111, 47);
            this.SearchBtn1.TabIndex = 7;
            this.SearchBtn1.Text = "Search";
            this.SearchBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.SearchBtn1.UseAccentColor = false;
            this.SearchBtn1.UseVisualStyleBackColor = true;
            this.SearchBtn1.Click += new System.EventHandler(this.SearchBtn1_Click);
            // 
            // SearchBox1
            // 
            this.SearchBox1.Controls.Add(this.SearchBtn1);
            this.SearchBox1.Controls.Add(this.SearchTxt1);
            this.SearchBox1.Location = new System.Drawing.Point(5, 313);
            this.SearchBox1.Name = "SearchBox1";
            this.SearchBox1.Size = new System.Drawing.Size(390, 76);
            this.SearchBox1.TabIndex = 6;
            this.SearchBox1.TabStop = false;
            this.SearchBox1.Text = "Search";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.GateIDTxt1);
            this.groupBox1.Controls.Add(this.GatePWTxt1);
            this.groupBox1.Controls.Add(this.LoginBtn1);
            this.groupBox1.Location = new System.Drawing.Point(5, 165);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(390, 142);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1086, 597);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.SearchBox1);
            this.Controls.Add(this.TestBtn1);
            this.Controls.Add(this.ConnectBtn1);
            this.Controls.Add(this.StartBtn1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainUI";
            this.Padding = new System.Windows.Forms.Padding(2, 43, 2, 2);
            this.Text = "GATE BOT";
            this.SearchBox1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton StartBtn1;
        private MaterialSkin.Controls.MaterialButton ConnectBtn1;
        private MaterialSkin.Controls.MaterialTextBox GateIDTxt1;
        private MaterialSkin.Controls.MaterialTextBox GatePWTxt1;
        private MaterialSkin.Controls.MaterialButton LoginBtn1;
        private MaterialSkin.Controls.MaterialButton TestBtn1;
        private MaterialSkin.Controls.MaterialTextBox SearchTxt1;
        private MaterialSkin.Controls.MaterialButton SearchBtn1;
        private System.Windows.Forms.GroupBox SearchBox1;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

