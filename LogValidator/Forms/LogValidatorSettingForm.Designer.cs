namespace GateHelper.LogValidator
{
    partial class LogValidatorSettingForm
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
            this.btnSave = new MaterialSkin.Controls.MaterialButton();
            this.btnCancel = new MaterialSkin.Controls.MaterialButton();
            this.txtEquipmentTypes = new MaterialSkin.Controls.MaterialMultiLineTextBox2();
            this.txtLineZones = new MaterialSkin.Controls.MaterialMultiLineTextBox2();
            this.txtFactoryPrefixes = new MaterialSkin.Controls.MaterialMultiLineTextBox2();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnSave.Depth = 0;
            this.btnSave.HighEmphasis = true;
            this.btnSave.Icon = null;
            this.btnSave.Location = new System.Drawing.Point(345, 315);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSave.Name = "btnSave";
            this.btnSave.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnSave.Size = new System.Drawing.Size(64, 36);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSave.UseAccentColor = false;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCancel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnCancel.Depth = 0;
            this.btnCancel.HighEmphasis = true;
            this.btnCancel.Icon = null;
            this.btnCancel.Location = new System.Drawing.Point(417, 315);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCancel.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnCancel.Size = new System.Drawing.Size(77, 36);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnCancel.UseAccentColor = false;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtEquipmentTypes
            // 
            this.txtEquipmentTypes.AnimateReadOnly = false;
            this.txtEquipmentTypes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.txtEquipmentTypes.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.txtEquipmentTypes.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtEquipmentTypes.Depth = 0;
            this.txtEquipmentTypes.HideSelection = true;
            this.txtEquipmentTypes.Location = new System.Drawing.Point(264, 79);
            this.txtEquipmentTypes.MaxLength = 32767;
            this.txtEquipmentTypes.MouseState = MaterialSkin.MouseState.OUT;
            this.txtEquipmentTypes.Name = "txtEquipmentTypes";
            this.txtEquipmentTypes.PasswordChar = '\0';
            this.txtEquipmentTypes.ReadOnly = false;
            this.txtEquipmentTypes.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtEquipmentTypes.SelectedText = "";
            this.txtEquipmentTypes.SelectionLength = 0;
            this.txtEquipmentTypes.SelectionStart = 0;
            this.txtEquipmentTypes.ShortcutsEnabled = true;
            this.txtEquipmentTypes.Size = new System.Drawing.Size(123, 125);
            this.txtEquipmentTypes.TabIndex = 8;
            this.txtEquipmentTypes.TabStop = false;
            this.txtEquipmentTypes.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtEquipmentTypes.UseSystemPasswordChar = false;
            // 
            // txtLineZones
            // 
            this.txtLineZones.AnimateReadOnly = false;
            this.txtLineZones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.txtLineZones.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.txtLineZones.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtLineZones.Depth = 0;
            this.txtLineZones.HideSelection = true;
            this.txtLineZones.Location = new System.Drawing.Point(135, 79);
            this.txtLineZones.MaxLength = 32767;
            this.txtLineZones.MouseState = MaterialSkin.MouseState.OUT;
            this.txtLineZones.Name = "txtLineZones";
            this.txtLineZones.PasswordChar = '\0';
            this.txtLineZones.ReadOnly = false;
            this.txtLineZones.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtLineZones.SelectedText = "";
            this.txtLineZones.SelectionLength = 0;
            this.txtLineZones.SelectionStart = 0;
            this.txtLineZones.ShortcutsEnabled = true;
            this.txtLineZones.Size = new System.Drawing.Size(123, 125);
            this.txtLineZones.TabIndex = 9;
            this.txtLineZones.TabStop = false;
            this.txtLineZones.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtLineZones.UseSystemPasswordChar = false;
            // 
            // txtFactoryPrefixes
            // 
            this.txtFactoryPrefixes.AnimateReadOnly = false;
            this.txtFactoryPrefixes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.txtFactoryPrefixes.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.txtFactoryPrefixes.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtFactoryPrefixes.Depth = 0;
            this.txtFactoryPrefixes.HideSelection = true;
            this.txtFactoryPrefixes.Location = new System.Drawing.Point(6, 79);
            this.txtFactoryPrefixes.MaxLength = 32767;
            this.txtFactoryPrefixes.MouseState = MaterialSkin.MouseState.OUT;
            this.txtFactoryPrefixes.Name = "txtFactoryPrefixes";
            this.txtFactoryPrefixes.PasswordChar = '\0';
            this.txtFactoryPrefixes.ReadOnly = false;
            this.txtFactoryPrefixes.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtFactoryPrefixes.SelectedText = "";
            this.txtFactoryPrefixes.SelectionLength = 0;
            this.txtFactoryPrefixes.SelectionStart = 0;
            this.txtFactoryPrefixes.ShortcutsEnabled = true;
            this.txtFactoryPrefixes.Size = new System.Drawing.Size(123, 125);
            this.txtFactoryPrefixes.TabIndex = 10;
            this.txtFactoryPrefixes.TabStop = false;
            this.txtFactoryPrefixes.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtFactoryPrefixes.UseSystemPasswordChar = false;
            // 
            // LogValidatorSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 373);
            this.Controls.Add(this.txtFactoryPrefixes);
            this.Controls.Add(this.txtLineZones);
            this.Controls.Add(this.txtEquipmentTypes);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Name = "LogValidatorSettingForm";
            this.Text = "LogValidatorSettingForm";
            this.Load += new System.EventHandler(this.LogValidatorSettingForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MaterialSkin.Controls.MaterialButton btnSave;
        private MaterialSkin.Controls.MaterialButton btnCancel;
        private MaterialSkin.Controls.MaterialMultiLineTextBox2 txtEquipmentTypes;
        private MaterialSkin.Controls.MaterialMultiLineTextBox2 txtLineZones;
        private MaterialSkin.Controls.MaterialMultiLineTextBox2 txtFactoryPrefixes;
    }
}