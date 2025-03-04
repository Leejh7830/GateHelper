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
            this.GateIDBox1 = new MaterialSkin.Controls.MaterialTextBox();
            this.GatePWBox1 = new MaterialSkin.Controls.MaterialTextBox();
            this.LoginBtn1 = new MaterialSkin.Controls.MaterialButton();
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
            this.StartBtn1.Size = new System.Drawing.Size(390, 40);
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
            this.ConnectBtn1.Size = new System.Drawing.Size(390, 39);
            this.ConnectBtn1.TabIndex = 1;
            this.ConnectBtn1.Text = "Connect";
            this.ConnectBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.ConnectBtn1.UseAccentColor = false;
            this.ConnectBtn1.UseVisualStyleBackColor = true;
            this.ConnectBtn1.Click += new System.EventHandler(this.ConnectBtn1_Click);
            // 
            // GateIDBox1
            // 
            this.GateIDBox1.AnimateReadOnly = false;
            this.GateIDBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.GateIDBox1.Depth = 0;
            this.GateIDBox1.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.GateIDBox1.LeadingIcon = null;
            this.GateIDBox1.Location = new System.Drawing.Point(5, 165);
            this.GateIDBox1.MaxLength = 30;
            this.GateIDBox1.MouseState = MaterialSkin.MouseState.OUT;
            this.GateIDBox1.Multiline = false;
            this.GateIDBox1.Name = "GateIDBox1";
            this.GateIDBox1.Size = new System.Drawing.Size(272, 50);
            this.GateIDBox1.TabIndex = 2;
            this.GateIDBox1.Text = "";
            this.GateIDBox1.TrailingIcon = null;
            // 
            // GatePWBox1
            // 
            this.GatePWBox1.AnimateReadOnly = false;
            this.GatePWBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.GatePWBox1.Depth = 0;
            this.GatePWBox1.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.GatePWBox1.LeadingIcon = null;
            this.GatePWBox1.Location = new System.Drawing.Point(5, 221);
            this.GatePWBox1.MaxLength = 30;
            this.GatePWBox1.MouseState = MaterialSkin.MouseState.OUT;
            this.GatePWBox1.Multiline = false;
            this.GatePWBox1.Name = "GatePWBox1";
            this.GatePWBox1.Password = true;
            this.GatePWBox1.Size = new System.Drawing.Size(272, 50);
            this.GatePWBox1.TabIndex = 3;
            this.GatePWBox1.Text = "";
            this.GatePWBox1.TrailingIcon = null;
            // 
            // LoginBtn1
            // 
            this.LoginBtn1.AutoSize = false;
            this.LoginBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.LoginBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.LoginBtn1.Depth = 0;
            this.LoginBtn1.HighEmphasis = true;
            this.LoginBtn1.Icon = null;
            this.LoginBtn1.Location = new System.Drawing.Point(284, 165);
            this.LoginBtn1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.LoginBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.LoginBtn1.Name = "LoginBtn1";
            this.LoginBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.LoginBtn1.Size = new System.Drawing.Size(111, 106);
            this.LoginBtn1.TabIndex = 4;
            this.LoginBtn1.Text = "LOGIN";
            this.LoginBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.LoginBtn1.UseAccentColor = false;
            this.LoginBtn1.UseVisualStyleBackColor = true;
            this.LoginBtn1.Click += new System.EventHandler(this.LoginBtn1_Click);
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1086, 597);
            this.Controls.Add(this.LoginBtn1);
            this.Controls.Add(this.GatePWBox1);
            this.Controls.Add(this.GateIDBox1);
            this.Controls.Add(this.ConnectBtn1);
            this.Controls.Add(this.StartBtn1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainUI";
            this.Padding = new System.Windows.Forms.Padding(2, 43, 2, 2);
            this.Text = "GATE BOT";
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton StartBtn1;
        private MaterialSkin.Controls.MaterialButton ConnectBtn1;
        private MaterialSkin.Controls.MaterialTextBox GateIDBox1;
        private MaterialSkin.Controls.MaterialTextBox GatePWBox1;
        private MaterialSkin.Controls.MaterialButton LoginBtn1;
    }
}

