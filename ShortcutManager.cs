using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    public class ShortcutItem
    {
        public string DisplayName { get; set; }
        public string AbsolutePath { get; set; }
    }

    public class ShortcutManager
    {
        private readonly string _shortcutPath;
        private List<ShortcutItem> _shortcutItems = new List<ShortcutItem>();
        private FlowLayoutPanel _targetPanel;

        private System.Windows.Forms.Timer _integrityTimer;
        private Dictionary<ShortcutItem, Button> _buttonMapping = new Dictionary<ShortcutItem, Button>();
        private Color _originalPanelColor;

        private readonly MaterialSkin.MaterialSkinManager _skinManager = MaterialSkin.MaterialSkinManager.Instance;
        private bool IsDarkMode => _skinManager.Theme == MaterialSkin.MaterialSkinManager.Themes.DARK;

        public ShortcutManager()
        {
            _shortcutPath = Util.GetMetaPath("Shortcuts.json");

            _integrityTimer = new System.Windows.Forms.Timer();
            _integrityTimer.Interval = 5000;
            _integrityTimer.Tick += (s, ev) => CheckFileIntegrity();
        }

        public void BindPanel(FlowLayoutPanel panel)
        {
            _targetPanel = panel;
            _targetPanel.AllowDrop = true;
            _targetPanel.WrapContents = true;
            _targetPanel.AutoScroll = true;

            _originalPanelColor = _targetPanel.BackColor;

            _targetPanel.DragEnter += Panel_DragEnter;
            _targetPanel.DragLeave += Panel_DragLeave;
            _targetPanel.DragDrop += Panel_DragDrop;

            if (_targetPanel.IsHandleCreated)
            {
                InitializeShortcuts();
            }
            else
            {
                _targetPanel.HandleCreated += (s, ev) => InitializeShortcuts();
            }
        }

        private void InitializeShortcuts()
        {
            LoadData();

            _targetPanel.Controls.Clear();
            _buttonMapping.Clear();

            foreach (var item in _shortcutItems)
            {
                RenderButton(item);
            }

            _integrityTimer.Start();
            CheckFileIntegrity();
        }

        private void LoadData()
        {
            try
            {
                if (!File.Exists(_shortcutPath)) return;
                string json = File.ReadAllText(_shortcutPath);
                _shortcutItems = JsonConvert.DeserializeObject<List<ShortcutItem>>(json) ?? new List<ShortcutItem>();
            }
            catch (Exception ex)
            {
                LogMessage($"[ShortcutManager] 로드 실패: {ex.Message}", Level.Error);
            }
        }

        private void SaveData()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_shortcutItems, Formatting.Indented);
                File.WriteAllText(_shortcutPath, json);
            }
            catch (Exception ex)
            {
                LogMessage($"[ShortcutManager] 저장 실패: {ex.Message}", Level.Error);
            }
        }

        private void CheckFileIntegrity()
        {
            if (_targetPanel == null || _targetPanel.IsDisposed) return;

            foreach (var kvp in _buttonMapping)
            {
                ShortcutItem item = kvp.Key;
                Button btn = kvp.Value;

                bool exists = File.Exists(item.AbsolutePath);

                if (btn.InvokeRequired)
                {
                    btn.Invoke(new Action(() => UpdateButtonIntegrityUI(btn, exists)));
                }
                else
                {
                    UpdateButtonIntegrityUI(btn, exists);
                }
            }
        }

        private (Color BackColor, Color ForeColor) GetButtonColors(bool exists)
        {
            if (IsDarkMode)
            {
                return exists
                    ? (Color.FromArgb(50, 50, 50), Color.White)
                    : (Color.FromArgb(35, 35, 35), Color.FromArgb(180, 180, 180));
            }
            else
            {
                return exists
                    ? (Color.FromArgb(230, 230, 230), Color.Black)
                    : (Color.FromArgb(242, 242, 242), Color.FromArgb(140, 140, 140));
            }
        }

        // 💡 [수정 핵심 인터락] 무결성 업데이트 시 테마에 의해 오버라이드된 폰트 크기까지 강제로 재연산 복원
        private void UpdateButtonIntegrityUI(Button btn, bool exists)
        {
            // 1. 색상 복원
            var colors = GetButtonColors(exists);
            btn.BackColor = colors.BackColor;
            btn.ForeColor = colors.ForeColor;

            // 2. 테마 변경으로 인해 초기화된 폰트 크기 강제 재교정
            if (!string.IsNullOrEmpty(btn.Text) && btn.Text.Length > 5)
            {
                btn.Font = new Font(btn.Font.FontFamily, 7.5f, FontStyle.Regular);
            }
            else
            {
                btn.Font = new Font(btn.Font.FontFamily, 9.0f, FontStyle.Regular);
            }
        }

        private string GetResolvedPath(string path)
        {
            if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    object shell = Activator.CreateInstance(shellType);
                    object shortcut = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { path });
                    string targetPath = (string)shortcut.GetType().InvokeMember("TargetPath", System.Reflection.BindingFlags.GetProperty, null, shortcut, null);

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(shortcut);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(shell);

                    return targetPath;
                }
                catch
                {
                    return path;
                }
            }
            return path;
        }

        private void Panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                _targetPanel.BackColor = Color.FromArgb(45, 52, 71);
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Panel_DragLeave(object sender, EventArgs e)
        {
            _targetPanel.BackColor = _originalPanelColor;
        }

        private void Panel_DragDrop(object sender, DragEventArgs e)
        {
            _targetPanel.BackColor = _originalPanelColor;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            bool isModified = false;

            foreach (string filePath in files)
            {
                string displayName = Path.GetFileNameWithoutExtension(filePath);
                string actualPath = GetResolvedPath(filePath);

                if (!File.Exists(actualPath)) continue;

                if (_shortcutItems.Any(x => x.AbsolutePath.Equals(actualPath, StringComparison.OrdinalIgnoreCase))) continue;

                var newItem = new ShortcutItem { DisplayName = displayName, AbsolutePath = actualPath };

                _shortcutItems.Add(newItem);
                RenderButton(newItem);
                isModified = true;
            }

            if (isModified)
            {
                SaveData();
                LogMessage("새로운 바로가기가 등록되었습니다.", Level.Info);
                CheckFileIntegrity();
            }
        }

        private void RenderButton(ShortcutItem item)
        {
            Button btn = new Button();

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            bool initialExists = File.Exists(item.AbsolutePath);
            var initialColors = GetButtonColors(initialExists);
            btn.BackColor = initialColors.BackColor;
            btn.ForeColor = initialColors.ForeColor;

            if (initialExists)
            {
                try
                {
                    Icon icon = Icon.ExtractAssociatedIcon(item.AbsolutePath);
                    if (icon != null)
                    {
                        btn.Image = icon.ToBitmap();
                        icon.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"[아이콘 추출 실패] {item.DisplayName}: {ex.Message}", Level.Warning);
                }
            }

            btn.Text = item.DisplayName;

            // 초기 생성 시점의 폰트 연산
            if (!string.IsNullOrEmpty(item.DisplayName) && item.DisplayName.Length > 5)
            {
                btn.Font = new Font(btn.Font.FontFamily, 7.5f, FontStyle.Regular);
            }
            else
            {
                btn.Font = new Font(btn.Font.FontFamily, 9.0f, FontStyle.Regular);
            }

            btn.Size = new Size(62, 75);
            btn.Margin = new Padding(2);

            btn.ImageAlign = ContentAlignment.TopCenter;
            btn.TextAlign = ContentAlignment.BottomCenter;
            btn.TextImageRelation = TextImageRelation.ImageAboveText;
            btn.Padding = new Padding(0, 6, 0, 2);

            btn.Cursor = Cursors.Hand;
            btn.UseCompatibleTextRendering = true;

            ToolTip btnToolTip = new ToolTip();
            btnToolTip.InitialDelay = 500;
            btnToolTip.SetToolTip(btn, $"파일명: {item.DisplayName}\n경로: {item.AbsolutePath}");

            _buttonMapping[item] = btn;

            ContextMenuStrip cms = new ContextMenuStrip();

            ToolStripMenuItem menuRename = new ToolStripMenuItem("이름 변경");
            menuRename.Click += (s, ev) =>
            {
                string updatedName = ShowRenameDialog(item.DisplayName);
                if (!string.IsNullOrWhiteSpace(updatedName) && updatedName != item.DisplayName)
                {
                    item.DisplayName = updatedName;
                    btn.Text = updatedName;

                    if (updatedName.Length > 5)
                        btn.Font = new Font(btn.Font.FontFamily, 7.5f, FontStyle.Regular);
                    else
                        btn.Font = new Font(btn.Font.FontFamily, 9.0f, FontStyle.Regular);

                    btnToolTip.SetToolTip(btn, $"파일명: {item.DisplayName}\n경로: {item.AbsolutePath}");
                    SaveData();
                    LogMessage($"이름 변경 완료: '{item.DisplayName}'", Level.Info);
                }
            };

            ToolStripMenuItem menuOpenFolder = new ToolStripMenuItem("파일 위치 열기");
            menuOpenFolder.Click += (s, ev) =>
            {
                if (item != null && File.Exists(item.AbsolutePath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{item.AbsolutePath}\"");
                }
                else
                {
                    MessageBox.Show("원본 파일 위치를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            ToolStripMenuItem menuDelete = new ToolStripMenuItem("삭제");
            menuDelete.ForeColor = Color.Red;
            menuDelete.Click += (s, ev) =>
            {
                DialogResult dr = MessageBox.Show($"'{item.DisplayName}' 바로가기를 리스트에서 삭제하시겠습니까?\n(원본 파일은 지워지지 않습니다.)",
                                                  "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    _targetPanel.Controls.Remove(btn);

                    _buttonMapping.Remove(item);
                    btnToolTip.Dispose();
                    btn.Dispose();

                    _shortcutItems.Remove(item);
                    SaveData();

                    LogMessage($"바로가기 삭제 완료: {item.DisplayName}", Level.Info);
                }
            };

            cms.Items.Add(menuRename);
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(menuOpenFolder);
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(menuDelete);

            btn.ContextMenuStrip = cms;

            btn.Click += (s, ev) =>
            {
                if (item == null || string.IsNullOrEmpty(item.AbsolutePath))
                {
                    MessageBox.Show("올바르지 않은 바로가기 데이터입니다.", "실행 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string targetPath = item.AbsolutePath;

                if (!File.Exists(targetPath))
                {
                    MessageBox.Show("파일이 이동되거나 삭제되어 실행할 수 없습니다.\n우클릭 메뉴를 통해 삭제할 수 있습니다.", "실행 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CheckFileIntegrity();
                    return;
                }

                try
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(targetPath)
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(targetPath)
                    };
                    System.Diagnostics.Process.Start(psi);

                    LogMessage($"바로가기 실행 성공: {targetPath}", Level.Info);
                }
                catch (System.ComponentModel.Win32Exception win32Ex)
                {
                    LogMessage($"실행 권한/경로 오류: {win32Ex.Message}", Level.Error);
                    MessageBox.Show($"운영체제에서 파일 실행을 거부했습니다.\n사유: {win32Ex.Message}", "실행 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    LogMessage($"실행 중 알 수 없는 예외: {ex.Message}", Level.Error);
                    MessageBox.Show($"알 수 없는 오류가 발생했습니다.\n{ex.Message}", "실행 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            _targetPanel.Controls.Add(btn);
        }

        private string ShowRenameDialog(string currentName)
        {
            Color dialogBg = IsDarkMode ? Color.FromArgb(45, 45, 45) : Color.FromArgb(240, 240, 240);
            Color dialogFg = IsDarkMode ? Color.White : Color.Black;

            Form prompt = new Form()
            {
                Width = 320,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "이름 변경",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = dialogBg,
                ForeColor = dialogFg
            };

            Label textLabel = new Label() { Left = 20, Top = 15, Text = "변경할 명칭을 입력하십시오:", Width = 260 };
            TextBox textBox = new TextBox() { Left = 20, Top = 40, Width = 260, Text = currentName };

            Button confirmation = new Button() { Text = "확인", Left = 110, Width = 80, Top = 75, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat };
            Button cancel = new Button() { Text = "취소", Left = 200, Width = 80, Top = 75, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat };

            confirmation.FlatAppearance.BorderSize = 1;
            cancel.FlatAppearance.BorderSize = 1;

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            prompt.Load += (s, ev) => { textBox.Focus(); textBox.SelectAll(); };

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : currentName;
        }
    }
}