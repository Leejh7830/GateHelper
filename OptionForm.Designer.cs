
namespace GateHelper
{
    partial class OptionForm
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
            this.CBox_RemoveDuplicate = new MaterialSkin.Controls.MaterialCheckbox();
            this.CBox_AutoLogin = new MaterialSkin.Controls.MaterialCheckbox();
            this.CBox_ServerClickConnect = new MaterialSkin.Controls.MaterialCheckbox();
            this.CBox_TestMode = new MaterialSkin.Controls.MaterialCheckbox();
            this.CBox_DisablePopup = new MaterialSkin.Controls.MaterialCheckbox();
            this.CBox_FavOneClickConnect = new MaterialSkin.Controls.MaterialCheckbox();
            this.SuspendLayout();
            // 
            // CBox_RemoveDuplicate
            // 
            this.CBox_RemoveDuplicate.AutoSize = true;
            this.CBox_RemoveDuplicate.Depth = 0;
            this.CBox_RemoveDuplicate.Location = new System.Drawing.Point(17, 74);
            this.CBox_RemoveDuplicate.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_RemoveDuplicate.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_RemoveDuplicate.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_RemoveDuplicate.Name = "CBox_RemoveDuplicate";
            this.CBox_RemoveDuplicate.ReadOnly = false;
            this.CBox_RemoveDuplicate.Ripple = true;
            this.CBox_RemoveDuplicate.Size = new System.Drawing.Size(163, 37);
            this.CBox_RemoveDuplicate.TabIndex = 1;
            this.CBox_RemoveDuplicate.Text = "Remove Duplicate";
            this.CBox_RemoveDuplicate.UseVisualStyleBackColor = true;
            this.CBox_RemoveDuplicate.CheckedChanged += new System.EventHandler(this.CBox_RemoveDuplicate_CheckedChanged);
            // 
            // CBox_AutoLogin
            // 
            this.CBox_AutoLogin.AutoSize = true;
            this.CBox_AutoLogin.Depth = 0;
            this.CBox_AutoLogin.Location = new System.Drawing.Point(17, 102);
            this.CBox_AutoLogin.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_AutoLogin.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_AutoLogin.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_AutoLogin.Name = "CBox_AutoLogin";
            this.CBox_AutoLogin.ReadOnly = false;
            this.CBox_AutoLogin.Ripple = true;
            this.CBox_AutoLogin.Size = new System.Drawing.Size(112, 37);
            this.CBox_AutoLogin.TabIndex = 2;
            this.CBox_AutoLogin.Text = "Auto Login";
            this.CBox_AutoLogin.UseVisualStyleBackColor = true;
            this.CBox_AutoLogin.CheckedChanged += new System.EventHandler(this.CBox_AutoLogin_CheckedChanged);
            // 
            // CBox_ServerClickConnect
            // 
            this.CBox_ServerClickConnect.AutoSize = true;
            this.CBox_ServerClickConnect.Depth = 0;
            this.CBox_ServerClickConnect.Location = new System.Drawing.Point(17, 187);
            this.CBox_ServerClickConnect.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_ServerClickConnect.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_ServerClickConnect.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_ServerClickConnect.Name = "CBox_ServerClickConnect";
            this.CBox_ServerClickConnect.ReadOnly = false;
            this.CBox_ServerClickConnect.Ripple = true;
            this.CBox_ServerClickConnect.Size = new System.Drawing.Size(179, 37);
            this.CBox_ServerClickConnect.TabIndex = 5;
            this.CBox_ServerClickConnect.Text = "Server Click Connect";
            this.CBox_ServerClickConnect.UseVisualStyleBackColor = true;
            this.CBox_ServerClickConnect.CheckedChanged += new System.EventHandler(this.CBox_ServerClick_CheckedChanged);
            // 
            // CBox_TestMode
            // 
            this.CBox_TestMode.AutoSize = true;
            this.CBox_TestMode.Depth = 0;
            this.CBox_TestMode.Location = new System.Drawing.Point(17, 159);
            this.CBox_TestMode.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_TestMode.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_TestMode.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_TestMode.Name = "CBox_TestMode";
            this.CBox_TestMode.ReadOnly = false;
            this.CBox_TestMode.Ripple = true;
            this.CBox_TestMode.Size = new System.Drawing.Size(123, 37);
            this.CBox_TestMode.TabIndex = 4;
            this.CBox_TestMode.Text = "TEST MODE";
            this.CBox_TestMode.UseVisualStyleBackColor = true;
            this.CBox_TestMode.CheckedChanged += new System.EventHandler(this.CBox_TestMode_CheckedChanged);
            // 
            // CBox_DisablePopup
            // 
            this.CBox_DisablePopup.AutoSize = true;
            this.CBox_DisablePopup.Depth = 0;
            this.CBox_DisablePopup.Location = new System.Drawing.Point(17, 131);
            this.CBox_DisablePopup.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_DisablePopup.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_DisablePopup.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_DisablePopup.Name = "CBox_DisablePopup";
            this.CBox_DisablePopup.ReadOnly = false;
            this.CBox_DisablePopup.Ripple = true;
            this.CBox_DisablePopup.Size = new System.Drawing.Size(157, 37);
            this.CBox_DisablePopup.TabIndex = 3;
            this.CBox_DisablePopup.Text = "DISABLE POP-UP";
            this.CBox_DisablePopup.UseVisualStyleBackColor = true;
            this.CBox_DisablePopup.CheckedChanged += new System.EventHandler(this.CBox_DisablePopup_CheckedChanged);
            // 
            // CBox_FavOneClickConnect
            // 
            this.CBox_FavOneClickConnect.AutoSize = true;
            this.CBox_FavOneClickConnect.Depth = 0;
            this.CBox_FavOneClickConnect.Enabled = false;
            this.CBox_FavOneClickConnect.Location = new System.Drawing.Point(17, 216);
            this.CBox_FavOneClickConnect.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_FavOneClickConnect.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_FavOneClickConnect.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_FavOneClickConnect.Name = "CBox_FavOneClickConnect";
            this.CBox_FavOneClickConnect.ReadOnly = false;
            this.CBox_FavOneClickConnect.Ripple = true;
            this.CBox_FavOneClickConnect.Size = new System.Drawing.Size(193, 37);
            this.CBox_FavOneClickConnect.TabIndex = 6;
            this.CBox_FavOneClickConnect.Text = "Fav One-Click Connect";
            this.CBox_FavOneClickConnect.UseVisualStyleBackColor = true;
            // 
            // OptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 329);
            this.Controls.Add(this.CBox_AutoLogin);
            this.Controls.Add(this.CBox_ServerClickConnect);
            this.Controls.Add(this.CBox_TestMode);
            this.Controls.Add(this.CBox_DisablePopup);
            this.Controls.Add(this.CBox_FavOneClickConnect);
            this.Controls.Add(this.CBox_RemoveDuplicate);
            this.Name = "OptionForm";
            this.Text = "OptionForm";
            this.Load += new System.EventHandler(this.OptionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialCheckbox CBox_RemoveDuplicate;
        private MaterialSkin.Controls.MaterialCheckbox CBox_AutoLogin;
        private MaterialSkin.Controls.MaterialCheckbox CBox_ServerClickConnect;
        private MaterialSkin.Controls.MaterialCheckbox CBox_TestMode;
        private MaterialSkin.Controls.MaterialCheckbox CBox_DisablePopup;
        private MaterialSkin.Controls.MaterialCheckbox CBox_FavOneClickConnect;
    }
}