using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq; // ⬅ Max, Any 등을 위해 추가
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using static GateHelper.LogManager;
using static GateHelper.WorkLogEntry;

namespace GateHelper
{
    public partial class WorkLogForm : MaterialForm
    {
        private readonly MaterialSkinManager _materialSkinManager;

        // 데이터 경로(_meta/worklog.json)
        private readonly string _dataPath = Util.GetMetaPath("WorkLog.json");

        // 메모리 모델
        private List<WorkLogEntry> _items = new List<WorkLogEntry>();

        // 현재 검색어 (부분 하이라이트를 그릴 때 사용)
        private string _currentFilter = string.Empty;

        // 상태 선택 옵션
        private static readonly string[] StatusOptions = new[] { "OPEN", "ING..", "DONE", "FIXED" };

        // 컨텍스트 메뉴
        private ContextMenuStrip _cms;

        // Pont Size
        private float _currentFontSize = 10f; // Default

        private bool _isDatePickerDropDownOpen = false;

        public WorkLogForm()
        {
            InitializeComponent();

            _materialSkinManager = MaterialSkinManager.Instance;
            _materialSkinManager.AddFormToManage(this);

            // 폼 로드 시 초기화
            this.Load -= WorkLogForm_Load;
            this.Load += WorkLogForm_Load;
        }

        private void WorkLogForm_Load(object sender, EventArgs e)
        {
            InitListView();
            WireEvents();
            SetupContextMenu();
            LoadData();
        }

