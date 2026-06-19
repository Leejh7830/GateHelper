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

        public ShortcutManager()
        {
            _shortcutPath = Util.GetMetaPath("Shortcuts.json");
        }

        public void BindPanel(FlowLayoutPanel panel)
        {
            _targetPanel = panel;
            _targetPanel.AllowDrop = true;

            _targetPanel.DragEnter += Panel_DragEnter;
            _targetPanel.DragDrop += Panel_DragDrop;

            LoadData();
            foreach (var item in _shortcutItems)
            {
                RenderButton(item);
            }
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

        // =================================================================
        // 💡 [추가] 윈도우 COM 객체를 이용한 바로가기 원본 경로 추적 엔진
        // =================================================================
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
                    return path; // COM 파싱 실패 시 예외를 삼키고 원본 경로 반환
                }
            }
            return path;
        }

        private void Panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Panel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            bool isModified = false;

            foreach (string filePath in files)
            {
                // 1. 표시 명칭은 드롭한 아이콘의 이름을 그대로 사용 (UX 유지)
                string displayName = Path.GetFileNameWithoutExtension(filePath);

                // 2. 물리적 절대 경로는 껍데기를 까서 내부 원본 경로를 추출
                string actualPath = GetResolvedPath(filePath);

                // 3. 추출된 원본 경로가 실제로 존재하는지 검증 (유령 파일 차단)
                if (!File.Exists(actualPath)) continue;

                // 4. 원본 경로 기준으로 중복 등록 검사
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
            }
        }

        private void RenderButton(ShortcutItem item)
        {
            Button btn = new Button();
            btn.Text = item.DisplayName;
            btn.Tag = item.AbsolutePath;
            btn.Size = new Size(100, 80);
            btn.TextAlign = ContentAlignment.BottomCenter;
            btn.Cursor = Cursors.Hand;

            // =================================================================
            // 💡 [추가] 우클릭 컨텍스트 메뉴 (삭제 기능) 결합
            // =================================================================
            ContextMenuStrip cms = new ContextMenuStrip();

            // (선택) 편의 기능: 대상 폴더 열기
            ToolStripMenuItem menuOpenFolder = new ToolStripMenuItem("파일 위치 열기");
            menuOpenFolder.Click += (s, ev) =>
            {
                string targetPath = btn.Tag.ToString();
                if (File.Exists(targetPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{targetPath}\"");
                }
            };

            // 핵심 기능: 삭제
            ToolStripMenuItem menuDelete = new ToolStripMenuItem("삭제");
            menuDelete.ForeColor = Color.Red; // 시각적 경고
            menuDelete.Click += (s, ev) =>
            {
                DialogResult dr = MessageBox.Show($"'{item.DisplayName}' 바로가기를 리스트에서 삭제하시겠습니까?\n(원본 파일은 지워지지 않습니다.)",
                                                  "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    // 1. View (화면)에서 버튼 제거
                    _targetPanel.Controls.Remove(btn);
                    btn.Dispose();

                    // 2. Model (메모리)에서 데이터 제거
                    _shortcutItems.Remove(item);

                    // 3. Persistence (물리 파일) 덮어쓰기
                    SaveData();

                    LogMessage($"바로가기 삭제 완료: {item.DisplayName}", Level.Info);
                }
            };

            cms.Items.Add(menuOpenFolder);
            cms.Items.Add(new ToolStripSeparator()); // 구분선
            cms.Items.Add(menuDelete);

            // 생성된 메뉴를 버튼의 우클릭 속성에 매핑
            btn.ContextMenuStrip = cms;

            // =================================================================
            // 기존 좌클릭 (실행) 이벤트 유지
            // =================================================================
            btn.Click += (s, ev) =>
            {
                string targetPath = (s as Button).Tag.ToString();

                if (File.Exists(targetPath))
                {
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
                }
                else
                {
                    MessageBox.Show("파일이 이동되거나 삭제되었습니다.", "실행 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            _targetPanel.Controls.Add(btn);
        }












    }
}