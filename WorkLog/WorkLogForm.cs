using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager; // LogManager 활용을 위한 static import

namespace GateHelper
{
    public partial class WorkLogForm : MaterialForm
    {
        private readonly MaterialSkinManager _materialSkinManager;
        private readonly string _dataPath = Util.GetMetaPath("WorkLog.json");
        private List<WorkLogEntry> _items = new List<WorkLogEntry>();
        private string _currentFilter = string.Empty;
        private static readonly string[] StatusOptions = new[] { "OPEN", "ING..", "DONE", "FIXED" };
        private ContextMenuStrip _cms;
        private float _currentFontSize = 10f;
        private bool _isDatePickerDropDownOpen = false;
        private DateTime _lastPasteTime = DateTime.MinValue;

        // Model Data
        private WorkLogData _data;

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
            InitializeLogFile(); // 로그 시스템 초기화

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
            {
                col.MinimumWidth = 50;
            }

            foreach (var col in OlvWorkLog.AllColumns)
            {
                var aspect = col.AspectName ?? string.Empty;
                if (aspect == nameof(WorkLogEntry.Date) || aspect == nameof(WorkLogEntry.LastUpdated))
                {
                    col.AspectToStringFormat = aspect == nameof(WorkLogEntry.LastUpdated) ? "{0:yyyy-MM-dd HH:mm}" : "{0:yyyy-MM-dd}";
                }
                if (aspect == nameof(WorkLogEntry.No) || aspect == nameof(WorkLogEntry.LastUpdated))
                {
                    col.IsEditable = false;
                }
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
            this.FormClosing += WorkLogForm_FormClosing;

            OlvWorkLog.KeyDown += OlvWorkLog_KeyDown;
            OlvWorkLog.DoubleClick += OlvWorkLog_DoubleClick;

            OlvWorkLog.MouseWheel += (s, e) =>
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    ChangeFontSize(e.Delta > 0 ? 1f : -1f);
                    ((HandledMouseEventArgs)e).Handled = true;
                }
            };
        }

        private void SetupContextMenu()
        {
            _cms = new ContextMenuStrip();
            OlvWorkLog.ContextMenuStrip = _cms;
            _cms.Items.Add(new ToolStripMenuItem("Add New Item", null, (s, e) => AddNewEntry()));
            _cms.Items.Add(new ToolStripMenuItem("Delete Selected", null, (s, e) => DeleteSelectedEntries()));
            _cms.Items.Add(new ToolStripSeparator());
            // _cms.Items.Add(new ToolStripMenuItem("Open Log File", null, (s, e) => OpenLogFile())); // 로그 열기 메뉴 추가
        }

        private void SaveData()
        {
            try
            {
                if (_data == null) _data = new WorkLogData();
                _data.Items = _items;

                string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
                File.WriteAllText(_dataPath, json);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                if (!File.Exists(_dataPath))
                {
                    _data = new WorkLogData();
                    _items = _data.Items;
                    LogMessage("New Data File Created (First Run)", Level.Info); // 최초 실행 시만 기록
                    return;
                }

                string json = File.ReadAllText(_dataPath);
                _data = JsonConvert.DeserializeObject<WorkLogData>(json);

                if (_data != null)
                {
                    _items = _data.Items ?? new List<WorkLogEntry>();
                    _data.Items = _items;
                    chkHideDone.Checked = _data.HideDone;

                    // [핵심] 여러 줄의 로그를 이 시점에 한 줄로 요약
                    LogMessage($"WorkLog Started - Loaded Items: {_items.Count}, FontSize: {_data.FontSize}", Level.Info);

                    this.BeginInvoke(new Action(() =>
                    {
                        ChangeFontSize(0);
                        ApplyFilter(TxtWorkLog.Text);
                    }));
                }
                else
                {
                    LogMessage("Data Load Failed (Deserialization null)", Level.Error);
                    _data = new WorkLogData();
                    _items = _data.Items;
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                _data = new WorkLogData();
                _items = _data.Items;
                OlvWorkLog.SetObjects(_items);
            }
        }

        private async void OlvWorkLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
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
                    using (Image img = Clipboard.GetImage())
                    {
                        if (img == null) return;

                        string imgDir = Path.Combine(Path.GetDirectoryName(_dataPath), "WorkLog_Images");
                        if (!Directory.Exists(imgDir)) Directory.CreateDirectory(imgDir);

                        string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{DateTime.Now.Ticks}.jpg";
                        string fullPath = Path.Combine(imgDir, fileName);

                        LogMessage($"Saving image: {fileName}", Level.Info);

                        using (Bitmap bmp = new Bitmap(img))
                        {
                            await Task.Run(() => bmp.Save(fullPath, System.Drawing.Imaging.ImageFormat.Jpeg));
                        }

                        if (File.Exists(fullPath))
                        {
                            entry.ImagePaths.Add(fileName);
                            entry.Touch();
                            OlvWorkLog.RefreshObject(entry);
                            SaveData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex, Level.Error);
                    MessageBox.Show($"Failed to save image: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OlvWorkLog_DoubleClick(object sender, EventArgs e)
        {
            Point mousePos = OlvWorkLog.PointToClient(Control.MousePosition);
            OlvListViewHitTestInfo hitTest = OlvWorkLog.OlvHitTest(mousePos.X, mousePos.Y);

            if (hitTest.Item != null && hitTest.Column != null)
            {
                if (hitTest.Column.Text == "Images" || hitTest.Column.AspectName == "ImagePaths")
                {
                    if (hitTest.RowObject is WorkLogEntry entry && entry.HasImage)
                    {
                        ShowImageSelectionMenu(entry);
                    }
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
            string imgDir = Path.Combine(Path.GetDirectoryName(_dataPath), "WorkLog_Images");

            for (int i = entry.ImagePaths.Count - 1; i >= 0; i--)
            {
                string fileName = entry.ImagePaths[i];
                string fullPath = Path.Combine(imgDir, fileName);

                ToolStripMenuItem item = new ToolStripMenuItem(fileName);
                item.Click += (s, e) => OpenImageFile(fileName);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        using (var stream = new MemoryStream(File.ReadAllBytes(fullPath)))
                        {
                            Image original = Image.FromStream(stream);
                            item.Image = original.GetThumbnailImage(64, 64, null, IntPtr.Zero);
                        }
                    }
                    catch { }
                }
                menu.Items.Add(item);
            }
            menu.Show(Cursor.Position);
        }

        private void OpenImageFile(string fileName)
        {
            string imgDir = Path.Combine(Path.GetDirectoryName(_dataPath), "WorkLog_Images");
            string fullPath = Path.Combine(imgDir, fileName);

            if (File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullPath) { UseShellExecute = true });
            }
            else
            {
                LogMessage($"Image file missing: {fileName}", Level.Error);
                MessageBox.Show("File not found: " + fileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteSelectedEntries()
        {
            var selected = OlvWorkLog.SelectedObjects?.Cast<WorkLogEntry>().ToList();
            if (selected == null || selected.Count == 0) return;

            var result = MessageBox.Show($"Delete {selected.Count} item(s)?\n(Images will be also deleted.)",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            LogMessage($"Deleting {selected.Count} entries.", Level.Info);
            List<string> failedFiles = new List<string>();
            string imgDir = Path.Combine(Path.GetDirectoryName(_dataPath), "WorkLog_Images");

            foreach (var entry in selected)
            {
                foreach (var fileName in entry.ImagePaths)
                {
                    try
                    {
                        string fullPath = Path.Combine(imgDir, fileName);
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        LogException(ex, Level.Error, $"File Delete Fail: {fileName}");
                        failedFiles.Add(fileName);
                    }
                }
                _items.Remove(entry);
                OlvWorkLog.RemoveObject(entry);
            }

            if (failedFiles.Count > 0)
            {
                MessageBox.Show("Some files could not be deleted.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            SaveData();
        }

        private void OlvWorkLog_CellEditStarting(object sender, CellEditEventArgs e)
        {
            if (e.Column == null) return;
            if (e.Column.AspectName == "ImagePaths" || e.Column.Text == "Images")
            {
                e.Cancel = true;
                return;
            }

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
                    if (cb != null && !cb.IsDisposed)
                    {
                        cb.Focus();
                        cb.DroppedDown = true;
                    }
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

            if (aspect == nameof(WorkLogEntry.Date) && _isDatePickerDropDownOpen)
            {
                e.Cancel = true;
                return;
            }

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

        private void TxtWorkLog_TextChanged(object sender, EventArgs e) => ApplyFilter(TxtWorkLog.Text);
        private void TxtWorkLog_KeyUp(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) ApplyFilter(TxtWorkLog.Text); }

        private void ApplyFilter(string q)
        {
            _currentFilter = q?.Trim() ?? "";
            OlvWorkLog.BeginUpdate();
            var filteredList = _items.Where(RowMatchesFilter).ToList();
            OlvWorkLog.SetObjects(filteredList);
            OlvWorkLog.EndUpdate();
        }

        private bool RowMatchesFilter(WorkLogEntry entry)
        {
            if (chkHideDone.Checked && entry.Status == "DONE") return false;

            string q = _currentFilter.ToLower();
            if (string.IsNullOrEmpty(q)) return true;

            return (entry.Title?.ToLower().Contains(q) ?? false) ||
                   (entry.Content?.ToLower().Contains(q) ?? false) ||
                   (entry.Tags?.ToLower().Contains(q) ?? false) ||
                   (entry.Memo?.ToLower().Contains(q) ?? false) ||
                   (entry.Status?.ToLower().Contains(q) ?? false);
        }

        private void AddNewEntry()
        {
            try
            {
                int nextNo = _items.Count == 0 ? 1 : _items.Max(x => x.No) + 1;
                var entry = new WorkLogEntry { No = nextNo, Date = DateTime.Now };
                _items.Add(entry);
                OlvWorkLog.AddObject(entry); // 리스트뷰에 즉시 반영
                OlvWorkLog.DeselectAll(); // 기존에 선택된 모든 항목을 해제

                OlvWorkLog.SelectedObject = entry; // 새로 만든 줄 선택
                OlvWorkLog.EnsureModelVisible(entry); // 화면 밖이면 스크롤 이동

                LogMessage($"Entry added. No: {nextNo}", Level.Info);
                SaveData();
            }
            catch (Exception ex) { LogException(ex, Level.Error); }
        }

        private void chkHideDone_CheckedChanged(object sender, EventArgs e)
        {
            if (_data == null) return;
            _data.HideDone = chkHideDone.Checked;
            ApplyFilter(TxtWorkLog.Text);
            SaveData();
        }

        private void WorkLogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogMessage("WorkLogForm Closing.", Level.Info);
            OlvWorkLog.Parent = null;
            OlvWorkLog.Dispose();
        }

        private void OlvWorkLog_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        { e.DrawBackground(); TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.Black, TextFormatFlags.VerticalCenter); }

        private void OlvWorkLog_DrawItem(object sender, DrawListViewItemEventArgs e) => e.DrawBackground();

        private void OlvWorkLog_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            bool highlight = !string.IsNullOrEmpty(_currentFilter) && e.SubItem.Text.ToLower().Contains(_currentFilter.ToLower());
            Font f = highlight ? new Font(e.SubItem.Font, FontStyle.Bold) : e.SubItem.Font;
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, f, e.Bounds, e.SubItem.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void OlvWorkLog_FormatCell(object sender, FormatCellEventArgs e) { }

        private void OlvWorkLog_FormatRow(object sender, FormatRowEventArgs e)
        {
            if (e.Model is WorkLogEntry entry)
            {
                // 1. 기본 색상 로직
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

                // 2. 리마인더 로직 (7일 이상 경과)
                if (entry.Status == "OPEN" || entry.Status == "ING..")
                {
                    if ((DateTime.Now - entry.Date).TotalDays >= 7)
                    {
                        e.Item.ForeColor = Color.Red;
                        // FontStyle만 추가할 때는 아래와 같이 기존 폰트를 활용하는 것이 안전합니다.
                        e.Item.Font = new Font(OlvWorkLog.Font, FontStyle.Bold);
                    }
                }
            }
        }

        private void ChangeFontSize(float delta)
        {
            if (_data == null) return;

            _data.FontSize = Math.Max(8f, Math.Min(24f, _data.FontSize + delta));
            _currentFontSize = _data.FontSize;

            Font nFont = new Font("맑은 고딕", _data.FontSize);
            OlvWorkLog.Font = nFont;
            OlvWorkLog.RowHeight = (int)(_data.FontSize * 2.2);

            OlvWorkLog.BeginUpdate();
            try
            {
                foreach (OLVColumn col in OlvWorkLog.AllColumns)
                {
                    try { col.HeaderFont = nFont; } catch { }
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

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            AddNewEntry();
        }
    }
}