        // InitListView 안에 보완 설정 추가
        private void InitListView()
        {
            OlvWorkLog.FullRowSelect = true;
            OlvWorkLog.ShowGroups = false;
            OlvWorkLog.CellEditActivation = ObjectListView.CellEditActivateMode.DoubleClick;
            OlvWorkLog.CellEditUseWholeCell = true;
            OlvWorkLog.UseAlternatingBackColors = false;
            OlvWorkLog.MultiSelect = true;

            // 필터링 활성화 (실시간 검색)
            OlvWorkLog.UseFiltering = true;

            // Owner-draw 활성화: 부분 텍스트를 Bold로 렌더링하기 위함
            OlvWorkLog.OwnerDraw = true;

            // 헤더 테마 겹침 방지
            OlvWorkLog.HeaderUsesThemes = false;

            // 툴팁 관련 기능을 완전히 무력화 (에러 방지)
            OlvWorkLog.ShowItemToolTips = false;
            OlvWorkLog.CellToolTipGetter = null;
            OlvWorkLog.HeaderToolTipGetter = null;

            foreach (var col in OlvWorkLog.AllColumns)
            {
                var aspect = col.AspectName ?? string.Empty;
                if (string.Equals(aspect, nameof(WorkLogEntry.Date), StringComparison.Ordinal) ||
                    string.Equals(aspect, nameof(WorkLogEntry.LastUpdated), StringComparison.Ordinal))
                {
                    col.AspectToStringFormat = string.Equals(aspect, nameof(WorkLogEntry.LastUpdated), StringComparison.Ordinal)
                        ? "{0:yyyy-MM-dd HH:mm}"
                        : "{0:yyyy-MM-dd}";
                }

                // ⭐ Not editable columns 
                if (string.Equals(aspect, nameof(WorkLogEntry.No), StringComparison.Ordinal) ||
                    string.Equals(aspect, nameof(WorkLogEntry.LastUpdated), StringComparison.Ordinal))
                {
                    col.IsEditable = false;
                }
            }

            // ⭐ MaterialSkin의 폰트 강제 주입을 이겨내기 위한 BeginInvoke
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 시스템에서 가장 안정적인 트루타입 폰트 생성
                    Font malgunFont = new Font("맑은 고딕", 10f, FontStyle.Regular);

                    // 1. 전체 리스트뷰 폰트 강제 설정
                    OlvWorkLog.Font = malgunFont;

                    // 2. 헤더 폰트 강제 설정 (HeaderStyle을 통한 접근) - 삭제
                    // if (OlvWorkLog.HeaderStyle != null)
                    //{
                    //    // 별도의 HeaderFont 속성이 있다면 사용
                    //   OlvWorkLog.HeaderMinimumHeight = 30; // 헤더가 잘리지 않게 높이 조절
                    //}

                    // 3. 모든 컬럼의 폰트를 순회하며 강제 지정
                    foreach (OLVColumn col in OlvWorkLog.AllColumns)
                    {
                        // HeaderFont는 일부 버전에서 지원되지 않을 수 있으므로 체크하며 적용
                        try { col.HeaderFont = malgunFont; } catch { }
                    }

                    OlvWorkLog.Refresh();
                }
                catch { /* 폰트 설정 중 오류 무시 */ }
            }));

        }

        private void WireEvents()
        {
            // 셀 편집(시작/종료)
            OlvWorkLog.CellEditStarting -= OlvWorkLog_CellEditStarting;
            OlvWorkLog.CellEditStarting += OlvWorkLog_CellEditStarting;

            // 편집 “종료 직전” 이벤트에서 달력 열림 상태면 종료 취소
            OlvWorkLog.CellEditFinishing -= OlvWorkLog_CellEditFinishing;
            OlvWorkLog.CellEditFinishing += OlvWorkLog_CellEditFinishing;

            OlvWorkLog.CellEditFinished -= OlvWorkLog_CellEditFinished;
            OlvWorkLog.CellEditFinished += OlvWorkLog_CellEditFinished;

            // 검색 필터
            TxtWorkLog.TextChanged -= TxtWorkLog_TextChanged;
            TxtWorkLog.TextChanged += TxtWorkLog_TextChanged;

            // 커스텀 드로잉 핸들러
            OlvWorkLog.DrawColumnHeader -= OlvWorkLog_DrawColumnHeader;
            OlvWorkLog.DrawColumnHeader += OlvWorkLog_DrawColumnHeader;

            OlvWorkLog.DrawItem -= OlvWorkLog_DrawItem;
            OlvWorkLog.DrawItem += OlvWorkLog_DrawItem;

            OlvWorkLog.DrawSubItem -= OlvWorkLog_DrawSubItem;
            OlvWorkLog.DrawSubItem += OlvWorkLog_DrawSubItem;

            // FormatCell: 셀 단위 포맷(부분 하이라이트가 동작하지 않을 때 전체 셀 Bold 처리 폴백)
            OlvWorkLog.FormatCell -= OlvWorkLog_FormatCell;
            OlvWorkLog.FormatCell += OlvWorkLog_FormatCell;

            // 폼이 닫힐 때 발생하는 이벤트 추가
            this.FormClosing -= WorkLogForm_FormClosing;
            this.FormClosing += WorkLogForm_FormClosing;

            // 추가된 부분: 키 업 이벤트
            TxtWorkLog.KeyUp -= TxtWorkLog_KeyUp;
            TxtWorkLog.KeyUp += TxtWorkLog_KeyUp;

            // FormatRow: 행 전체의 스타일을 데이터 조건에 따라 변경
            OlvWorkLog.FormatRow -= OlvWorkLog_FormatRow;
            OlvWorkLog.FormatRow += OlvWorkLog_FormatRow;

            // FontSize Control
            OlvWorkLog.MouseWheel += (s, e) =>
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    // 휠을 위로 돌리면 +, 아래로 돌리면 -
                    float delta = e.Delta > 0 ? 1f : -1f;
                    ChangeFontSize(delta);

                    // 리스트뷰가 스크롤되는 것을 방지
                    ((HandledMouseEventArgs)e).Handled = true;
                }
            };
        }

        private void SetupContextMenu()
        {
            _cms = OlvWorkLog.ContextMenuStrip ?? new ContextMenuStrip();
            OlvWorkLog.ContextMenuStrip = _cms;
            _cms.Items.Clear();

            var addItem = new ToolStripMenuItem("새 항목 추가", null, (s, e) => AddNewEntry());
            var delItem = new ToolStripMenuItem("선택 항목 삭제", null, (s, e) => DeleteSelectedEntries());

            _cms.Items.Add(addItem);
            _cms.Items.Add(delItem);
        }

        private void SaveData()
        {
            try
            {
                var data = new WorkLogData
                {
                    FontSize = _currentFontSize, // 현재 폰트 사이즈 포함
                    Items = _items
                };

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"
                };

                string json = JsonConvert.SerializeObject(data, settings);
                File.WriteAllText(_dataPath, json);
            }
            catch (Exception ex) { LogException(ex, Level.Error); }
        }

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
                    _currentFontSize = data.FontSize > 0 ? data.FontSize : 10f; // 폰트 로드

                    // 로드된 폰트 사이즈 즉시 적용
                    ChangeFontSize(0);
                }

                OlvWorkLog.SetObjects(_items);
            }
            catch (Exception ex) { LogException(ex, Level.Error); }
        }

        // 날짜/상태 컬럼 커스텀 에디터 적용, 텍스트 컬럼 Enter 저장
        private void OlvWorkLog_CellEditStarting(object sender, CellEditEventArgs e)
        {
            if (e.Column == null) return;
            var aspect = e.Column.AspectName ?? string.Empty;

            if (string.Equals(aspect, nameof(WorkLogEntry.Status), StringComparison.Ordinal))
            {
                var cb = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Bounds = e.CellBounds,
                    Font = new System.Drawing.Font("Consolas", 10f)
                };
                cb.Items.AddRange(StatusOptions);

                // 현재 값 매칭
                cb.SelectedItem = StatusOptions.Contains(e.Value?.ToString()) ? e.Value.ToString() : "Open";

                // ⭐ 중요: 선택 즉시 편집을 끝내고 싶다면 사용
                cb.SelectedIndexChanged += (s, _) =>
                {
                    // 이 호출이 아래 CellEditFinishing을 유발합니다.
                    this.BeginInvoke(new Action(() => OlvWorkLog.FinishCellEdit()));
                };

                e.Control = cb;
            }

            // [Date] Column Setting
            if (string.Equals(aspect, nameof(WorkLogEntry.Date), StringComparison.Ordinal))
            {
                var dtp = new DateTimePicker
                {
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "yyyy-MM-dd HH:mm:ss",
                    Value = e.Value is DateTime dt && dt != default(DateTime) ? dt : DateTime.Today,
                    // ⭐ 에디터의 위치와 크기를 현재 셀의 위치와 정확히 일치시킵니다.
                    Bounds = e.CellBounds
                };

                dtp.DropDown += (s, _) => _isDatePickerDropDownOpen = true;
                dtp.CloseUp += (s, _) =>
                {
                    _isDatePickerDropDownOpen = false;
                    // ⭐ 날짜 선택이 완료되면 바로 값을 반영하고 편집을 마칩니다.
                    OlvWorkLog.FinishCellEdit();
                };

                dtp.KeyDown += (s, ke) =>
                {
                    if (ke.KeyCode == Keys.Enter)
                    {
                        _isDatePickerDropDownOpen = false;
                        OlvWorkLog.FinishCellEdit();
                        ke.Handled = true;
                    }
                };

                e.Control = dtp;
            }
            // [Status] Column Setting
            else if (string.Equals(aspect, nameof(WorkLogEntry.Status), StringComparison.Ordinal))
            {
                var cb = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Bounds = e.CellBounds,
                    Font = new System.Drawing.Font("맑은 고딕", 10f)
                };
                cb.Items.AddRange(StatusOptions);

                // 현재 값 선택
                var current = e.Value as string;
                cb.SelectedItem = StatusOptions.Contains(current) ? current : "Open";

                // ⭐ 핵심: 값이 바뀌면 '새 값'임을 알려주고 편집을 끝냄
                cb.SelectedIndexChanged += (s, _) =>
                {
                    e.NewValue = cb.SelectedItem.ToString(); // OLV에 새 값을 전달

                    // 지연 실행으로 편집 종료를 유도 (즉시 종료 시 간섭 발생 방지)
                    this.BeginInvoke(new Action(() =>
                    {
                        OlvWorkLog.FinishCellEdit();
                    }));
                };

                e.Control = cb;
            }
        }

        // 달력 열림 중에는 편집 종료를 막는다
        private void OlvWorkLog_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            if (e.Column == null) return;

            // Status 컬럼인 경우
            if (string.Equals(e.Column.AspectName, nameof(WorkLogEntry.Status), StringComparison.Ordinal))
            {
                if (e.Control is ComboBox cb)
                {
                    // ⭐ 에디터(ComboBox)에 선택된 값을 직접 추출해서 NewValue에 할당
                    e.NewValue = cb.SelectedItem?.ToString();

                    // 만약 선택된 게 없다면 기존 값 유지 (Null 방지)
                    if (e.NewValue == null) e.Cancel = true;
                }
            }
            // Date 컬럼인 경우 (기존 로직)
            else if (string.Equals(e.Column.AspectName, nameof(WorkLogEntry.Date), StringComparison.Ordinal)
                     && _isDatePickerDropDownOpen)
            {
                e.Cancel = true;
            }
        }

        private void OlvWorkLog_CellEditFinished(object sender, CellEditEventArgs e)
        {
            if (e.Cancel) return;

            if (e.RowObject is WorkLogEntry entry)
            {
                // NewValue가 확실히 들어있으므로 모델에 반영
                if (e.NewValue != null)
                {
                    if (string.Equals(e.Column.AspectName, nameof(WorkLogEntry.Status), StringComparison.Ordinal))
                        entry.Status = e.NewValue.ToString();
                    else if (string.Equals(e.Column.AspectName, nameof(WorkLogEntry.Title), StringComparison.Ordinal))
                        entry.Title = e.NewValue.ToString();
                    // ... 나머지 컬umn ...
                }

                entry.Touch(); // LastUpdated 갱신
                OlvWorkLog.RefreshObject(entry);
                SaveData();
            }
        }

        // TxtWorkLog_TextChanged와 KeyUp에서 필터 설정 후 즉시 리빌드/리프레시
        private void TxtWorkLog_TextChanged(object sender, EventArgs e)
        {
            var q = TxtWorkLog.Text?.Trim() ?? string.Empty;

            // 현재 검색어 저장
            _currentFilter = q;

            OlvWorkLog.BeginUpdate();
            try
            {
                // 기존 TextMatchFilter 하이라이트를 비활성화하기 위해 수동으로 SetObjects를 사용
                ApplyFilter(q);

                // 필터 적용 즉시 리스트를 재구성
                OlvWorkLog.BuildList(true);
                OlvWorkLog.Refresh();
            }
            finally
            {
                OlvWorkLog.EndUpdate();
            }
        }

        private void OlvWorkLog_FormatRow(object sender, FormatRowEventArgs e)
        {
            if (e.Model is WorkLogEntry entry)
            {
                // 상태에 따른 색상 지정
                switch (entry.Status?.ToUpper())
                {
                    case "DONE":
                        e.Item.BackColor = Color.LightGray;
                        e.Item.ForeColor = Color.DimGray; // 글자색도 살짝 흐리게
                        break;

                    case "ING..":
                        e.Item.BackColor = Color.LemonChiffon;
                        e.Item.ForeColor = Color.Black;
                        break;

                    case "FIXED":
                        e.Item.BackColor = Color.LightBlue; // FIXED 상태일 때 예시
                        e.Item.ForeColor = Color.Black;
                        break;

                    default:
                        // OPEN 등 기타 상태는 기본 테마 색상 유지
                        e.Item.BackColor = Color.Empty;
                        e.Item.ForeColor = Color.Empty;
                        break;
                }
            }
        }

        // 추가된 TxtWorkLog_KeyUp 핸들러도 동일하게 처리
        private void TxtWorkLog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                return;

            var q = TxtWorkLog.Text?.Trim() ?? string.Empty;

            // 현재 검색어 저장
            _currentFilter = q;

            OlvWorkLog.BeginUpdate();
            try
            {
                ApplyFilter(q);

                OlvWorkLog.BuildList(true);
                OlvWorkLog.Refresh();
            }
            finally
            {
                OlvWorkLog.EndUpdate();
            }
        }

        // 중앙화된 필터 적용: 하이라이트 색상(배경/전경)을 명시적으로 지정하여 가시성 향상
        private void ApplyFilter(string q)
        {
            // 수동 필터링: TextMatchFilter를 사용하지 않고, 표시할 객체만 SetObjects로 전달합니다.
            _currentFilter = q ?? string.Empty;

            if (string.IsNullOrEmpty(q))
            {
                OlvWorkLog.SetObjects(_items);
                return;
            }

            var filtered = _items.Where(item => RowMatchesFilter(item, q)).ToList();
            OlvWorkLog.SetObjects(filtered);
        }

        // WorkLogEntry에 대해 단순한 대소문자 무시 포함 검사
        private bool RowMatchesFilter(WorkLogEntry entry, string q)
        {
            if (entry == null || string.IsNullOrEmpty(q)) return false;
            string lower = q.ToLowerInvariant();

            // 검사 대상 필드
            if ((!string.IsNullOrEmpty(entry.Title) && entry.Title.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(entry.Content) && entry.Content.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(entry.Tags) && entry.Tags.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(entry.Memo) && entry.Memo.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrEmpty(entry.Status) && entry.Status.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            // 숫자 및 날짜 검사
            if (entry.No.ToString().IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (entry.Date.ToString("yyyy-MM-dd HH:mm:ss").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                entry.Date.ToString("yyyy-MM-dd").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (entry.LastUpdated.ToString("yyyy-MM-dd HH:mm").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        private void CommitOlvEdit()
        {
            // 키 이벤트 직후에는 즉시 포커스 이동이 막힐 수 있어 BeginInvoke로 지연
            this.BeginInvoke(new Action(() => OlvWorkLog.Focus()));
        }

        private void AddNewEntry()
        {
            try
            {
                int nextNo = _items.Count == 0 ? 1 : (_items.Max(x => x.No) + 1);

                var entry = new WorkLogEntry
                {
                    No = nextNo,
                    Date = DateTime.Now,
                    Status = "OPEN",
                    LastUpdated = DateTime.Now
                };

                _items.Add(entry);
                OlvWorkLog.AddObject(entry);

                // 선택/가시화 및 즉시 제목 편집
                OlvWorkLog.SelectObject(entry, true);
                OlvWorkLog.EnsureModelVisible(entry);

                int titleColIndex = FindColumnIndexByAspectName(nameof(WorkLogEntry.Title));
                var rowItem = OlvWorkLog.ModelToItem(entry);
                if (rowItem != null && titleColIndex >= 0)
                {
                    // 제목부터 바로 입력하도록 편집 시작
                    OlvWorkLog.StartCellEdit(rowItem, titleColIndex);
                }

                SaveData();
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show(this, "An error occurred while adding the item.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedEntries()
        {
            try
            {
                var selected = OlvWorkLog.SelectedObjects?.Cast<object>().ToList() ?? new List<object>();
                if (selected.Count == 0) return;

                var confirm = MessageBox.Show(this,
                    $"Are you sure you want to delete the {selected.Count} selected items?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                foreach (var obj in selected)
                {
                    if (obj is WorkLogEntry entry)
                    {
                        _items.Remove(entry);
                        OlvWorkLog.RemoveObject(entry);
                    }
                }

                SaveData();
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show(this, "항목을 삭제하는 중 오류가 발생했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int FindColumnIndexByAspectName(string aspectName)
        {
            for (int i = 0; i < OlvWorkLog.AllColumns.Count; i++)
            {
                var c = OlvWorkLog.AllColumns[i];
                if (string.Equals(c.AspectName, aspectName, StringComparison.Ordinal))
                {
                    // 표시되지 않는 컬럼이면 화면상 인덱스를 다시 매핑
                    int visibleIndex = OlvWorkLog.Columns.IndexOf(c);
                    return visibleIndex >= 0 ? visibleIndex : i;
                }
            }
            return -1;
        }

        

        private void WorkLogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 1. 레이아웃 및 툴팁 기능 중지
                OlvWorkLog.SuspendLayout();
                OlvWorkLog.ShowItemToolTips = false;

                // 2. 중요: 부모 폼과의 관계를 먼저 끊습니다.
                // 이렇게 하면 폼이 파괴될 때 OLV가 잘못된 시스템 폰트 핸들을 참조하는 것을 방지합니다.
                OlvWorkLog.Parent = null;

                this.FormClosing -= WorkLogForm_FormClosing;

                if (!OlvWorkLog.IsDisposed)
                {
                    OlvWorkLog.Dispose();
                }

                OlvWorkLog.Visible = false;
                OlvWorkLog.Parent = null; // 핸들 연결 해제
            }
            catch (Exception)
            {
                // 여기서 발생하는 '트루타입 글꼴' 예외는 무시해도 무방합니다. (종료 시점이므로)
            }
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                base.WndProc(ref m);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("트루타입") || ex.Message.Contains("TrueType"))
            {
                // 폰트 핸들 정리 중 발생하는 WinForms 고질적 버그이므로 로그만 남기고 무시
                System.Diagnostics.Debug.WriteLine("폰트 예외 무시됨: " + ex.Message);
            }
        }

        // ColumnHeader 기본 렌더링
        private void OlvWorkLog_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            try
            {
                e.DrawBackground();
                TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, e.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            catch
            {
                // 무시
            }
        }

        // Item 배경만 그리기 (SubItem에서 텍스트를 그림)
        private void OlvWorkLog_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            try
            {
                e.DrawBackground();
            }
            catch
            {
            }
        }

        // SubItem 부분 문자열 Bold 렌더링
        private void OlvWorkLog_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            try
            {
                e.DrawBackground();

                string text = e.SubItem.Text ?? string.Empty;

                // 검색어가 비어있으면 기본 그리기
                if (string.IsNullOrEmpty(_currentFilter))
                {
                    TextRenderer.DrawText(e.Graphics, text, e.SubItem.Font ?? OlvWorkLog.Font, e.Bounds, e.SubItem.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                    return;
                }

                string lower = text.ToLowerInvariant();
                string q = _currentFilter.ToLowerInvariant();

                int start = 0;
                int idx = lower.IndexOf(q, start, StringComparison.Ordinal);

                // 좌측 패딩
                int x = e.Bounds.X + 2;
                int y = e.Bounds.Y;
                int height = e.Bounds.Height;

                Font normalFont = e.SubItem.Font ?? OlvWorkLog.Font;
                using (Font boldFont = new Font(normalFont, FontStyle.Bold))
                {
                    var flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;

                    // 만약 셀에 필터 문자열이 포함되어 있으면 전체 텍스트를 Bold로 그려서 가시성을 보장합니다.
                    if (idx >= 0)
                    {
                        // 전체 텍스트 Bold로 그리기 (부분 하이라이트가 보이지 않을 때의 확실한 대체)
                        TextRenderer.DrawText(e.Graphics, text, boldFont, e.Bounds, e.SubItem.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                        return;
                    }

                    // (부분 렌더링 로직을 보류했습니다. 필요하면 되돌려 부분 굵게 처리 가능)
                 }
             }
             catch
             {
                 // 실패 시 기본 그리기
                 try { e.DrawText(); } catch { }
             }
         }

        // FormatCell 핸들러: 셀 텍스트에 현재 필터가 포함되어 있으면 Bold 처리 (폴백)
        private void OlvWorkLog_FormatCell(object sender, FormatCellEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentFilter))
                {
                    // 원래 폰트로 복원
                    e.SubItem.Font = OlvWorkLog.Font;
                    return;
                }

                string cellText = e.SubItem.Text ?? string.Empty;
                if (cellText.IndexOf(_currentFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Bold로 표시
                    e.SubItem.Font = new Font(OlvWorkLog.Font, FontStyle.Bold);
                }
                else
                {
                    e.SubItem.Font = OlvWorkLog.Font;
                }
            }
            catch
            {
                // 무시
            }
        }

        private void ChangeFontSize(float delta)
        {
            _currentFontSize += delta;

            // 최소/최대 크기 제한 (안전장치)
            if (_currentFontSize < 8f) _currentFontSize = 8f;
            if (_currentFontSize > 24f) _currentFontSize = 24f;

            Font newFont = new Font("맑은 고딕", _currentFontSize, FontStyle.Regular);

            OlvWorkLog.Font = newFont;

            // 헤더 폰트도 함께 조절하고 싶을 경우
            foreach (OLVColumn col in OlvWorkLog.AllColumns)
            {
                try { col.HeaderFont = newFont; } catch { }
            }

            // 변경사항 즉시 반영
            OlvWorkLog.Refresh();

            // 사이즈 변경 시 자동 저장 (선택 사항)
            SaveData();
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            ChangeFontSize(1f);
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            ChangeFontSize(-1f);
        }
    } // 클래스 끝

    // Work Log 한 행을 표현하는 데이터 모델
    [DataContract]
    public class WorkLogEntry
    {
        [DataMember(Order = 1)]
        public int No { get; set; }

        [DataMember(Order = 2)]
        public DateTime Date { get; set; }

        [DataMember(Order = 3)]
        public string Title { get; set; }

        [DataMember(Order = 4)]
        public string Content { get; set; }

        [DataMember(Order = 5)]
        public string Status { get; set; }

        [DataMember(Order = 6)]
        public string Tags { get; set; }

        [DataMember(Order = 7)]
        public string Memo { get; set; }

        [DataMember(Order = 8)]
        public DateTime LastUpdated { get; set; }

        public WorkLogEntry()
        {
            Date = DateTime.Today;
            LastUpdated = DateTime.Now;
            Title = string.Empty;
            Content = string.Empty;
            Status = string.Empty;
            Tags = string.Empty;
            Memo = string.Empty;
        }

        public void Touch()
        {
            LastUpdated = DateTime.Now;
        }

        public class WorkLogData
        {
            [DataMember]
            public float FontSize { get; set; } = 10f;

            [DataMember]
            public List<WorkLogEntry> Items { get; set; } = new List<WorkLogEntry>();
        }

    }
}
