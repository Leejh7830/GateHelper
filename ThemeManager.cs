using BrightIdeasSoftware;
using MaterialSkin;
using System.Drawing;
using System.Windows.Forms;

public class ThemeManager
{
    private readonly MaterialSkinManager _materialSkinManager;
    private readonly ContextMenuStrip _contextMenuStrip;
    private bool _isDarkMode;

    public ThemeManager(MaterialSkinManager materialSkinManager, ContextMenuStrip contextMenuStrip)
    {
        _materialSkinManager = materialSkinManager;
        _contextMenuStrip = contextMenuStrip;
        _isDarkMode = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK;

        if (_contextMenuStrip != null)
        {
            _contextMenuStrip.Opening -= ContextMenuStrip_Opening;
            _contextMenuStrip.Opening += ContextMenuStrip_Opening;

            // 중요: 전역 렌더러 영향 차단
            _contextMenuStrip.RenderMode = ToolStripRenderMode.Professional;
        }
    }

    public bool IsDarkMode => _isDarkMode;

    public void SetTheme(bool newIsDarkMode, PictureBox settingPictureBox, BrightIdeasSoftware.ObjectListView OLV)
    {
        _isDarkMode = newIsDarkMode;
        _materialSkinManager.Theme = newIsDarkMode
            ? MaterialSkinManager.Themes.DARK
            : MaterialSkinManager.Themes.LIGHT;

        ApplyContextMenuStripTheme(_contextMenuStrip);
        ForceRedrawObjectListView(OLV);

        if (settingPictureBox != null)
            settingPictureBox.Invalidate();
    }

    private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var cms = sender as ContextMenuStrip;
        if (cms == null) return;
        ApplyContextMenuStripTheme(cms);
    }

    private void ForceRedrawObjectListView(ObjectListView ovl)
    {
        if (ovl == null) return;
        ovl.BeginUpdate();
        ovl.HeaderControl?.Invalidate();
        ovl.Invalidate(true);
        if (ovl.Items.Count > 0)
            ovl.RedrawItems(0, ovl.Items.Count - 1, true);
        ovl.BuildList(true);
        ovl.EndUpdate();
        ovl.Refresh();
    }

    public void ApplyContextMenuStripTheme(ContextMenuStrip contextMenuStrip)
    {
        if (contextMenuStrip == null) return;

        // 중요: 전역(RendererManager) 무시하고 개별 렌더러 사용
        contextMenuStrip.RenderMode = ToolStripRenderMode.Professional;
        contextMenuStrip.Renderer = new DarkAwareToolStripRenderer(_isDarkMode);

        var bgColor = _isDarkMode ? Color.FromArgb(32, 32, 32) : Color.White;
        var textColor = _isDarkMode ? Color.White : Color.Black;

        contextMenuStrip.BackColor = bgColor;
        contextMenuStrip.ForeColor = textColor;

        ApplyColorsToItems(contextMenuStrip.Items, bgColor, textColor);
    }

    private static void ApplyColorsToItems(ToolStripItemCollection items, Color bgColor, Color textColor)
    {
        foreach (ToolStripItem item in items)
        {
            item.ForeColor = textColor;
            item.BackColor = bgColor;

            var menuItem = item as ToolStripMenuItem;
            if (menuItem?.DropDownItems != null && menuItem.DropDownItems.Count > 0)
            {
                // 드롭다운에도 동일 적용
                if (menuItem.DropDown is ToolStripDropDown dd)
                {
                    dd.RenderMode = ToolStripRenderMode.Professional;
                    dd.Renderer = new DarkAwareToolStripRenderer(textColor == Color.White);
                    dd.BackColor = bgColor;
                    dd.ForeColor = textColor;
                }
                ApplyColorsToItems(menuItem.DropDownItems, bgColor, textColor);
            }
        }
    }

    // 텍스트 색을 강제로 제어하는 렌더러
    private sealed class DarkAwareToolStripRenderer : ToolStripProfessionalRenderer
    {
        private readonly bool _dark;

        public DarkAwareToolStripRenderer(bool isDark)
            : base(new MaterialToolStripColorTable(isDark))
        {
            _dark = isDark;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            // 텍스트 색 강제
            e.TextColor = _dark ? Color.White : Color.Black;
            base.OnRenderItemText(e);
        }

        // 선택/호버 배경의 대비도 보장
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var rect = new Rectangle(Point.Empty, e.Item.Bounds.Size);
            bool selected = e.Item.Selected && e.Item.Enabled;

            Color back = _dark
                ? (selected ? Color.FromArgb(60, 60, 60) : Color.FromArgb(32, 32, 32))
                : (selected ? Color.FromArgb(240, 240, 240) : Color.White);

            using (var b = new SolidBrush(back))
                e.Graphics.FillRectangle(b, rect);

            // 테두리
            if (selected)
            {
                using (var p = new Pen(_dark ? Color.FromArgb(90, 90, 90) : Color.FromArgb(200, 200, 200)))
                    e.Graphics.DrawRectangle(p, new Rectangle(0, 0, rect.Width - 1, rect.Height - 1));
            }
        }
    }

    public class MaterialToolStripColorTable : ProfessionalColorTable
    {
        private readonly bool _dark;

        public MaterialToolStripColorTable(bool isDark)
        {
            _dark = isDark;
            UseSystemColors = false;
        }

        public override Color MenuItemSelected =>
            _dark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);

        public override Color MenuItemSelectedGradientBegin => MenuItemSelected;
        public override Color MenuItemSelectedGradientEnd => MenuItemSelected;

        public override Color MenuItemPressedGradientBegin =>
            _dark ? Color.FromArgb(70, 70, 70) : Color.FromArgb(230, 230, 230);

        public override Color MenuItemPressedGradientEnd => MenuItemPressedGradientBegin;

        public override Color MenuItemBorder =>
            _dark ? Color.FromArgb(90, 90, 90) : Color.FromArgb(200, 200, 200);

        public override Color ToolStripDropDownBackground =>
            _dark ? Color.FromArgb(32, 32, 32) : Color.White;

        public override Color ToolStripBorder =>
            _dark ? Color.FromArgb(64, 64, 64) : Color.FromArgb(200, 200, 200);
    }
}