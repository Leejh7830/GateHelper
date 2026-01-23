namespace GateHelper
{
    partial class SignalLinkControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        // 디자이너는 이 메서드가 "기계실"에 있어야만 안심하고 여기에 코드를 씁니다.
        private void InitializeComponent()
        {
            this.btnEasy = new MaterialSkin.Controls.MaterialButton();
            this.btnNormal = new MaterialSkin.Controls.MaterialButton();
            this.btnHard = new MaterialSkin.Controls.MaterialButton();
            this.btnReset = new MaterialSkin.Controls.MaterialButton();
            this.btnBack = new MaterialSkin.Controls.MaterialButton();
            this.SuspendLayout();
            // 
            // btnEasy
            // 
            this.btnEasy.AutoSize = false;
            this.btnEasy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnEasy.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnEasy.Depth = 0;
            this.btnEasy.HighEmphasis = true;
            this.btnEasy.Icon = null;
            this.btnEasy.Location = new System.Drawing.Point(214, 180);
            this.btnEasy.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnEasy.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnEasy.Name = "btnEasy";
            this.btnEasy.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnEasy.Size = new System.Drawing.Size(200, 50);
            this.btnEasy.TabIndex = 0;
            this.btnEasy.Text = "EASY";
            this.btnEasy.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnEasy.UseAccentColor = true;
            this.btnEasy.Click += new System.EventHandler(this.btnEasy_Click);
            // 
            // btnNormal
            // 
            this.btnNormal.AutoSize = false;
            this.btnNormal.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNormal.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnNormal.Depth = 0;
            this.btnNormal.HighEmphasis = true;
            this.btnNormal.Icon = null;
            this.btnNormal.Location = new System.Drawing.Point(214, 250);
            this.btnNormal.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnNormal.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnNormal.Name = "btnNormal";
            this.btnNormal.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnNormal.Size = new System.Drawing.Size(200, 50);
            this.btnNormal.TabIndex = 1;
            this.btnNormal.Text = "NORMAL";
            this.btnNormal.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnNormal.UseAccentColor = true;
            this.btnNormal.Click += new System.EventHandler(this.btnNormal_Click);
            // 
            // btnHard
            // 
            this.btnHard.AutoSize = false;
            this.btnHard.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnHard.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnHard.Depth = 0;
            this.btnHard.HighEmphasis = true;
            this.btnHard.Icon = null;
            this.btnHard.Location = new System.Drawing.Point(214, 320);
            this.btnHard.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnHard.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnHard.Name = "btnHard";
            this.btnHard.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnHard.Size = new System.Drawing.Size(200, 50);
            this.btnHard.TabIndex = 2;
            this.btnHard.Text = "HARD";
            this.btnHard.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnHard.UseAccentColor = true;
            this.btnHard.Click += new System.EventHandler(this.btnHard_Click);
            // 
            // btnReset
            // 
            this.btnReset.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnReset.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnReset.Depth = 0;
            this.btnReset.HighEmphasis = true;
            this.btnReset.Icon = null;
            this.btnReset.Location = new System.Drawing.Point(10, 10);
            this.btnReset.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnReset.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnReset.Name = "btnReset";
            this.btnReset.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnReset.Size = new System.Drawing.Size(65, 36);
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "RESET";
            this.btnReset.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnReset.UseAccentColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnBack
            // 
            this.btnBack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBack.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnBack.Depth = 0;
            this.btnBack.HighEmphasis = false;
            this.btnBack.Icon = null;
            this.btnBack.Location = new System.Drawing.Point(83, 10);
            this.btnBack.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnBack.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnBack.Name = "btnBack";
            this.btnBack.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnBack.Size = new System.Drawing.Size(64, 36);
            this.btnBack.TabIndex = 4;
            this.btnBack.Text = "BACK";
            this.btnBack.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnBack.UseAccentColor = false;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // SignalLinkControl
            // 
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnHard);
            this.Controls.Add(this.btnNormal);
            this.Controls.Add(this.btnEasy);
            this.Name = "SignalLinkControl";
            this.Size = new System.Drawing.Size(629, 630);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private MaterialSkin.Controls.MaterialButton btnEasy;
        private MaterialSkin.Controls.MaterialButton btnNormal;
        private MaterialSkin.Controls.MaterialButton btnHard;
        private MaterialSkin.Controls.MaterialButton btnReset;
        private MaterialSkin.Controls.MaterialButton btnBack;
    }
}