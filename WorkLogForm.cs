using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // ⬅ Max, Any 등을 위해 추가
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    public partial class WorkLogForm : MaterialForm
    {
        private readonly MaterialSkinManager _materialSkinManager;

        // 데이터 경로(_meta/worklog.json)
        private readonly string _dataPath = Util.GetMetaPath("WorkLog.json");

        // 메모리 모델
        private List<WorkLogEntry> _items = new List<WorkLogEntry>();

        // 상태 선택 옵션
        private static readonly string[] StatusOptions = new[] { "Open", "ING..", "Done", "Blocked" };

        // 컨텍스트 메뉴
        private ContextMenuStrip _cms;

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

            // 헤더 테마 겹침 방지
            OlvWorkLog.HeaderUsesThemes = false;

            // 툴팁 관련 기능을 완전히 무력화 (에러 방지)
            OlvWorkLog.ShowItemToolTips = false;
            OlvWorkLog.CellToolTipGetter = null;
            OlvWorkLog.HeaderToolTipGetter = null;
            OlvWorkLog.ToolTipControl.IsActive = false;

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

            // 폼이 닫힐 때 발생하는 이벤트 추가
            this.FormClosing -= WorkLogForm_FormClosing;
            this.FormClosing += WorkLogForm_FormClosing;

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
                // 날짜 포맷 설정 객체 생성
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented, // 들여쓰기 적용
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"
                };

                string json = JsonConvert.SerializeObject(_items, settings);

                Directory.CreateDirectory(Path.GetDirectoryName(_dataPath) ?? Application.StartupPath);
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
                if (!File.Exists(_dataPath)) return;

                string json = File.ReadAllText(_dataPath);

                var settings = new JsonSerializerSettings
                {
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"
                };

                _items = JsonConvert.DeserializeObject<List<WorkLogEntry>>(json, settings) ?? new List<WorkLogEntry>();
                OlvWorkLog.SetObjects(_items);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                _items = new List<WorkLogEntry>();
                OlvWorkLog.SetObjects(_items);
            }
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
                    Font = new System.Drawing.Font("맑은 고딕", 10f)
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
                    CustomFormat = "yyyy-MM-dd",
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
                    // ... 나머지 컬럼 ...
                }

                entry.Touch(); // LastUpdated 갱신
                OlvWorkLog.RefreshObject(entry);
                SaveData();
            }
        }

        private void TxtWorkLog_TextChanged(object sender, EventArgs e)
        {
            var q = TxtWorkLog.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(q))
            {
                OlvWorkLog.ModelFilter = null;
                return;
            }

            // 간단 부분일치 필터(모든 표시 텍스트 대상)
            OlvWorkLog.ModelFilter = TextMatchFilter.Contains(OlvWorkLog, q);
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
                    Date = DateTime.Today,
                    Status = "Open",
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
                //-----------------폰트오류 방지용----------------
                OlvWorkLog.SuspendLayout();
                OlvWorkLog.ShowItemToolTips = false;

                OlvWorkLog.CellToolTipGetter = null;
                OlvWorkLog.HeaderToolTipGetter = null;

                this.FormClosing -= WorkLogForm_FormClosing;

                if (!OlvWorkLog.IsDisposed)
                {
                    OlvWorkLog.Dispose();
                }
                //-----------------폰트오류 방지용----------------
            }
            catch
            {
                // 종료 시 발생하는 폰트 예외는 무시하도록 처리
            }
        }
    }




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
    }
}
