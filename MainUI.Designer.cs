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
            this.SuspendLayout();
            // 
            // startBtn1
            // 
            this.startBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.startBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.startBtn1.Depth = 0;
            this.startBtn1.HighEmphasis = true;
            this.startBtn1.Icon = null;
            this.startBtn1.Location = new System.Drawing.Point(30, 82);
            this.startBtn1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.startBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.startBtn1.Name = "startBtn1";
            this.startBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.startBtn1.Size = new System.Drawing.Size(151, 36);
            this.startBtn1.TabIndex = 0;
            this.startBtn1.Text = "Start Operation";
            this.startBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.startBtn1.UseAccentColor = false;
            this.startBtn1.UseVisualStyleBackColor = true;
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1551, 896);
            this.Controls.Add(this.startBtn1);
            this.Name = "MainUI";
            this.Text = "GATE BOT";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton startBtn1;
    }
}

