using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    public partial class WorkLogForm : MaterialForm
    {
        private readonly WorkLogRepository _repo = new WorkLogRepository();
        private readonly WorkLogService _service = new WorkLogService();
        private readonly MaterialSkinManager _materialSkinManager;

        private List<WorkLogEntry> _items = new List<WorkLogEntry>();
        private WorkLogData _data;

        private WorkLogEntry _selectedEntry = null;
        private int _currentImageIndex = -1;
        private Image _currentViewerImage = null;

        private string _currentSearchQuery = string.Empty;
        private string _quickFilter = "ALL"; // ALL, ING, OPEN, TODAY, HIGH
        private bool _isBinding = false;

        private Timer _searchDebounceTimer;
        private Timer _saveDebounceTimer;
        private DateTime _lastPasteTime = DateTime.MinValue;

        private ContextMenuStrip _menuStatus;
        private ContextMenuStrip _menuPriority;
        private ToolTip _toolTip;

        // GDI+ 캐시 브러시 & 폰트 (제로 리소스 준수)
        private Font _regularFont;
        private Font _boldFont;
        private float _detailFontSize = 12f;
        private Font _detailFont;
        private static readonly Brush _brushSelected = new SolidBrush(Color.FromArgb(38, 48, 64));
        private static readonly Brush _brushBadgeOpen = new SolidBrush(Color.FromArgb(10, 80, 110));
        private static readonly Brush _brushBadgeIng = new SolidBrush(Color.FromArgb(120, 85, 10));
        private static readonly Brush _brushBadgeDone = new SolidBrush(Color.FromArgb(15, 80, 45));
        private static readonly Brush _brushBadgeFixed = new SolidBrush(Color.FromArgb(70, 35, 110));

        // 행 배경 브러시 캐시 (ING: 연한 노란색 음영, DONE/FIXED: 연회색 음영)
        private static readonly Brush _brushRowDefault = new SolidBrush(Color.FromArgb(13, 17, 23));
        private static readonly Brush _brushRowIng = new SolidBrush(Color.FromArgb(46, 38, 16));
        private static readonly Brush _brushRowDone = new SolidBrush(Color.FromArgb(34, 38, 46));
        private static readonly Brush _brushSelectedDefault = new SolidBrush(Color.FromArgb(38, 48, 64));
        private static readonly Brush _brushSelectedIng = new SolidBrush(Color.FromArgb(64, 54, 28));
        private static readonly Brush _brushSelectedDone = new SolidBrush(Color.FromArgb(46, 52, 64));

        // 고유번호/업무제목 뱃지 브러시 & 펜 캐시 (네온 테두리 + 연한 파스텔 배경)
        private static readonly Brush _brushBadgePastelBg = new SolidBrush(Color.FromArgb(22, 44, 56));
        private static readonly Pen _penBadgeNeonBorder = new Pen(Color.FromArgb(0, 229, 255), 1f);

        public WorkLogForm()
        {
            InitializeComponent();
            _materialSkinManager = MaterialSkinManager.Instance;
            _materialSkinManager.AddFormToManage(this);

            this.Load += WorkLogForm_Load;
            this.FormClosing += WorkLogForm_FormClosing;
        }

        private void WorkLogForm_Load(object sender, EventArgs e)
        {
            InitializeLogFile();
            InitListView();
            InitTimers();
            InitDropdownMenus();
            InitToolTips();
            WireEvents();
            LoadData();

            lblTitleBadge.SendToBack();
            txtTitle.BringToFront();

            lblDetailNo.Paint += BadgeLabel_Paint;
            lblTitleBadge.Paint += BadgeLabel_Paint;

            // MaterialSkin 매니저의 폰트 덮어쓰기 방지: 본문 및 제목 폰트 적용
            UpdateDetailFontSize(_data?.DetailFontSize > 0 ? _data.DetailFontSize : 12F);
            txtTitle.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
        }

        private void InitTimers()
        {
            _searchDebounceTimer = new Timer { Interval = 250 };
            _searchDebounceTimer.Tick += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                ApplyFilter();
            };

            _saveDebounceTimer = new Timer { Interval = 600 };
            _saveDebounceTimer.Tick += (s, e) =>
            {
                _saveDebounceTimer.Stop();
                SaveData();
            };
        }

        private void InitDropdownMenus()
        {
            // 1. 상태(Status) 드롭다운 메뉴
            _menuStatus = new ContextMenuStrip();
            _menuStatus.BackColor = Color.FromArgb(22, 27, 34);
            _menuStatus.ForeColor = Color.FromArgb(230, 237, 243);
            _menuStatus.ShowImageMargin = false;
            _menuStatus.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);

            var statuses = new[]
            {
                new { Key = "OPEN", Text = "⏳  OPEN  (대기)", Color = Color.FromArgb(0, 229, 255) },
                new { Key = "ING..", Text = "🔥  ING..  (진행 중)", Color = Color.FromArgb(255, 193, 7) },
                new { Key = "DONE", Text = "✅  DONE  (완료)", Color = Color.FromArgb(0, 230, 118) },
                new { Key = "FIXED", Text = "🟣  FIXED (수정/검증)", Color = Color.FromArgb(187, 134, 252) }
            };

            foreach (var st in statuses)
            {
                var item = new ToolStripMenuItem(st.Text)
                {
                    ForeColor = st.Color,
                    BackColor = Color.FromArgb(22, 27, 34),
                    Padding = new Padding(10, 6, 10, 6)
                };
                string key = st.Key;
                item.Click += (s, e) => SetEntryStatus(key);
                _menuStatus.Items.Add(item);
            }

            // 2. 중요도(Priority) 드롭다운 메뉴
            _menuPriority = new ContextMenuStrip();
            _menuPriority.BackColor = Color.FromArgb(22, 27, 34);
            _menuPriority.ForeColor = Color.FromArgb(230, 237, 243);
            _menuPriority.ShowImageMargin = false;
            _menuPriority.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);

            var priorities = new[]
            {
                new { Key = "HIGH", Text = "🔥  긴급 (HIGH)", Color = Color.FromArgb(255, 82, 82) },
                new { Key = "NORMAL", Text = "⚡  보통 (NORMAL)", Color = Color.FromArgb(88, 166, 255) },
                new { Key = "LOW", Text = "☕  낮음 (LOW)", Color = Color.FromArgb(140, 160, 180) }
            };

            foreach (var pr in priorities)
            {
                var item = new ToolStripMenuItem(pr.Text)
                {
                    ForeColor = pr.Color,
                    BackColor = Color.FromArgb(22, 27, 34),
                    Padding = new Padding(10, 6, 10, 6)
                };
                string key = pr.Key;
                item.Click += (s, e) => SetEntryPriority(key);
                _menuPriority.Items.Add(item);
            }
        }

        private void InitToolTips()
        {
            _toolTip = new ToolTip
            {
                BackColor = Color.FromArgb(22, 27, 34),
                ForeColor = Color.FromArgb(230, 237, 243),
                AutoPopDelay = 5000,
                InitialDelay = 300,
                ReshowDelay = 150
            };

            _toolTip.SetToolTip(btnFilterAll, "전체 업무 로그를 표시합니다.");
            _toolTip.SetToolTip(btnFilterIng, "현재 진행 중(ING..)인 업무만 필터링합니다.");
            _toolTip.SetToolTip(btnFilterOpen, "아직 착수하지 않은 대기(OPEN) 업무만 필터링합니다.");
            _toolTip.SetToolTip(btnFilterToday, "오늘 등록되었거나 오늘 작업/수정된 업무만 필터링합니다.");
            _toolTip.SetToolTip(btnFilterHigh, "중요도가 '🔥 긴급(HIGH)'인 업무만 필터링합니다.");
            _toolTip.SetToolTip(chkHideDone, "완료(DONE/FIXED)된 업무를 목록에서 숨깁니다.");
            _toolTip.SetToolTip(btnStatusToggle, "클릭하여 업무 상태(대기/진행/완료/수정)를 드롭다운에서 선택합니다.");
            _toolTip.SetToolTip(btnPriorityToggle, "클릭하여 업무 중요도(긴급/보통/낮음)를 드롭다운에서 선택합니다.");
            _toolTip.SetToolTip(lblDetailNo, "업무 고유 번호 (#No)");
            _toolTip.SetToolTip(lblTitleBadge, "업무 제목");
            _toolTip.SetToolTip(btnSaveEntry, "현재 업무 로그를 즉시 저장합니다. (평상시에는 자동 저장됩니다)");
            _toolTip.SetToolTip(btnDeleteEntry, "현재 선택된 업무 항목과 첨부된 이미지를 삭제합니다.");
            _toolTip.SetToolTip(btnDetailZoomIn, "상세 본문/메모 글자 크기 확대 (Ctrl + 휠 위)");
            _toolTip.SetToolTip(btnDetailZoomOut, "상세 본문/메모 글자 크기 축소 (Ctrl + 휠 아래)");
        }

        private void InitListView()
        {
            OlvWorkLog.FullRowSelect = true;
            OlvWorkLog.ShowGroups = false;
            OlvWorkLog.UseFiltering = true;
            OlvWorkLog.OwnerDraw = true;
            OlvWorkLog.HeaderUsesThemes = false;
            OlvWorkLog.ShowItemToolTips = false;
            OlvWorkLog.RowHeight = 32;

            colNo.AspectGetter = row => $"#{((WorkLogEntry)row).No}";
            colNo.Width = 36;
            colStatus.Width = 66;

            colPriority.AspectGetter = row =>
            {
                var entry = (WorkLogEntry)row;
                switch (entry.Priority)
                {
                    case "HIGH": return "🔥긴급";
                    case "LOW": return "☕낮음";
                    default: return "⚡보통";
                }
            };
            colPriority.Width = 70;

            colTitle.FillsFreeSpace = true;
            colTitle.MinimumWidth = 200;
            colTitle.Width = 240;

            colImages.AspectGetter = row =>
            {
                var entry = (WorkLogEntry)row;
                return entry.HasImage ? $"📸 {entry.ImagePaths.Count}" : "";
            };
            colImages.Width = 40;

            colDate.AspectToStringFormat = "{0:yyyy-MM-dd}";
            colDate.Width = 95;
            colDate.TextAlign = HorizontalAlignment.Center;

            // 커스텀 렌더링
            OlvWorkLog.DrawColumnHeader += (s, e) =>
            {
                using (var bgBrush = new SolidBrush(Color.FromArgb(22, 27, 34)))
                using (var textBrush = new SolidBrush(Color.FromArgb(200, 210, 225)))
                {
                    e.Graphics.FillRectangle(bgBrush, e.Bounds);
                    TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds,
                        Color.FromArgb(200, 210, 225), TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            };

            OlvWorkLog.DrawItem += (s, e) => e.DrawBackground();

            OlvWorkLog.DrawSubItem += (s, e) =>
            {
                if (!(e.Item is OLVListItem olvItem) || !(olvItem.RowObject is WorkLogEntry entry))
                {
                    e.DrawDefault = true;
                    return;
                }

                bool isSelected = e.Item.Selected;
                var g = e.Graphics;

                // 배경 칠하기 (ING: 연한 노란색 음영, DONE/FIXED: 연회색 음영)
                Brush bgBrush;
                if (isSelected)
                {
                    if (entry.Status == "ING..")
                        bgBrush = _brushSelectedIng;
                    else if (entry.Status == "DONE" || entry.Status == "FIXED")
                        bgBrush = _brushSelectedDone;
                    else
                        bgBrush = _brushSelectedDefault;
                }
                else
                {
                    if (entry.Status == "ING..")
                        bgBrush = _brushRowIng;
                    else if (entry.Status == "DONE" || entry.Status == "FIXED")
                        bgBrush = _brushRowDone;
                    else
                        bgBrush = _brushRowDefault;
                }
                g.FillRectangle(bgBrush, e.Bounds);

                var col = OlvWorkLog.GetColumn(e.ColumnIndex);
                if (col == colStatus)
                {
                    // 상태 뱃지 렌더링
                    DrawStatusBadge(g, entry.Status, e.Bounds);
                }
                else if (col == colPriority)
                {
                    // 우선순위 렌더링
                    Color pColor = entry.Priority == "HIGH" ? Color.FromArgb(255, 82, 82) :
                                   entry.Priority == "LOW" ? Color.FromArgb(120, 140, 160) :
                                   Color.FromArgb(200, 215, 230);
                    TextRenderer.DrawText(g, colPriority.GetStringValue(entry), e.SubItem.Font, e.Bounds,
                        pColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                else if (col == colNo)
                {
                    TextRenderer.DrawText(g, $"#{entry.No}", e.SubItem.Font, e.Bounds,
                        Color.FromArgb(0, 229, 255), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                else if (col == colTitle)
                {
                    Color titleColor = entry.Status == "DONE" || entry.Status == "FIXED"
                        ? Color.FromArgb(140, 150, 160)
                        : (entry.Status == "ING.." ? Color.FromArgb(255, 245, 220) : Color.FromArgb(240, 246, 252));

                    TextRenderer.DrawText(g, entry.Title, isSelected ? _boldFont : _regularFont, e.Bounds,
                        titleColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                }
                else if (col == colImages)
                {
                    if (entry.HasImage)
                    {
                        TextRenderer.DrawText(g, $"📸{entry.ImagePaths.Count}", e.SubItem.Font, e.Bounds,
                            Color.FromArgb(255, 215, 0), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                    }
                }
                else if (col == colDate)
                {
                    Color dateColor = entry.Status == "DONE" || entry.Status == "FIXED"
                        ? Color.FromArgb(120, 130, 140)
                        : Color.FromArgb(180, 195, 210);
                    string dtText = entry.Date.ToString("yyyy-MM-dd");
                    TextRenderer.DrawText(g, dtText, e.SubItem.Font, e.Bounds,
                        dateColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
                else
                {
                    Color foreColor = entry.Status == "DONE" ? Color.FromArgb(120, 130, 140) : Color.FromArgb(180, 190, 205);
                    TextRenderer.DrawText(g, e.SubItem.Text, e.SubItem.Font, e.Bounds,
                        foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                }
            };
        }

        private void DrawStatusBadge(Graphics g, string status, Rectangle bounds)
        {
            var badgeRect = new Rectangle(bounds.X + 4, bounds.Y + 5, bounds.Width - 8, bounds.Height - 10);
            Brush bg;
            Color textCol;

            switch (status)
            {
                case "ING..":
                    bg = _brushBadgeIng;
                    textCol = Color.FromArgb(255, 193, 7);
                    break;
                case "DONE":
                    bg = _brushBadgeDone;
                    textCol = Color.FromArgb(0, 230, 118);
                    break;
                case "FIXED":
                    bg = _brushBadgeFixed;
                    textCol = Color.FromArgb(187, 134, 252);
                    break;
                case "OPEN":
                default:
                    bg = _brushBadgeOpen;
                    textCol = Color.FromArgb(0, 229, 255);
                    break;
            }

            g.FillRectangle(bg, badgeRect);
            using (var borderPen = new Pen(textCol, 1f))
            {
                g.DrawRectangle(borderPen, badgeRect);
            }
            TextRenderer.DrawText(g, status, _boldFont, badgeRect, textCol,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        private void BadgeLabel_Paint(object sender, PaintEventArgs e)
        {
            if (!(sender is Label lbl)) return;

            var g = e.Graphics;
            var rect = new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1);

            // 1. 연한 파스텔 시안 배경색
            g.FillRectangle(_brushBadgePastelBg, rect);

            // 2. 다른 네온 버튼들과 통일된 1px 네온 시안 테두리
            g.DrawRectangle(_penBadgeNeonBorder, rect);

            // 3. 텍스트 중앙 렌더링
            TextRenderer.DrawText(g, lbl.Text, lbl.Font, rect, lbl.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void WireEvents()
        {
            OlvWorkLog.SelectionChanged += OlvWorkLog_SelectionChanged;
            txtSearch.TextChanged += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            };

            // 편의 단축키: 검색창에서 ESC 입력 시 검색어 즉시 초기화
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape && !string.IsNullOrEmpty(txtSearch.Text))
                {
                    txtSearch.Text = "";
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            // 편의 단축키: 제목 입력 후 Enter 입력 시 맑은 고딕 본문(상세 내용)으로 자동 포커스 이동
            txtTitle.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    txtContent.Focus();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            // 마우스 휠 본문 글자 크기 줌 (Ctrl + 마우스 휠)
            txtContent.MouseWheel += (s, e) =>
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    if (e.Delta > 0) UpdateDetailFontSize(_detailFontSize + 1f);
                    else if (e.Delta < 0) UpdateDetailFontSize(_detailFontSize - 1f);
                    SaveData();
                    if (e is HandledMouseEventArgs hme) hme.Handled = true;
                }
            };

            txtMemo.MouseWheel += (s, e) =>
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    if (e.Delta > 0) UpdateDetailFontSize(_detailFontSize + 1f);
                    else if (e.Delta < 0) UpdateDetailFontSize(_detailFontSize - 1f);
                    SaveData();
                    if (e is HandledMouseEventArgs hme) hme.Handled = true;
                }
            };

            // 스크린샷 드래그 앤 드롭 지원
            zoomViewer.AllowDrop = true;
            zoomViewer.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length > 0)
                    {
                        string ext = Path.GetExtension(files[0])?.ToLower();
                        if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif")
                        {
                            e.Effect = DragDropEffects.Copy;
                            return;
                        }
                    }
                }
                e.Effect = DragDropEffects.None;
            };

            zoomViewer.DragDrop += async (s, e) =>
            {
                if (_selectedEntry == null)
                {
                    MessageBox.Show("스크린샷을 첨부할 업무 항목을 먼저 선택해주세요.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length > 0)
                    {
                        foreach (string file in files)
                        {
                            string ext = Path.GetExtension(file)?.ToLower();
                            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif")
                            {
                                try
                                {
                                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                                    using (var orig = Image.FromStream(fs))
                                    using (var safeBmp = new Bitmap(orig))
                                    {
                                        int nextIdx = _selectedEntry.ImagePaths.Count + 1;
                                        string savedName = await _repo.SaveImageAsync(safeBmp, _selectedEntry.No, nextIdx);
                                        if (savedName != null)
                                        {
                                            _selectedEntry.ImagePaths.Add(savedName);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogException(ex, Level.Error);
                                }
                            }
                        }
                        _selectedEntry.Touch();
                        OlvWorkLog.RefreshObject(_selectedEntry);
                        BuildThumbnailStrip();
                        LoadImageToViewer(_selectedEntry.ImagePaths.Count - 1);
                        SaveData();
                    }
                }
            };

            // 폼 전역 키다운 (Ctrl+V 이미지 붙여넣기, Ctrl+S 즉시 저장)
            this.KeyPreview = true;
            this.KeyDown += WorkLogForm_KeyDown;
            zoomViewer.KeyDown += WorkLogForm_KeyDown;
            OlvWorkLog.KeyDown += WorkLogForm_KeyDown;
        }

        private void WorkLogForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                PasteClipboardImage();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveData();
                e.Handled = true;
            }
        }

        // --- 데이터 로드 & 저장 ---

        private void LoadData()
        {
            try
            {
                _data = _repo.Load();
                _items = _data.Items ?? new List<WorkLogEntry>();

                // 최초 실행 또는 데이터가 비어있을 때 이해하기 쉬운 고품질 가이드/샘플 데이터 생성
                if (_items.Count == 0)
                {
                    _items = _service.CreateSampleEntries();
                }
                _data.Items = _items;

                // 스플리터 위치 복원 (최소 560px 확보하여 좌측 그리드의 연-월-일/제목/중요도가 시원하게 표시되도록 보정)
                if (_data.SplitterDistance < 560) _data.SplitterDistance = 580;
                if (_data.SplitterDistance > splitMain.Width - 560)
                    _data.SplitterDistance = Math.Max(480, splitMain.Width - 560);
                if (_data.SplitterDistance >= 480 && _data.SplitterDistance < splitMain.Width - 200)
                    splitMain.SplitterDistance = _data.SplitterDistance;

                // 상하 스플리터 위치 복원 (본문 높이 140px로 최적화)
                if (_data.DetailSplitterDistance > 200 || _data.DetailSplitterDistance < 90)
                    _data.DetailSplitterDistance = 140;
                splitDetail.SplitterDistance = _data.DetailSplitterDistance;

                _quickFilter = string.IsNullOrEmpty(_data.QuickFilter) ? "ALL" : _data.QuickFilter;
                chkHideDone.Checked = _data.HideDone;

                UpdateFontSize(_data.FontSize);
                UpdateDetailFontSize(_data.DetailFontSize > 0 ? _data.DetailFontSize : 12f);
                UpdateQuickFilterButtonsUI();
                UpdateStatsHud();
                ApplyFilter();

                // 첫 항목 선택 및 상세 바인딩
                if (OlvWorkLog.GetItemCount() > 0)
                {
                    var first = OlvWorkLog.GetModelObject(0) as WorkLogEntry;
                    OlvWorkLog.SelectObject(first);
                    if (first != null) BindDetailView(first);
                    else ClearDetailView();
                }
                else
                {
                    ClearDetailView();
                }

                LogMessage($"WorkLog Studio Loaded. Items: {_items.Count}", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
        }

        private void SaveData()
        {
            if (_data == null) _data = new WorkLogData();
            _data.Items = _items;
            _data.HideDone = chkHideDone.Checked;
            _data.QuickFilter = _quickFilter;
            _data.SplitterDistance = splitMain.SplitterDistance;
            _data.DetailSplitterDistance = splitDetail.SplitterDistance;
            _data.DetailFontSize = _detailFontSize;

            _repo.Save(_data);
        }

        private void UpdateStatsHud()
        {
            var stats = _service.GetStats(_items);
            lblStatsTotal.Text = $"전체: {stats.Total}건";
            lblStatsIng.Text = $"🔥 진행: {stats.Ing}건";
            lblStatsOpen.Text = $"⏳ 대기: {stats.Open}건";
            lblStatsDone.Text = $"✅ 완료: {stats.Done + stats.Fixed}건 ({stats.DonePercent}%)";
        }

        // --- 필터링 & 검색 ---

        private void ApplyFilter()
        {
            _currentSearchQuery = txtSearch.Text?.Trim() ?? "";
            OlvWorkLog.BeginUpdate();
            try
            {
                var filtered = _service.FilterItems(_items, _currentSearchQuery, chkHideDone.Checked, _quickFilter);
                OlvWorkLog.SetObjects(filtered);
            }
            finally
            {
                OlvWorkLog.EndUpdate();
            }

            // 필터링 후 선택 유지
            if (_selectedEntry != null && OlvWorkLog.Objects != null)
            {
                var cur = OlvWorkLog.Objects.Cast<WorkLogEntry>().FirstOrDefault(x => x.No == _selectedEntry.No);
                if (cur != null)
                {
                    OlvWorkLog.SelectObject(cur);
                    OlvWorkLog.EnsureModelVisible(cur);
                    BindDetailView(cur);
                }
                else if (OlvWorkLog.GetItemCount() > 0)
                {
                    var first = OlvWorkLog.GetModelObject(0) as WorkLogEntry;
                    OlvWorkLog.SelectObject(first);
                    if (first != null) BindDetailView(first);
                    else ClearDetailView();
                }
                else
                {
                    ClearDetailView();
                }
            }
            else if (OlvWorkLog.GetItemCount() > 0)
            {
                var first = OlvWorkLog.GetModelObject(0) as WorkLogEntry;
                OlvWorkLog.SelectObject(first);
                if (first != null) BindDetailView(first);
                else ClearDetailView();
            }
            else
            {
                ClearDetailView();
            }
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            if (sender == btnFilterAll) _quickFilter = "ALL";
            else if (sender == btnFilterIng) _quickFilter = "ING";
            else if (sender == btnFilterOpen) _quickFilter = "OPEN";
            else if (sender == btnFilterToday) _quickFilter = "TODAY";
            else if (sender == btnFilterHigh) _quickFilter = "HIGH";

            UpdateQuickFilterButtonsUI();
            ApplyFilter();
        }

        private void UpdateQuickFilterButtonsUI()
        {
            Button[] btns = { btnFilterAll, btnFilterIng, btnFilterOpen, btnFilterToday, btnFilterHigh };
            string[] tags = { "ALL", "ING", "OPEN", "TODAY", "HIGH" };
            Color[] colors = {
                Color.FromArgb(200, 215, 230),
                Color.FromArgb(255, 193, 7),
                Color.FromArgb(0, 229, 255),
                Color.FromArgb(0, 230, 118),
                Color.FromArgb(255, 82, 82)
            };
            Color[] activeBgs = {
                Color.FromArgb(50, 70, 95),
                Color.FromArgb(55, 42, 18),
                Color.FromArgb(20, 50, 65),
                Color.FromArgb(20, 55, 35),
                Color.FromArgb(65, 25, 30)
            };
            Color[] normalBgs = {
                Color.FromArgb(30, 36, 44),
                Color.FromArgb(32, 28, 18),
                Color.FromArgb(16, 28, 36),
                Color.FromArgb(16, 32, 24),
                Color.FromArgb(36, 18, 22)
            };

            for (int i = 0; i < btns.Length; i++)
            {
                bool active = (_quickFilter == tags[i]);
                btns[i].BackColor = active ? activeBgs[i] : normalBgs[i];
                btns[i].FlatAppearance.BorderColor = active ? colors[i] : Color.FromArgb(colors[i].R / 2, colors[i].G / 2, colors[i].B / 2);
                btns[i].FlatAppearance.BorderSize = 1;
                btns[i].Font = new Font("맑은 고딕", 8.25F, active ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        private void chkHideDone_CheckedChanged(object sender, EventArgs e)
        {
            ApplyFilter();
            SaveData();
        }

        // --- 마스터-디테일 바인딩 ---

        private void OlvWorkLog_SelectionChanged(object sender, EventArgs e)
        {
            if (OlvWorkLog.SelectedObject is WorkLogEntry entry)
            {
                BindDetailView(entry);
            }
            else
            {
                ClearDetailView();
            }
        }

        private void BindDetailView(WorkLogEntry entry)
        {
            _isBinding = true;
            _selectedEntry = entry;

            SetDetailControlsEnabled(true);

            lblDetailNo.Text = $"#{entry.No}";
            txtTitle.Text = entry.Title;
            txtTags.Text = entry.Tags;
            txtContent.Text = entry.Content;
            txtMemo.Text = entry.Memo;
            dtpDate.Value = entry.Date;

            UpdateStatusButtonUI(entry.Status);
            UpdatePriorityButtonUI(entry.Priority);

            // 이미지 썸네일 스트립 및 뷰어 갱신
            BuildThumbnailStrip();

            _isBinding = false;
        }

        private void ClearDetailView()
        {
            _isBinding = true;
            _selectedEntry = null;

            lblDetailNo.Text = "#--";
            txtTitle.Text = "";
            txtTags.Text = "";
            txtContent.Text = "";
            txtMemo.Text = "";
            btnStatusToggle.Text = "[ - ▼ ]";
            btnStatusToggle.ForeColor = Color.FromArgb(120, 130, 140);
            btnStatusToggle.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 80);
            btnStatusToggle.BackColor = Color.FromArgb(22, 27, 34);

            btnPriorityToggle.Text = "⚡ - ▼";
            btnPriorityToggle.ForeColor = Color.FromArgb(120, 130, 140);
            btnPriorityToggle.FlatAppearance.BorderColor = Color.FromArgb(60, 70, 80);
            btnPriorityToggle.BackColor = Color.FromArgb(22, 27, 34);

            _currentViewerImage?.Dispose();
            _currentViewerImage = null;
            zoomViewer.ClearImage();
            flowThumbnails.Controls.Clear();
            flowThumbnails.Visible = false;
            lblImageTitle.Text = "🖼️ 캡처 (0장)";

            SetDetailControlsEnabled(false);
            _isBinding = false;
        }

        private void SetDetailControlsEnabled(bool enabled)
        {
            txtTitle.Enabled = enabled;
            txtContent.Enabled = enabled;
            txtMemo.Enabled = enabled;
            dtpDate.Enabled = enabled;
            btnStatusToggle.Enabled = enabled;
            btnPriorityToggle.Enabled = enabled;
            btnSaveEntry.Enabled = enabled;
            btnDeleteEntry.Enabled = enabled;
            btnPasteImage.Enabled = enabled;
        }

        private void UpdateStatusButtonUI(string status)
        {
            btnStatusToggle.Text = $"[ {status} ▼ ]";
            btnStatusToggle.FlatStyle = FlatStyle.Flat;
            btnStatusToggle.FlatAppearance.BorderSize = 1;

            switch (status)
            {
                case "ING..":
                    btnStatusToggle.ForeColor = Color.FromArgb(255, 193, 7);
                    btnStatusToggle.FlatAppearance.BorderColor = Color.FromArgb(255, 193, 7);
                    btnStatusToggle.BackColor = Color.FromArgb(46, 40, 24);
                    break;
                case "DONE":
                    btnStatusToggle.ForeColor = Color.FromArgb(0, 230, 118);
                    btnStatusToggle.FlatAppearance.BorderColor = Color.FromArgb(0, 230, 118);
                    btnStatusToggle.BackColor = Color.FromArgb(18, 42, 32);
                    break;
                case "FIXED":
                    btnStatusToggle.ForeColor = Color.FromArgb(187, 134, 252);
                    btnStatusToggle.FlatAppearance.BorderColor = Color.FromArgb(187, 134, 252);
                    btnStatusToggle.BackColor = Color.FromArgb(38, 24, 52);
                    break;
                case "OPEN":
                default:
                    btnStatusToggle.ForeColor = Color.FromArgb(0, 229, 255);
                    btnStatusToggle.FlatAppearance.BorderColor = Color.FromArgb(0, 229, 255);
                    btnStatusToggle.BackColor = Color.FromArgb(18, 40, 48);
                    break;
            }
        }

        private void UpdatePriorityButtonUI(string priority)
        {
            btnPriorityToggle.FlatStyle = FlatStyle.Flat;
            btnPriorityToggle.FlatAppearance.BorderSize = 1;

            switch (priority)
            {
                case "HIGH":
                    btnPriorityToggle.Text = "🔥 긴급 ▼";
                    btnPriorityToggle.ForeColor = Color.FromArgb(255, 82, 82);
                    btnPriorityToggle.FlatAppearance.BorderColor = Color.FromArgb(255, 82, 82);
                    btnPriorityToggle.BackColor = Color.FromArgb(51, 24, 28);
                    break;
                case "LOW":
                    btnPriorityToggle.Text = "☕ 낮음 ▼";
                    btnPriorityToggle.ForeColor = Color.FromArgb(140, 160, 180);
                    btnPriorityToggle.FlatAppearance.BorderColor = Color.FromArgb(80, 100, 120);
                    btnPriorityToggle.BackColor = Color.FromArgb(24, 30, 38);
                    break;
                case "NORMAL":
                default:
                    btnPriorityToggle.Text = "⚡ 보통 ▼";
                    btnPriorityToggle.ForeColor = Color.FromArgb(88, 166, 255);
                    btnPriorityToggle.FlatAppearance.BorderColor = Color.FromArgb(88, 166, 255);
                    btnPriorityToggle.BackColor = Color.FromArgb(24, 40, 56);
                    break;
            }
        }

        // --- 인라인 텍스트 및 속성 편집 동기화 ---

        private void txtDetail_TextChanged(object sender, EventArgs e)
        {
            if (_isBinding || _selectedEntry == null) return;

            _selectedEntry.Title = txtTitle.Text;
            _selectedEntry.Tags = txtTags.Text;
            _selectedEntry.Content = txtContent.Text;
            _selectedEntry.Memo = txtMemo.Text;
            _selectedEntry.Touch();

            OlvWorkLog.RefreshObject(_selectedEntry);
            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }

        private void dtpDate_ValueChanged(object sender, EventArgs e)
        {
            if (_isBinding || _selectedEntry == null) return;
            _selectedEntry.Date = dtpDate.Value;
            _selectedEntry.Touch();
            OlvWorkLog.RefreshObject(_selectedEntry);
            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }

        private void btnStatusToggle_Click(object sender, EventArgs e)
        {
            if (_selectedEntry == null) return;
            _menuStatus.Show(btnStatusToggle, 0, btnStatusToggle.Height);
        }

        private void SetEntryStatus(string newStatus)
        {
            if (_selectedEntry == null) return;

            _selectedEntry.Status = newStatus;
            _selectedEntry.Touch();
            UpdateStatusButtonUI(newStatus);

            OlvWorkLog.RefreshObject(_selectedEntry);
            UpdateStatsHud();
            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }

        private void btnPriorityToggle_Click(object sender, EventArgs e)
        {
            if (_selectedEntry == null) return;
            _menuPriority.Show(btnPriorityToggle, 0, btnPriorityToggle.Height);
        }

        private void SetEntryPriority(string newPriority)
        {
            if (_selectedEntry == null) return;

            _selectedEntry.Priority = newPriority;
            _selectedEntry.Touch();
            UpdatePriorityButtonUI(newPriority);

            OlvWorkLog.RefreshObject(_selectedEntry);
            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }

        private void btnSaveEntry_Click(object sender, EventArgs e)
        {
            SaveData();
            MessageBox.Show("업무 로그가 안전하게 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDeleteEntry_Click(object sender, EventArgs e)
        {
            if (_selectedEntry == null) return;

            var res = MessageBox.Show($"정말 [#{_selectedEntry.No} - {_selectedEntry.Title}] 항목을 삭제하시겠습니까?\n첨부된 스크린샷 이미지도 함께 정리됩니다.",
                "업무 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (res != DialogResult.Yes) return;

            // 연결된 이미지 삭제
            if (_selectedEntry.ImagePaths != null && _selectedEntry.ImagePaths.Count > 0)
            {
                _repo.DeleteImages(_selectedEntry.ImagePaths);
            }

            _items.Remove(_selectedEntry);
            OlvWorkLog.RemoveObject(_selectedEntry);
            SaveData();
            UpdateStatsHud();

            if (OlvWorkLog.GetItemCount() > 0)
            {
                var next = OlvWorkLog.GetModelObject(0) as WorkLogEntry;
                OlvWorkLog.SelectObject(next);
                if (next != null) BindDetailView(next);
                else ClearDetailView();
            }
            else
            {
                ClearDetailView();
            }
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            // 새 업무 등록 시 현재 필터로 인해 새로 생성된 항목이 목록에서 가려지는 현상 원천 방지
            if (_quickFilter != "ALL")
            {
                _quickFilter = "ALL";
                UpdateQuickFilterButtonsUI();
            }
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                txtSearch.Text = "";
            }

            var newEntry = _service.CreateNewEntry(_items);
            _items.Add(newEntry);
            SaveData();
            UpdateStatsHud();
            ApplyFilter();

            int newIdx = OlvWorkLog.IndexOf(newEntry);
            if (newIdx >= 0)
            {
                OlvWorkLog.SelectedIndex = newIdx;
            }
            else
            {
                OlvWorkLog.SelectObject(newEntry);
            }
            OlvWorkLog.EnsureModelVisible(newEntry);
            BindDetailView(newEntry);
            txtTitle.Focus();
            txtTitle.SelectAll();
        }

        // --- 스크린샷 & 고화질 딥 줌 뷰어 시스템 ---

        private void BuildThumbnailStrip()
        {
            // 기존 썸네일 컨트롤 안전 정리
            foreach (Control c in flowThumbnails.Controls)
            {
                if (c is Button btn && btn.Image != null)
                {
                    btn.Image.Dispose();
                }
                c.Dispose();
            }
            flowThumbnails.Controls.Clear();

            if (_selectedEntry == null || _selectedEntry.ImagePaths == null || _selectedEntry.ImagePaths.Count == 0)
            {
                lblImageTitle.Text = "🖼️ 캡처 (0장)";
                _currentViewerImage?.Dispose();
                _currentViewerImage = null;
                zoomViewer.ClearImage();
                _currentImageIndex = -1;
                flowThumbnails.Visible = false;
                return;
            }

            int count = _selectedEntry.ImagePaths.Count;
            lblImageTitle.Text = $"🖼️ 캡처 ({count}장)";
            flowThumbnails.Visible = (count > 1); // 1장 이하일 때는 하단 바 숨겨서 세로 높이 극대화


            for (int i = 0; i < count; i++)
            {
                int idx = i;
                string fileName = _selectedEntry.ImagePaths[i];
                string fullPath = _repo.GetFullImagePath(fileName);

                var btnThumb = new Button
                {
                    Size = new Size(54, 42),
                    Margin = new Padding(2, 2, 4, 2),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = (i == 0) ? Color.FromArgb(0, 120, 215) : Color.FromArgb(30, 36, 44),
                    Cursor = Cursors.Hand,
                    Text = $"#{i + 1}",
                    ForeColor = Color.White,
                    Font = new Font("맑은 고딕", 7.5F, FontStyle.Bold),
                    ImageAlign = ContentAlignment.MiddleCenter,
                    TextImageRelation = TextImageRelation.Overlay
                };
                btnThumb.FlatAppearance.BorderSize = 1;
                btnThumb.FlatAppearance.BorderColor = Color.FromArgb(50, 60, 75);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        using (var stream = new MemoryStream(File.ReadAllBytes(fullPath)))
                        using (var orig = Image.FromStream(stream))
                        {
                            btnThumb.Image = orig.GetThumbnailImage(50, 38, null, IntPtr.Zero);
                        }
                    }
                    catch { }
                }

                btnThumb.Click += (s, e) => LoadImageToViewer(idx);
                flowThumbnails.Controls.Add(btnThumb);
            }

            // 기본으로 첫 번째 이미지 로드
            LoadImageToViewer(0);
        }

        private void LoadImageToViewer(int index)
        {
            if (_selectedEntry == null || _selectedEntry.ImagePaths == null || index < 0 || index >= _selectedEntry.ImagePaths.Count)
            {
                _currentViewerImage?.Dispose();
                _currentViewerImage = null;
                zoomViewer.ClearImage();
                _currentImageIndex = -1;
                return;
            }

            _currentImageIndex = index;
            string fileName = _selectedEntry.ImagePaths[index];
            string fullPath = _repo.GetFullImagePath(fileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    _currentViewerImage?.Dispose();
                    // 원본 바이트 배열을 메모리로 읽어 독립적인 Bitmap 생성 (파일 락 방지)
                    using (var ms = new MemoryStream(File.ReadAllBytes(fullPath)))
                    {
                        _currentViewerImage = new Bitmap(Image.FromStream(ms));
                    }
                    zoomViewer.SetImage(_currentViewerImage, fullPath, autoFit: true);
                }
                catch (Exception ex)
                {
                    LogException(ex, Level.Error);
                }
            }
            else
            {
                zoomViewer.ClearImage();
            }

            // 썸네일 버튼 하이라이트 갱신
            for (int i = 0; i < flowThumbnails.Controls.Count; i++)
            {
                if (flowThumbnails.Controls[i] is Button btn)
                {
                    btn.BackColor = (i == index) ? Color.FromArgb(0, 150, 255) : Color.FromArgb(30, 36, 44);
                }
            }
        }

        private async void PasteClipboardImage()
        {
            if (_selectedEntry == null)
            {
                MessageBox.Show("스크린샷을 첨부할 업무 항목을 먼저 선택해주세요.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if ((DateTime.Now - _lastPasteTime).TotalMilliseconds < 800) return;
            _lastPasteTime = DateTime.Now;

            if (!Clipboard.ContainsImage() && !Clipboard.ContainsFileDropList())
            {
                MessageBox.Show("클립보드에 이미지가 없습니다.\r\n화면 캡처(Win+Shift+S 등) 또는 이미지 파일 복사(Ctrl+C) 후 다시 붙여넣어 주세요.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Image clipImg = null;
                if (Clipboard.ContainsImage())
                {
                    clipImg = Clipboard.GetImage();
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    var files = Clipboard.GetFileDropList();
                    foreach (string f in files)
                    {
                        string ext = Path.GetExtension(f)?.ToLower();
                        if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif")
                        {
                            try
                            {
                                using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read))
                                using (var orig = Image.FromStream(fs))
                                {
                                    clipImg = new Bitmap(orig);
                                }
                                break;
                            }
                            catch { }
                        }
                    }
                }

                if (clipImg == null) return;
                Bitmap safeBmp = new Bitmap(clipImg);
                clipImg.Dispose();

                int nextIndex = _selectedEntry.ImagePaths.Count + 1;
                string fileName = await _repo.SaveImageAsync(safeBmp, _selectedEntry.No, nextIndex);
                safeBmp.Dispose();

                if (fileName != null)
                {
                    _selectedEntry.ImagePaths.Add(fileName);
                    _selectedEntry.Touch();

                    OlvWorkLog.RefreshObject(_selectedEntry);
                    BuildThumbnailStrip();
                    // 방금 추가된 마지막 이미지로 전환
                    LoadImageToViewer(_selectedEntry.ImagePaths.Count - 1);
                    SaveData();
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show("이미지 저장 중 오류가 발생했습니다: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnToggleText_Click(object sender, EventArgs e)
        {
            splitDetail.Panel1Collapsed = !splitDetail.Panel1Collapsed;
            btnToggleText.Text = splitDetail.Panel1Collapsed ? "▼ 본문 펼치기" : "▲ 본문 접기";
            btnToggleText.ForeColor = splitDetail.Panel1Collapsed ? Color.FromArgb(255, 193, 7) : Color.FromArgb(0, 229, 255);
        }

        private void btnPasteImage_Click(object sender, EventArgs e) => PasteClipboardImage();

        private void btnActualSize_Click(object sender, EventArgs e) => zoomViewer.ZoomToActualSize();

        private void btnFitScreen_Click(object sender, EventArgs e) => zoomViewer.FitToScreen();

        private void btnPopout_Click(object sender, EventArgs e)
        {
            if (_currentViewerImage == null || zoomViewer.CurrentImage == null)
            {
                MessageBox.Show("팝아웃할 스크린샷이 없습니다.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string title = _selectedEntry != null ? $"[#{_selectedEntry.No}] {_selectedEntry.Title}" : "";
            var popup = new WorkLogImagePopupForm(_currentViewerImage, zoomViewer.CurrentFilePath, title);
            popup.Show(this);
        }

        private void btnCopyImage_Click(object sender, EventArgs e) => zoomViewer.CopyToClipboard();

        private void btnOpenExternal_Click(object sender, EventArgs e) => zoomViewer.OpenInExternalViewer();

        private void btnDeleteImage_Click(object sender, EventArgs e)
        {
            if (_selectedEntry == null || _currentImageIndex < 0 || _currentImageIndex >= _selectedEntry.ImagePaths.Count)
                return;

            string fileName = _selectedEntry.ImagePaths[_currentImageIndex];
            var res = MessageBox.Show($"현재 스크린샷 ({fileName})을 삭제하시겠습니까?", "스크린샷 삭제", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            _repo.DeleteImages(new List<string> { fileName });
            _selectedEntry.ImagePaths.RemoveAt(_currentImageIndex);
            _selectedEntry.Touch();

            OlvWorkLog.RefreshObject(_selectedEntry);
            BuildThumbnailStrip();
            SaveData();
        }

        // --- 1-Click 일일 업무 보고서 생성기 ---

        private void btnDailyReport_Click(object sender, EventArgs e)
        {
            string report = _service.GenerateDailyReport(_items);
            Clipboard.SetText(report);
            MessageBox.Show("오늘의 일일 업무 보고서가 클립보드에 복사되었습니다!\r\n\r\n메신저(Slack, Teams, 잔디)나 메일에 바로 [Ctrl+V] 하세요.",
                "보고서 복사 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // --- 폰트 크기 조절 ---

        private void UpdateFontSize(float size)
        {
            if (_data == null) return;
            _data.FontSize = size;

            _regularFont?.Dispose();
            _boldFont?.Dispose();

            _regularFont = new Font("맑은 고딕", _data.FontSize, FontStyle.Regular);
            _boldFont = new Font("맑은 고딕", _data.FontSize, FontStyle.Bold);

            OlvWorkLog.Font = _regularFont;
            OlvWorkLog.RowHeight = (int)(_data.FontSize * 3.0);
            OlvWorkLog.Invalidate();
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            if (_data == null) return;
            UpdateFontSize(_service.ClampFontSize(_data.FontSize, 1f));
            SaveData();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            if (_data == null) return;
            UpdateFontSize(_service.ClampFontSize(_data.FontSize, -1f));
            SaveData();
        }

        // --- 상세 본문/메모 폰트 크기 조절 ---

        private void UpdateDetailFontSize(float size)
        {
            if (_data == null) return;
            _detailFontSize = _service.ClampFontSize(size, 0);
            _data.DetailFontSize = _detailFontSize;

            _detailFont?.Dispose();
            _detailFont = new Font("맑은 고딕", _detailFontSize, FontStyle.Regular);

            txtContent.Font = _detailFont;
            txtMemo.Font = _detailFont;
        }

        private void btnDetailZoomIn_Click(object sender, EventArgs e)
        {
            UpdateDetailFontSize(_detailFontSize + 1f);
            SaveData();
        }

        private void btnDetailZoomOut_Click(object sender, EventArgs e)
        {
            UpdateDetailFontSize(_detailFontSize - 1f);
            SaveData();
        }

        // --- 폼 종료 자원 정리 ---

        private void WorkLogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveData();

            _searchDebounceTimer?.Dispose();
            _saveDebounceTimer?.Dispose();
            _regularFont?.Dispose();
            _boldFont?.Dispose();
            _detailFont?.Dispose();
            _currentViewerImage?.Dispose();
            _toolTip?.Dispose();
            _menuStatus?.Dispose();
            _menuPriority?.Dispose();

            // 썸네일 해제
            foreach (Control c in flowThumbnails.Controls)
            {
                if (c is Button btn && btn.Image != null)
                {
                    btn.Image.Dispose();
                }
            }
        }
    }
}