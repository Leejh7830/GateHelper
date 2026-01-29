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
            this.btnHint = new MaterialSkin.Controls.MaterialButton();
            this.btnExtreme = new MaterialSkin.Controls.MaterialButton();
            this.btnInsane = new MaterialSkin.Controls.MaterialButton();
            this.btnExtreme2 = new MaterialSkin.Controls.MaterialButton();
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
            this.btnEasy.Location = new System.Drawing.Point(213, 141);
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
            this.btnNormal.Location = new System.Drawing.Point(213, 203);
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
            this.btnHard.Location = new System.Drawing.Point(213, 265);
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
            // btnHint
            // 
            this.btnHint.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnHint.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnHint.Depth = 0;
            this.btnHint.HighEmphasis = true;
            this.btnHint.Icon = null;
            this.btnHint.Location = new System.Drawing.Point(548, 10);
            this.btnHint.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnHint.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnHint.Name = "btnHint";
            this.btnHint.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnHint.Size = new System.Drawing.Size(64, 36);
            this.btnHint.TabIndex = 5;
            this.btnHint.Text = "HINT";
            this.btnHint.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnHint.UseAccentColor = true;
            this.btnHint.Click += new System.EventHandler(this.btnHint_Click);
            // 
            // btnExtreme
            // 
            this.btnExtreme.AutoSize = false;
            this.btnExtreme.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExtreme.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnExtreme.Depth = 0;
            this.btnExtreme.HighEmphasis = true;
            this.btnExtreme.Icon = null;
            this.btnExtreme.Location = new System.Drawing.Point(213, 389);
            this.btnExtreme.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnExtreme.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnExtreme.Name = "btnExtreme";
            this.btnExtreme.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnExtreme.Size = new System.Drawing.Size(200, 50);
            this.btnExtreme.TabIndex = 6;
            this.btnExtreme.Text = "EXTREME";
            this.btnExtreme.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnExtreme.UseAccentColor = true;
            this.btnExtreme.Click += new System.EventHandler(this.btnExtreme_Click);
            // 
            // btnInsane
            // 
            this.btnInsane.AutoSize = false;
            this.btnInsane.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnInsane.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnInsane.Depth = 0;
            this.btnInsane.HighEmphasis = true;
            this.btnInsane.Icon = null;
            this.btnInsane.Location = new System.Drawing.Point(213, 327);
            this.btnInsane.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnInsane.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnInsane.Name = "btnInsane";
            this.btnInsane.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnInsane.Size = new System.Drawing.Size(200, 50);
            this.btnInsane.TabIndex = 7;
            this.btnInsane.Text = "INSANE";
            this.btnInsane.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnInsane.UseAccentColor = true;
            this.btnInsane.Click += new System.EventHandler(this.btnInsane_Click);
            // 
            // btnExtreme2
            // 
            this.btnExtreme2.AutoSize = false;
            this.btnExtreme2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExtreme2.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnExtreme2.Depth = 0;
            this.btnExtreme2.HighEmphasis = true;
            this.btnExtreme2.Icon = null;
            this.btnExtreme2.Location = new System.Drawing.Point(213, 451);
            this.btnExtreme2.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnExtreme2.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnExtreme2.Name = "btnExtreme2";
            this.btnExtreme2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnExtreme2.Size = new System.Drawing.Size(200, 50);
            this.btnExtreme2.TabIndex = 8;
            this.btnExtreme2.Text = "EXTREME - II";
            this.btnExtreme2.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnExtreme2.UseAccentColor = true;
            this.btnExtreme2.Click += new System.EventHandler(this.btnExtreme2_Click);
            // 
            // SignalLinkControl
            // 
            this.Controls.Add(this.btnExtreme2);
            this.Controls.Add(this.btnInsane);
            this.Controls.Add(this.btnExtreme);
            this.Controls.Add(this.btnHint);
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
        private MaterialSkin.Controls.MaterialButton btnHint;
        private MaterialSkin.Controls.MaterialButton btnExtreme;
        private MaterialSkin.Controls.MaterialButton btnInsane;
        private MaterialSkin.Controls.MaterialButton btnExtreme2;
    }
}