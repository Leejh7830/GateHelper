using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;

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

        public WorkLogForm()
        {
            InitializeComponent();
            _materialSkinManager = MaterialSkinManager.Instance;
            _materialSkinManager.AddFormToManage(this);

            this.Load -= WorkLogForm_Load;
            this.Load += WorkLogForm_Load;
        }

        /// <summary>
        /// 폼 로드 시 초기 UI 설정 및 데이터 로딩을 수행합니다.
        /// </summary>
        private void WorkLogForm_Load(object sender, EventArgs e)
        {
            InitListView();
            WireEvents();
            SetupContextMenu();
            LoadData();
        }

        /// <summary>
        /// ObjectListView의 컬럼 속성, 편집 모드, 폰트 등을 초기화합니다.
        /// </summary>
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

            // 폼 확장/축소 시 컬럼 크기 조절
            foreach (OLVColumn col in OlvWorkLog.AllColumns)
            {
                col.MinimumWidth = 50; // 너무 작아지는 것 방지
            }

            // 컬럼별 날짜 포맷 및 편집 가능 여부 설정
            foreach (var col in OlvWorkLog.AllColumns)
            {
                var aspect = col.AspectName ?? string.Empty;
                if (aspect == nameof(WorkLogEntry.Date) || aspect == nameof(WorkLogEntry.LastUpdated))
                {
                    col.AspectToStringFormat = aspect == nameof(WorkLogEntry.LastUpdated) ? "{0:yyyy-MM-dd HH:mm}" : "{0:yyyy-MM-dd}";
                }
                if (aspect == nameof(WorkLogEntry.No) || aspect == nameof(WorkLogEntry.LastUpdated))
                {
                    col.IsEditable = false; // No와 수정시간은 자동관리되므로 편집 불가
                }
            }

            // UI 스레드에서 폰트 적용
            this.BeginInvoke(new Action(() =>
            {
                Font malgunFont = new Font("맑은 고딕", _currentFontSize, FontStyle.Regular);
                OlvWorkLog.Font = malgunFont;
                foreach (OLVColumn col in OlvWorkLog.AllColumns)
                {
                    try { col.HeaderFont = malgunFont; } catch { }
                }
                OlvWorkLog.Refresh();
            }));

            // Images 열을 찾아서 커스텀 출력 설정
            var colImages = OlvWorkLog.AllColumns.Cast<OLVColumn>()
                        .FirstOrDefault(x => x.Text == "Images" || x.AspectName == "ImagePaths");

            if (colImages != null)
            {
                colImages.AspectGetter = row => {
                    var entry = (WorkLogEntry)row;
                    return entry.HasImage ? $"📸 ({entry.ImagePaths.Count})" : "";
                };
                colImages.IsEditable = false;
                colImages.TextAlign = HorizontalAlignment.Center;
                colImages.Width = 80; // 적당한 너비 설정
            }



        }

        /// <summary>
        /// 각종 컨트롤의 이벤트(편집, 검색, 이미지 처리, 휠 줌 등)를 연결합니다.
        /// </summary>
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

            // 핵심: 단축키 및 더블클릭 이미지 이벤트
            OlvWorkLog.KeyDown += OlvWorkLog_KeyDown;
            OlvWorkLog.DoubleClick += OlvWorkLog_DoubleClick;

            // Ctrl + 마우스 휠을 통한 폰트 크기 조절
            OlvWorkLog.MouseWheel += (s, e) =>
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    ChangeFontSize(e.Delta > 0 ? 1f : -1f);
                    ((HandledMouseEventArgs)e).Handled = true;
                }
            };
        }

        /// <summary>
        /// 리스트뷰 우클릭 메뉴(추가/삭제)를 구성합니다.
        /// </summary>
        private void SetupContextMenu()
        {
            _cms = new ContextMenuStrip();
            OlvWorkLog.ContextMenuStrip = _cms;
            _cms.Items.Add(new ToolStripMenuItem("Add New Item", null, (s, e) => AddNewEntry()));
            _cms.Items.Add(new ToolStripMenuItem("Delete Selected", null, (s, e) => DeleteSelectedEntries()));
        }

        /// <summary>
        /// 현재 메모리상의 데이터를 JSON 파일로 영구 저장합니다.
        /// </summary>
        private void SaveData()
        {
            try
            {
                var data = new WorkLogData { FontSize = _currentFontSize, Items = _items };
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(_dataPath, json);
            }
            catch (Exception ex) { LogException(ex, Level.Error); }
        }

        /// <summary>
        /// JSON 파일로부터 데이터를 로드하고 리스트뷰에 표시합니다.
        /// </summary>
        private void LoadData()
        {
            try
            {
                if (!File.Exists(_dataPath)) return;
                string json = File.ReadAllText(_dataPath);
                var data = JsonConvert.DeserializeObject<WorkLogData>(json);
                if (data != null)
                {
                    _items = data.Items ?? new List<WorkLogEntry>();
                    _currentFontSize = data.FontSize > 0 ? data.FontSize : 10f;
                    ChangeFontSize(0);
                }
                OlvWorkLog.SetObjects(_items);
            }
            catch (Exception ex) { LogException(ex, Level.Error); }
        }

        /// <summary>
        /// [핵심 기능] 클립보드 이미지 붙여넣기 및 예외 상황(Abnormal) 처리
        /// </summary>
        private async void OlvWorkLog_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + V 감지
            if (e.Control && e.KeyCode == Keys.V)
            {
                // 1. [인터락] 연타 방지: 0.8초 이내 재입력 무시
                if ((DateTime.Now - _lastPasteTime).TotalMilliseconds < 800) return;
                _lastPasteTime = DateTime.Now;

                // 2. [인터락] 선택된 데이터가 없는 경우
                if (!(OlvWorkLog.SelectedObject is WorkLogEntry entry)) return;

                // 3. [인터락] 클립보드에 이미지 데이터가 없는 경우
                if (!Clipboard.ContainsImage()) return;

                try
                {
                    using (Image img = Clipboard.GetImage())
                    {
                        // 4. [Abnormal] 특정 앱에서 복사 시 이미지 객체가 null이 되는 현상 대응
                        if (img == null) return;

                        // 5. [Abnormal] 초고해상도 이미지 경고 (선택 사항)
                        if (img.Width > 5000 || img.Height > 5000)
                        {
                            var res = MessageBox.Show("This image is extremely large. Continue?", "Large Image", MessageBoxButtons.YesNo);
                            if (res != DialogResult.Yes) return;
                        }

                        string imgDir = Path.Combine(Path.GetDirectoryName(_dataPath), "WorkLog_Images");
                        if (!Directory.Exists(imgDir)) Directory.CreateDirectory(imgDir);

                        string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{DateTime.Now.Ticks}.jpg";
                        string fullPath = Path.Combine(imgDir, fileName);

                        // 6. [성능] 비동기로 저장하여 UI 프리징 차단
                        using (Bitmap bmp = new Bitmap(img))
                        {
                            await Task.Run(() => bmp.Save(fullPath, System.Drawing.Imaging.ImageFormat.Jpeg));
                        }

                        // 7. [확인] 실제 저장이 성공했는지 검증 후 데이터 연결
                        if (File.Exists(fullPath))
                        {
                            entry.ImagePaths.Add(fileName);
                            entry.Touch();
                            OlvWorkLog.RefreshObject(entry); // 이미지 개수 표시가 즉시 갱신됨
                            SaveData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 8. [Abnormal] 권한 부족, 디스크 꽉 참 등 물리적 에러 처리
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
                        // [변경] 바로 열지 않고 개수에 따라 분기 처리
                        ShowImageSelectionMenu(entry);
                    }
                }
            }
        }

        /// <summary>
        /// 이미지가 여러 장일 경우 선택 메뉴를 띄우고, 한 장이면 바로 엽니다.
        /// </summary>
        private void ShowImageSelectionMenu(WorkLogEntry entry)
        {
            if (entry.ImagePaths.Count == 1)
            {
                OpenImageFile(entry.ImagePaths[0]);
                return;
            }

            ContextMenuStrip menu = new ContextMenuStrip();
            // 미리보기 이미지가 잘 보이도록 이미지 크기 설정 (기본값은 너무 작음)
            menu.ImageScalingSize = new Size(64, 64);

            string imgDir = Path.Combine(Path.GetDirectoryName(_dataPath), "WorkLog_Images");

            for (int i = entry.ImagePaths.Count - 1; i >= 0; i--)
            {
                string fileName = entry.ImagePaths[i];
                string fullPath = Path.Combine(imgDir, fileName);

                ToolStripMenuItem item = new ToolStripMenuItem(fileName);
                item.Click += (s, e) => OpenImageFile(fileName);

                // [핵심] 미리보기(Thumbnail) 생성 로직
                if (File.Exists(fullPath))
                {
                    try
                    {
                        // 파일을 직접 물고 있지 않게 하기 위해 메모리 스트림으로 복사하여 로드
                        using (var stream = new MemoryStream(File.ReadAllBytes(fullPath)))
                        {
                            Image original = Image.FromStream(stream);
                            // 64x64 크기의 썸네일 생성하여 메뉴 아이콘에 할당
                            item.Image = original.GetThumbnailImage(64, 64, null, IntPtr.Zero);
                        }
                    }
                    catch { /* 이미지 손상 시 아이콘 생략 */ }
                }

                menu.Items.Add(item);
            }

            menu.Show(Cursor.Position);
        }

        /// <summary>
        /// 공통 이미지 실행 로직
        /// </summary>
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
                MessageBox.Show("File not found: " + fileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        /// <summary>
        /// [핵심 기능] 항목 삭제 시 물리적 이미지 파일도 함께 삭제를 시도합니다.
        /// </summary>
        private void DeleteSelectedEntries()
        {
            var selected = OlvWorkLog.SelectedObjects?.Cast<WorkLogEntry>().ToList();
            if (selected == null || selected.Count == 0) return;

            var result = MessageBox.Show($"Are you sure you want to delete {selected.Count} selected item(s)?\n(Connected image files will also be deleted.)",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            List<string> failedFiles = new List<string>();
            string imgDir = Path.Combine(Path.GetDirectoryName(_dataPath), "WorkLog_Images");

            foreach (var entry in selected)
            {
                foreach (var fileName in entry.ImagePaths)
                {
                    try
                    {
                        string fullPath = Path.Combine(imgDir, fileName);
                        // 인터락: 파일이 사진 앱 등으로 열려 있어 삭제가 안 될 때 예외 처리
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                    }
                    catch
                    {
                        failedFiles.Add(fileName); // 삭제 실패 리스트 수집
                    }
                }
                _items.Remove(entry);
                OlvWorkLog.RemoveObject(entry);
            }

            // 삭제 실패 시 유저에게 알림 (고아 파일 방지)
            if (failedFiles.Count > 0)
            {
                string message = "Data was removed, but the following files could not be deleted (likely in use):\n\n" + string.Join("\n", failedFiles);
                MessageBox.Show(message, "Partial Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            SaveData();
        }

        /// <summary>
        /// 셀 편집 시작 시 콤보박스나 날짜 선택기 등을 생성합니다.
        /// </summary>
        private void OlvWorkLog_CellEditStarting(object sender, CellEditEventArgs e)
        {
            if (e.Column == null) return;

            // 1. [인터락] 이미지 열은 텍스트 편집 기능을 아예 차단 (가장 먼저 체크)
            if (e.Column.AspectName == "ImagePaths" || e.Column.Text == "Images")
            {
                e.Cancel = true;
                return;
            }

            var aspect = e.Column.AspectName;

            // 2. 상태(Status) 열: 콤보박스 생성
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
                e.Control = cb;
            }
            // 3. 날짜(Date) 열: 날짜 선택기 생성
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

        /// <summary>
        /// 날짜 선택기가 열려 있는 동안은 편집이 종료되지 않도록 방어합니다.
        /// </summary>
        private void OlvWorkLog_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            if (e.Column == null || e.RowObject == null) return;
            var entry = (WorkLogEntry)e.RowObject;
            var aspect = e.Column.AspectName;

            // 1. 날짜 피커 열려있으면 취소
            if (aspect == nameof(WorkLogEntry.Date) && _isDatePickerDropDownOpen)
            {
                e.Cancel = true;
                return;
            }

            // 2. 컨트롤로부터 최신 값 추출
            if (e.Control is ComboBox cb) e.NewValue = cb.SelectedItem?.ToString();
            else if (e.Control is DateTimePicker dtp) e.NewValue = dtp.Value;
            // 일반 TextBox는 이미 e.NewValue에 들어있음

            // 3. [Abnormal 대응] 모델에 강제 쓰기 (이 코드가 없으면 Tags 등이 null 될 수 있음)
            if (!e.Cancel && e.NewValue != null)
            {
                if (aspect == nameof(WorkLogEntry.Tags)) entry.Tags = e.NewValue.ToString();
                else if (aspect == nameof(WorkLogEntry.Status)) entry.Status = e.NewValue.ToString();
                else if (aspect == nameof(WorkLogEntry.Title)) entry.Title = e.NewValue.ToString();
                else if (aspect == nameof(WorkLogEntry.Content)) entry.Content = e.NewValue.ToString();
                else if (aspect == nameof(WorkLogEntry.Memo)) entry.Memo = e.NewValue.ToString();
                // 날짜는 Finished에서 처리하거나 여기서 직접 캐스팅하여 할당
            }
        }

        /// <summary>
        /// 편집 완료 후 수정 시간을 갱신하고 데이터를 저장합니다.
        /// </summary>
        private void OlvWorkLog_CellEditFinished(object sender, CellEditEventArgs e)
        {
            if (e.Cancel || !(e.RowObject is WorkLogEntry entry)) return;
            entry.Touch();
            OlvWorkLog.RefreshObject(entry);
            SaveData();
        }

        /// <summary>
        /// 검색창 텍스트 변경 시 실시간 필터링을 수행합니다.
        /// </summary>
        private void TxtWorkLog_TextChanged(object sender, EventArgs e) => ApplyFilter(TxtWorkLog.Text);

        /// <summary>
        /// 검색창에서 엔터 키 입력 시 필터링을 강제 수행합니다.
        /// </summary>
        private void TxtWorkLog_KeyUp(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) ApplyFilter(TxtWorkLog.Text); }

        /// <summary>
        /// 입력된 쿼리에 맞춰 리스트뷰 목록을 필터링합니다.
        /// </summary>
        private void ApplyFilter(string q)
        {
            _currentFilter = q?.Trim() ?? "";
            OlvWorkLog.BeginUpdate();
            OlvWorkLog.SetObjects(string.IsNullOrEmpty(_currentFilter) ? _items : _items.Where(RowMatchesFilter).ToList());
            OlvWorkLog.EndUpdate();
        }

        /// <summary>
        /// 행 데이터가 현재 필터 쿼리와 일치하는지 판별합니다.
        /// </summary>
        private bool RowMatchesFilter(WorkLogEntry entry)
        {
            string q = _currentFilter.ToLower();
            return (entry.Title?.ToLower().Contains(q) ?? false) ||
                   (entry.Content?.ToLower().Contains(q) ?? false) ||
                   (entry.Tags?.ToLower().Contains(q) ?? false) ||
                   (entry.Memo?.ToLower().Contains(q) ?? false);
        }

        /// <summary>
        /// 새로운 로그 항목을 생성하고 리스트 최하단에 추가합니다.
        /// </summary>
        private void AddNewEntry()
        {
            int nextNo = _items.Count == 0 ? 1 : _items.Max(x => x.No) + 1;
            var entry = new WorkLogEntry { No = nextNo, Date = DateTime.Now };
            _items.Add(entry);
            OlvWorkLog.AddObject(entry);
            OlvWorkLog.SelectObject(entry, true);
            SaveData();
        }

        /// <summary>
        /// 폼 종료 시 리스트뷰 자원을 해제합니다.
        /// </summary>
        private void WorkLogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            OlvWorkLog.Parent = null;
            OlvWorkLog.Dispose();
        }

        // --- 커스텀 드로잉 로직 (UI 커스터마이징) ---

        private void OlvWorkLog_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        { e.DrawBackground(); TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.Black, TextFormatFlags.VerticalCenter); }

        private void OlvWorkLog_DrawItem(object sender, DrawListViewItemEventArgs e) => e.DrawBackground();

        /// <summary>
        /// 검색어가 포함된 텍스트를 Bold체로 강조하여 출력합니다.
        /// </summary>
        private void OlvWorkLog_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            bool highlight = !string.IsNullOrEmpty(_currentFilter) && e.SubItem.Text.ToLower().Contains(_currentFilter.ToLower());
            Font f = highlight ? new Font(e.SubItem.Font, FontStyle.Bold) : e.SubItem.Font;
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, f, e.Bounds, e.SubItem.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void OlvWorkLog_FormatCell(object sender, FormatCellEventArgs e) { }

        /// <summary>
        /// 상태(Status) 값에 따라 행의 배경색을 다르게 표시합니다.
        /// </summary>
        private void OlvWorkLog_FormatRow(object sender, FormatRowEventArgs e)
        {
            if (e.Model is WorkLogEntry entry)
            {
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
            }
        }

        /// <summary>
        /// Adjusts the font size of the list view and synchronizes column headers.
        /// </summary>
        /// <param name="delta">The amount to change the font size by.</param>
        private void ChangeFontSize(float delta)
        {
            // 8pt ~ 24pt 사이로 제한
            _currentFontSize = Math.Max(8f, Math.Min(24f, _currentFontSize + delta));
            Font nFont = new Font("맑은 고딕", _currentFontSize);

            OlvWorkLog.Font = nFont;

            // 헤더 폰트도 함께 변경
            foreach (OLVColumn col in OlvWorkLog.AllColumns)
            {
                try { col.HeaderFont = nFont; } catch { }
            }

            // 행 높이를 폰트 크기에 맞춰 자동 조절 (ObjectListView 전용)
            OlvWorkLog.RowHeight = (int)(_currentFontSize * 2.0);

            OlvWorkLog.Refresh();
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            ChangeFontSize(1f);
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            ChangeFontSize(-1f);
        }

    } ////////////////////////////////////////////////////////////////////////////////////////////       클래스 끝

    /// <summary>
    /// 개별 로그 항목 데이터를 담는 데이터 클래스입니다.
    /// </summary>
    [DataContract]
    public class WorkLogEntry
    {
        [DataMember] public int No { get; set; }
        [DataMember] public DateTime Date { get; set; } = DateTime.Now;
        [DataMember] public string Title { get; set; } = "";
        [DataMember] public string Content { get; set; } = "";
        [DataMember] public string Status { get; set; } = "OPEN";
        [DataMember] public string Tags { get; set; } = "";
        [DataMember] public string Memo { get; set; } = "";
        [DataMember] public DateTime LastUpdated { get; set; } = DateTime.Now;
        [DataMember] public List<string> ImagePaths { get; set; } = new List<string>();

        // 인터락 포인트: 이미지 존재 여부 확인 프로퍼티
        public bool HasImage => ImagePaths != null && ImagePaths.Count > 0;

        // 데이터 변경 시 수정 시간을 갱신합니다.
        public void Touch() => LastUpdated = DateTime.Now;


        public WorkLogEntry()
        {
            Title = "";
            Content = "";
            Status = "OPEN";
            Tags = "";
            Memo = "";
            ImagePaths = new List<string>();
        }
    }

    /// <summary>
    /// JSON 저장 및 폰트 설정을 포함하는 전체 데이터 구조입니다.
    /// </summary>
    public class WorkLogData
    {
        public float FontSize { get; set; }
        public List<WorkLogEntry> Items { get; set; }
    }
}