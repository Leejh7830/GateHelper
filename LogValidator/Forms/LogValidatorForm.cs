using BrightIdeasSoftware;
using GateHelper.LogValidator.Core;
using GateHelper.LogValidator.Models;
using GateHelper.LogValidator.Services;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GateHelper.LogValidator
{
    public partial class LogValidatorForm : MaterialForm
    {
        private readonly MaterialSkinManager _skinManager = MaterialSkinManager.Instance;
        private readonly LogValidatorService _logValidatorService = new LogValidatorService(); // 💡 파일 로딩/분류/정렬/검증 서비스

        private List<RawLogModel> _validatorRawLogList = new List<RawLogModel>();
        private List<ScenarioEvaluator> _currentRepositoryList = new List<ScenarioEvaluator>();
        private readonly string _scenarioBaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_meta", "Scenarios");

        private int _currentSelectedStartLineNo = -1;

        // 💡 현재 로드된 파일 경로 목록 - UI 표시 및 초기화 판단에 사용
        private readonly List<string> _loadedFilePaths = new List<string>();

        // 💡 파일 목록 패널 컨트롤 (코드에서 동적 생성 - Designer의 tabPage1 위에 올라감)
        private Panel _fileListPanel;
        private FlowLayoutPanel _fileTagContainer;
        private bool _fileListExpanded = false;

        // 💡 우측 메뉴 패널 슬라이드 토글용 필드
        private Button _btnSideToggle;
        private bool _sidePanelVisible = false;
        private const int SIDE_PANEL_WIDTH = 150;

        public LogValidatorForm()
        {
            InitializeComponent();
            _skinManager.AddFormToManage(this);

            InitializeValidatorRawLogGridView();
            InitializeScenarioRepositoryGridView();
            InitializeValidationResultGridView();
            InitializeValidatorDropZone();
            InitializeFileListPanel();    // 💡 파일 목록 패널 (tabPage1 상단에 코드로 생성)

            InitializeScenarioTreeViewInterlock();
            InitializeRuntimeFilter();

            btnReset.Click += btnReset_Click;

            InitializeSideToggle(); // 💡 우측 메뉴 슬라이드 토글 버튼 초기화

            ScenarioEventBroker.OnScenarioSaved += OnRuntimeScenarioRefresh;
        }

        #region 🛠 1. 그리드 뼈대 셋업 및 우클릭 컨텍스트 메뉴 바인딩

        private void InitializeValidatorRawLogGridView()
        {
            olvValidatorRawLog.Columns.Clear();
            olvValidatorRawLog.Font = new System.Drawing.Font("Malgun Gothic", 10.5f, System.Drawing.FontStyle.Regular);

            var colLineNo = new OLVColumn("Line", "LineNo") { Width = 55, TextAlign = HorizontalAlignment.Center };
            var colTime = new OLVColumn("Time", "LogTime") { Width = 110, TextAlign = HorizontalAlignment.Center };
            var colSourceFile = new OLVColumn("Source File", "SourceFileName") { Width = 155, TextAlign = HorizontalAlignment.Center };
            var colMessage = new OLVColumn("Log Message", "LogMessage") { Width = 500 };

            // 💡 LogTime을 HH:mm:ss.fff 형식으로 표시 (날짜 제외)
            // MinValue(파싱 실패)인 경우 빈 문자열로 표시
            colTime.AspectGetter = row =>
            {
                if (row is RawLogModel log)
                    return log.LogTime == DateTime.MinValue ? "" : log.LogTime.ToString("HH:mm:ss.fff");
                return "";
            };

            olvValidatorRawLog.Columns.AddRange(new ColumnHeader[] { colLineNo, colTime, colSourceFile, colMessage });
            olvValidatorRawLog.View = View.Details;
            olvValidatorRawLog.FullRowSelect = true;
            olvValidatorRawLog.GridLines = true;
            olvValidatorRawLog.ShowItemToolTips = false;

            // 💡 Time Jump UI: Designer에서 만든 컨트롤에 이벤트 연결
            InitializeTimeJumpUI();
        }

        private void InitializeTimeJumpUI()
        {
            // 💡 btnTimeJump 클릭 시 cmbDateFilter(날짜) + dtpTimeJump(시간) 조합으로 점프
            btnTimeJump.Click += (s, e) =>
            {
                if (_validatorRawLogList == null || _validatorRawLogList.Count == 0) return;
                if (cmbDateFilter.SelectedItem == null) return;

                // 선택된 날짜 + 시간 조합으로 DateTime 생성
                DateTime selectedDate = (DateTime)cmbDateFilter.SelectedItem;
                TimeSpan selectedTime = dtpTimeJump.Value.TimeOfDay;
                DateTime target = selectedDate.Date + selectedTime;

                // 💡 target과 가장 가까운 로그 행 탐색 (날짜+시간 모두 비교)
                RawLogModel closest = null;
                TimeSpan minDiff = TimeSpan.MaxValue;

                foreach (var log in _validatorRawLogList)
                {
                    if (log.LogTime == DateTime.MinValue) continue;
                    TimeSpan diff = (log.LogTime - target).Duration();
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        closest = log;
                    }
                }

                if (closest == null) return;

                int idx = _validatorRawLogList.IndexOf(closest);
                olvValidatorRawLog.SelectedObject = closest;
                olvValidatorRawLog.TopItemIndex = Math.Max(0, idx - 3);
                olvValidatorRawLog.EnsureVisible(idx);
                olvValidatorRawLog.Focus();
            };
        }

        /// <summary>
        /// 로그 드롭 후 cmbDateFilter에 날짜 목록을 채웁니다.
        /// 항상 표시하고, 날짜가 1개면 자동 선택됩니다.
        /// </summary>
        private void RefreshDateFilter()
        {
            cmbDateFilter.Items.Clear();

            if (_validatorRawLogList == null || _validatorRawLogList.Count == 0) return;

            // 💡 로그에서 유효한 날짜만 추출 → 중복 제거 → 오름차순 정렬
            var dates = _validatorRawLogList
                .Where(l => l.LogTime != DateTime.MinValue)
                .Select(l => l.LogTime.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // 💡 "MM/dd (ddd)" 형식으로 표시하기 위해 FormattingEnabled + Format 이벤트 사용
            // Format 이벤트는 최초 1회만 등록 (중복 등록 방지)
            cmbDateFilter.FormattingEnabled = true;
            cmbDateFilter.Format -= CmbDateFilter_Format;
            cmbDateFilter.Format += CmbDateFilter_Format;

            foreach (var date in dates)
                cmbDateFilter.Items.Add(date);

            if (cmbDateFilter.Items.Count > 0)
                cmbDateFilter.SelectedIndex = 0;
        }

        private void CmbDateFilter_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.Value is DateTime d)
                e.Value = d.ToString("MM/dd (ddd)");
        }

        private void InitializeScenarioRepositoryGridView()
        {
            olvScenarioRepository.Columns.Clear();
            olvScenarioRepository.Font = new System.Drawing.Font("Malgun Gothic", 10.5f, System.Drawing.FontStyle.Bold);
            olvScenarioRepository.RowHeight = 30;
            olvScenarioRepository.CheckBoxes = true;
            olvScenarioRepository.ShowGroups = false; // 그룹화 중복 타이틀 제거 락
            olvScenarioRepository.ShowItemToolTips = false;

            var colName = new OLVColumn("Scenario Unit Name", "ScenarioName") { Width = olvScenarioRepository.Width - 40 };

            olvScenarioRepository.Columns.AddRange(new ColumnHeader[] { colName });
            olvScenarioRepository.View = View.Details;
            olvScenarioRepository.FullRowSelect = true; // 💡 행 전체 선택 활성화로 1클릭 영역 인프라 확보
            olvScenarioRepository.GridLines = true;

            // 💡 [우클릭 편집기 소환 인터락 셋업]
            InitializeRepositoryContextMenu();

            // 존재하지 않는 속성들을 전면 폐기하고, 순정 CellClick 이벤트 내에서 
            // 사용자가 체크박스 기호 자체를 누르든, 우측 글자나 빈 여백을 누르든 
            // 중복 연산 버그 없이 정확히 '단 1회'만 토글되도록 인터락을 락인(Lock-in)합니다.
            olvScenarioRepository.CellClick += (s, e) =>
            {
                if (e.Model is ScenarioEvaluator selectedEval)
                {
                    // 마우스 우클릭 시에는 체크박스가 돌지 않도록 좌클릭 분기 가드
                    if (Control.MouseButtons == MouseButtons.Left)
                    {
                        // 현재 대상 객체의 체크 상태를 조사
                        bool isChecked = olvScenarioRepository.IsChecked(selectedEval);

                        if (isChecked)
                        {
                            olvScenarioRepository.UncheckObject(selectedEval);
                        }
                        else
                        {
                            olvScenarioRepository.CheckObject(selectedEval);
                        }

                        // 변경된 그래픽 상태를 UI 스레드 레이어에 즉시 새로고침
                        olvScenarioRepository.RefreshObject(selectedEval);

                        // 💡 [핵심 인터락 가드] 컴포넌트 내부의 순정 체크 트리거가 
                        // 후속으로 중복 가동되어 상태를 다시 뒤집는 현상을 완벽히 차단합니다.
                        e.Handled = true;
                    }
                }
            };
        }

        private void InitializeRepositoryContextMenu()
        {
            var cms = new ContextMenuStrip();
            var menuEdit = new ToolStripMenuItem("시나리오 편집 (Edit)", null, (s, e) =>
            {
                if (olvScenarioRepository.SelectedObject == null) return;

                var selected = olvScenarioRepository.SelectedObject as ScenarioEvaluator;
                if (selected == null) return;

                if (treeScenarioGroup.SelectedNode == null || treeScenarioGroup.SelectedNode.Tag == null) return;
                string currentActiveDirectory = treeScenarioGroup.SelectedNode.Tag.ToString();

                // 💡 [자산 타겟팅 락] 선택된 시나리오의 정확한 물리 JSON 절대 경로 계산
                string fullPath = Path.Combine(currentActiveDirectory, $"{selected.ScenarioName}.json");

                // 💡 파일 경로 탄환을 장전하여 편집기 확장 생성자로 인스턴스 즉시 팝업 소환
                LogScenarioForm scenarioForm = new LogScenarioForm(fullPath);
                scenarioForm.Show();
            });

            cms.Opening += (s, e) =>
            {
                menuEdit.Enabled = (olvScenarioRepository.SelectedObject != null);
            };

            cms.Items.Add(menuEdit);
            olvScenarioRepository.ContextMenuStrip = cms;
        }

        private void InitializeValidationResultGridView()
        {
            olvValidationResult.Columns.Clear();
            olvValidationResult.Font = new System.Drawing.Font("Malgun Gothic", 10.5f, System.Drawing.FontStyle.Bold);
            olvValidationResult.RowHeight = 35;
            olvValidationResult.ShowGroups = false;
            olvValidationResult.ShowItemToolTips = false;

            // [컬럼 명세]
            var colName = new OLVColumn("Scenario Name", "ScenarioName") { Width = 220 };
            colName.AspectGetter = row => row is ScenarioEvaluator ? ((ScenarioEvaluator)row).ScenarioName : ((StepValidationReport)row).StepDisplayHeader;

            var colStatus = new OLVColumn("Status", "Status") { Width = 100, TextAlign = HorizontalAlignment.Center };
            colStatus.AspectGetter = row => row is ScenarioEvaluator ? ((ScenarioEvaluator)row).Status.ToString().ToUpper() : ((StepValidationReport)row).StepStatus?.ToUpper();

            var colProgress = new OLVColumn("Progress", "Progress") { Width = 130, TextAlign = HorizontalAlignment.Center };
            colProgress.AspectGetter = row => row is ScenarioEvaluator ? ((ScenarioEvaluator)row).Progress : ((StepValidationReport)row).StepProgress;

            var colMsg = new OLVColumn("Validation Message", "Message") { Width = 450 };
            colMsg.AspectGetter = row => row is ScenarioEvaluator ? ((ScenarioEvaluator)row).Message : ((StepValidationReport)row).StepMessage;

            olvValidationResult.Columns.AddRange(new ColumnHeader[] { colName, colStatus, colProgress, colMsg });
            olvValidationResult.View = View.Details;
            olvValidationResult.FullRowSelect = true;
            olvValidationResult.GridLines = true;

            olvValidationResult.UseCellFormatEvents = true;
            olvValidationResult.OwnerDraw = true;

            // [통합 스타일 엔진: 행 배경색 + 텍스트 강조]
            olvValidationResult.FormatCell += (s, e) =>
            {
                // 1. 부모 행 배경색 (연빨강/연초록)
                if (e.Model is ScenarioEvaluator parentRow)
                {
                    if (parentRow.Status == EvaluationResultStatus.FAILED)
                        e.SubItem.BackColor = System.Drawing.Color.FromArgb(255, 218, 218);
                    else if (parentRow.Status == EvaluationResultStatus.SUCCESS)
                        e.SubItem.BackColor = System.Drawing.Color.FromArgb(224, 255, 224);
                }

                // 2. Status 셀 텍스트 강조 (뱃지 배경 없음)
                if (e.Column == colStatus)
                {
                    if (e.Model is ScenarioEvaluator parent)
                    {
                        if (parent.Status == EvaluationResultStatus.FAILED)
                        {
                            e.SubItem.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);
                            e.SubItem.Font = new System.Drawing.Font(olvValidationResult.Font, System.Drawing.FontStyle.Bold);
                        }
                        else if (parent.Status == EvaluationResultStatus.SUCCESS)
                        {
                            e.SubItem.ForeColor = System.Drawing.Color.MediumSeaGreen;
                            e.SubItem.Font = new System.Drawing.Font(olvValidationResult.Font, System.Drawing.FontStyle.Bold);
                        }
                    }
                    else if (e.Model is StepValidationReport child)
                    {
                        if (child.StepStatus == "FAILED")
                        {
                            e.SubItem.ForeColor = System.Drawing.Color.FromArgb(176, 42, 55);
                            e.SubItem.Font = new System.Drawing.Font(olvValidationResult.Font, System.Drawing.FontStyle.Bold);
                        }
                    }
                }
            };

            // [트리 파이프라인]
            olvValidationResult.CanExpandGetter = row => (row is ScenarioEvaluator parent) && parent.StepReports.Count > 0;
            olvValidationResult.ChildrenGetter = row => (row is ScenarioEvaluator parent) ? parent.StepReports : null;

            // [더블클릭 및 핫키]
            EventHandler handleSelect = (s, e) =>
            {
                var selected = olvValidationResult.SelectedObject;
                if (selected is ScenarioEvaluator parent)
                {
                    olvValidationResult.ToggleExpansion(parent);
                    if (parent.StepReports.Count > 0) olvValidationResult.SelectedObject = parent.StepReports[0];
                }
                else if (selected is StepValidationReport child)
                {
                    PerformLogTracking(child);
                }
            };

            olvValidationResult.DoubleClick += handleSelect;
            olvValidationResult.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    var timer = new System.Windows.Forms.Timer { Interval = 50 };
                    timer.Tick += (st, et) => { timer.Stop(); handleSelect(s, e); };
                    timer.Start();
                }
            };
        }

        // 💡 핫키 및 더블클릭 공통 로직 추출
        private void PerformLogTracking(StepValidationReport childReport)
        {
            if (childReport == null || childReport.StartLineNo <= 0) return;

            _currentSelectedStartLineNo = childReport.StartLineNo;

            // 💡 (LineNo, SourceFileName) 쌍으로 HashSet 구성 - 파일명이 달라도 같은 LineNo면 오하이라이팅되던 버그 수정
            var matchedLinesSet = new HashSet<(int, string)>(childReport.MatchedLineNumbers ?? new List<(int, string)>());

            // 시작 로그 탐색: LineNo + SourceFileName 모두 일치해야 정확한 위치
            int targetJumpIndex = -1;
            for (int i = 0; i < _validatorRawLogList.Count; i++)
            {
                var l = _validatorRawLogList[i];
                if (l != null && l.LineNo == childReport.StartLineNo && l.SourceFileName == childReport.StartSourceFileName)
                {
                    targetJumpIndex = i;
                    break;
                }
            }

            if (targetJumpIndex != -1)
            {
                olvValidatorRawLog.SelectedObject = null;
                olvValidatorRawLog.TopItemIndex = Math.Max(0, targetJumpIndex - 3);
                olvValidatorRawLog.EnsureVisible(targetJumpIndex);

                olvValidatorRawLog.RowFormatter = rowObject =>
                {
                    var logModel = rowObject.RowObject as RawLogModel;
                    if (logModel != null)
                    {
                        // 💡 LineNo + SourceFileName 동시 비교로 다른 파일의 같은 LineNo 오하이라이팅 방지
                        bool isMatched = matchedLinesSet.Contains((logModel.LineNo, logModel.SourceFileName));

                        rowObject.BackColor = isMatched ? System.Drawing.Color.FromArgb(255, 243, 205) : olvValidatorRawLog.BackColor;
                        rowObject.ForeColor = isMatched ? System.Drawing.Color.Black : olvValidatorRawLog.ForeColor;
                    }
                };
                olvValidatorRawLog.Refresh();
            }
        }

        private void InitializeValidatorDropZone()
        {
            olvValidatorRawLog.AllowDrop = true;

            olvValidatorRawLog.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
                else e.Effect = DragDropEffects.None;
            };

            // 💡 async void: UI 이벤트 핸들러에서 async를 쓸 때는 async void가 올바른 패턴
            olvValidatorRawLog.DragDrop += async (s, e) =>
            {
                string[] droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (droppedPaths == null || droppedPaths.Length == 0) return;

                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ResetAllLogData();
                    _validatorRawLogList = await _logValidatorService.LoadLogFilesAsync(droppedPaths);
                    _loadedFilePaths.AddRange(droppedPaths);

                    olvValidatorRawLog.BeginUpdate();
                    olvValidatorRawLog.SetObjects(_validatorRawLogList);
                    olvValidatorRawLog.EndUpdate();

                    LogValidatorConfigManager.Load();
                    BuildUnitTabsAndGrids(_validatorRawLogList);
                    UpdateFileListPanel();
                    RefreshDateFilter(); // 💡 날짜 콤보박스 갱신
                }
                catch (Exception ex)
                {
                    olvValidatorRawLog.EndUpdate();
                    MessageBox.Show($"로그 로드 오류:\n{ex.Message}", "Drop Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // 💡 finally: 예외가 발생해도 커서가 WaitCursor에 멈추지 않도록 보장
                    Cursor.Current = Cursors.Default;
                }
            };
        }

        // 💡 Designer에서 만든 btnReset과 연결되는 클릭 핸들러
        private void btnReset_Click(object sender, EventArgs e)
        {
            if (_validatorRawLogList == null || _validatorRawLogList.Count == 0) return;

            var result = MessageBox.Show(
                "로드된 모든 로그와 검증 결과를 초기화합니다.\n계속하시겠습니까?",
                "초기화 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            ResetAllLogData();
            UpdateFileListPanel();
        }

        // 💡 우측 메뉴 슬라이드 토글
        // ─────────────────────────────────────────────
        private void InitializeSideToggle()
        {
            _btnSideToggle = new Button
            {
                Text = "〈",
                Font = new System.Drawing.Font("Malgun Gothic", 8f, System.Drawing.FontStyle.Bold),
                Size = new System.Drawing.Size(16, 80),
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(70, 100, 160),
                ForeColor = System.Drawing.Color.White,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
            };
            _btnSideToggle.FlatAppearance.BorderSize = 0;
            _btnSideToggle.Click += (s, e) => ToggleSidePanel();

            splitContainer2.Panel2.Controls.Add(_btnSideToggle);
            _btnSideToggle.BringToFront();

            // 초기 상태: panel2 너비 0으로 숨김
            panel2.Visible = false;
            panel2.Width = 0;
            _sidePanelVisible = false;
        }

        private void ToggleSidePanel()
        {
            // 💡 애니메이션 중 중복 클릭 방지
            _btnSideToggle.Enabled = false;

            _sidePanelVisible = !_sidePanelVisible;
            _btnSideToggle.Text = _sidePanelVisible ? "〉" : "〈";

            if (_sidePanelVisible)
            {
                // 열기: 0 → SIDE_PANEL_WIDTH 로 단계적 확장
                panel2.Width = 0;
                panel2.Visible = true;
                AnimateSidePanel(opening: true);
            }
            else
            {
                // 닫기: SIDE_PANEL_WIDTH → 0 으로 단계적 축소
                AnimateSidePanel(opening: false);
            }
        }

        private void AnimateSidePanel(bool opening)
        {
            // 💡 Timer로 10ms마다 15px씩 이동 → 약 100ms에 완료 (자연스럽고 빠른 속도)
            const int STEP = 15;
            const int INTERVAL = 10;

            var timer = new System.Windows.Forms.Timer { Interval = INTERVAL };
            timer.Tick += (s, e) =>
            {
                if (opening)
                {
                    int next = panel2.Width + STEP;
                    if (next >= SIDE_PANEL_WIDTH)
                    {
                        panel2.Width = SIDE_PANEL_WIDTH;
                        timer.Stop();
                        timer.Dispose();
                        _btnSideToggle.Enabled = true;
                    }
                    else
                    {
                        panel2.Width = next;
                    }
                }
                else
                {
                    int next = panel2.Width - STEP;
                    if (next <= 0)
                    {
                        panel2.Width = 0;
                        panel2.Visible = false;
                        timer.Stop();
                        timer.Dispose();
                        _btnSideToggle.Enabled = true;
                    }
                    else
                    {
                        panel2.Width = next;
                    }
                }
            };
            timer.Start();
        }

        // ─────────────────────────────────────────────
        // 💡 파일 목록 패널: tabPage1(전체 탭) 상단에 접이식 UI로 추가
        // ─────────────────────────────────────────────
        private void InitializeFileListPanel()
        {
            const int TOGGLE_HEIGHT = 24;

            // 토글 헤더 버튼
            var btnToggle = new Button
            {
                Text = "▶  로드된 파일  (0개)",
                Font = new System.Drawing.Font("Malgun Gothic", 8.5f),
                Height = TOGGLE_HEIGHT,
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245),
                ForeColor = System.Drawing.Color.FromArgb(80, 80, 80),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Top
            };
            btnToggle.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
            btnToggle.FlatAppearance.BorderSize = 1;
            btnToggle.Name = "btnFileListToggle";

            // 파일 태그가 들어갈 FlowLayoutPanel (초기에는 숨김)
            _fileTagContainer = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(4),
                BackColor = System.Drawing.Color.FromArgb(250, 250, 250),
                Visible = false,
                Dock = DockStyle.Top
            };

            // 💡 Dock: Top/Fill 충돌 회피 전략:
            // _fileListPanel은 tabPage1에 절대좌표(Anchor)로 배치하고,
            // olvValidatorRawLog의 위치/크기를 패널 높이에 맞춰 직접 조정한다.
            _fileListPanel = new Panel
            {
                Location = new System.Drawing.Point(0, 0),
                Height = TOGGLE_HEIGHT,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(0)
            };
            _fileListPanel.Controls.Add(_fileTagContainer);
            _fileListPanel.Controls.Add(btnToggle);

            // olvValidatorRawLog를 Dock:Fill에서 Anchor로 전환하여 패널 아래부터 채우도록 설정
            olvValidatorRawLog.Dock = DockStyle.None;
            olvValidatorRawLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // 패널 높이 반영해서 그리드 위치/크기 계산
            Action recalcLayout = () =>
            {
                int panelH = _fileListPanel.Height;
                int tabW = tabPage1.ClientSize.Width;
                int tabH = tabPage1.ClientSize.Height;
                _fileListPanel.Width = tabW;
                olvValidatorRawLog.Location = new System.Drawing.Point(0, panelH);
                olvValidatorRawLog.Size = new System.Drawing.Size(tabW, tabH - panelH);
            };

            // 토글 클릭 시 펼침/접기 + 레이아웃 재계산
            btnToggle.Click += (s, e) =>
            {
                _fileListExpanded = !_fileListExpanded;
                _fileTagContainer.Visible = _fileListExpanded;
                _fileListPanel.Height = _fileListExpanded
                    ? TOGGLE_HEIGHT + _fileTagContainer.Height
                    : TOGGLE_HEIGHT;
                btnToggle.Text = (_fileListExpanded ? "▼" : "▶") +
                    $"  로드된 파일  ({_loadedFilePaths.Count}개)";
                recalcLayout();
            };

            // tabPage1 크기 변경 시 레이아웃 재계산 (폼 리사이즈 대응)
            tabPage1.SizeChanged += (s, e) => recalcLayout();

            tabPage1.Controls.Add(_fileListPanel);
            tabPage1.Controls.Add(olvValidatorRawLog);

            // 초기 레이아웃 적용
            recalcLayout();
        }

        /// <summary>
        /// 파일 목록 패널을 현재 _loadedFilePaths 기준으로 갱신합니다.
        /// </summary>
        private void UpdateFileListPanel()
        {
            if (_fileTagContainer == null) return;

            _fileTagContainer.Controls.Clear();

            foreach (string path in _loadedFilePaths)
            {
                bool isFolder = System.IO.Directory.Exists(path);
                string displayName = isFolder
                    ? "📁 " + System.IO.Path.GetFileName(path)
                    : "📄 " + System.IO.Path.GetFileName(path);

                var tag = new Label
                {
                    Text = displayName,
                    AutoSize = true,
                    Font = new System.Drawing.Font("Malgun Gothic", 8.5f),
                    ForeColor = System.Drawing.Color.FromArgb(40, 80, 140),
                    BackColor = System.Drawing.Color.FromArgb(230, 240, 255),
                    Padding = new Padding(6, 3, 6, 3),
                    Margin = new Padding(3, 3, 3, 3),
                    Cursor = Cursors.Default,
                    BorderStyle = BorderStyle.FixedSingle
                };
                _fileTagContainer.Controls.Add(tag);
            }

            var btnToggle = _fileListPanel?.Controls["btnFileListToggle"] as Button;

            // 💡 자동으로 펼치지 않음 - 사용자가 필요할 때 직접 펼치는 방식
            // 파일 수는 헤더 텍스트로 표시해서 접힌 상태에서도 몇 개 로드됐는지 확인 가능
            if (btnToggle != null)
                btnToggle.Text = (_fileListExpanded ? "▼" : "▶") +
                    $"  로드된 파일  ({_loadedFilePaths.Count}개)";

            // 💡 태그 추가 후 FlowLayoutPanel 높이가 확정되므로 그 시점에 패널/그리드 레이아웃 재계산
            _fileTagContainer.PerformLayout();
            if (_fileListPanel != null)
            {
                const int TOGGLE_HEIGHT = 24;
                _fileListPanel.Height = _fileListExpanded
                    ? TOGGLE_HEIGHT + _fileTagContainer.Height
                    : TOGGLE_HEIGHT;

                int panelH = _fileListPanel.Height;
                int tabW = tabPage1.ClientSize.Width;
                int tabH = tabPage1.ClientSize.Height;
                _fileListPanel.Width = tabW;
                olvValidatorRawLog.Location = new System.Drawing.Point(0, panelH);
                olvValidatorRawLog.Size = new System.Drawing.Size(tabW, tabH - panelH);
            }
        }

        /// <summary>
        /// 로드된 로그 데이터, 검증 결과, UI를 전부 초기 상태로 되돌립니다.
        /// </summary>
        private void ResetAllLogData()
        {
            // 데이터 초기화
            _validatorRawLogList.Clear();
            _loadedFilePaths.Clear();

            // 그리드 초기화
            olvValidatorRawLog.ClearObjects();
            olvValidatorRawLog.RowFormatter = null;
            olvValidationResult.ClearObjects();
            _currentSelectedStartLineNo = -1;

            // 유닛 탭 초기화 (tabPage1 제외)
            while (tabControl1.TabPages.Count > 1)
                tabControl1.TabPages.RemoveAt(1);

            // 변칙 경고 레이블 초기화
            lblAnomalyWarning.Visible = false;

            // 날짜 콤보박스 초기화
            cmbDateFilter.Items.Clear();

            // 파일 목록 패널 접기
            _fileListExpanded = false;
            if (_fileTagContainer != null) _fileTagContainer.Visible = false;
            if (_fileTagContainer != null) _fileTagContainer.Controls.Clear();

            var btnToggle = _fileListPanel?.Controls["btnFileListToggle"] as Button;
            if (btnToggle != null) btnToggle.Text = "▶  로드된 파일  (0개)";

            // 💡 패널 높이를 접힌 상태(24px)로 복원하고 그리드 레이아웃 재계산
            if (_fileListPanel != null)
            {
                _fileListPanel.Height = 24;
                int tabW = tabPage1.ClientSize.Width;
                int tabH = tabPage1.ClientSize.Height;
                _fileListPanel.Width = tabW;
                olvValidatorRawLog.Location = new System.Drawing.Point(0, 24);
                olvValidatorRawLog.Size = new System.Drawing.Size(tabW, tabH - 24);
            }
        }

        #endregion

        #region 🌳 2. 트리뷰 제어 및 실시간 런타임 리로드 인터락 엔진

        private void InitializeScenarioTreeViewInterlock()
        {
            if (!Directory.Exists(_scenarioBaseDirectory))
            {
                Directory.CreateDirectory(_scenarioBaseDirectory);
            }

            treeScenarioGroup.Nodes.Clear();
            treeScenarioGroup.Font = new System.Drawing.Font("Malgun Gothic", 10.0f, System.Drawing.FontStyle.Regular);

            var rootNode = new TreeNode("📁 Scenarios") { Tag = _scenarioBaseDirectory };

            string[] subDirectories = Directory.GetDirectories(_scenarioBaseDirectory);
            foreach (var dir in subDirectories)
            {
                var subNode = new TreeNode($"📁 {Path.GetFileName(dir)}") { Tag = dir };
                rootNode.Nodes.Add(subNode);
            }

            treeScenarioGroup.Nodes.Add(rootNode);
            treeScenarioGroup.ExpandAll();
            treeScenarioGroup.AfterSelect += TreeScenarioGroup_AfterSelect;

            treeScenarioGroup.SelectedNode = rootNode;
        }

        private void TreeScenarioGroup_AfterSelect(object sender, TreeViewEventArgs e)
        {
            LoadFilesFromTargetNode(e.Node);
        }

        /// <summary>
        /// 지정된 노드의 물리 경로를 기반으로 우측 시나리오 유닛 목록을 채우는 격리 메서드
        /// </summary>
        private void LoadFilesFromTargetNode(TreeNode node)
        {
            if (node == null || node.Tag == null) return;
            string targetDirectoryPath = node.Tag.ToString();

            try
            {
                if (!Directory.Exists(targetDirectoryPath)) return;

                string[] jsonFiles = Directory.GetFiles(targetDirectoryPath, "*.json");
                _currentRepositoryList = new List<ScenarioEvaluator>();

                foreach (var file in jsonFiles)
                {
                    _currentRepositoryList.Add(new ScenarioEvaluator
                    {
                        ScenarioName = Path.GetFileNameWithoutExtension(file),
                        Status = EvaluationResultStatus.Ready,
                        Steps = new List<ScenarioStepModel>()
                    });
                }

                olvScenarioRepository.SetObjects(_currentRepositoryList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"자산 동적 로드 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 💡 [브로커 수신 핸들러] 편집기단에서 저장 신호가 오면, 현재 선택된 트리 폴더 문맥을 그대로 유지한 채 목록만 강제 갱신합니다.
        /// </summary>
        private void OnRuntimeScenarioRefresh()
        {
            if (treeScenarioGroup.InvokeRequired)
            {
                treeScenarioGroup.Invoke(new MethodInvoker(() => LoadFilesFromTargetNode(treeScenarioGroup.SelectedNode)));
            }
            else
            {
                LoadFilesFromTargetNode(treeScenarioGroup.SelectedNode);
            }
        }

        #endregion

        #region ⚙️ 3. 검증 실행 엔진 구역

        private void btnStartValidation_Click(object sender, EventArgs e)
        {
            olvValidatorRawLog.RowFormatter = null;
            _currentSelectedStartLineNo = -1;
            olvValidatorRawLog.Refresh();

            if (_validatorRawLogList == null || _validatorRawLogList.Count == 0)
            {
                MessageBox.Show("No log files loaded.\nPlease drop log files on the left grid.", "Validation Guard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var checkedObjects = olvScenarioRepository.CheckedObjects;
            if (checkedObjects == null || checkedObjects.Count == 0)
            {
                MessageBox.Show("No scenario selected.\nPlease check at least one scenario.", "Validation Guard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (treeScenarioGroup.SelectedNode == null || treeScenarioGroup.SelectedNode.Tag == null) return;
            string currentActiveDirectory = treeScenarioGroup.SelectedNode.Tag.ToString();

            try
            {
                var checkedEvaluators = checkedObjects.Cast<ScenarioEvaluator>();
                var reports = _logValidatorService.RunValidation(_validatorRawLogList, checkedEvaluators, currentActiveDirectory);
                olvValidationResult.SetObjects(reports);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Validation error:\n{ex.Message}", "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ⚙️ 4. 세팅 연동형 유닛 탭 동적 스캔 및 그리드 생성 엔진

        // 💡 [최종 조립 정규식 엔진] 세팅 데이터 기반으로 런타임에 빌드될 전역 정규식 객체
        private Regex _dynamicUnitRegex;

        /// <summary>
        /// 세팅 창의 설비 조합 리스트를 기반으로 최적화된 컴파일 정규식을 조립합니다.
        /// </summary>
        private void BuildDynamicRegexPattern()
        {
            var config = LogValidatorConfigManager.Current;
            var combinedList = config.CombinedEquipmentList;

            if (combinedList == null || combinedList.Count == 0)
            {
                // 💡 [선택적 매칭(?:\-\d+)? 이식 기본 패턴 가드]
                _dynamicUnitRegex = new Regex(@"J1[EAFP](?:STO|OHS|CNV)\d+(?:\-\d+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return;
            }

            string escapedTypes = string.Join("|", combinedList.Select(Regex.Escape));

            // 💡 [교정된 핵심 정규식 패턴]
            // 규격: (설비조합)(숫자1자이상) + [선택부: (-숫자1자이상)]
            // 예시: J1ESTO12345 (매칭 성공), J1ESTO12345-101 (매칭 성공)
            string finalPattern = $@"\b({escapedTypes})\d+(?:\-\d+)?\b";

            _dynamicUnitRegex = new Regex(finalPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 파일 드롭 완료 후 호출되어 탭과 그리드를 유닛별로 찢어발겨 분배하는 통합 제어 메커니즘
        /// </summary>
        private void BuildUnitTabsAndGrids(List<RawLogModel> totalLogs)
        {
            if (totalLogs == null || totalLogs.Count == 0) return;

            // 0. 스캔 시작 전 정규식 패턴을 최신 세팅 사양으로 동적 리로드
            BuildDynamicRegexPattern();

            // UI 대량 갱신 시 깜빡임 및 렌더링 병목을 방지하기 위한 레이아웃 가드 개시
            tabControl1.SuspendLayout();

            // 1. [초기화 인터락] 첫 번째 탭(index 0: [전체/tabPage1])을 제외한 기존의 모든 동적 유닛 탭 서브셋 축출
            while (tabControl1.TabPages.Count > 1)
            {
                tabControl1.TabPages.RemoveAt(1);
            }

            // 2. [고속 1회 스캔] 단 한 번의 루프로 각 로그 행의 본문에서 유닛 ID를 추출하여 매핑
            foreach (var log in totalLogs)
            {
                if (string.IsNullOrEmpty(log.LogMessage))
                {
                    log.UnitID = "SYSTEM";
                    continue;
                }

                Match match = _dynamicUnitRegex.Match(log.LogMessage);
                if (match.Success)
                {
                    log.UnitID = match.Value.ToUpper(); // J1FCNV12301-107 형태로 락인
                }
                else
                {
                    log.UnitID = "SYSTEM"; // 설비 코드가 없는 공통 시스템 로그 분류
                }
            }

            // 3. [LINQ GroupBy 고속 격리] UnitID 별로 데이터 서브셋 그룹화 연산 및 정렬
            var unitGroups = totalLogs.GroupBy(l => l.UnitID).OrderBy(g => g.Key);

            // 4. [동적 UI 이식 시퀀스] 그룹별로 TabPage와 ObjectListView 그리드를 실시간 생성하여 Fill 바인딩
            foreach (var group in unitGroups)
            {
                string unitName = group.Key;
                List<RawLogModel> groupData = group.ToList();

                // 탭 페이지 생성 및 기본 흰색 도색
                TabPage newPage = new TabPage(unitName);
                newPage.BackColor = System.Drawing.Color.White;

                // 💡 [순정 호환 런타임 New 인스턴스] 동적 ObjectListView 레이아웃 락인
                ObjectListView unitGrid = new ObjectListView();
                unitGrid.Dock = DockStyle.Fill;
                unitGrid.View = View.Details;
                unitGrid.FullRowSelect = true;
                unitGrid.GridLines = true;
                unitGrid.Font = olvValidatorRawLog.Font; // 메인 그리드와 폰트 통일
                unitGrid.ShowItemToolTips = false;
                unitGrid.ShowGroups = false; // 파란색 그룹화 제거

                // 마스터 그리드(olvValidatorRawLog)에 설정되어 있는 정교한 컬럼 구조(Line, Log Message)를 그대로 이식
                foreach (OLVColumn col in olvValidatorRawLog.Columns)
                {
                    OLVColumn newCol = new OLVColumn(col.Text, col.AspectName);
                    newCol.Width = col.Width;
                    newCol.TextAlign = col.TextAlign;
                    unitGrid.AllColumns.Add(newCol);
                }
                unitGrid.RebuildColumns();

                // 이 유닛 그룹에 해당하는 데이터셋 바인딩 수송
                unitGrid.BeginUpdate();
                unitGrid.SetObjects(groupData);
                unitGrid.EndUpdate();

                // 동적으로 생성한 그리드를 탭에 얹고, 그 탭을 좌측 메인 탭컨트롤에 최종 연동 결합
                newPage.Controls.Add(unitGrid);
                tabControl1.TabPages.Add(newPage);
            }
            // 💡 [여기에 연동 호출부 삽입]: 탭 분할이 완결된 직후 변칙 탐지 엔진을 돌립니다.
            RunAnomalyDetectionEngine(totalLogs);

            // 모든 UI 결합 공정 완료 후 레이아웃 락 해제 및 최종 출력
            tabControl1.ResumeLayout();
        }

        #endregion

        #region 🚨 5. 변칙 로그 혼입 탐지 및 경고 인터락 엔진 (상시 표출형 부모 호기 유연 모드)

        /// <summary>
        /// 단일 파일 내에 타 설비 로그가 비정상적으로 섞여 들어왔는지 부모 호기 기준으로 검증하고 결과를 상시 표출합니다.
        /// </summary>
        private void RunAnomalyDetectionEngine(List<RawLogModel> totalLogs)
        {
            // [상태 A 방어]: 파일이 비어있거나 파싱 실패 시 초기화 가드
            if (totalLogs == null || totalLogs.Count == 0)
            {
                lblAnomalyWarning.Visible = false;
                toolTipAnomaly.RemoveAll();
                return;
            }

            // 💡 [도메인 인터락 가드]: 하이픈(-) 뒷자리를 잘라내어 "순수 부모 호기명"을 반환하는 로컬 함수
            string GetParentUnit(string unitId)
            {
                if (string.IsNullOrEmpty(unitId) || unitId == "SYSTEM") return "SYSTEM";
                int hyphenIdx = unitId.IndexOf('-');
                return hyphenIdx > 0 ? unitId.Substring(0, hyphenIdx) : unitId;
            }

            // 1. 순수 시스템 로그("SYSTEM")를 제외한 데이터에서 '부모 호기 ID' 기준으로 지분율 집계
            var validParents = totalLogs
                .Where(l => !string.IsNullOrEmpty(l.UnitID) && l.UnitID != "SYSTEM")
                .GroupBy(l => GetParentUnit(l.UnitID))
                .Select(g => new { ParentID = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // 호기 식별이 불가능한 순수 공통 덤프 파일일 경우의 가드
            if (validParents.Count == 0)
            {
                lblAnomalyWarning.Visible = true;
                lblAnomalyWarning.Text = "Abnormal Logs: 0 (System Only)";
                lblAnomalyWarning.ForeColor = System.Drawing.Color.DimGray;
                toolTipAnomaly.RemoveAll();
                return;
            }

            // 💡 [대표 부모 호기 락인]: 지분율 1위인 부모 호기(예: J1FSTO11308)를 이 파일의 마스터 주인으로 정의
            string masterParentID = validParents[0].ParentID;

            // 2. 부모 호기 자체가 마스터랑 다른 이질적인 행들만 "오염된 변칙 로그"로 적출
            var abnormalLogs = totalLogs
                .Where(l => l.UnitID != "SYSTEM" && GetParentUnit(l.UnitID) != masterParentID)
                .OrderBy(l => l.LineNo)
                .ToList();

            int anomalyCount = abnormalLogs.Count;

            // 3. [상태 B & C 분기 트리거 가동]: 결과 값에 따른 상시 가시화 제어
            lblAnomalyWarning.Visible = true;
            toolTipAnomaly.RemoveAll();

            if (anomalyCount == 0)
            {
                // 🟢 [상태 B: 무결성 검증 통과 영수증 표출]
                lblAnomalyWarning.Text = "Abnormal Logs: 0";
                lblAnomalyWarning.ForeColor = System.Drawing.Color.SeaGreen; // 차분하고 안전한 그린 시그널
                toolTipAnomaly.SetToolTip(lblAnomalyWarning, $"[Master Parent Unit: {masterParentID}]\n백엔드 무결성 스캔 완료: 이질적인 타 호기 데이터가 발견되지 않았습니다.");
            }
            else
            {
                // 🔴 [상태 C: 변칙 데이터 혼입 감지]
                lblAnomalyWarning.Text = $"⚠️ Abnormal Logs: {anomalyCount}";
                lblAnomalyWarning.ForeColor = System.Drawing.Color.Crimson; // 강렬한 경고 레드

                StringBuilder tipBuilder = new StringBuilder();
                tipBuilder.AppendLine($"[Master Parent Unit: {masterParentID}]");
                tipBuilder.AppendLine("Detected foreign unit logs in this file:");
                tipBuilder.AppendLine("-----------------------------------------");

                var displayLimit = abnormalLogs.Take(15);
                foreach (var ab in displayLimit)
                {
                    tipBuilder.AppendLine($"Line {ab.LineNo,-5} : ({ab.UnitID})");
                }

                if (anomalyCount > 15)
                {
                    tipBuilder.AppendLine($"... and {anomalyCount - 15} more lines.");
                }

                toolTipAnomaly.SetToolTip(lblAnomalyWarning, tipBuilder.ToString());
            }
        }

        #endregion

        #region 🔍 6. 런타임 실시간 정규식 본문 필터링 엔진

        private void InitializeRuntimeFilter()
        {
            if (txtLogFilter == null) return;

            // 💡 입력할 때마다 실시간으로 현재 보고 있는 탭 그리드의 본문을 압축합니다.
            txtLogFilter.TextChanged += (s, e) => ApplyFilterToActiveGrid();

            // 💡 탭을 전환했을 때도 검색어 필터 상태가 그대로 유지되어 동기화되도록 바인딩
            tabControl1.SelectedIndexChanged += (s, e) => ApplyFilterToActiveGrid();
        }

        private void ApplyFilterToActiveGrid()
        {
            string keyword = txtLogFilter.Text;

            // 현재 보고 있는 탭의 그리드 추출
            ObjectListView activeGrid = olvValidatorRawLog;
            if (tabControl1.SelectedIndex > 0 && tabControl1.SelectedTab != null)
            {
                var subGrid = tabControl1.SelectedTab.Controls.OfType<ObjectListView>().FirstOrDefault();
                if (subGrid != null) activeGrid = subGrid;
            }

            if (string.IsNullOrWhiteSpace(keyword))
            {
                // 필터 해제 및 전체 복원
                activeGrid.ModelFilter = null;
                activeGrid.DefaultRenderer = null;
            }
            else
            {
                try
                {
                    // ObjectListView 순정 고속 정규식 스캐너 컴파일
                    var filter = TextMatchFilter.Regex(activeGrid, keyword);
                    activeGrid.ModelFilter = filter;

                    // 일치하는 단어 그래픽 하일라이팅 렌더러 장착
                    activeGrid.DefaultRenderer = new HighlightTextRenderer(filter);
                }
                catch (Exception)
                {
                    // 사용자가 정규식 대괄호 '['를 입력하고 닫지 않는 등 
                    // 타이핑 도중 발생하는 일시적 문법 예외는 UI 멈춤 방지를 위해 침묵 흡수
                }
            }
        }

        #endregion











        private void LogValidatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 좀비 메모리 방지
                ScenarioEventBroker.OnScenarioSaved -= OnRuntimeScenarioRefresh;

                // 폼이 파괴되기 전에 ObjectListView가 소유한 내부 툴팁 컨트롤 자원의 
                // 이벤트 연결을 강제로 해제하여 무효한 폰트 핸들(HDC) 참조 연산을 전면 차단합니다.
                if (olvValidatorRawLog != null) olvValidatorRawLog.CellToolTipShowing -= null;
                if (olvScenarioRepository != null) olvScenarioRepository.CellToolTipShowing -= null;

                if (olvValidationResult != null)
                {
                    olvValidationResult.CellToolTipShowing -= null;
                    // 결과 그리드의 컨텍스트 메뉴 바인딩 해제
                    olvValidationResult.ContextMenuStrip = null;
                }

                olvValidatorRawLog?.Dispose();
                olvScenarioRepository?.Dispose();
                olvValidationResult?.Dispose();
            }
            catch (Exception)
            {
                // 메인 UI 스레드 종료 루프에 절대 간섭을 주지 않기 위해 예외 흡수
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            // 현재 트리에서 선택된 시나리오 폴더를 탐색기로 열기
            if (treeScenarioGroup.SelectedNode?.Tag == null) return;

            string folderPath = treeScenarioGroup.SelectedNode.Tag.ToString();
            if (!System.IO.Directory.Exists(folderPath))
            {
                MessageBox.Show("Folder not found.", "Open Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            System.Diagnostics.Process.Start("explorer.exe", folderPath);
        }

        private void btnOpenScenarioEditor_Click(object sender, EventArgs e)
        {
            // 시나리오 편집기를 빈 상태로 열기 (파일 없이 새로 작성 모드)
            // 기존에는 시나리오 우클릭 → 편집으로만 접근 가능했으나 여기서 바로 열 수 있음
            if (treeScenarioGroup.SelectedNode?.Tag == null)
            {
                MessageBox.Show("Please select a scenario folder from the tree first.",
                    "Scenario Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string folderPath = treeScenarioGroup.SelectedNode.Tag.ToString();

            // 선택된 시나리오가 있으면 해당 파일로, 없으면 빈 편집기로 오픈
            var selectedEval = olvScenarioRepository.SelectedObject as ScenarioEvaluator;
            if (selectedEval != null)
            {
                string fullPath = System.IO.Path.Combine(folderPath, $"{selectedEval.ScenarioName}.json");
                new LogScenarioForm(fullPath).Show();
            }
            else
            {
                new LogScenarioForm().Show();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close(); // FormClosing 이벤트가 자동으로 호출되어 리소스 정리
        }
    }
}