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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainUI));
            System.Windows.Forms.ListViewGroup listViewGroup7 = new System.Windows.Forms.ListViewGroup("ListViewGroup", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup8 = new System.Windows.Forms.ListViewGroup("ListViewGroup", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("111111");
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("22222");
            this.BtnStart1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnReConfig1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnLogin1 = new MaterialSkin.Controls.MaterialButton();
            this.TestBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnSearch1 = new MaterialSkin.Controls.MaterialButton();
            this.GroupTemp1 = new System.Windows.Forms.GroupBox();
            this.picBox1 = new System.Windows.Forms.PictureBox();
            this.materialListView1 = new MaterialSkin.Controls.MaterialListView();
            this.Group = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.서버이름 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.IP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SearchTxt1 = new MaterialSkin.Controls.MaterialTextBox2();
            this.GroupConnect1 = new System.Windows.Forms.GroupBox();
            this.BtnLoadServers1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnConnect1 = new MaterialSkin.Controls.MaterialButton();
            this.ComboBoxServerList1 = new MaterialSkin.Controls.MaterialComboBox();
            this.GroupFav1 = new System.Windows.Forms.GroupBox();
            this.BtnFav1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnFav2 = new MaterialSkin.Controls.MaterialButton();
            this.BtnFav3 = new MaterialSkin.Controls.MaterialButton();
            this.GatePWTxt1 = new MaterialSkin.Controls.MaterialTextBox2();
            this.GateIDTxt1 = new MaterialSkin.Controls.MaterialTextBox2();
            this.TabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.TabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
            this.CBox_DisablePopup1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.BtnStart2 = new MaterialSkin.Controls.MaterialButton();
            this.CBox_FavOneClickConnect1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.BtnOpenConfig1 = new MaterialSkin.Controls.MaterialButton();
            this.groupShortCut1 = new System.Windows.Forms.GroupBox();
            this.BtnShortCut1 = new MaterialSkin.Controls.MaterialButton();
            this.toolTip_Question1 = new System.Windows.Forms.ToolTip(this.components);
            this.picBox2 = new System.Windows.Forms.PictureBox();
            this.toolTip_FavOneClickConnect1 = new System.Windows.Forms.ToolTip(this.components);
            this.GroupTemp1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBox1)).BeginInit();
            this.GroupConnect1.SuspendLayout();
            this.GroupFav1.SuspendLayout();
            this.TabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupShortCut1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBox2)).BeginInit();
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
            this.BtnStart1.Location = new System.Drawing.Point(13, 73);
            this.BtnStart1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.BtnStart1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnStart1.Name = "BtnStart1";
            this.BtnStart1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnStart1.Size = new System.Drawing.Size(210, 40);
            this.BtnStart1.TabIndex = 0;
            this.BtnStart1.Text = "Start Operation";
            this.BtnStart1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
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
            this.BtnReConfig1.Location = new System.Drawing.Point(238, 73);
            this.BtnReConfig1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnReConfig1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnReConfig1.Name = "BtnReConfig1";
            this.BtnReConfig1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnReConfig1.Size = new System.Drawing.Size(67, 40);
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
            this.BtnLogin1.Location = new System.Drawing.Point(723, 69);
            this.BtnLogin1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnLogin1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnLogin1.Name = "BtnLogin1";
            this.BtnLogin1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnLogin1.Size = new System.Drawing.Size(117, 106);
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
            this.TestBtn1.Location = new System.Drawing.Point(863, 70);
            this.TestBtn1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.TestBtn1.MouseState = MaterialSkin.MouseState.HOVER;
            this.TestBtn1.Name = "TestBtn1";
            this.TestBtn1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.TestBtn1.Size = new System.Drawing.Size(98, 36);
            this.TestBtn1.TabIndex = 5;
            this.TestBtn1.Text = "Func Test";
            this.TestBtn1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.TestBtn1.UseAccentColor = false;
            this.TestBtn1.UseVisualStyleBackColor = true;
            // 
            // BtnSearch1
            // 
            this.BtnSearch1.AutoSize = false;
            this.BtnSearch1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnSearch1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnSearch1.Depth = 0;
            this.BtnSearch1.HighEmphasis = true;
            this.BtnSearch1.Icon = null;
            this.BtnSearch1.Location = new System.Drawing.Point(267, 23);
            this.BtnSearch1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnSearch1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnSearch1.Name = "BtnSearch1";
            this.BtnSearch1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnSearch1.Size = new System.Drawing.Size(104, 40);
            this.BtnSearch1.TabIndex = 7;
            this.BtnSearch1.Text = "Search";
            this.BtnSearch1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnSearch1.UseAccentColor = false;
            this.BtnSearch1.UseVisualStyleBackColor = true;
            this.BtnSearch1.Click += new System.EventHandler(this.SearchBtn1_Click);
            // 
            // GroupTemp1
            // 
            this.GroupTemp1.Controls.Add(this.picBox1);
            this.GroupTemp1.Location = new System.Drawing.Point(3, 3);
            this.GroupTemp1.Name = "GroupTemp1";
            this.GroupTemp1.Size = new System.Drawing.Size(388, 303);
            this.GroupTemp1.TabIndex = 6;
            this.GroupTemp1.TabStop = false;
            this.GroupTemp1.Text = "History";
            // 
            // picBox1
            // 
            this.picBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picBox1.Image = ((System.Drawing.Image)(resources.GetObject("picBox1.Image")));
            this.picBox1.Location = new System.Drawing.Point(3, 17);
            this.picBox1.Name = "picBox1";
            this.picBox1.Size = new System.Drawing.Size(382, 283);
            this.picBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picBox1.TabIndex = 21;
            this.picBox1.TabStop = false;
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
            listViewGroup7.Header = "ListViewGroup";
            listViewGroup7.Name = "Group1";
            listViewGroup8.Header = "ListViewGroup";
            listViewGroup8.Name = "Group2";
            this.materialListView1.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup7,
            listViewGroup8});
            this.materialListView1.HideSelection = false;
            this.materialListView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem7,
            listViewItem8});
            this.materialListView1.Location = new System.Drawing.Point(472, 183);
            this.materialListView1.Margin = new System.Windows.Forms.Padding(2);
            this.materialListView1.MinimumSize = new System.Drawing.Size(140, 67);
            this.materialListView1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialListView1.MouseState = MaterialSkin.MouseState.OUT;
            this.materialListView1.Name = "materialListView1";
            this.materialListView1.OwnerDraw = true;
            this.materialListView1.Size = new System.Drawing.Size(347, 165);
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
            this.SearchTxt1.Location = new System.Drawing.Point(5, 19);
            this.SearchTxt1.Margin = new System.Windows.Forms.Padding(2);
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
            this.SearchTxt1.Size = new System.Drawing.Size(245, 48);
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
            this.GroupConnect1.Controls.Add(this.GroupFav1);
            this.GroupConnect1.Controls.Add(this.BtnSearch1);
            this.GroupConnect1.Controls.Add(this.SearchTxt1);
            this.GroupConnect1.Location = new System.Drawing.Point(3, 3);
            this.GroupConnect1.Name = "GroupConnect1";
            this.GroupConnect1.Size = new System.Drawing.Size(384, 303);
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
            this.BtnLoadServers1.Location = new System.Drawing.Point(13, 79);
            this.BtnLoadServers1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnLoadServers1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnLoadServers1.Name = "BtnLoadServers1";
            this.BtnLoadServers1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnLoadServers1.Size = new System.Drawing.Size(97, 40);
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
            this.BtnConnect1.Location = new System.Drawing.Point(13, 132);
            this.BtnConnect1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnConnect1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnConnect1.Name = "BtnConnect1";
            this.BtnConnect1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnConnect1.Size = new System.Drawing.Size(357, 41);
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
            this.ComboBoxServerList1.Location = new System.Drawing.Point(125, 77);
            this.ComboBoxServerList1.Margin = new System.Windows.Forms.Padding(2);
            this.ComboBoxServerList1.MaxDropDownItems = 4;
            this.ComboBoxServerList1.MouseState = MaterialSkin.MouseState.OUT;
            this.ComboBoxServerList1.Name = "ComboBoxServerList1";
            this.ComboBoxServerList1.Size = new System.Drawing.Size(246, 49);
            this.ComboBoxServerList1.StartIndex = 0;
            this.ComboBoxServerList1.TabIndex = 13;
            // 
            // GroupFav1
            // 
            this.GroupFav1.Controls.Add(this.BtnFav1);
            this.GroupFav1.Controls.Add(this.BtnFav2);
            this.GroupFav1.Controls.Add(this.BtnFav3);
            this.GroupFav1.Location = new System.Drawing.Point(13, 179);
            this.GroupFav1.Name = "GroupFav1";
            this.GroupFav1.Size = new System.Drawing.Size(357, 80);
            this.GroupFav1.TabIndex = 19;
            this.GroupFav1.TabStop = false;
            this.GroupFav1.Text = "Fav";
            // 
            // BtnFav1
            // 
            this.BtnFav1.AutoSize = false;
            this.BtnFav1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnFav1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnFav1.Depth = 0;
            this.BtnFav1.HighEmphasis = true;
            this.BtnFav1.Icon = null;
            this.BtnFav1.Location = new System.Drawing.Point(7, 23);
            this.BtnFav1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnFav1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnFav1.Name = "BtnFav1";
            this.BtnFav1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnFav1.Size = new System.Drawing.Size(100, 40);
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
            this.BtnFav2.Location = new System.Drawing.Point(115, 23);
            this.BtnFav2.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnFav2.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnFav2.Name = "BtnFav2";
            this.BtnFav2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnFav2.Size = new System.Drawing.Size(100, 40);
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
            this.BtnFav3.Location = new System.Drawing.Point(223, 23);
            this.BtnFav3.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnFav3.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnFav3.Name = "BtnFav3";
            this.BtnFav3.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnFav3.Size = new System.Drawing.Size(100, 40);
            this.BtnFav3.TabIndex = 16;
            this.BtnFav3.Text = "Favorite3";
            this.BtnFav3.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnFav3.UseAccentColor = false;
            this.BtnFav3.UseVisualStyleBackColor = true;
            this.BtnFav3.Click += new System.EventHandler(this.BtnTFav3_Click);
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
            this.GatePWTxt1.Location = new System.Drawing.Point(472, 114);
            this.GatePWTxt1.Margin = new System.Windows.Forms.Padding(2);
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
            this.GatePWTxt1.Size = new System.Drawing.Size(245, 48);
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
            this.GateIDTxt1.Location = new System.Drawing.Point(472, 73);
            this.GateIDTxt1.Margin = new System.Windows.Forms.Padding(2);
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
            this.GateIDTxt1.Size = new System.Drawing.Size(245, 48);
            this.GateIDTxt1.TabIndex = 12;
            this.GateIDTxt1.TabStop = false;
            this.GateIDTxt1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.GateIDTxt1.TrailingIcon = null;
            this.GateIDTxt1.UseSystemPasswordChar = false;
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.tabPage1);
            this.TabControl1.Controls.Add(this.tabPage2);
            this.TabControl1.Depth = 0;
            this.TabControl1.Location = new System.Drawing.Point(6, 176);
            this.TabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.TabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.TabControl1.Multiline = true;
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(397, 332);
            this.TabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.GroupConnect1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(389, 306);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "SERVER";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.GroupTemp1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage2.Size = new System.Drawing.Size(389, 306);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "TEMP";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // TabSelector1
            // 
            this.TabSelector1.BaseTabControl = this.TabControl1;
            this.TabSelector1.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.TabSelector1.Depth = 0;
            this.TabSelector1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TabSelector1.ForeColor = System.Drawing.SystemColors.Control;
            this.TabSelector1.Location = new System.Drawing.Point(17, 143);
            this.TabSelector1.Margin = new System.Windows.Forms.Padding(2);
            this.TabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
            this.TabSelector1.Name = "TabSelector1";
            this.TabSelector1.Size = new System.Drawing.Size(365, 32);
            this.TabSelector1.TabIndex = 5;
            this.TabSelector1.TabIndicatorHeight = 3;
            this.TabSelector1.Text = "TabSelector1";
            // 
            // CBox_DisablePopup1
            // 
            this.CBox_DisablePopup1.AutoSize = true;
            this.CBox_DisablePopup1.Depth = 0;
            this.CBox_DisablePopup1.Enabled = false;
            this.CBox_DisablePopup1.Location = new System.Drawing.Point(18, 602);
            this.CBox_DisablePopup1.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_DisablePopup1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_DisablePopup1.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_DisablePopup1.Name = "CBox_DisablePopup1";
            this.CBox_DisablePopup1.ReadOnly = false;
            this.CBox_DisablePopup1.Ripple = true;
            this.CBox_DisablePopup1.Size = new System.Drawing.Size(201, 37);
            this.CBox_DisablePopup1.TabIndex = 11;
            this.CBox_DisablePopup1.Text = "DISABLE POP-UP (Non)";
            this.CBox_DisablePopup1.UseVisualStyleBackColor = true;
            this.CBox_DisablePopup1.CheckedChanged += new System.EventHandler(this.DisablePopupCheckBox1_CheckedChanged);
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
            this.BtnStart2.Location = new System.Drawing.Point(863, 106);
            this.BtnStart2.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnStart2.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnStart2.Name = "BtnStart2";
            this.BtnStart2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnStart2.Size = new System.Drawing.Size(147, 40);
            this.BtnStart2.TabIndex = 17;
            this.BtnStart2.Text = "Start2";
            this.BtnStart2.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnStart2.UseAccentColor = false;
            this.BtnStart2.UseVisualStyleBackColor = true;
            this.BtnStart2.Click += new System.EventHandler(this.BtnStart2_Click);
            // 
            // CBox_FavOneClickConnect1
            // 
            this.CBox_FavOneClickConnect1.AutoSize = true;
            this.CBox_FavOneClickConnect1.Depth = 0;
            this.CBox_FavOneClickConnect1.Enabled = false;
            this.CBox_FavOneClickConnect1.Location = new System.Drawing.Point(18, 565);
            this.CBox_FavOneClickConnect1.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_FavOneClickConnect1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_FavOneClickConnect1.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_FavOneClickConnect1.Name = "CBox_FavOneClickConnect1";
            this.CBox_FavOneClickConnect1.ReadOnly = false;
            this.CBox_FavOneClickConnect1.Ripple = true;
            this.CBox_FavOneClickConnect1.Size = new System.Drawing.Size(193, 37);
            this.CBox_FavOneClickConnect1.TabIndex = 18;
            this.CBox_FavOneClickConnect1.Text = "Fav One-Click Connect";
            this.toolTip_FavOneClickConnect1.SetToolTip(this.CBox_FavOneClickConnect1, "설정 : 검색후 접속까지\r\n해제 : 검색만\r\n");
            this.CBox_FavOneClickConnect1.UseVisualStyleBackColor = true;
            // 
            // BtnOpenConfig1
            // 
            this.BtnOpenConfig1.AutoSize = false;
            this.BtnOpenConfig1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnOpenConfig1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnOpenConfig1.Depth = 0;
            this.BtnOpenConfig1.HighEmphasis = true;
            this.BtnOpenConfig1.Icon = null;
            this.BtnOpenConfig1.Location = new System.Drawing.Point(309, 73);
            this.BtnOpenConfig1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnOpenConfig1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnOpenConfig1.Name = "BtnOpenConfig1";
            this.BtnOpenConfig1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnOpenConfig1.Size = new System.Drawing.Size(67, 40);
            this.BtnOpenConfig1.TabIndex = 20;
            this.BtnOpenConfig1.Text = "Open Config";
            this.BtnOpenConfig1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnOpenConfig1.UseAccentColor = false;
            this.BtnOpenConfig1.UseVisualStyleBackColor = true;
            this.BtnOpenConfig1.Click += new System.EventHandler(this.BtnOpenConfig1_Click);
            // 
            // groupShortCut1
            // 
            this.groupShortCut1.Controls.Add(this.BtnShortCut1);
            this.groupShortCut1.Location = new System.Drawing.Point(472, 365);
            this.groupShortCut1.Name = "groupShortCut1";
            this.groupShortCut1.Size = new System.Drawing.Size(358, 275);
            this.groupShortCut1.TabIndex = 7;
            this.groupShortCut1.TabStop = false;
            this.groupShortCut1.Text = "ShortCut";
            // 
            // BtnShortCut1
            // 
            this.BtnShortCut1.AutoSize = false;
            this.BtnShortCut1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnShortCut1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnShortCut1.Depth = 0;
            this.BtnShortCut1.Enabled = false;
            this.BtnShortCut1.HighEmphasis = true;
            this.BtnShortCut1.Icon = null;
            this.BtnShortCut1.Location = new System.Drawing.Point(7, 23);
            this.BtnShortCut1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnShortCut1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnShortCut1.Name = "BtnShortCut1";
            this.BtnShortCut1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnShortCut1.Size = new System.Drawing.Size(70, 33);
            this.BtnShortCut1.TabIndex = 18;
            this.BtnShortCut1.Text = "ShortCut1";
            this.BtnShortCut1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnShortCut1.UseAccentColor = false;
            this.BtnShortCut1.UseVisualStyleBackColor = true;
            // 
            // picBox2
            // 
            this.picBox2.Image = global::GateHelper.Properties.Resources.ico_question;
            this.picBox2.Location = new System.Drawing.Point(366, 667);
            this.picBox2.Name = "picBox2";
            this.picBox2.Size = new System.Drawing.Size(28, 29);
            this.picBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picBox2.TabIndex = 21;
            this.picBox2.TabStop = false;
            this.toolTip_Question1.SetToolTip(this.picBox2, "C# Windows Forms based Selenium Automation Program\r\n(Description: To automate spe" +
        "cific web tasks)\r\nVersion: 1.1.0\r\nMade by: LeeJH");
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1109, 733);
            this.Controls.Add(this.picBox2);
            this.Controls.Add(this.groupShortCut1);
            this.Controls.Add(this.materialListView1);
            this.Controls.Add(this.CBox_DisablePopup1);
            this.Controls.Add(this.BtnOpenConfig1);
            this.Controls.Add(this.CBox_FavOneClickConnect1);
            this.Controls.Add(this.BtnStart2);
            this.Controls.Add(this.GatePWTxt1);
            this.Controls.Add(this.TabSelector1);
            this.Controls.Add(this.TabControl1);
            this.Controls.Add(this.GateIDTxt1);
            this.Controls.Add(this.BtnLogin1);
            this.Controls.Add(this.TestBtn1);
            this.Controls.Add(this.BtnReConfig1);
            this.Controls.Add(this.BtnStart1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainUI";
            this.Padding = new System.Windows.Forms.Padding(2, 43, 2, 2);
            this.Sizable = false;
            this.Text = "GATE HELPER";
            this.GroupTemp1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBox1)).EndInit();
            this.GroupConnect1.ResumeLayout(false);
            this.GroupFav1.ResumeLayout(false);
            this.TabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupShortCut1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBox2)).EndInit();
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
        private MaterialSkin.Controls.MaterialTabControl TabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private MaterialSkin.Controls.MaterialTabSelector TabSelector1;
        private MaterialSkin.Controls.MaterialListView materialListView1;
        private System.Windows.Forms.ColumnHeader Group;
        private System.Windows.Forms.ColumnHeader 서버이름;
        private System.Windows.Forms.ColumnHeader IP;
        private MaterialSkin.Controls.MaterialCheckbox CBox_DisablePopup1;
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
        private MaterialSkin.Controls.MaterialCheckbox CBox_FavOneClickConnect1;
        private System.Windows.Forms.GroupBox GroupFav1;
        private MaterialSkin.Controls.MaterialButton BtnOpenConfig1;
        private System.Windows.Forms.PictureBox picBox1;
        private System.Windows.Forms.GroupBox groupShortCut1;
        private MaterialSkin.Controls.MaterialButton BtnShortCut1;
        private System.Windows.Forms.ToolTip toolTip_Question1;
        private System.Windows.Forms.PictureBox picBox2;
        private System.Windows.Forms.ToolTip toolTip_FavOneClickConnect1;
    }
}

