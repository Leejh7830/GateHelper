namespace GateHelper.LogAnalyzer
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
            this.btnOpenValidator.Location = new System.Drawing.Point(374, 86);
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
            this.btnOpenEditor.Location = new System.Drawing.Point(374, 141);
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
            // LogValidatorMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 415);
            this.Controls.Add(this.btnOpenEditor);
            this.Controls.Add(this.btnOpenValidator);
            this.Name = "LogValidatorMain";
            this.Text = "Log Validator Main";
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton btnOpenValidator;
        private MaterialSkin.Controls.MaterialButton btnOpenEditor;
    }
}