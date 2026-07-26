namespace GateHelper.Mgmt.ExcelAnalyzer
{
    partial class ExcelAnalyzerForm
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
            this.PnlDropFile = new System.Windows.Forms.Panel();
            this.materialLabel5 = new MaterialSkin.Controls.MaterialLabel();
            this.CmbDescCol = new MaterialSkin.Controls.MaterialComboBox();
            this.CmbSheet = new MaterialSkin.Controls.MaterialComboBox();
            this.CmbValueCol = new MaterialSkin.Controls.MaterialComboBox();
            this.CmbNameCol = new MaterialSkin.Controls.MaterialComboBox();
            this.CmbMachineCol = new MaterialSkin.Controls.MaterialComboBox();
            this.materialLabel4 = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
            this.BtnStartAnalyze = new MaterialSkin.Controls.MaterialButton();
            this.BtnStartRuleSetup = new MaterialSkin.Controls.MaterialButton();
            this.PnlRuleSetup = new System.Windows.Forms.Panel();
            this.materialLabel6 = new MaterialSkin.Controls.MaterialLabel();
            this.BtnGoToAnalyze = new MaterialSkin.Controls.MaterialButton();
            this.BtnBackToHome1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnSaveRule = new MaterialSkin.Controls.MaterialButton();
            this.DgvVariables = new System.Windows.Forms.DataGridView();
            this.ColVarName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColUnique = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColCommon = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.BtnDeleteScenario = new MaterialSkin.Controls.MaterialButton();
            this.BtnAddScenario = new MaterialSkin.Controls.MaterialButton();
            this.ClbMachines = new MaterialSkin.Controls.MaterialCheckedListBox();
            this.lblMachines = new MaterialSkin.Controls.MaterialLabel();
            this.LstScenarios = new MaterialSkin.Controls.MaterialListBox();
            this.PnlAnalysis = new System.Windows.Forms.Panel();
            this.chkShowErrorsOnly = new MaterialSkin.Controls.MaterialCheckbox();
            this.BtnBackToHome2 = new MaterialSkin.Controls.MaterialButton();
            this.BtnExport = new MaterialSkin.Controls.MaterialButton();
            this.BtnRunValidation = new MaterialSkin.Controls.MaterialButton();
            this.DgvResults = new System.Windows.Forms.DataGridView();
            this.ColStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColRule = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMachine = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColVariable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.PnlDropFile.SuspendLayout();
            this.PnlRuleSetup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DgvVariables)).BeginInit();
            this.PnlAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DgvResults)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PnlDropFile
            // 
            this.PnlDropFile.Controls.Add(this.panel1);
            this.PnlDropFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlDropFile.Location = new System.Drawing.Point(3, 64);
            this.PnlDropFile.Name = "PnlDropFile";
            this.PnlDropFile.Size = new System.Drawing.Size(1243, 549);
            this.PnlDropFile.TabIndex = 0;
            // 
            // materialLabel5
            // 
            this.materialLabel5.AutoSize = true;
            this.materialLabel5.Depth = 0;
            this.materialLabel5.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel5.Location = new System.Drawing.Point(455, 35);
            this.materialLabel5.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel5.Name = "materialLabel5";
            this.materialLabel5.Size = new System.Drawing.Size(25, 19);
            this.materialLabel5.TabIndex = 114;
            this.materialLabel5.Text = "설명";
            // 
            // CmbDescCol
            // 
            this.CmbDescCol.AutoResize = false;
            this.CmbDescCol.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbDescCol.Depth = 0;
            this.CmbDescCol.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbDescCol.DropDownHeight = 174;
            this.CmbDescCol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbDescCol.DropDownWidth = 121;
            this.CmbDescCol.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbDescCol.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbDescCol.FormattingEnabled = true;
            this.CmbDescCol.IntegralHeight = false;
            this.CmbDescCol.ItemHeight = 43;
            this.CmbDescCol.Location = new System.Drawing.Point(413, 69);
            this.CmbDescCol.MaxDropDownItems = 4;
            this.CmbDescCol.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbDescCol.Name = "CmbDescCol";
            this.CmbDescCol.Size = new System.Drawing.Size(153, 49);
            this.CmbDescCol.StartIndex = 0;
            this.CmbDescCol.TabIndex = 113;
            // 
            // CmbSheet
            // 
            this.CmbSheet.AutoResize = false;
            this.CmbSheet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbSheet.Depth = 0;
            this.CmbSheet.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbSheet.DropDownHeight = 174;
            this.CmbSheet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbSheet.DropDownWidth = 121;
            this.CmbSheet.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbSheet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbSheet.FormattingEnabled = true;
            this.CmbSheet.Hint = "시트 선택";
            this.CmbSheet.IntegralHeight = false;
            this.CmbSheet.ItemHeight = 43;
            this.CmbSheet.Location = new System.Drawing.Point(32, 137);
            this.CmbSheet.MaxDropDownItems = 4;
            this.CmbSheet.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbSheet.Name = "CmbSheet";
            this.CmbSheet.Size = new System.Drawing.Size(200, 49);
            this.CmbSheet.StartIndex = 0;
            this.CmbSheet.TabIndex = 8;
            this.CmbSheet.UseAccent = false;
            // 
            // CmbValueCol
            // 
            this.CmbValueCol.AutoResize = false;
            this.CmbValueCol.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbValueCol.Depth = 0;
            this.CmbValueCol.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbValueCol.DropDownHeight = 174;
            this.CmbValueCol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbValueCol.DropDownWidth = 121;
            this.CmbValueCol.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbValueCol.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbValueCol.FormattingEnabled = true;
            this.CmbValueCol.IntegralHeight = false;
            this.CmbValueCol.ItemHeight = 43;
            this.CmbValueCol.Location = new System.Drawing.Point(286, 68);
            this.CmbValueCol.MaxDropDownItems = 4;
            this.CmbValueCol.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbValueCol.Name = "CmbValueCol";
            this.CmbValueCol.Size = new System.Drawing.Size(121, 49);
            this.CmbValueCol.StartIndex = 0;
            this.CmbValueCol.TabIndex = 112;
            // 
            // CmbNameCol
            // 
            this.CmbNameCol.AutoResize = false;
            this.CmbNameCol.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbNameCol.Depth = 0;
            this.CmbNameCol.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbNameCol.DropDownHeight = 174;
            this.CmbNameCol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbNameCol.DropDownWidth = 121;
            this.CmbNameCol.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbNameCol.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbNameCol.FormattingEnabled = true;
            this.CmbNameCol.IntegralHeight = false;
            this.CmbNameCol.ItemHeight = 43;
            this.CmbNameCol.Location = new System.Drawing.Point(159, 68);
            this.CmbNameCol.MaxDropDownItems = 4;
            this.CmbNameCol.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbNameCol.Name = "CmbNameCol";
            this.CmbNameCol.Size = new System.Drawing.Size(121, 49);
            this.CmbNameCol.StartIndex = 0;
            this.CmbNameCol.TabIndex = 111;
            // 
            // CmbMachineCol
            // 
            this.CmbMachineCol.AutoResize = false;
            this.CmbMachineCol.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CmbMachineCol.Depth = 0;
            this.CmbMachineCol.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CmbMachineCol.DropDownHeight = 174;
            this.CmbMachineCol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbMachineCol.DropDownWidth = 121;
            this.CmbMachineCol.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CmbMachineCol.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.CmbMachineCol.FormattingEnabled = true;
            this.CmbMachineCol.IntegralHeight = false;
            this.CmbMachineCol.ItemHeight = 43;
            this.CmbMachineCol.Location = new System.Drawing.Point(32, 69);
            this.CmbMachineCol.MaxDropDownItems = 4;
            this.CmbMachineCol.MouseState = MaterialSkin.MouseState.OUT;
            this.CmbMachineCol.Name = "CmbMachineCol";
            this.CmbMachineCol.Size = new System.Drawing.Size(121, 49);
            this.CmbMachineCol.StartIndex = 0;
            this.CmbMachineCol.TabIndex = 110;
            // 
            // materialLabel4
            // 
            this.materialLabel4.AutoSize = true;
            this.materialLabel4.Depth = 0;
            this.materialLabel4.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel4.Location = new System.Drawing.Point(324, 35);
            this.materialLabel4.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel4.Name = "materialLabel4";
            this.materialLabel4.Size = new System.Drawing.Size(37, 19);
            this.materialLabel4.TabIndex = 109;
            this.materialLabel4.Text = "설정값";
            // 
            // materialLabel3
            // 
            this.materialLabel3.AutoSize = true;
            this.materialLabel3.Depth = 0;
            this.materialLabel3.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel3.Location = new System.Drawing.Point(197, 35);
            this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel3.Name = "materialLabel3";
            this.materialLabel3.Size = new System.Drawing.Size(37, 19);
            this.materialLabel3.TabIndex = 108;
            this.materialLabel3.Text = "변수명";
            // 
            // materialLabel2
            // 
            this.materialLabel2.AutoSize = true;
            this.materialLabel2.Depth = 0;
            this.materialLabel2.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel2.Location = new System.Drawing.Point(75, 35);
            this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel2.Name = "materialLabel2";
            this.materialLabel2.Size = new System.Drawing.Size(37, 19);
            this.materialLabel2.TabIndex = 107;
            this.materialLabel2.Text = "호기명";
            // 
            // BtnStartAnalyze
            // 
            this.BtnStartAnalyze.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnStartAnalyze.AutoSize = false;
            this.BtnStartAnalyze.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnStartAnalyze.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnStartAnalyze.Depth = 0;
            this.BtnStartAnalyze.HighEmphasis = true;
            this.BtnStartAnalyze.Icon = null;
            this.BtnStartAnalyze.Location = new System.Drawing.Point(428, 249);
            this.BtnStartAnalyze.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnStartAnalyze.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnStartAnalyze.Name = "BtnStartAnalyze";
            this.BtnStartAnalyze.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnStartAnalyze.Size = new System.Drawing.Size(120, 38);
            this.BtnStartAnalyze.TabIndex = 106;
            this.BtnStartAnalyze.Text = "Analyze";
            this.BtnStartAnalyze.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnStartAnalyze.UseAccentColor = false;
            this.BtnStartAnalyze.UseVisualStyleBackColor = true;
            this.BtnStartAnalyze.Visible = false;
            // 
            // BtnStartRuleSetup
            // 
            this.BtnStartRuleSetup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnStartRuleSetup.AutoSize = false;
            this.BtnStartRuleSetup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnStartRuleSetup.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnStartRuleSetup.Depth = 0;
            this.BtnStartRuleSetup.HighEmphasis = true;
            this.BtnStartRuleSetup.Icon = null;
            this.BtnStartRuleSetup.Location = new System.Drawing.Point(300, 249);
            this.BtnStartRuleSetup.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnStartRuleSetup.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnStartRuleSetup.Name = "BtnStartRuleSetup";
            this.BtnStartRuleSetup.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnStartRuleSetup.Size = new System.Drawing.Size(120, 38);
            this.BtnStartRuleSetup.TabIndex = 105;
            this.BtnStartRuleSetup.Text = "Rule Setup";
            this.BtnStartRuleSetup.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnStartRuleSetup.UseAccentColor = false;
            this.BtnStartRuleSetup.UseVisualStyleBackColor = true;
            this.BtnStartRuleSetup.Visible = false;
            // 
            // PnlRuleSetup
            // 
            this.PnlRuleSetup.Controls.Add(this.materialLabel6);
            this.PnlRuleSetup.Controls.Add(this.BtnGoToAnalyze);
            this.PnlRuleSetup.Controls.Add(this.BtnBackToHome1);
            this.PnlRuleSetup.Controls.Add(this.BtnSaveRule);
            this.PnlRuleSetup.Controls.Add(this.DgvVariables);
            this.PnlRuleSetup.Controls.Add(this.BtnDeleteScenario);
            this.PnlRuleSetup.Controls.Add(this.BtnAddScenario);
            this.PnlRuleSetup.Controls.Add(this.ClbMachines);
            this.PnlRuleSetup.Controls.Add(this.lblMachines);
            this.PnlRuleSetup.Controls.Add(this.LstScenarios);
            this.PnlRuleSetup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlRuleSetup.Location = new System.Drawing.Point(3, 64);
            this.PnlRuleSetup.Name = "PnlRuleSetup";
            this.PnlRuleSetup.Size = new System.Drawing.Size(1243, 549);
            this.PnlRuleSetup.TabIndex = 1;
            // 
            // materialLabel6
            // 
            this.materialLabel6.AutoSize = true;
            this.materialLabel6.Depth = 0;
            this.materialLabel6.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel6.Location = new System.Drawing.Point(7, 17);
            this.materialLabel6.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel6.Name = "materialLabel6";
            this.materialLabel6.Size = new System.Drawing.Size(53, 19);
            this.materialLabel6.TabIndex = 110;
            this.materialLabel6.Text = "규칙 목록";
            // 
            // BtnGoToAnalyze
            // 
            this.BtnGoToAnalyze.AutoSize = false;
            this.BtnGoToAnalyze.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnGoToAnalyze.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnGoToAnalyze.Depth = 0;
            this.BtnGoToAnalyze.HighEmphasis = true;
            this.BtnGoToAnalyze.Icon = null;
            this.BtnGoToAnalyze.Location = new System.Drawing.Point(1096, 290);
            this.BtnGoToAnalyze.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnGoToAnalyze.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnGoToAnalyze.Name = "BtnGoToAnalyze";
            this.BtnGoToAnalyze.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnGoToAnalyze.Size = new System.Drawing.Size(119, 41);
            this.BtnGoToAnalyze.TabIndex = 109;
            this.BtnGoToAnalyze.Text = "▶ Validation";
            this.BtnGoToAnalyze.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnGoToAnalyze.UseAccentColor = false;
            this.BtnGoToAnalyze.UseVisualStyleBackColor = true;
            // 
            // BtnBackToHome1
            // 
            this.BtnBackToHome1.AutoSize = false;
            this.BtnBackToHome1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnBackToHome1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnBackToHome1.Depth = 0;
            this.BtnBackToHome1.HighEmphasis = true;
            this.BtnBackToHome1.Icon = null;
            this.BtnBackToHome1.Location = new System.Drawing.Point(1096, 459);
            this.BtnBackToHome1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnBackToHome1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnBackToHome1.Name = "BtnBackToHome1";
            this.BtnBackToHome1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnBackToHome1.Size = new System.Drawing.Size(119, 41);
            this.BtnBackToHome1.TabIndex = 108;
            this.BtnBackToHome1.Text = "Back To Home";
            this.BtnBackToHome1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnBackToHome1.UseAccentColor = false;
            this.BtnBackToHome1.UseVisualStyleBackColor = true;
            // 
            // BtnSaveRule
            // 
            this.BtnSaveRule.AutoSize = false;
            this.BtnSaveRule.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnSaveRule.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnSaveRule.Depth = 0;
            this.BtnSaveRule.HighEmphasis = true;
            this.BtnSaveRule.Icon = null;
            this.BtnSaveRule.Location = new System.Drawing.Point(1096, 237);
            this.BtnSaveRule.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnSaveRule.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnSaveRule.Name = "BtnSaveRule";
            this.BtnSaveRule.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnSaveRule.Size = new System.Drawing.Size(119, 41);
            this.BtnSaveRule.TabIndex = 107;
            this.BtnSaveRule.Text = "Save Rule";
            this.BtnSaveRule.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnSaveRule.UseAccentColor = false;
            this.BtnSaveRule.UseVisualStyleBackColor = true;
            // 
            // DgvVariables
            // 
            this.DgvVariables.AllowUserToAddRows = false;
            this.DgvVariables.AllowUserToDeleteRows = false;
            this.DgvVariables.AllowUserToResizeRows = false;
            this.DgvVariables.BackgroundColor = System.Drawing.SystemColors.Control;
            this.DgvVariables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DgvVariables.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColVarName,
            this.ColUnique,
            this.ColCommon});
            this.DgvVariables.Location = new System.Drawing.Point(413, 11);
            this.DgvVariables.Name = "DgvVariables";
            this.DgvVariables.RowHeadersVisible = false;
            this.DgvVariables.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DgvVariables.Size = new System.Drawing.Size(676, 535);
            this.DgvVariables.TabIndex = 20;
            // 
            // ColVarName
            // 
            this.ColVarName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColVarName.HeaderText = "변수명";
            this.ColVarName.Name = "ColVarName";
            this.ColVarName.ReadOnly = true;
            // 
            // ColUnique
            // 
            this.ColUnique.HeaderText = "고유(Unique)";
            this.ColUnique.Name = "ColUnique";
            // 
            // ColCommon
            // 
            this.ColCommon.HeaderText = "공통(Common)";
            this.ColCommon.Name = "ColCommon";
            // 
            // BtnDeleteScenario
            // 
            this.BtnDeleteScenario.AutoSize = false;
            this.BtnDeleteScenario.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnDeleteScenario.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnDeleteScenario.Depth = 0;
            this.BtnDeleteScenario.HighEmphasis = true;
            this.BtnDeleteScenario.Icon = null;
            this.BtnDeleteScenario.Location = new System.Drawing.Point(1096, 88);
            this.BtnDeleteScenario.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnDeleteScenario.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnDeleteScenario.Name = "BtnDeleteScenario";
            this.BtnDeleteScenario.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnDeleteScenario.Size = new System.Drawing.Size(119, 41);
            this.BtnDeleteScenario.TabIndex = 106;
            this.BtnDeleteScenario.Text = "Delete\r\nScenario";
            this.BtnDeleteScenario.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnDeleteScenario.UseAccentColor = false;
            this.BtnDeleteScenario.UseVisualStyleBackColor = true;
            // 
            // BtnAddScenario
            // 
            this.BtnAddScenario.AutoSize = false;
            this.BtnAddScenario.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnAddScenario.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnAddScenario.Depth = 0;
            this.BtnAddScenario.HighEmphasis = true;
            this.BtnAddScenario.Icon = null;
            this.BtnAddScenario.Location = new System.Drawing.Point(1096, 35);
            this.BtnAddScenario.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnAddScenario.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnAddScenario.Name = "BtnAddScenario";
            this.BtnAddScenario.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnAddScenario.Size = new System.Drawing.Size(119, 41);
            this.BtnAddScenario.TabIndex = 105;
            this.BtnAddScenario.Text = "Add\r\nScenario";
            this.BtnAddScenario.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnAddScenario.UseAccentColor = false;
            this.BtnAddScenario.UseVisualStyleBackColor = true;
            // 
            // ClbMachines
            // 
            this.ClbMachines.AutoScroll = true;
            this.ClbMachines.BackColor = System.Drawing.SystemColors.Control;
            this.ClbMachines.Depth = 0;
            this.ClbMachines.Location = new System.Drawing.Point(189, 41);
            this.ClbMachines.MouseState = MaterialSkin.MouseState.HOVER;
            this.ClbMachines.Name = "ClbMachines";
            this.ClbMachines.Size = new System.Drawing.Size(218, 505);
            this.ClbMachines.Striped = false;
            this.ClbMachines.StripeDarkColor = System.Drawing.Color.Empty;
            this.ClbMachines.TabIndex = 1;
            // 
            // lblMachines
            // 
            this.lblMachines.AutoSize = true;
            this.lblMachines.Depth = 0;
            this.lblMachines.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblMachines.Location = new System.Drawing.Point(190, 18);
            this.lblMachines.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblMachines.Name = "lblMachines";
            this.lblMachines.Size = new System.Drawing.Size(88, 19);
            this.lblMachines.TabIndex = 10;
            this.lblMachines.Text = "설비(호기) 목록";
            // 
            // LstScenarios
            // 
            this.LstScenarios.BackColor = System.Drawing.Color.White;
            this.LstScenarios.BorderColor = System.Drawing.Color.LightGray;
            this.LstScenarios.Depth = 0;
            this.LstScenarios.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.LstScenarios.Location = new System.Drawing.Point(3, 41);
            this.LstScenarios.MouseState = MaterialSkin.MouseState.HOVER;
            this.LstScenarios.Name = "LstScenarios";
            this.LstScenarios.SelectedIndex = -1;
            this.LstScenarios.SelectedItem = null;
            this.LstScenarios.Size = new System.Drawing.Size(180, 505);
            this.LstScenarios.TabIndex = 0;
            // 
            // PnlAnalysis
            // 
            this.PnlAnalysis.Controls.Add(this.chkShowErrorsOnly);
            this.PnlAnalysis.Controls.Add(this.BtnBackToHome2);
            this.PnlAnalysis.Controls.Add(this.BtnExport);
            this.PnlAnalysis.Controls.Add(this.BtnRunValidation);
            this.PnlAnalysis.Controls.Add(this.DgvResults);
            this.PnlAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PnlAnalysis.Location = new System.Drawing.Point(3, 64);
            this.PnlAnalysis.Name = "PnlAnalysis";
            this.PnlAnalysis.Size = new System.Drawing.Size(1243, 549);
            this.PnlAnalysis.TabIndex = 1;
            // 
            // chkShowErrorsOnly
            // 
            this.chkShowErrorsOnly.AutoSize = true;
            this.chkShowErrorsOnly.Depth = 0;
            this.chkShowErrorsOnly.Location = new System.Drawing.Point(254, 9);
            this.chkShowErrorsOnly.Margin = new System.Windows.Forms.Padding(0);
            this.chkShowErrorsOnly.MouseLocation = new System.Drawing.Point(-1, -1);
            this.chkShowErrorsOnly.MouseState = MaterialSkin.MouseState.HOVER;
            this.chkShowErrorsOnly.Name = "chkShowErrorsOnly";
            this.chkShowErrorsOnly.ReadOnly = false;
            this.chkShowErrorsOnly.Ripple = true;
            this.chkShowErrorsOnly.Size = new System.Drawing.Size(99, 37);
            this.chkShowErrorsOnly.TabIndex = 111;
            this.chkShowErrorsOnly.Text = "오류만 보기";
            this.chkShowErrorsOnly.UseVisualStyleBackColor = true;
            // 
            // BtnBackToHome2
            // 
            this.BtnBackToHome2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnBackToHome2.AutoSize = false;
            this.BtnBackToHome2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnBackToHome2.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnBackToHome2.Depth = 0;
            this.BtnBackToHome2.HighEmphasis = true;
            this.BtnBackToHome2.Icon = null;
            this.BtnBackToHome2.Location = new System.Drawing.Point(1120, 6);
            this.BtnBackToHome2.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnBackToHome2.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnBackToHome2.Name = "BtnBackToHome2";
            this.BtnBackToHome2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnBackToHome2.Size = new System.Drawing.Size(119, 41);
            this.BtnBackToHome2.TabIndex = 110;
            this.BtnBackToHome2.Text = "Back To Home";
            this.BtnBackToHome2.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnBackToHome2.UseAccentColor = false;
            this.BtnBackToHome2.UseVisualStyleBackColor = true;
            // 
            // BtnExport
            // 
            this.BtnExport.AutoSize = false;
            this.BtnExport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnExport.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnExport.Depth = 0;
            this.BtnExport.HighEmphasis = true;
            this.BtnExport.Icon = null;
            this.BtnExport.Location = new System.Drawing.Point(131, 6);
            this.BtnExport.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnExport.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnExport.Name = "BtnExport";
            this.BtnExport.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnExport.Size = new System.Drawing.Size(119, 41);
            this.BtnExport.TabIndex = 109;
            this.BtnExport.Text = "Export";
            this.BtnExport.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnExport.UseAccentColor = false;
            this.BtnExport.UseVisualStyleBackColor = true;
            // 
            // BtnRunValidation
            // 
            this.BtnRunValidation.AutoSize = false;
            this.BtnRunValidation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnRunValidation.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnRunValidation.Depth = 0;
            this.BtnRunValidation.HighEmphasis = true;
            this.BtnRunValidation.Icon = null;
            this.BtnRunValidation.Location = new System.Drawing.Point(4, 6);
            this.BtnRunValidation.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnRunValidation.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnRunValidation.Name = "BtnRunValidation";
            this.BtnRunValidation.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnRunValidation.Size = new System.Drawing.Size(119, 41);
            this.BtnRunValidation.TabIndex = 108;
            this.BtnRunValidation.Text = "▶  Validation";
            this.BtnRunValidation.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnRunValidation.UseAccentColor = false;
            this.BtnRunValidation.UseVisualStyleBackColor = true;
            // 
            // DgvResults
            // 
            this.DgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DgvResults.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColStatus,
            this.ColRule,
            this.ColMachine,
            this.ColVariable,
            this.ColValue,
            this.ColDesc});
            this.DgvResults.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.DgvResults.Location = new System.Drawing.Point(0, 85);
            this.DgvResults.Name = "DgvResults";
            this.DgvResults.RowTemplate.Height = 23;
            this.DgvResults.Size = new System.Drawing.Size(1243, 464);
            this.DgvResults.TabIndex = 0;
            // 
            // ColStatus
            // 
            this.ColStatus.HeaderText = "결과 상태";
            this.ColStatus.Name = "ColStatus";
            // 
            // ColRule
            // 
            this.ColRule.HeaderText = "적용 규칙";
            this.ColRule.Name = "ColRule";
            // 
            // ColMachine
            // 
            this.ColMachine.HeaderText = "설비명";
            this.ColMachine.Name = "ColMachine";
            // 
            // ColVariable
            // 
            this.ColVariable.HeaderText = "변수명";
            this.ColVariable.Name = "ColVariable";
            // 
            // ColValue
            // 
            this.ColValue.HeaderText = "입력된 값";
            this.ColValue.Name = "ColValue";
            // 
            // ColDesc
            // 
            this.ColDesc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColDesc.HeaderText = "오류 상세 설명";
            this.ColDesc.Name = "ColDesc";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.materialLabel2);
            this.panel1.Controls.Add(this.BtnStartAnalyze);
            this.panel1.Controls.Add(this.materialLabel5);
            this.panel1.Controls.Add(this.BtnStartRuleSetup);
            this.panel1.Controls.Add(this.materialLabel3);
            this.panel1.Controls.Add(this.CmbDescCol);
            this.panel1.Controls.Add(this.materialLabel4);
            this.panel1.Controls.Add(this.CmbSheet);
            this.panel1.Controls.Add(this.CmbMachineCol);
            this.panel1.Controls.Add(this.CmbValueCol);
            this.panel1.Controls.Add(this.CmbNameCol);
            this.panel1.Location = new System.Drawing.Point(25, 18);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(594, 313);
            this.panel1.TabIndex = 115;
            // 
            // ExcelAnalyzerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1249, 616);
            this.Controls.Add(this.PnlDropFile);
            this.Controls.Add(this.PnlAnalysis);
            this.Controls.Add(this.PnlRuleSetup);
            this.Name = "ExcelAnalyzerForm";
            this.Text = "ExcelAnalyzerForm";
            this.PnlDropFile.ResumeLayout(false);
            this.PnlRuleSetup.ResumeLayout(false);
            this.PnlRuleSetup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DgvVariables)).EndInit();
            this.PnlAnalysis.ResumeLayout(false);
            this.PnlAnalysis.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DgvResults)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PnlDropFile;
        private System.Windows.Forms.Panel PnlRuleSetup;
        private System.Windows.Forms.Panel PnlAnalysis;
        private MaterialSkin.Controls.MaterialButton BtnStartAnalyze;
        private MaterialSkin.Controls.MaterialButton BtnStartRuleSetup;
        private MaterialSkin.Controls.MaterialListBox LstScenarios;
        private MaterialSkin.Controls.MaterialCheckedListBox ClbMachines;
        private MaterialSkin.Controls.MaterialButton BtnDeleteScenario;
        private MaterialSkin.Controls.MaterialButton BtnAddScenario;
        private MaterialSkin.Controls.MaterialButton BtnBackToHome1;
        private MaterialSkin.Controls.MaterialButton BtnSaveRule;
        private System.Windows.Forms.DataGridView DgvVariables;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColVarName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColUnique;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColCommon;
        private MaterialSkin.Controls.MaterialButton BtnBackToHome2;
        private MaterialSkin.Controls.MaterialButton BtnExport;
        private MaterialSkin.Controls.MaterialButton BtnRunValidation;
        private System.Windows.Forms.DataGridView DgvResults;
        private MaterialSkin.Controls.MaterialLabel materialLabel2;
        private MaterialSkin.Controls.MaterialComboBox CmbValueCol;
        private MaterialSkin.Controls.MaterialComboBox CmbNameCol;
        private MaterialSkin.Controls.MaterialComboBox CmbMachineCol;
        private MaterialSkin.Controls.MaterialLabel materialLabel4;
        private MaterialSkin.Controls.MaterialLabel materialLabel3;
        private MaterialSkin.Controls.MaterialComboBox CmbSheet;
        private MaterialSkin.Controls.MaterialLabel lblMachines;

        private MaterialSkin.Controls.MaterialLabel materialLabel5;
        private MaterialSkin.Controls.MaterialComboBox CmbDescCol;
        private MaterialSkin.Controls.MaterialButton BtnGoToAnalyze;
        private MaterialSkin.Controls.MaterialCheckbox chkShowErrorsOnly;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColRule;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColMachine;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColVariable;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColDesc;
        private MaterialSkin.Controls.MaterialLabel materialLabel6;
        private System.Windows.Forms.Panel panel1;
    }
}