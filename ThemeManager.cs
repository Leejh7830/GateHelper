using MaterialSkin;
using System.Drawing;
using System.Windows.Forms;
using GateHelper;

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

    // ⭐ SetTheme 메서드만 남겨두고 ToggleTheme은 삭제
    public void SetTheme(bool newIsDarkMode, PictureBox settingPictureBox)
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