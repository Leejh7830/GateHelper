namespace GateHelper.LogValidator
{
    partial class LogValidatorMain
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
            this.btnOpenValidator = new MaterialSkin.Controls.MaterialButton();
            this.btnOpenEditor = new MaterialSkin.Controls.MaterialButton();
            this.btnSettings = new MaterialSkin.Controls.MaterialButton();
            this.btnClose = new MaterialSkin.Controls.MaterialButton();
            this.btnIndex = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOpenValidator
            // 
            this.btnOpenValidator.AutoSize = false;
            this.btnOpenValidator.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOpenValidator.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnOpenValidator.Depth = 0;
            this.btnOpenValidator.HighEmphasis = true;
            this.btnOpenValidator.Icon = null;
            this.btnOpenValidator.Location = new System.Drawing.Point(32, 83);
            this.btnOpenValidator.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOpenValidator.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnOpenValidator.Name = "btnOpenValidator";
            this.btnOpenValidator.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnOpenValidator.Size = new System.Drawing.Size(119, 43);
            this.btnOpenValidator.TabIndex = 107;
            this.btnOpenValidator.Text = "Validator";
            this.btnOpenValidator.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnOpenValidator.UseAccentColor = false;
            this.btnOpenValidator.UseVisualStyleBackColor = true;
            this.btnOpenValidator.Click += new System.EventHandler(this.btnOpenValidator_Click);
            // 
            // btnOpenEditor
            // 
            this.btnOpenEditor.AutoSize = false;
            this.btnOpenEditor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOpenEditor.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnOpenEditor.Depth = 0;
            this.btnOpenEditor.HighEmphasis = true;
            this.btnOpenEditor.Icon = null;
            this.btnOpenEditor.Location = new System.Drawing.Point(32, 138);
            this.btnOpenEditor.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOpenEditor.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnOpenEditor.Name = "btnOpenEditor";
            this.btnOpenEditor.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnOpenEditor.Size = new System.Drawing.Size(119, 43);
            this.btnOpenEditor.TabIndex = 108;
            this.btnOpenEditor.Text = "Scenario";
            this.btnOpenEditor.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnOpenEditor.UseAccentColor = false;
            this.btnOpenEditor.UseVisualStyleBackColor = true;
            this.btnOpenEditor.Click += new System.EventHandler(this.btnOpenEditor_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.AutoSize = false;
            this.btnSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSettings.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSettings.Depth = 0;
            this.btnSettings.HighEmphasis = true;
            this.btnSettings.Icon = null;
            this.btnSettings.Location = new System.Drawing.Point(32, 285);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSettings.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSettings.Size = new System.Drawing.Size(119, 43);
            this.btnSettings.TabIndex = 100;
            this.btnSettings.Text = "Settings";
            this.btnSettings.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSettings.UseAccentColor = false;
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnClose
            // 
            this.btnClose.AutoSize = false;
            this.btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnClose.Depth = 0;
            this.btnClose.HighEmphasis = false;
            this.btnClose.Icon = null;
            this.btnClose.Location = new System.Drawing.Point(32, 340);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnClose.Name = "btnClose";
            this.btnClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnClose.Size = new System.Drawing.Size(120, 36);
            this.btnClose.TabIndex = 101;
            this.btnClose.Text = "✕   Close";
            this.btnClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnClose.UseAccentColor = true;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnIndex
            // 
            this.btnIndex.Location = new System.Drawing.Point(6, 67);
            this.btnIndex.Name = "btnIndex";
            this.btnIndex.Size = new System.Drawing.Size(1, 1);
            this.btnIndex.TabIndex = 1;
            this.btnIndex.UseVisualStyleBackColor = true;
            // 
            // LogValidatorMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 415);
            this.Controls.Add(this.btnIndex);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnOpenEditor);
            this.Controls.Add(this.btnOpenValidator);
            this.Name = "LogValidatorMain";
            this.Sizable = false;
            this.Text = "Log Validator Main";
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton btnOpenValidator;
        private MaterialSkin.Controls.MaterialButton btnOpenEditor;
        private MaterialSkin.Controls.MaterialButton btnSettings;
        private MaterialSkin.Controls.MaterialButton btnClose;
        private System.Windows.Forms.Button btnIndex;
    }
}