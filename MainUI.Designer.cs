using System.Drawing;

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
            this.BtnGateOneLogin1 = new MaterialSkin.Controls.MaterialButton();
            this.TestBtn1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnSearch1 = new MaterialSkin.Controls.MaterialButton();
            this.ListViewServer1 = new MaterialSkin.Controls.MaterialListView();
            this.No = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SVName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LastConnected = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Memo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SearchTxt1 = new MaterialSkin.Controls.MaterialTextBox2();
            this.GroupConnect1 = new System.Windows.Forms.GroupBox();
            this.BtnLoadServers1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnConnect1 = new MaterialSkin.Controls.MaterialButton();
            this.ComboBoxServerList1 = new MaterialSkin.Controls.MaterialComboBox();
            this.GroupFav1 = new System.Windows.Forms.GroupBox();
            this.BtnFav1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnFav2 = new MaterialSkin.Controls.MaterialButton();
            this.BtnFav3 = new MaterialSkin.Controls.MaterialButton();
            this.TabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dummyControlOutsideTab = new MaterialSkin.Controls.MaterialButton();
            this.BtnOpenImages1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnReloadImages1 = new MaterialSkin.Controls.MaterialButton();
            this.GroupRef2 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.ListViewServer2 = new System.Windows.Forms.ListView();
            this.No1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SVName1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LastConnected1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Memo1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
            this.CBox_DisablePopup1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.BtnStart2 = new MaterialSkin.Controls.MaterialButton();
            this.CBox_FavOneClickConnect1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.BtnOpenConfig1 = new MaterialSkin.Controls.MaterialButton();
            this.groupShortCut1 = new System.Windows.Forms.GroupBox();
            this.BtnShortCut1 = new MaterialSkin.Controls.MaterialButton();
            this.toolTip_Question1 = new System.Windows.Forms.ToolTip(this.components);
            this.PicBox_Question = new System.Windows.Forms.PictureBox();
            this.toolTip_FavOneClickConnect1 = new System.Windows.Forms.ToolTip(this.components);
            this.BtnStart1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnReConfig1 = new MaterialSkin.Controls.MaterialButton();
            this.BtnOpenLog1 = new MaterialSkin.Controls.MaterialButton();
            this.PicBox_Arrow = new System.Windows.Forms.PictureBox();
            this.PicBox_Setting = new System.Windows.Forms.PictureBox();
            this.CBox_TestMode1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.dataListView1 = new BrightIdeasSoftware.DataListView();
            this.No2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.SVName2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.LastConnected2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Memo2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.lblDriverStatus = new System.Windows.Forms.Label();
            this.lblInternetStatus = new System.Windows.Forms.Label();
            this.CBox_ListViewClickConnect = new MaterialSkin.Controls.MaterialCheckbox();
            this.CBox_AutoLogin1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.GroupConnect1.SuspendLayout();
            this.GroupFav1.SuspendLayout();
            this.TabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.GroupRef2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupShortCut1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicBox_Question)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicBox_Arrow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicBox_Setting)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnGateOneLogin1
            // 
            this.BtnGateOneLogin1.AutoSize = false;
            this.BtnGateOneLogin1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnGateOneLogin1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnGateOneLogin1.Depth = 0;
            this.BtnGateOneLogin1.HighEmphasis = true;
            this.BtnGateOneLogin1.Icon = null;
            this.BtnGateOneLogin1.Location = new System.Drawing.Point(417, 127);
            this.BtnGateOneLogin1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnGateOneLogin1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnGateOneLogin1.Name = "BtnGateOneLogin1";
            this.BtnGateOneLogin1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnGateOneLogin1.Size = new System.Drawing.Size(103, 40);
            this.BtnGateOneLogin1.TabIndex = 4;
            this.BtnGateOneLogin1.Text = "GATEONE LOGIN";
            this.BtnGateOneLogin1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnGateOneLogin1.UseAccentColor = false;
            this.BtnGateOneLogin1.UseVisualStyleBackColor = true;
            this.BtnGateOneLogin1.Click += new System.EventHandler(this.BtnGateOneLogin1_Click);
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
            this.BtnSearch1.Click += new System.EventHandler(this.BtnSearch1_Click);
            // 
            // ListViewServer1
            // 
            this.ListViewServer1.AutoSizeTable = false;
            this.ListViewServer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ListViewServer1.BackgroundImageTiled = true;
            this.ListViewServer1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListViewServer1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.No,
            this.SVName,
            this.LastConnected,
            this.Memo});
            this.ListViewServer1.Depth = 0;
            this.ListViewServer1.FullRowSelect = true;
            this.ListViewServer1.HideSelection = false;
            this.ListViewServer1.LabelEdit = true;
            this.ListViewServer1.Location = new System.Drawing.Point(598, 369);
            this.ListViewServer1.Margin = new System.Windows.Forms.Padding(2);
            this.ListViewServer1.MinimumSize = new System.Drawing.Size(140, 67);
            this.ListViewServer1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.ListViewServer1.MouseState = MaterialSkin.MouseState.OUT;
            this.ListViewServer1.Name = "ListViewServer1";
            this.ListViewServer1.OwnerDraw = true;
            this.ListViewServer1.Size = new System.Drawing.Size(486, 169);
            this.ListViewServer1.TabIndex = 10;
            this.ListViewServer1.UseCompatibleStateImageBehavior = false;
            this.ListViewServer1.View = System.Windows.Forms.View.Details;
            // 
            // No
            // 
            this.No.Text = "No";
            this.No.Width = 50;
            // 
            // SVName
            // 
            this.SVName.Text = "Name";
            this.SVName.Width = 170;
            // 
            // LastConnected
            // 
            this.LastConnected.Text = "Last Connected";
            this.LastConnected.Width = 180;
            // 
            // Memo
            // 
            this.Memo.Text = "Memo";
            this.Memo.Width = 220;
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
            this.SearchTxt1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchTxt1_KeyDown);
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
            this.GroupConnect1.Size = new System.Drawing.Size(388, 303);
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
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.tabPage1);
            this.TabControl1.Controls.Add(this.tabPage3);
            this.TabControl1.Controls.Add(this.tabPage4);
            this.TabControl1.Depth = 0;
            this.TabControl1.Location = new System.Drawing.Point(6, 169);
            this.TabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.TabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.TabControl1.Multiline = true;
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(543, 377);
            this.TabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.GroupConnect1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(535, 351);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "SERVER";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dummyControlOutsideTab);
            this.tabPage3.Controls.Add(this.BtnOpenImages1);
            this.tabPage3.Controls.Add(this.BtnReloadImages1);
            this.tabPage3.Controls.Add(this.GroupRef2);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage3.Size = new System.Drawing.Size(535, 351);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "REFERENCE";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dummyControlOutsideTab
            // 
            this.dummyControlOutsideTab.AutoSize = false;
            this.dummyControlOutsideTab.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.dummyControlOutsideTab.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.dummyControlOutsideTab.Depth = 0;
            this.dummyControlOutsideTab.HighEmphasis = true;
            this.dummyControlOutsideTab.Icon = null;
            this.dummyControlOutsideTab.Location = new System.Drawing.Point(526, 8);
            this.dummyControlOutsideTab.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.dummyControlOutsideTab.MouseState = MaterialSkin.MouseState.HOVER;
            this.dummyControlOutsideTab.Name = "dummyControlOutsideTab";
            this.dummyControlOutsideTab.NoAccentTextColor = System.Drawing.Color.Empty;
            this.dummyControlOutsideTab.Size = new System.Drawing.Size(0, 0);
            this.dummyControlOutsideTab.TabIndex = 29;
            this.dummyControlOutsideTab.Text = "ShortCut1";
            this.dummyControlOutsideTab.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.dummyControlOutsideTab.UseAccentColor = true;
            this.dummyControlOutsideTab.UseVisualStyleBackColor = true;
            // 
            // BtnOpenImages1
            // 
            this.BtnOpenImages1.AutoSize = false;
            this.BtnOpenImages1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnOpenImages1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnOpenImages1.Depth = 0;
            this.BtnOpenImages1.HighEmphasis = true;
            this.BtnOpenImages1.Icon = null;
            this.BtnOpenImages1.Location = new System.Drawing.Point(83, 310);
            this.BtnOpenImages1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnOpenImages1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnOpenImages1.Name = "BtnOpenImages1";
            this.BtnOpenImages1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnOpenImages1.Size = new System.Drawing.Size(67, 40);
            this.BtnOpenImages1.TabIndex = 31;
            this.BtnOpenImages1.Text = "Open Images";
            this.BtnOpenImages1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnOpenImages1.UseAccentColor = false;
            this.BtnOpenImages1.UseVisualStyleBackColor = true;
            this.BtnOpenImages1.Click += new System.EventHandler(this.BtnOpenImages1_Click);
            // 
            // BtnReloadImages1
            // 
            this.BtnReloadImages1.AutoSize = false;
            this.BtnReloadImages1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnReloadImages1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnReloadImages1.Depth = 0;
            this.BtnReloadImages1.HighEmphasis = true;
            this.BtnReloadImages1.Icon = null;
            this.BtnReloadImages1.Location = new System.Drawing.Point(10, 310);
            this.BtnReloadImages1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnReloadImages1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnReloadImages1.Name = "BtnReloadImages1";
            this.BtnReloadImages1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnReloadImages1.Size = new System.Drawing.Size(67, 40);
            this.BtnReloadImages1.TabIndex = 32;
            this.BtnReloadImages1.Text = "Reload Images";
            this.BtnReloadImages1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnReloadImages1.UseAccentColor = false;
            this.BtnReloadImages1.UseVisualStyleBackColor = true;
            this.BtnReloadImages1.Click += new System.EventHandler(this.BtnReloadImages1_Click);
            // 
            // GroupRef2
            // 
            this.GroupRef2.Controls.Add(this.flowLayoutPanel1);
            this.GroupRef2.Location = new System.Drawing.Point(3, 3);
            this.GroupRef2.Name = "GroupRef2";
            this.GroupRef2.Size = new System.Drawing.Size(388, 303);
            this.GroupRef2.TabIndex = 7;
            this.GroupRef2.TabStop = false;
            this.GroupRef2.Text = "Reference";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 17);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(380, 281);
            this.flowLayoutPanel1.TabIndex = 29;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.ListViewServer2);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage4.Size = new System.Drawing.Size(535, 351);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "HISTORY";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // ListViewServer2
            // 
            this.ListViewServer2.BackColor = System.Drawing.SystemColors.Info;
            this.ListViewServer2.CheckBoxes = true;
            this.ListViewServer2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.No1,
            this.SVName1,
            this.LastConnected1,
            this.Memo1});
            this.ListViewServer2.Font = new System.Drawing.Font("휴먼옛체", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ListViewServer2.FullRowSelect = true;
            this.ListViewServer2.GridLines = true;
            this.ListViewServer2.HideSelection = false;
            this.ListViewServer2.HoverSelection = true;
            this.ListViewServer2.Location = new System.Drawing.Point(2, 2);
            this.ListViewServer2.Margin = new System.Windows.Forms.Padding(2);
            this.ListViewServer2.MinimumSize = new System.Drawing.Size(141, 68);
            this.ListViewServer2.Name = "ListViewServer2";
            this.ListViewServer2.Size = new System.Drawing.Size(535, 349);
            this.ListViewServer2.TabIndex = 28;
            this.ListViewServer2.UseCompatibleStateImageBehavior = false;
            this.ListViewServer2.View = System.Windows.Forms.View.Details;
            this.ListViewServer2.DoubleClick += new System.EventHandler(this.ListViewServer2_DoubleClick);
            // 
            // No1
            // 
            this.No1.Text = "No";
            this.No1.Width = 50;
            // 
            // SVName1
            // 
            this.SVName1.Text = "Name";
            this.SVName1.Width = 180;
            // 
            // LastConnected1
            // 
            this.LastConnected1.Text = "Last Connected";
            this.LastConnected1.Width = 180;
            // 
            // Memo1
            // 
            this.Memo1.Text = "Memo";
            this.Memo1.Width = 220;
            // 
            // TabSelector1
            // 
            this.TabSelector1.BaseTabControl = this.TabControl1;
            this.TabSelector1.CharacterCasing = MaterialSkin.Controls.MaterialTabSelector.CustomCharacterCasing.Normal;
            this.TabSelector1.Depth = 0;
            this.TabSelector1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TabSelector1.ForeColor = System.Drawing.SystemColors.Control;
            this.TabSelector1.Location = new System.Drawing.Point(19, 135);
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
            this.CBox_DisablePopup1.Location = new System.Drawing.Point(19, 567);
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
            this.BtnStart2.HighEmphasis = true;
            this.BtnStart2.Icon = null;
            this.BtnStart2.Location = new System.Drawing.Point(417, 79);
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
            this.CBox_FavOneClickConnect1.Location = new System.Drawing.Point(19, 539);
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
            this.BtnOpenConfig1.Location = new System.Drawing.Point(236, 73);
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
            this.groupShortCut1.Location = new System.Drawing.Point(574, 581);
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
            // PicBox_Question
            // 
            this.PicBox_Question.Image = global::GateHelper.Properties.Resources.question;
            this.PicBox_Question.Location = new System.Drawing.Point(351, 643);
            this.PicBox_Question.Name = "PicBox_Question";
            this.PicBox_Question.Size = new System.Drawing.Size(35, 33);
            this.PicBox_Question.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PicBox_Question.TabIndex = 21;
            this.PicBox_Question.TabStop = false;
            this.toolTip_Question1.SetToolTip(this.PicBox_Question, "C# Windows Forms based Selenium V 1.0 (2025.04.08)\r\nMade by LeeJH");
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
            this.BtnStart1.Location = new System.Drawing.Point(17, 73);
            this.BtnStart1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.BtnStart1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnStart1.Name = "BtnStart1";
            this.BtnStart1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnStart1.Size = new System.Drawing.Size(136, 40);
            this.BtnStart1.TabIndex = 22;
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
            this.BtnReConfig1.Location = new System.Drawing.Point(162, 73);
            this.BtnReConfig1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnReConfig1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnReConfig1.Name = "BtnReConfig1";
            this.BtnReConfig1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnReConfig1.Size = new System.Drawing.Size(67, 40);
            this.BtnReConfig1.TabIndex = 23;
            this.BtnReConfig1.Text = "Re Config";
            this.BtnReConfig1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnReConfig1.UseAccentColor = false;
            this.BtnReConfig1.UseVisualStyleBackColor = true;
            this.BtnReConfig1.Click += new System.EventHandler(this.BtnReConfig1_Click);
            // 
            // BtnOpenLog1
            // 
            this.BtnOpenLog1.AutoSize = false;
            this.BtnOpenLog1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BtnOpenLog1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.BtnOpenLog1.Depth = 0;
            this.BtnOpenLog1.HighEmphasis = true;
            this.BtnOpenLog1.Icon = null;
            this.BtnOpenLog1.Location = new System.Drawing.Point(309, 73);
            this.BtnOpenLog1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.BtnOpenLog1.MouseState = MaterialSkin.MouseState.HOVER;
            this.BtnOpenLog1.Name = "BtnOpenLog1";
            this.BtnOpenLog1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.BtnOpenLog1.Size = new System.Drawing.Size(67, 40);
            this.BtnOpenLog1.TabIndex = 24;
            this.BtnOpenLog1.Text = "Open Log";
            this.BtnOpenLog1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.BtnOpenLog1.UseAccentColor = false;
            this.BtnOpenLog1.UseVisualStyleBackColor = true;
            this.BtnOpenLog1.Click += new System.EventHandler(this.BtnOpenLog1_Click);
            // 
            // PicBox_Arrow
            // 
            this.PicBox_Arrow.Image = global::GateHelper.Properties.Resources.arrow_right;
            this.PicBox_Arrow.Location = new System.Drawing.Point(269, 643);
            this.PicBox_Arrow.Name = "PicBox_Arrow";
            this.PicBox_Arrow.Size = new System.Drawing.Size(35, 33);
            this.PicBox_Arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PicBox_Arrow.TabIndex = 26;
            this.PicBox_Arrow.TabStop = false;
            this.PicBox_Arrow.Click += new System.EventHandler(this.PicBox_Arrow_Click);
            // 
            // PicBox_Setting
            // 
            this.PicBox_Setting.Image = global::GateHelper.Properties.Resources.sun;
            this.PicBox_Setting.Location = new System.Drawing.Point(310, 643);
            this.PicBox_Setting.Name = "PicBox_Setting";
            this.PicBox_Setting.Size = new System.Drawing.Size(35, 33);
            this.PicBox_Setting.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PicBox_Setting.TabIndex = 25;
            this.PicBox_Setting.TabStop = false;
            this.PicBox_Setting.Click += new System.EventHandler(this.PicBox_Setting_Click);
            // 
            // CBox_TestMode1
            // 
            this.CBox_TestMode1.AutoSize = true;
            this.CBox_TestMode1.Depth = 0;
            this.CBox_TestMode1.Location = new System.Drawing.Point(19, 595);
            this.CBox_TestMode1.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_TestMode1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_TestMode1.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_TestMode1.Name = "CBox_TestMode1";
            this.CBox_TestMode1.ReadOnly = false;
            this.CBox_TestMode1.Ripple = true;
            this.CBox_TestMode1.Size = new System.Drawing.Size(123, 37);
            this.CBox_TestMode1.TabIndex = 27;
            this.CBox_TestMode1.Text = "TEST MODE";
            this.CBox_TestMode1.UseVisualStyleBackColor = true;
            this.CBox_TestMode1.CheckedChanged += new System.EventHandler(this.CBox_TestMode1_CheckedChanged);
            // 
            // dataListView1
            // 
            this.dataListView1.AllColumns.Add(this.No2);
            this.dataListView1.AllColumns.Add(this.SVName2);
            this.dataListView1.AllColumns.Add(this.LastConnected2);
            this.dataListView1.AllColumns.Add(this.Memo2);
            this.dataListView1.CellEditUseWholeCell = false;
            this.dataListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.No2,
            this.SVName2,
            this.LastConnected2,
            this.Memo2});
            this.dataListView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.dataListView1.DataSource = null;
            this.dataListView1.GridLines = true;
            this.dataListView1.HideSelection = false;
            this.dataListView1.Location = new System.Drawing.Point(598, 209);
            this.dataListView1.Margin = new System.Windows.Forms.Padding(2);
            this.dataListView1.Name = "dataListView1";
            this.dataListView1.Size = new System.Drawing.Size(487, 151);
            this.dataListView1.TabIndex = 28;
            this.dataListView1.UseCompatibleStateImageBehavior = false;
            this.dataListView1.View = System.Windows.Forms.View.Details;
            // 
            // No2
            // 
            this.No2.Text = "No";
            // 
            // SVName2
            // 
            this.SVName2.Text = "Name";
            this.SVName2.Width = 150;
            // 
            // LastConnected2
            // 
            this.LastConnected2.Text = "Last Connected";
            this.LastConnected2.Width = 150;
            // 
            // Memo2
            // 
            this.Memo2.Text = "Memo";
            this.Memo2.Width = 150;
            // 
            // lblDriverStatus
            // 
            this.lblDriverStatus.AutoSize = true;
            this.lblDriverStatus.Location = new System.Drawing.Point(211, 43);
            this.lblDriverStatus.Name = "lblDriverStatus";
            this.lblDriverStatus.Size = new System.Drawing.Size(37, 12);
            this.lblDriverStatus.TabIndex = 29;
            this.lblDriverStatus.Text = "Driver";
            // 
            // lblInternetStatus
            // 
            this.lblInternetStatus.AutoSize = true;
            this.lblInternetStatus.Location = new System.Drawing.Point(292, 43);
            this.lblInternetStatus.Name = "lblInternetStatus";
            this.lblInternetStatus.Size = new System.Drawing.Size(46, 12);
            this.lblInternetStatus.TabIndex = 30;
            this.lblInternetStatus.Text = "Internet";
            // 
            // CBox_ListViewClickConnect
            // 
            this.CBox_ListViewClickConnect.AutoSize = true;
            this.CBox_ListViewClickConnect.Depth = 0;
            this.CBox_ListViewClickConnect.Location = new System.Drawing.Point(19, 623);
            this.CBox_ListViewClickConnect.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_ListViewClickConnect.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_ListViewClickConnect.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_ListViewClickConnect.Name = "CBox_ListViewClickConnect";
            this.CBox_ListViewClickConnect.ReadOnly = false;
            this.CBox_ListViewClickConnect.Ripple = true;
            this.CBox_ListViewClickConnect.Size = new System.Drawing.Size(195, 37);
            this.CBox_ListViewClickConnect.TabIndex = 34;
            this.CBox_ListViewClickConnect.Text = "ListView Click Connect";
            this.CBox_ListViewClickConnect.UseVisualStyleBackColor = true;
            // 
            // CBox_AutoLogin1
            // 
            this.CBox_AutoLogin1.AutoSize = true;
            this.CBox_AutoLogin1.Depth = 0;
            this.CBox_AutoLogin1.Location = new System.Drawing.Point(19, 651);
            this.CBox_AutoLogin1.Margin = new System.Windows.Forms.Padding(0);
            this.CBox_AutoLogin1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.CBox_AutoLogin1.MouseState = MaterialSkin.MouseState.HOVER;
            this.CBox_AutoLogin1.Name = "CBox_AutoLogin1";
            this.CBox_AutoLogin1.ReadOnly = false;
            this.CBox_AutoLogin1.Ripple = true;
            this.CBox_AutoLogin1.Size = new System.Drawing.Size(112, 37);
            this.CBox_AutoLogin1.TabIndex = 35;
            this.CBox_AutoLogin1.Text = "Auto Login";
            this.CBox_AutoLogin1.UseVisualStyleBackColor = true;
            this.CBox_AutoLogin1.CheckedChanged += new System.EventHandler(this.CBox_AutoLogin1_CheckedChanged);
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1114, 714);
            this.Controls.Add(this.CBox_AutoLogin1);
            this.Controls.Add(this.CBox_ListViewClickConnect);
            this.Controls.Add(this.lblInternetStatus);
            this.Controls.Add(this.lblDriverStatus);
            this.Controls.Add(this.dataListView1);
            this.Controls.Add(this.ListViewServer1);
            this.Controls.Add(this.CBox_TestMode1);
            this.Controls.Add(this.PicBox_Arrow);
            this.Controls.Add(this.PicBox_Setting);
            this.Controls.Add(this.BtnOpenLog1);
            this.Controls.Add(this.BtnReConfig1);
            this.Controls.Add(this.BtnStart1);
            this.Controls.Add(this.PicBox_Question);
            this.Controls.Add(this.groupShortCut1);
            this.Controls.Add(this.CBox_DisablePopup1);
            this.Controls.Add(this.BtnOpenConfig1);
            this.Controls.Add(this.CBox_FavOneClickConnect1);
            this.Controls.Add(this.BtnStart2);
            this.Controls.Add(this.TabSelector1);
            this.Controls.Add(this.TabControl1);
            this.Controls.Add(this.BtnGateOneLogin1);
            this.Controls.Add(this.TestBtn1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainUI";
            this.Padding = new System.Windows.Forms.Padding(2, 43, 2, 2);
            this.Sizable = false;
            this.Text = "GATE HELPER";
            this.Load += new System.EventHandler(this.MainUI_Load);
            this.GroupConnect1.ResumeLayout(false);
            this.GroupFav1.ResumeLayout(false);
            this.TabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.GroupRef2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.groupShortCut1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PicBox_Question)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicBox_Arrow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicBox_Setting)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataListView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MaterialSkin.Controls.MaterialButton BtnGateOneLogin1;
        private MaterialSkin.Controls.MaterialButton TestBtn1;
        private MaterialSkin.Controls.MaterialButton BtnSearch1;
        private System.Windows.Forms.GroupBox GroupConnect1;
        private MaterialSkin.Controls.MaterialTabControl TabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private MaterialSkin.Controls.MaterialTabSelector TabSelector1;
        private MaterialSkin.Controls.MaterialListView ListViewServer1;
        private System.Windows.Forms.ColumnHeader No;
        private System.Windows.Forms.ColumnHeader SVName;
        private System.Windows.Forms.ColumnHeader LastConnected;
        private MaterialSkin.Controls.MaterialCheckbox CBox_DisablePopup1;
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
        private System.Windows.Forms.GroupBox groupShortCut1;
        private MaterialSkin.Controls.MaterialButton BtnShortCut1;
        private System.Windows.Forms.ToolTip toolTip_Question1;
        private System.Windows.Forms.PictureBox PicBox_Question;
        private System.Windows.Forms.ToolTip toolTip_FavOneClickConnect1;
        private MaterialSkin.Controls.MaterialButton BtnStart1;
        private MaterialSkin.Controls.MaterialButton BtnReConfig1;
        private MaterialSkin.Controls.MaterialButton BtnOpenLog1;
        private System.Windows.Forms.PictureBox PicBox_Setting;
        private System.Windows.Forms.PictureBox PicBox_Arrow;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ColumnHeader Memo;
        private MaterialSkin.Controls.MaterialCheckbox CBox_TestMode1;
        private System.Windows.Forms.ColumnHeader No1;
        private System.Windows.Forms.ColumnHeader SVName1;
        private System.Windows.Forms.ColumnHeader LastConnected1;
        private System.Windows.Forms.ColumnHeader Memo1;
        private System.Windows.Forms.ListView ListViewServer2;
        private BrightIdeasSoftware.DataListView dataListView1;
        private BrightIdeasSoftware.OLVColumn No2;
        private BrightIdeasSoftware.OLVColumn SVName2;
        private BrightIdeasSoftware.OLVColumn LastConnected2;
        private BrightIdeasSoftware.OLVColumn Memo2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.GroupBox GroupRef2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private MaterialSkin.Controls.MaterialButton BtnOpenImages1;
        private MaterialSkin.Controls.MaterialButton BtnReloadImages1;
        private MaterialSkin.Controls.MaterialButton dummyControlOutsideTab;
        private System.Windows.Forms.Label lblDriverStatus;
        private System.Windows.Forms.Label lblInternetStatus;
        private MaterialSkin.Controls.MaterialCheckbox CBox_ListViewClickConnect;
        private MaterialSkin.Controls.MaterialCheckbox CBox_AutoLogin1;
    }
}

