namespace GateHelper
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("ListViewGroup", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("ListViewGroup", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("111111");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("22222");
            this.BtnStart1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnReConfig1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnLogin1 = new MaterialSkin.Controls.MaterialButton();
            this.TestBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnSearch1 = new MaterialSkin.Controls.MaterialButton();
            this.GroupTemp1 = new System.Windows.Forms.GroupBox();
            this.materialListView1 = new MaterialSkin.Controls.MaterialListView();
            this.Group = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.서버이름 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.IP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SearchTxt1 = new MaterialSkin.Controls.MaterialTextBox2();
            this.GroupConnect1 = new System.Windows.Forms.GroupBox();
            this.BtnLoadServers1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnConnect1 = new MaterialSkin.Controls.MaterialButton();
            this.ComboBoxServerList1 = new MaterialSkin.Controls.MaterialComboBox();
            this.GatePWTxt1 = new MaterialSkin.Controls.MaterialTextBox2();
            this.GateIDTxt1 = new MaterialSkin.Controls.MaterialTextBox2();
            this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.TabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
            this.CBoxDisablePopup1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.BtnFav1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnFav2 = new MaterialSkin.Controls.MaterialButton();
            this.BtnFav3 = new MaterialSkin.Controls.MaterialButton();
            this.BtnStart2 = new MaterialSkin.Controls.MaterialButton();
            this.CBoxOneClickConnect1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.GroupFav1 = new System.Windows.Forms.GroupBox();
            this.BtnOpenConfig1 = new MaterialSkin.Controls.MaterialButton();
            this.GroupTemp1.SuspendLayout();
            this.GroupConnect1.SuspendLayout();
            this.materialTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.GroupFav1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnStart1
            // 
            this.BtnStart1.AutoSize = false;
            this.BtnStart1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnStart1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnStart1.Depth = 0;
            this.BtnStart1.Font = new System.Drawing.Font("돋움", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnStart1.HighEmphasis = true;
            this.BtnStart1.Icon = null;
            this.BtnStart1.Location = new System.Drawing.Point(19, 110);
            this.BtnStart1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnStart1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnStart1.Name = "BtnStart1";
            this.BtnStart1.NoAccentTextColor = System.Drawing.Color.DarkBlue;
            this.BtnStart1.Size = new System.Drawing.Size(300, 60);
            this.BtnStart1.TabIndex = 0;
            this.BtnStart1.Text = "Start Operation";
            this.BtnStart1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.BtnStart1.UseAccentColor = false;
            this.BtnStart1.UseVisualStyleBackColor = true;
            this.BtnStart1.Click += new System.EventHandler(this.BtnStart1_Click);
            // 
            // BtnReConfig1
            // 
            this.BtnReConfig1.AutoSize = false;
            this.BtnReConfig1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnReConfig1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnReConfig1.Depth = 0;
            this.BtnReConfig1.HighEmphasis = true;
            this.BtnReConfig1.Icon = null;
            this.BtnReConfig1.Location = new System.Drawing.Point(340, 110);
            this.BtnReConfig1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnReConfig1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnReConfig1.Name = "BtnReConfig1";
            this.BtnReConfig1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnReConfig1.Size = new System.Drawing.Size(96, 60);
            this.BtnReConfig1.TabIndex = 1;
            this.BtnReConfig1.Text = "Re Config";
            this.BtnReConfig1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnReConfig1.UseAccentColor = false;
            this.BtnReConfig1.UseVisualStyleBackColor = true;
            this.BtnReConfig1.Click += new System.EventHandler(this.BtnReConfig1_Click);
            // 
            // BtnLogin1
            // 
            this.BtnLogin1.AutoSize = false;
            this.BtnLogin1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnLogin1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnLogin1.Depth = 0;
            this.BtnLogin1.HighEmphasis = true;
            this.BtnLogin1.Icon = null;
            this.BtnLogin1.Location = new System.Drawing.Point(679, 118);
            this.BtnLogin1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnLogin1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnLogin1.Name = "BtnLogin1";
            this.BtnLogin1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnLogin1.Size = new System.Drawing.Size(167, 159);
            this.BtnLogin1.TabIndex = 4;
            this.BtnLogin1.Text = "GATEONE LOGIN";
            this.BtnLogin1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnLogin1.UseAccentColor = false;
            this.BtnLogin1.UseVisualStyleBackColor = true;
            this.BtnLogin1.Click += new System.EventHandler(this.LoginBtn1_Click);
            // 
            // TestBtn1
            // 
            this.TestBtn1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TestBtn1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.TestBtn1.Depth = 0;
            this.TestBtn1.HighEmphasis = true;
            this.TestBtn1.Icon = null;
            this.TestBtn1.Location = new System.Drawing.Point(1233, 105);
            this.TestBtn1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.TestBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.TestBtn1.Name = "TestBtn1";
            this.TestBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.TestBtn1.Size = new System.Drawing.Size(98, 36);
            this.TestBtn1.TabIndex = 5;
            this.TestBtn1.Text = "Func Test";
            this.TestBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.TestBtn1.UseAccentColor = false;
            this.TestBtn1.UseVisualStyleBackColor = true;
            this.TestBtn1.Click += new System.EventHandler(this.TestBtn1_Click);
            // 
            // BtnSearch1
            // 
            this.BtnSearch1.AutoSize = false;
            this.BtnSearch1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnSearch1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnSearch1.Depth = 0;
            this.BtnSearch1.HighEmphasis = true;
            this.BtnSearch1.Icon = null;
            this.BtnSearch1.Location = new System.Drawing.Point(361, 32);
            this.BtnSearch1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnSearch1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnSearch1.Name = "BtnSearch1";
            this.BtnSearch1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnSearch1.Size = new System.Drawing.Size(144, 60);
            this.BtnSearch1.TabIndex = 7;
            this.BtnSearch1.Text = "Search";
            this.BtnSearch1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnSearch1.UseAccentColor = false;
            this.BtnSearch1.UseVisualStyleBackColor = true;
            this.BtnSearch1.Click += new System.EventHandler(this.SearchBtn1_Click);
            // 
            // GroupTemp1
            // 
            this.GroupTemp1.Controls.Add(this.materialListView1);
            this.GroupTemp1.Location = new System.Drawing.Point(4, 4);
            this.GroupTemp1.Margin = new System.Windows.Forms.Padding(4);
            this.GroupTemp1.Name = "GroupTemp1";
            this.GroupTemp1.Padding = new System.Windows.Forms.Padding(4);
            this.GroupTemp1.Size = new System.Drawing.Size(521, 207);
            this.GroupTemp1.TabIndex = 6;
            this.GroupTemp1.TabStop = false;
            this.GroupTemp1.Text = "History";
            // 
            // materialListView1
            // 
            this.materialListView1.AutoSizeTable = false;
            this.materialListView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.materialListView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.materialListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Group,
            this.서버이름,
            this.IP});
            this.materialListView1.Depth = 0;
            this.materialListView1.FullRowSelect = true;
            listViewGroup1.Header = "ListViewGroup";
            listViewGroup1.Name = "Group1";
            listViewGroup2.Header = "ListViewGroup";
            listViewGroup2.Name = "Group2";
            this.materialListView1.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.materialListView1.HideSelection = false;
            this.materialListView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2});
            this.materialListView1.Location = new System.Drawing.Point(7, 28);
            this.materialListView1.MinimumSize = new System.Drawing.Size(200, 100);
            this.materialListView1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialListView1.MouseState = MaterialSkin.MouseState.OUT;
            this.materialListView1.Name = "materialListView1";
            this.materialListView1.OwnerDraw = true;
            this.materialListView1.Size = new System.Drawing.Size(496, 248);
            this.materialListView1.TabIndex = 10;
            this.materialListView1.UseCompatibleStateImageBehavior = false;
            this.materialListView1.View = System.Windows.Forms.View.Details;
            // 
            // Group
            // 
            this.Group.Width = 91;
            // 
            // 서버이름
            // 
            this.서버이름.Width = 131;
            // 
            // IP
            // 
            this.IP.Width = 126;
            // 
            // SearchTxt1
            // 
            this.SearchTxt1.AnimateReadOnly = false;
            this.SearchTxt1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.SearchTxt1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SearchTxt1.Depth = 0;
            this.SearchTxt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.SearchTxt1.HideSelection = true;
            this.SearchTxt1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SearchTxt1.LeadingIcon = null;
            this.SearchTxt1.Location = new System.Drawing.Point(7, 28);
            this.SearchTxt1.MaxLength = 32767;
            this.SearchTxt1.MouseState = MaterialSkin.MouseState.OUT;
            this.SearchTxt1.Name = "SearchTxt1";
            this.SearchTxt1.PasswordChar = '\0';
            this.SearchTxt1.PrefixSuffixText = null;
            this.SearchTxt1.ReadOnly = false;
            this.SearchTxt1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SearchTxt1.SelectedText = "";
            this.SearchTxt1.SelectionLength = 0;
            this.SearchTxt1.SelectionStart = 0;
            this.SearchTxt1.ShortcutsEnabled = true;
            this.SearchTxt1.Size = new System.Drawing.Size(350, 48);
            this.SearchTxt1.TabIndex = 13;
            this.SearchTxt1.TabStop = false;
            this.SearchTxt1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.SearchTxt1.TrailingIcon = null;
            this.SearchTxt1.UseSystemPasswordChar = false;
            // 
            // GroupConnect1
            // 
            this.GroupConnect1.Controls.Add(this.BtnLoadServers1);
            this.GroupConnect1.Controls.Add(this.BtnConnect1);
            this.GroupConnect1.Controls.Add(this.ComboBoxServerList1);
            this.GroupConnect1.Controls.Add(this.BtnSearch1);
            this.GroupConnect1.Controls.Add(this.SearchTxt1);
            this.GroupConnect1.Location = new System.Drawing.Point(7, 8);
            this.GroupConnect1.Margin = new System.Windows.Forms.Padding(4);
            this.GroupConnect1.Name = "GroupConnect1";
            this.GroupConnect1.Padding = new System.Windows.Forms.Padding(4);
            this.GroupConnect1.Size = new System.Drawing.Size(511, 273);
            this.GroupConnect1.TabIndex = 7;
            this.GroupConnect1.TabStop = false;
            this.GroupConnect1.Text = "Server Info";
            // 
            // BtnLoadServers1
            // 
            this.BtnLoadServers1.AutoSize = false;
            this.BtnLoadServers1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnLoadServers1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnLoadServers1.Depth = 0;
            this.BtnLoadServers1.HighEmphasis = true;
            this.BtnLoadServers1.Icon = null;
            this.BtnLoadServers1.Location = new System.Drawing.Point(10, 118);
            this.BtnLoadServers1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnLoadServers1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnLoadServers1.Name = "BtnLoadServers1";
            this.BtnLoadServers1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnLoadServers1.Size = new System.Drawing.Size(139, 60);
            this.BtnLoadServers1.TabIndex = 12;
            this.BtnLoadServers1.Text = "SERVER LIST";
            this.BtnLoadServers1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnLoadServers1.UseAccentColor = false;
            this.BtnLoadServers1.UseVisualStyleBackColor = true;
            this.BtnLoadServers1.Click += new System.EventHandler(this.BtnLoadServers1_Click);
            // 
            // BtnConnect1
            // 
            this.BtnConnect1.AutoSize = false;
            this.BtnConnect1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnConnect1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnConnect1.Depth = 0;
            this.BtnConnect1.HighEmphasis = true;
            this.BtnConnect1.Icon = null;
            this.BtnConnect1.Location = new System.Drawing.Point(10, 194);
            this.BtnConnect1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnConnect1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnConnect1.Name = "BtnConnect1";
            this.BtnConnect1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnConnect1.Size = new System.Drawing.Size(496, 62);
            this.BtnConnect1.TabIndex = 12;
            this.BtnConnect1.Text = "Connect";
            this.BtnConnect1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnConnect1.UseAccentColor = false;
            this.BtnConnect1.UseVisualStyleBackColor = true;
            this.BtnConnect1.Click += new System.EventHandler(this.BtnConnect1_Click);
            // 
            // ComboBoxServerList1
            // 
            this.ComboBoxServerList1.AutoResize = false;
            this.ComboBoxServerList1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ComboBoxServerList1.Depth = 0;
            this.ComboBoxServerList1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.ComboBoxServerList1.DropDownHeight = 174;
            this.ComboBoxServerList1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBoxServerList1.DropDownWidth = 121;
            this.ComboBoxServerList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.ComboBoxServerList1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ComboBoxServerList1.FormattingEnabled = true;
            this.ComboBoxServerList1.IntegralHeight = false;
            this.ComboBoxServerList1.ItemHeight = 43;
            this.ComboBoxServerList1.Location = new System.Drawing.Point(154, 114);
            this.ComboBoxServerList1.MaxDropDownItems = 4;
            this.ComboBoxServerList1.MouseState = MaterialSkin.MouseState.OUT;
            this.ComboBoxServerList1.Name = "ComboBoxServerList1";
            this.ComboBoxServerList1.Size = new System.Drawing.Size(350, 49);
            this.ComboBoxServerList1.StartIndex = 0;
            this.ComboBoxServerList1.TabIndex = 13;
            // 
            // GatePWTxt1
            // 
            this.GatePWTxt1.AnimateReadOnly = false;
            this.GatePWTxt1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.GatePWTxt1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.GatePWTxt1.Depth = 0;
            this.GatePWTxt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.GatePWTxt1.HideSelection = true;
            this.GatePWTxt1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.GatePWTxt1.LeadingIcon = null;
            this.GatePWTxt1.Location = new System.Drawing.Point(664, 498);
            this.GatePWTxt1.MaxLength = 32767;
            this.GatePWTxt1.MouseState = MaterialSkin.MouseState.OUT;
            this.GatePWTxt1.Name = "GatePWTxt1";
            this.GatePWTxt1.PasswordChar = '\0';
            this.GatePWTxt1.PrefixSuffixText = null;
            this.GatePWTxt1.ReadOnly = false;
            this.GatePWTxt1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.GatePWTxt1.SelectedText = "";
            this.GatePWTxt1.SelectionLength = 0;
            this.GatePWTxt1.SelectionStart = 0;
            this.GatePWTxt1.ShortcutsEnabled = true;
            this.GatePWTxt1.Size = new System.Drawing.Size(350, 48);
            this.GatePWTxt1.TabIndex = 13;
            this.GatePWTxt1.TabStop = false;
            this.GatePWTxt1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.GatePWTxt1.TrailingIcon = null;
            this.GatePWTxt1.UseSystemPasswordChar = false;
            // 
            // GateIDTxt1
            // 
            this.GateIDTxt1.AnimateReadOnly = false;
            this.GateIDTxt1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.GateIDTxt1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.GateIDTxt1.Depth = 0;
            this.GateIDTxt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.GateIDTxt1.HideSelection = true;
            this.GateIDTxt1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.GateIDTxt1.LeadingIcon = null;
            this.GateIDTxt1.Location = new System.Drawing.Point(664, 390);
            this.GateIDTxt1.MaxLength = 32767;
            this.GateIDTxt1.MouseState = MaterialSkin.MouseState.OUT;
            this.GateIDTxt1.Name = "GateIDTxt1";
            this.GateIDTxt1.PasswordChar = '\0';
            this.GateIDTxt1.PrefixSuffixText = null;
            this.GateIDTxt1.ReadOnly = false;
            this.GateIDTxt1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.GateIDTxt1.SelectedText = "";
            this.GateIDTxt1.SelectionLength = 0;
            this.GateIDTxt1.SelectionStart = 0;
            this.GateIDTxt1.ShortcutsEnabled = true;
            this.GateIDTxt1.Size = new System.Drawing.Size(350, 48);
            this.GateIDTxt1.TabIndex = 12;
            this.GateIDTxt1.TabStop = false;
            this.GateIDTxt1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.GateIDTxt1.TrailingIcon = null;
            this.GateIDTxt1.UseSystemPasswordChar = false;
            // 
            // materialTabControl1
            // 
            this.materialTabControl1.Controls.Add(this.tabPage1);
            this.materialTabControl1.Controls.Add(this.tabPage2);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.Location = new System.Drawing.Point(26, 264);
            this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabControl1.Multiline = true;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 0;
            this.materialTabControl1.Size = new System.Drawing.Size(534, 330);
            this.materialTabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.GroupConnect1);
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(526, 298);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "SERVER";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.GroupTemp1);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(526, 298);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "TEMP";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // TabSelector1
            // 
            this.TabSelector1.BaseTabControl = this.materialTabControl1;
            this.TabSelector1.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.TabSelector1.Depth = 0;
            this.TabSelector1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TabSelector1.ForeColor = System.Drawing.SystemColors.Control;
            this.TabSelector1.Location = new System.Drawing.Point(25, 214);
            this.TabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
            this.TabSelector1.Name = "TabSelector1";
            this.TabSelector1.Size = new System.Drawing.Size(521, 48);
            this.TabSelector1.TabIndex = 5;
            this.TabSelector1.TabIndicatorHeight = 3;
            this.TabSelector1.Text = "materialTabSelector1";
            // 
            // CBoxDisablePopup1
            // 
            this.CBoxDisablePopup1.AutoSize = true;
            this.CBoxDisablePopup1.Depth = 0;
            this.CBoxDisablePopup1.Enabled = false;
            this.CBoxDisablePopup1.Location = new System.Drawing.Point(26, 792);
            this.CBoxDisablePopup1.Margin = new System.Windows.Forms.Padding(0);
            this.CBoxDisablePopup1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBoxDisablePopup1.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBoxDisablePopup1.Name = "CBoxDisablePopup1";
            this.CBoxDisablePopup1.ReadOnly = false;
            this.CBoxDisablePopup1.Ripple = true;
            this.CBoxDisablePopup1.Size = new System.Drawing.Size(201, 37);
            this.CBoxDisablePopup1.TabIndex = 11;
            this.CBoxDisablePopup1.Text = "DISABLE POP-UP (Non)";
            this.CBoxDisablePopup1.UseVisualStyleBackColor = true;
            this.CBoxDisablePopup1.CheckedChanged += new System.EventHandler(this.DisablePopupCheckBox1_CheckedChanged);
            // 
            // BtnFav1
            // 
            this.BtnFav1.AutoSize = false;
            this.BtnFav1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnFav1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnFav1.Depth = 0;
            this.BtnFav1.HighEmphasis = true;
            this.BtnFav1.Icon = null;
            this.BtnFav1.Location = new System.Drawing.Point(10, 34);
            this.BtnFav1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnFav1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnFav1.Name = "BtnFav1";
            this.BtnFav1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnFav1.Size = new System.Drawing.Size(143, 60);
            this.BtnFav1.TabIndex = 14;
            this.BtnFav1.Text = "Favorite1";
            this.BtnFav1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnFav1.UseAccentColor = false;
            this.BtnFav1.UseVisualStyleBackColor = true;
            this.BtnFav1.Click += new System.EventHandler(this.BtnFav1_Click);
            // 
            // BtnFav2
            // 
            this.BtnFav2.AutoSize = false;
            this.BtnFav2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnFav2.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnFav2.Depth = 0;
            this.BtnFav2.HighEmphasis = true;
            this.BtnFav2.Icon = null;
            this.BtnFav2.Location = new System.Drawing.Point(164, 34);
            this.BtnFav2.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnFav2.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnFav2.Name = "BtnFav2";
            this.BtnFav2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnFav2.Size = new System.Drawing.Size(143, 60);
            this.BtnFav2.TabIndex = 15;
            this.BtnFav2.Text = "Favorite2";
            this.BtnFav2.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnFav2.UseAccentColor = false;
            this.BtnFav2.UseVisualStyleBackColor = true;
            this.BtnFav2.Click += new System.EventHandler(this.BtnFav2_Click);
            // 
            // BtnFav3
            // 
            this.BtnFav3.AutoSize = false;
            this.BtnFav3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnFav3.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnFav3.Depth = 0;
            this.BtnFav3.HighEmphasis = true;
            this.BtnFav3.Icon = null;
            this.BtnFav3.Location = new System.Drawing.Point(319, 34);
            this.BtnFav3.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnFav3.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnFav3.Name = "BtnFav3";
            this.BtnFav3.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnFav3.Size = new System.Drawing.Size(143, 60);
            this.BtnFav3.TabIndex = 16;
            this.BtnFav3.Text = "Favorite3";
            this.BtnFav3.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnFav3.UseAccentColor = false;
            this.BtnFav3.UseVisualStyleBackColor = true;
            this.BtnFav3.Click += new System.EventHandler(this.BtnTFav3_Click);
            // 
            // BtnStart2
            // 
            this.BtnStart2.AutoSize = false;
            this.BtnStart2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnStart2.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnStart2.Depth = 0;
            this.BtnStart2.Enabled = false;
            this.BtnStart2.HighEmphasis = true;
            this.BtnStart2.Icon = null;
            this.BtnStart2.Location = new System.Drawing.Point(679, 296);
            this.BtnStart2.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnStart2.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnStart2.Name = "BtnStart2";
            this.BtnStart2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnStart2.Size = new System.Drawing.Size(210, 60);
            this.BtnStart2.TabIndex = 17;
            this.BtnStart2.Text = "Start2";
            this.BtnStart2.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnStart2.UseAccentColor = false;
            this.BtnStart2.UseVisualStyleBackColor = true;
            this.BtnStart2.Click += new System.EventHandler(this.BtnStart2_Click);
            // 
            // CBoxOneClickConnect1
            // 
            this.CBoxOneClickConnect1.AutoSize = true;
            this.CBoxOneClickConnect1.Depth = 0;
            this.CBoxOneClickConnect1.Enabled = false;
            this.CBoxOneClickConnect1.Location = new System.Drawing.Point(26, 848);
            this.CBoxOneClickConnect1.Margin = new System.Windows.Forms.Padding(0);
            this.CBoxOneClickConnect1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBoxOneClickConnect1.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBoxOneClickConnect1.Name = "CBoxOneClickConnect1";
            this.CBoxOneClickConnect1.ReadOnly = false;
            this.CBoxOneClickConnect1.Ripple = true;
            this.CBoxOneClickConnect1.Size = new System.Drawing.Size(163, 37);
            this.CBoxOneClickConnect1.TabIndex = 18;
            this.CBoxOneClickConnect1.Text = "One-Click Connect";
            this.CBoxOneClickConnect1.UseVisualStyleBackColor = true;
            // 
            // GroupFav1
            // 
            this.GroupFav1.Controls.Add(this.BtnFav1);
            this.GroupFav1.Controls.Add(this.BtnFav2);
            this.GroupFav1.Controls.Add(this.BtnFav3);
            this.GroupFav1.Location = new System.Drawing.Point(31, 567);
            this.GroupFav1.Margin = new System.Windows.Forms.Padding(4);
            this.GroupFav1.Name = "GroupFav1";
            this.GroupFav1.Padding = new System.Windows.Forms.Padding(4);
            this.GroupFav1.Size = new System.Drawing.Size(523, 120);
            this.GroupFav1.TabIndex = 19;
            this.GroupFav1.TabStop = false;
            this.GroupFav1.Text = "Fav";
            // 
            // BtnOpenConfig1
            // 
            this.BtnOpenConfig1.AutoSize = false;
            this.BtnOpenConfig1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnOpenConfig1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnOpenConfig1.Depth = 0;
            this.BtnOpenConfig1.HighEmphasis = true;
            this.BtnOpenConfig1.Icon = null;
            this.BtnOpenConfig1.Location = new System.Drawing.Point(441, 110);
            this.BtnOpenConfig1.Margin = new System.Windows.Forms.Padding(6, 9, 6, 9);
            this.BtnOpenConfig1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnOpenConfig1.Name = "BtnOpenConfig1";
            this.BtnOpenConfig1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnOpenConfig1.Size = new System.Drawing.Size(96, 60);
            this.BtnOpenConfig1.TabIndex = 20;
            this.BtnOpenConfig1.Text = "Open Config";
            this.BtnOpenConfig1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnOpenConfig1.UseAccentColor = false;
            this.BtnOpenConfig1.UseVisualStyleBackColor = true;
            this.BtnOpenConfig1.Click += new System.EventHandler(this.BtnOpenConfig1_Click);
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1140, 1048);
            this.Controls.Add(this.BtnOpenConfig1);
            this.Controls.Add(this.GroupFav1);
            this.Controls.Add(this.CBoxOneClickConnect1);
            this.Controls.Add(this.BtnStart2);
            this.Controls.Add(this.CBoxDisablePopup1);
            this.Controls.Add(this.GatePWTxt1);
            this.Controls.Add(this.TabSelector1);
            this.Controls.Add(this.materialTabControl1);
            this.Controls.Add(this.GateIDTxt1);
            this.Controls.Add(this.BtnLogin1);
            this.Controls.Add(this.TestBtn1);
            this.Controls.Add(this.BtnReConfig1);
            this.Controls.Add(this.BtnStart1);
            this.Name = "MainUI";
            this.Sizable = false;
            this.Text = "GATE HELPER";
            this.GroupTemp1.ResumeLayout(false);
            this.GroupConnect1.ResumeLayout(false);
            this.materialTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.GroupFav1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialButton BtnStart1;
        private MaterialSkin.Controls.MaterialButton BtnReConfig1;
        private MaterialSkin.Controls.MaterialButton BtnLogin1;
        private MaterialSkin.Controls.MaterialButton TestBtn1;
        private MaterialSkin.Controls.MaterialButton BtnSearch1;
        private System.Windows.Forms.GroupBox GroupTemp1;
        private System.Windows.Forms.GroupBox GroupConnect1;
        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private MaterialSkin.Controls.MaterialTabSelector TabSelector1;
        private MaterialSkin.Controls.MaterialListView materialListView1;
        private System.Windows.Forms.ColumnHeader Group;
        private System.Windows.Forms.ColumnHeader 서버이름;
        private System.Windows.Forms.ColumnHeader IP;
        private MaterialSkin.Controls.MaterialCheckbox CBoxDisablePopup1;
        private MaterialSkin.Controls.MaterialTextBox2 GateIDTxt1;
        private MaterialSkin.Controls.MaterialTextBox2 GatePWTxt1;
        private MaterialSkin.Controls.MaterialButton BtnConnect1;
        private MaterialSkin.Controls.MaterialTextBox2 SearchTxt1;
        private MaterialSkin.Controls.MaterialComboBox ComboBoxServerList1;
        private MaterialSkin.Controls.MaterialButton BtnLoadServers1;
        private MaterialSkin.Controls.MaterialButton BtnFav1;
        private MaterialSkin.Controls.MaterialButton BtnFav2;
        private MaterialSkin.Controls.MaterialButton BtnFav3;
        private MaterialSkin.Controls.MaterialButton BtnStart2;
        private MaterialSkin.Controls.MaterialCheckbox CBoxOneClickConnect1;
        private System.Windows.Forms.GroupBox GroupFav1;
        private MaterialSkin.Controls.MaterialButton BtnOpenConfig1;
    }
}

