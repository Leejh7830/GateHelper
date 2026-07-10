using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    public partial class WorkLogForm : MaterialForm
    {
        // --- 의존성 (Repository/Service로 분리) ---
        private readonly WorkLogRepository _repo = new WorkLogRepository();
        private readonly WorkLogService _service = new WorkLogService();

        private readonly MaterialSkinManager _materialSkinManager;
        private List<WorkLogEntry> _items = new List<WorkLogEntry>();
        private string _currentFilter = string.Empty;
        private static readonly string[] StatusOptions = { "OPEN", "ING..", "DONE", "FIXED" };
        private ContextMenuStrip _cms;
        private bool _isDatePickerDropDownOpen = false;
        private DateTime _lastPasteTime = DateTime.MinValue;
        private Timer _searchTimer;
        private WorkLogData _data;

        // Font 캐시 - FormatRow/DrawSubItem에서 매번 new Font() 생성 방지
        private Font _boldFont;
        private Font _regularFont;

        public WorkLogForm()
        {
            InitializeComponent();
            _materialSkinManager = MaterialSkinManager.Instance;
            _materialSkinManager.AddFormToManage(this);

            this.Load -= WorkLogForm_Load;
            this.Load += WorkLogForm_Load;
        }

        private void WorkLogForm_Load(object sender, EventArgs e)
        {
            InitializeLogFile();
            InitListView();
            WireEvents();
            SetupContextMenu();
            LoadData();
        }

        private void InitListView()
        {
            OlvWorkLog.FullRowSelect = true;
            OlvWorkLog.ShowGroups = false;
            OlvWorkLog.CellEditActivation = ObjectListView.CellEditActivateMode.DoubleClick;
            OlvWorkLog.CellEditUseWholeCell = true;
            OlvWorkLog.UseFiltering = true;
            OlvWorkLog.OwnerDraw = true;
            OlvWorkLog.HeaderUsesThemes = false;
            OlvWorkLog.ShowItemToolTips = false;
            OlvWorkLog.IsSimpleDragSource = true;

            Content.FillsFreeSpace = true;

            foreach (OLVColumn col in OlvWorkLog.AllColumns)
                col.MinimumWidth = 50;

            foreach (var col in OlvWorkLog.AllColumns)
            {
                var aspect = col.AspectName ?? string.Empty;
                if (aspect == nameof(WorkLogEntry.Date) || aspect == nameof(WorkLogEntry.LastUpdated))
                    col.AspectToStringFormat = aspect == nameof(WorkLogEntry.LastUpdated) ? "{0:yyyy-MM-dd HH:mm}" : "{0:yyyy-MM-dd}";
                if (aspect == nameof(WorkLogEntry.No) || aspect == nameof(WorkLogEntry.LastUpdated))
                    col.IsEditable = false;
            }

            var colImages = OlvWorkLog.AllColumns.Cast<OLVColumn>()
                .FirstOrDefault(x => x.Text == "Images" || x.AspectName == "ImagePaths");
            if (colImages != null)
            {
                colImages.AspectGetter = row =>
                {
                    var entry = (WorkLogEntry)row;
                    return entry.HasImage ? $"📸 ({entry.ImagePaths.Count})" : "";
                };
                colImages.IsEditable = false;
                colImages.TextAlign = HorizontalAlignment.Center;
                colImages.Width = 80;
            }
        }

        private void WireEvents()
        {
            OlvWorkLog.CellEditStarting += OlvWorkLog_CellEditStarting;
            OlvWorkLog.CellEditFinishing += OlvWorkLog_CellEditFinishing;
            OlvWorkLog.CellEditFinished += OlvWorkLog_CellEditFinished;
            TxtWorkLog.TextChanged += TxtWorkLog_TextChanged;
            TxtWorkLog.KeyUp += TxtWorkLog_KeyUp;
            OlvWorkLog.DrawColumnHeader += OlvWorkLog_DrawColumnHeader;
            OlvWorkLog.DrawItem += OlvWorkLog_DrawItem;
            OlvWorkLog.DrawSubItem += OlvWorkLog_DrawSubItem;
            OlvWorkLog.FormatCell += OlvWorkLog_FormatCell;
            OlvWorkLog.FormatRow += OlvWorkLog_FormatRow;
            OlvWorkLog.DoubleClick += OlvWorkLog_DoubleClick;
            OlvWorkLog.KeyDown += OlvWorkLog_KeyDown; // Designer에서 제거했으므로 여기서 연결
            this.FormClosing += WorkLogForm_FormClosing;

            OlvWorkLog.MouseWheel += (s, e) =>
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    ChangeFontSize(e.Delta > 0 ? 1f : -1f);
                    ((HandledMouseEventArgs)e).Handled = true;
                }
            };

            _searchTimer = new Timer { Interval = 300 };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                ApplyFilter(TxtWorkLog.Text);
            };
        }

        private void SetupContextMenu()
        {
            _cms = new ContextMenuStrip();
            OlvWorkLog.ContextMenuStrip = _cms;
            _cms.Items.Add(new ToolStripMenuItem("Add New Item", null, (s, e) => AddNewEntry()));
            _cms.Items.Add(new ToolStripMenuItem("Delete Selected", null, (s, e) => DeleteSelectedEntries()));
            _cms.Items.Add(new ToolStripSeparator());
        }

        // --- 데이터 저장/로드 ---

        private void SaveData()
        {
            if (_data == null) _data = new WorkLogData();
            _data.Items = _items;
            _repo.Save(_data); // 임시파일 교체 방식으로 안전 저장
        }

        private void LoadData()
        {
            try
            {
                _data = _repo.Load();
                _items = _data.Items ?? new List<WorkLogEntry>();
                _data.Items = _items;
                chkHideDone.Checked = _data.HideDone;

                LogMessage($"WorkLog Started - Loaded Items: {_items.Count}, FontSize: {_data.FontSize}", Level.Info);

                this.BeginInvoke(new Action(() =>
                {
                    ChangeFontSize(0);
                    ApplyFilter(TxtWorkLog.Text);
                }));
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                _data = new WorkLogData();
                _items = _data.Items;
                OlvWorkLog.SetObjects(_items);
            }
        }

        // --- 이미지 붙여넣기 (Ctrl+V) ---

        private async void OlvWorkLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.V) return;
            if ((DateTime.Now - _lastPasteTime).TotalMilliseconds < 800) return;
            _lastPasteTime = DateTime.Now;

            if (!(OlvWorkLog.SelectedObject is WorkLogEntry entry)) return;
            if (!Clipboard.ContainsImage())
            {
                LogMessage("Clipboard paste attempted but no image found.", Level.Info);
                return;
            }

            try
            {
                // 1. UI 스레드에서 클립보드 접근 및 안전한 Bitmap 복사본 생성
                //    일부 캡처 툴 이미지는 원본 Image를 그대로 넘기면 ExternalException 발생
                Image clipImg = Clipboard.GetImage();
                if (clipImg == null) return;
                Bitmap safeBmp = new Bitmap(clipImg); // UI 스레드에서 안전한 복사본 생성
                clipImg.Dispose();

                // 2. 저장은 Repository 백그라운드 처리
                int nextIndex = entry.ImagePaths.Count + 1;
                string fileName = await _repo.SaveImageAsync(safeBmp, entry.No, nextIndex);
                safeBmp.Dispose();

                // 3. await 이후 UI 갱신은 BeginInvoke로 스레드 안전하게 처리
                if (fileName != null)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        entry.ImagePaths.Add(fileName);
                        entry.Touch();
                        OlvWorkLog.RefreshObject(entry);
                        SaveData();
                    }));
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"Failed to save image: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 이미지 뷰어 ---

        private void OlvWorkLog_DoubleClick(object sender, EventArgs e)
        {
            Point mousePos = OlvWorkLog.PointToClient(Control.MousePosition);
            OlvListViewHitTestInfo hitTest = OlvWorkLog.OlvHitTest(mousePos.X, mousePos.Y);

            if (hitTest.Item != null && hitTest.Column != null)
            {
                if (hitTest.Column.Text == "Images" || hitTest.Column.AspectName == "ImagePaths")
                {
                    if (hitTest.RowObject is WorkLogEntry entry && entry.HasImage)
                        ShowImageSelectionMenu(entry);
                }
            }
        }

        private void ShowImageSelectionMenu(WorkLogEntry entry)
        {
            if (entry.ImagePaths.Count == 1)
            {
                OpenImageFile(entry.ImagePaths[0]);
                return;
            }

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ImageScalingSize = new Size(64, 64);

            // 메뉴 닫힌 후 썸네일 이미지 일괄 Dispose
            // Closed 이벤트 처리 도중 Dispose하면 ObjectDisposedException 발생
            // → BeginInvoke로 이벤트 완료 후 다음 루프에서 해제
            menu.Closed += (s, ev) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (ToolStripItem item in menu.Items)
                    {
                        item.Image?.Dispose();
                        item.Image = null;
                    }
                    menu.Dispose();
                }));
            };

            for (int i = entry.ImagePaths.Count - 1; i >= 0; i--)
            {
                string fileName = entry.ImagePaths[i];
                string fullPath = _repo.GetFullImagePath(fileName);

                var menuItem = new ToolStripMenuItem(fileName);
                menuItem.Click += (s, ev) => OpenImageFile(fileName);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        // 원본 Image를 using으로 즉시 해제, 썸네일만 menuItem에 보관
                        using (var stream = new MemoryStream(File.ReadAllBytes(fullPath)))
                        using (var original = Image.FromStream(stream))
                        {
                            menuItem.Image = original.GetThumbnailImage(64, 64, null, IntPtr.Zero);
                        }
                    }
                    catch { }
                }
                menu.Items.Add(menuItem);
            }
            menu.Show(Cursor.Position);
        }

        private void OpenImageFile(string fileName)
        {
            string fullPath = _repo.GetFullImagePath(fileName);
            if (File.Exists(fullPath))
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullPath) { UseShellExecute = true });
            else
            {
                LogMessage($"Image file missing: {fileName}", Level.Error);
                MessageBox.Show("File not found: " + fileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // --- 항목 관리 ---

        private void DeleteSelectedEntries()
        {
            var selected = OlvWorkLog.SelectedObjects?.Cast<WorkLogEntry>().ToList();
            if (selected == null || selected.Count == 0) return;

            var result = MessageBox.Show($"Delete {selected.Count} item(s)?\n(Images will be also deleted.)",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            LogMessage($"Deleting {selected.Count} entries.", Level.Info);

            var deletedEntries = new List<WorkLogEntry>();
            var failedEntries = new List<(WorkLogEntry entry, List<string> files)>();

            foreach (var entry in selected)
            {
                var failed = _repo.DeleteImages(entry.ImagePaths);
                if (failed.Count > 0)
                    failedEntries.Add((entry, failed));  // 이미지 삭제 실패 → 행 보존
                else
                    deletedEntries.Add(entry);           // 이미지 삭제 성공 → 행 삭제
            }

            // 성공한 항목만 리스트/UI에서 제거
            foreach (var entry in deletedEntries)
            {
                _items.Remove(entry);
                OlvWorkLog.RemoveObject(entry);
            }

            // 실패한 항목은 행 유지 + 사용자에게 알림
            if (failedEntries.Count > 0)
            {
                var failedNames = failedEntries
                    .SelectMany(x => x.files)
                    .ToList();
                MessageBox.Show(
                    $"아래 이미지 파일을 삭제하지 못해 해당 항목은 삭제되지 않았습니다:\n{string.Join("\n", failedNames)}\n\n파일이 사용 중이거나 읽기 전용인지 확인하세요.",
                    "삭제 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogMessage($"Image delete failed: {string.Join(", ", failedNames)}", Level.Warning);
            }

            if (deletedEntries.Count > 0)
                SaveData();
        }

        private void AddNewEntry()
        {
            try
            {
                var entry = _service.CreateNewEntry(_items);
                _items.Add(entry);
                OlvWorkLog.AddObject(entry);
                OlvWorkLog.DeselectAll();
                OlvWorkLog.SelectedObject = entry;
                OlvWorkLog.EnsureModelVisible(entry);
                LogMessage($"Entry added. No: {entry.No}", Level.Info);
                SaveData();
            }
            catch (Exception ex) { LogException(ex, Level.Error); }
        }

        // --- 셀 편집 ---

        private void OlvWorkLog_CellEditStarting(object sender, CellEditEventArgs e)
        {
            if (e.Column == null) return;
            if (e.Column.AspectName == "ImagePaths" || e.Column.Text == "Images") { e.Cancel = true; return; }

            var aspect = e.Column.AspectName;
            if (aspect == nameof(WorkLogEntry.Status))
            {
                var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Bounds = e.CellBounds };
                cb.Items.AddRange(StatusOptions);
                cb.SelectedItem = StatusOptions.Contains(e.Value?.ToString()) ? e.Value.ToString() : "OPEN";
                cb.SelectedIndexChanged += (s, _) =>
                {
                    e.NewValue = cb.SelectedItem.ToString();
                    this.BeginInvoke(new Action(() => OlvWorkLog.FinishCellEdit()));
                };
                this.BeginInvoke(new Action(() =>
                {
                    if (cb != null && !cb.IsDisposed) { cb.Focus(); cb.DroppedDown = true; }
                }));
                e.Control = cb;
            }
            else if (aspect == nameof(WorkLogEntry.Date))
            {
                var dtp = new DateTimePicker
                {
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "yyyy-MM-dd HH:mm:ss",
                    Value = e.Value is DateTime dt ? dt : DateTime.Now,
                    Bounds = e.CellBounds
                };
                dtp.DropDown += (s, _) => _isDatePickerDropDownOpen = true;
                dtp.CloseUp += (s, _) => { _isDatePickerDropDownOpen = false; OlvWorkLog.FinishCellEdit(); };
                e.Control = dtp;
            }
        }

        private void OlvWorkLog_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            if (e.Column == null || e.RowObject == null) return;
            var entry = (WorkLogEntry)e.RowObject;
            var aspect = e.Column.AspectName;

            if (aspect == nameof(WorkLogEntry.Date) && _isDatePickerDropDownOpen) { e.Cancel = true; return; }
            if (e.Control is ComboBox cb) e.NewValue = cb.SelectedItem?.ToString();
            else if (e.Control is DateTimePicker dtp) e.NewValue = dtp.Value;

            if (!e.Cancel && e.NewValue != null)
            {
                if (aspect == nameof(WorkLogEntry.Tags)) entry.Tags = e.NewValue.ToString();
                else if (aspect == nameof(WorkLogEntry.Status)) entry.Status = e.NewValue.ToString();
                else if (aspect == nameof(WorkLogEntry.Title)) entry.Title = e.NewValue.ToString();
                else if (aspect == nameof(WorkLogEntry.Content)) entry.Content = e.NewValue.ToString();
                else if (aspect == nameof(WorkLogEntry.Memo)) entry.Memo = e.NewValue.ToString();
            }
        }

        private void OlvWorkLog_CellEditFinished(object sender, CellEditEventArgs e)
        {
            if (e.Cancel || !(e.RowObject is WorkLogEntry entry)) return;
            entry.Touch();
            OlvWorkLog.RefreshObject(entry);
            SaveData();
        }

        // --- 검색/필터 ---

        private void TxtWorkLog_TextChanged(object sender, EventArgs e)
        {
            if (_searchTimer == null) return;
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void TxtWorkLog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) ApplyFilter(TxtWorkLog.Text);
        }

        private void ApplyFilter(string q)
        {
            _currentFilter = q?.Trim() ?? "";
            OlvWorkLog.BeginUpdate();
            var filteredList = _service.FilterItems(_items, _currentFilter, chkHideDone.Checked);
            OlvWorkLog.SetObjects(filteredList);
            OlvWorkLog.EndUpdate();
        }

        private void chkHideDone_CheckedChanged(object sender, EventArgs e)
        {
            if (_data == null) return;
            _data.HideDone = chkHideDone.Checked;
            ApplyFilter(TxtWorkLog.Text);
            SaveData();
        }

        // --- 폼 닫기 ---

        private void WorkLogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogMessage("WorkLogForm Closing.", Level.Info);
            _boldFont?.Dispose();
            _regularFont?.Dispose();
            _searchTimer?.Dispose();
            _cms?.Dispose();
            OlvWorkLog.Parent = null;
            OlvWorkLog.Dispose();
        }

        // --- 렌더링 ---

        private void OlvWorkLog_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawBackground();
            // Color.Black 하드코딩 제거 → 다크모드 대응
            TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, e.ForeColor, TextFormatFlags.VerticalCenter);
        }

        private void OlvWorkLog_DrawItem(object sender, DrawListViewItemEventArgs e) => e.DrawBackground();

        private void OlvWorkLog_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            // 검색 Bold 강조: FormatRow에서 처리하므로 여기선 기본 렌더링만
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds,
                e.SubItem.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void OlvWorkLog_FormatCell(object sender, FormatCellEventArgs e) { }

        private void OlvWorkLog_FormatRow(object sender, FormatRowEventArgs e)
        {
            if (!(e.Model is WorkLogEntry entry)) return;

            if (entry.Status == "DONE")
            {
                e.Item.BackColor = Color.LightGray;
                e.Item.ForeColor = Color.DimGray;
            }
            else if (entry.Status == "ING..")
            {
                e.Item.BackColor = Color.Yellow;
                e.Item.ForeColor = Color.Black;
            }

            if ((entry.Status == "OPEN" || entry.Status == "ING..") &&
                (DateTime.Now - entry.Date).TotalDays >= 7)
            {
                e.Item.ForeColor = Color.Red;
                e.Item.Font = _boldFont;
            }

            // 검색어 매칭 행 Bold 강조
            if (!string.IsNullOrEmpty(_currentFilter))
            {
                string q = _currentFilter.ToLower();
                bool matched = (entry.Title?.ToLower().Contains(q) ?? false) ||
                               (entry.Content?.ToLower().Contains(q) ?? false) ||
                               (entry.Tags?.ToLower().Contains(q) ?? false) ||
                               (entry.Memo?.ToLower().Contains(q) ?? false) ||
                               (entry.Status?.ToLower().Contains(q) ?? false);
                if (matched)
                    e.Item.Font = _boldFont;
            }
        }

        // --- 폰트 크기 ---

        private void ChangeFontSize(float delta)
        {
            if (_data == null) return;

            _data.FontSize = _service.ClampFontSize(_data.FontSize, delta);

            // 폰트 캐시 갱신
            _boldFont?.Dispose();
            _regularFont?.Dispose();
            _boldFont = new Font("맑은 고딕", _data.FontSize, FontStyle.Bold);
            _regularFont = new Font("맑은 고딕", _data.FontSize, FontStyle.Regular);

            OlvWorkLog.Font = _regularFont;
            OlvWorkLog.RowHeight = (int)(_data.FontSize * 2.2);

            OlvWorkLog.BeginUpdate();
            try
            {
                foreach (OLVColumn col in OlvWorkLog.AllColumns)
                {
                    try { col.HeaderFont = _regularFont; } catch { }
                }
                OlvWorkLog.BuildList(true);
                OlvWorkLog.RefreshObjects(_items);
            }
            finally { OlvWorkLog.EndUpdate(); }

            OlvWorkLog.Invalidate(true);
            OlvWorkLog.Update();
            SaveData();
        }

        private void btnZoomIn_Click(object sender, EventArgs e) => ChangeFontSize(1f);
        private void btnZoomOut_Click(object sender, EventArgs e) => ChangeFontSize(-1f);
        private void btnAddNew_Click(object sender, EventArgs e) => AddNewEntry();
    }
}