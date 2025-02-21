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
            this.startBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.gateBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.SuspendLayout();
            // 
            // startBtn1
            // 
            this.startBtn1.AutoSize = false;
            this.startBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.startBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.startBtn1.Depth = 0;
            this.startBtn1.Font = new System.Drawing.Font("돋움", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startBtn1.HighEmphasis = true;
            this.startBtn1.Icon = null;
            this.startBtn1.Location = new System.Drawing.Point(5, 70);
            this.startBtn1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.startBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.startBtn1.Name = "startBtn1";
            this.startBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.startBtn1.Size = new System.Drawing.Size(390, 40);
            this.startBtn1.TabIndex = 0;
            this.startBtn1.Text = "Start Operation";
            this.startBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.startBtn1.UseAccentColor = false;
            this.startBtn1.UseVisualStyleBackColor = true;
            this.startBtn1.Click += new System.EventHandler(this.StartBtn1_Click);
            // 
            // gateBtn1
            // 
            this.gateBtn1.AutoSize = false;
            this.gateBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gateBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.gateBtn1.Depth = 0;
            this.gateBtn1.HighEmphasis = true;
            this.gateBtn1.Icon = null;
            this.gateBtn1.Location = new System.Drawing.Point(6, 120);
            this.gateBtn1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.gateBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.gateBtn1.Name = "gateBtn1";
            this.gateBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.gateBtn1.Size = new System.Drawing.Size(75, 36);
            this.gateBtn1.TabIndex = 1;
            this.gateBtn1.Text = "ID/PW";
            this.gateBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.gateBtn1.UseAccentColor = false;
            this.gateBtn1.UseVisualStyleBackColor = true;
            this.gateBtn1.Click += new System.EventHandler(this.gateBtn1_Click);
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1086, 597);
            this.Controls.Add(this.gateBtn1);
            this.Controls.Add(this.startBtn1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainUI";
            this.Padding = new System.Windows.Forms.Padding(2, 43, 2, 2);
            this.Text = "GATE BOT";
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton startBtn1;
        private MaterialSkin.Controls.MaterialButton gateBtn1;
    }
}

