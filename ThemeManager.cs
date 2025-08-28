using MaterialSkin;
using System.Drawing;
using System.Windows.Forms;
using GateHelper;
using BrightIdeasSoftware;

public class ThemeManager
{
    private readonly MaterialSkinManager _materialSkinManager;
    private readonly ContextMenuStrip _contextMenuStrip;
    private bool _isDarkMode;

    public ThemeManager(MaterialSkinManager materialSkinManager, ContextMenuStrip contextMenuStrip)
    {
        _materialSkinManager = materialSkinManager;
        _contextMenuStrip = contextMenuStrip;
        _isDarkMode = true; // 기본값
    }

    public bool IsDarkMode => _isDarkMode; // 외부에서 테마 상태를 확인할 수 있는 속성

    public void SetTheme(bool newIsDarkMode, PictureBox settingPictureBox, ObjectListView OLV)
    {
        _isDarkMode = newIsDarkMode;

        if (_isDarkMode)
        {
            _materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            settingPictureBox.Image = GateHelper.Properties.Resources.sun;
        }
        else
        {
            _materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            settingPictureBox.Image = GateHelper.Properties.Resources.moon;
        }

        ApplyContextMenuStripTheme(); // 컨텍스트 메뉴 테마 변경
        ForceRedrawObjectListView(OLV); // OLV 테마 변경
    }

    private void ForceRedrawObjectListView(ObjectListView ovl)
    {
        if (ovl == null) return;
        ovl.BeginUpdate();

        // 컬럼 헤더/행 전체 무효화
        ovl.HeaderControl?.Invalidate();
        ovl.Invalidate(true); // 컨트롤 전체 무효화

        // 모든 행/서브아이템 강제 그리기
        if (ovl.Items.Count > 0)
            ovl.RedrawItems(0, ovl.Items.Count - 1, true);

        // 내부 리스트 재빌드(필요 시)
        ovl.BuildList(true);

        ovl.EndUpdate();
        ovl.Refresh();
    }

    // ⭐ private 메서드로 변경하여 내부에서만 호출
    private void ApplyContextMenuStripTheme()
    {
        if (_isDarkMode)
        {
            ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new MaterialToolStripColorTable());
            _contextMenuStrip.ForeColor = Color.White;
        }
        else
        {
            ToolStripManager.Renderer = null;
            _contextMenuStrip.ForeColor = Color.Black;
        }
    }

    public void ApplyContextMenuStripTheme(ContextMenuStrip contextMenuStrip)
    {
        if (_isDarkMode)
        {
            ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new MaterialToolStripColorTable());
            contextMenuStrip.ForeColor = Color.White;
        }
        else
        {
            ToolStripManager.Renderer = null;
            contextMenuStrip.ForeColor = Color.Black;
        }
    }

    public class MaterialToolStripColorTable : ProfessionalColorTable // 컨텍스트용
    {
        public override Color MenuItemSelected => ColorTranslator.FromHtml("#424242");
        public override Color MenuItemSelectedGradientBegin => ColorTranslator.FromHtml("#424242");
        public override Color MenuItemSelectedGradientEnd => ColorTranslator.FromHtml("#424242");
        public override Color MenuItemPressedGradientBegin => ColorTranslator.FromHtml("#212121");
        public override Color MenuItemPressedGradientEnd => ColorTranslator.FromHtml("#212121");
        public override Color MenuItemBorder => ColorTranslator.FromHtml("#424242");
        public override Color ToolStripDropDownBackground => ColorTranslator.FromHtml("#212121");
        public override Color ToolStripBorder => ColorTranslator.FromHtml("#424242");
    }
}