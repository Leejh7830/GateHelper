using BrightIdeasSoftware;
using GateHelper.LogValidator.Core;
using GateHelper.LogValidator.Models;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GateHelper.LogValidator
{
    public partial class LogScenarioForm : MaterialForm
    {
        private readonly MaterialSkinManager _skinManager = MaterialSkinManager.Instance;
        private readonly LogParser _logParser = new LogParser(); // 💡 ScenarioForm 드롭존에서 직접 사용

        private List<RawLogModel> _rawLogList = new List<RawLogModel>();
        private List<UnitTemplateModel> _unitTemplateList = new List<UnitTemplateModel>();
        private List<ScenarioStepModel> _scenarioLadderList = new List<ScenarioStepModel>();

        // 💡 스텝 행 드래그 순서 변경용 추적 필드
        private ScenarioStepModel _draggingStep = null;
        private Point _dragStartPoint;

        // 💡 우측 컨트롤 패널 슬라이드 토글용 필드
        private Button _btnSideToggle;
        private bool _sidePanelVisible = false;
        private const int SIDE_PANEL_WIDTH = 200; // pnlControlButtons 너비와 동일

        private UnitTemplateModel _selectedTemplateForEdit = null;

        private string _originalLogBackup = string.Empty; // [수동 마스킹 보존 락] 원본 로그 백업용

        public LogScenarioForm()
        {
            InitializeComponent();
            _skinManager.AddFormToManage(this);

            InitializeRawLogGridView();
            InitializeUnitRepositoryGridView();
            InitializeScenarioLadderGridView();
            InitializeDropZone();
            InitializeBuilderContextMenu(); // 💡 생성자에서 1회만 생성 (선택 시마다 재생성하던 버그 수정)
            InitializeSideToggle();         // 💡 우측 컨트롤 패널 슬라이드 토글
        }

        // 💡 [신규 UX 고도화 인터락] 검증기에서 우클릭 소환 시 파일 경로를 직접 물고 열리는 생성자
        public LogScenarioForm(string autoLoadFilePath) : this()
        {
            // 폼이 로드되어 완전히 활성화된 시점에 안전하게 파일 로드 엔진 트리거
            this.Load += (s, e) =>
            {
                if (!string.IsNullOrEmpty(autoLoadFilePath) && File.Exists(autoLoadFilePath))
                {
                    try
                    {
                        // 💡 기존 편집기 내부에 이미 무결하게 구현되어 있는 
                        // "파일 선택 후 리스트뷰에 세팅하는 로직"의 핵심 메서드를 직접 강제 호출합니다.
                        // (메서드명이 다를 경우 현재 편집기 내부의 JSON 로드 실행 메서드명으로 매핑해 주십시오)
                        LoadScenarioFile(autoLoadFilePath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"시나리오 자동 타겟 로드 실패: {ex.Message}");
                    }
                }
            };
        }

        #region 🛠 1. 그리드 뼈대 셋업 및 통신형 컬럼 세팅 (폰트 스케일 및 화살표 대칭 정렬 교정)

        private void InitializeRawLogGridView()
        {
            olvScenarioRawLog.Columns.Clear();

            var colLineNo = new OLVColumn("Line", "LineNo") { Width = 60, TextAlign = HorizontalAlignment.Center };
            var colMessage = new OLVColumn("Log Message", "LogMessage") { Width = 750 };

            olvScenarioRawLog.Columns.AddRange(new ColumnHeader[] { colLineNo, colMessage });
            olvScenarioRawLog.View = View.Details;
            olvScenarioRawLog.FullRowSelect = true;
            olvScenarioRawLog.GridLines = true;
            olvScenarioRawLog.Visible = false;

            // =========================================================================
            // 💡 [교정 인터락: 편집기 내부 핀포인트 구간 마스킹 엔진 강제 주입]
            // =========================================================================
            olvScenarioRawLog.RowFormatter = rowObject =>
            {
                var logModel = rowObject.RowObject as RawLogModel;
                if (logModel != null && _scenarioLadderList != null && _scenarioLadderList.Count > 0)
                {
                    // ① 우측 사다리 룰 세트에 등록된 패턴들과 일치하는지 전수 검사
                    bool isMatchedRule = false;
                    foreach (var step in _scenarioLadderList)
                    {
                        if (string.IsNullOrEmpty(step.MaskingPattern)) continue;

                        // 와일드카드 패턴을 정규식으로 안전하게 전환하여 대조
                        string cleanPattern = step.MaskingPattern.Replace("*", ".*");
                        if (System.Text.RegularExpressions.Regex.IsMatch(logModel.LogMessage, cleanPattern))
                        {
                            isMatchedRule = true;
                            break;
                        }
                    }

                    // ② [구간 가드 연산] 최초 기동 메시지(CreateLogIfNecessary)를 품고 있는 줄은 
                    // 인덱스 오프셋 왜곡에 상관없이 무조건 마스킹 플래그 강제 적용
                    bool isInitialSequenceZone = false;
                    if (logModel.LogMessage != null && logModel.LogMessage.Contains("CreateLogIfNecessary"))
                    {
                        isInitialSequenceZone = true;
                    }

                    // ③ [스타일 락인]
                    if (isMatchedRule || isInitialSequenceZone)
                    {
                        rowObject.BackColor = System.Drawing.Color.FromArgb(255, 243, 205);
                        rowObject.ForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        rowObject.BackColor = olvScenarioRawLog.BackColor;
                        rowObject.ForeColor = olvScenarioRawLog.ForeColor;
                    }
                }
            };
        }

        private void InitializeUnitRepositoryGridView()
        {
            olvUnitRepository.Columns.Clear();

            // 💡 [폰트 무결성 인터락] 사다리와 동일한 스케일의 맑은 고딕(Bold)을 주입하여 한글 찌그러짐 원천 차단
            olvUnitRepository.Font = new Font("Malgun Gothic", 10.5f, FontStyle.Bold);

            // 폰트가 커짐에 따라 리스트 행 높이(RowHeight)도 촘촘하지 않게 여유 공간을 부여 (선택 사항)
            olvUnitRepository.RowHeight = 30;

            var colName = new OLVColumn("Unit Name", "EventName")
            {
                Width = olvUnitRepository.Width - 25,
                TextAlign = HorizontalAlignment.Left
            };

            olvUnitRepository.Columns.AddRange(new ColumnHeader[] { colName });
            olvUnitRepository.View = View.Details;
            olvUnitRepository.FullRowSelect = true;
            olvUnitRepository.GridLines = true;

            olvUnitRepository.ShowGroups = false;
            olvUnitRepository.HeaderUsesThemes = false;
            olvUnitRepository.ShowItemToolTips = false;
            olvUnitRepository.SelectedIndexChanged += olvUnitRepository_SelectedIndexChanged;
        }

        private void InitializeScenarioLadderGridView()
        {
            olvScenarioLadder.Columns.Clear();

            // 💡 [폰트 무결성 인터락] 한글 축소 왜곡 및 자간 찌그러짐을 방지하기 위해 가독성이 검증된 맑은 고딕(Bold)으로 통합 스케일 셋업
            olvScenarioLadder.Font = new Font("Malgun Gothic", 10.5f, FontStyle.Bold);

            var colStep = new OLVColumn("Step", "StepNo") { Width = 50, TextAlign = HorizontalAlignment.Center };

            var colEQP = new OLVColumn("EQP", "EventName") { Width = 130, TextAlign = HorizontalAlignment.Center };
            // 1. EQP 컬럼 보정
            colEQP.AspectGetter = row => {
                var step = (ScenarioStepModel)row;
                string dir = step.Direction?.Trim().ToUpper(); // 💡 공백 제거 및 대문자 강제 변환
                return dir == "TX" ? step.EventName : string.Empty;
            };

            var colServer = new OLVColumn("SERVER", "EventName") { Width = 130, TextAlign = HorizontalAlignment.Center };
            // 2. SERVER 컬럼 보정
            colServer.AspectGetter = row => {
                var step = (ScenarioStepModel)row;
                string dir = step.Direction?.Trim().ToUpper(); // 💡 공백 제거 및 대문자 강제 변환
                return dir == "RX" ? step.EventName : string.Empty;
            };

            var colLine = new OLVColumn("Flow Line", "Direction") { Width = 90, TextAlign = HorizontalAlignment.Center };
            // 3. Flow Line(화살표) 컬럼 보정
            colLine.AspectGetter = row => {
                string dir = ((ScenarioStepModel)row).Direction?.Trim().ToUpper();
                return dir == "TX" ? "───▶" : "◀───";
            };


            var colPattern = new OLVColumn("Message Format / Rules", "MaskingPattern") { Width = 480 };
            colPattern.AspectGetter = row =>
            {
                string rawPattern = ((ScenarioStepModel)row).MaskingPattern;
                if (string.IsNullOrEmpty(rawPattern)) return string.Empty;

                string formatted = Regex.Replace(rawPattern, @"\d{2}:\d{2}:\d{2}", "[TIME]");
                formatted = Regex.Replace(formatted, @"set:\s*\d+", "set: [VALUE]");
                return formatted;
            };

            olvScenarioLadder.Columns.AddRange(new ColumnHeader[] { colStep, colEQP, colLine, colServer, colPattern });

            olvScenarioLadder.Sorting = SortOrder.None;
            foreach (OLVColumn col in olvScenarioLadder.Columns)
            {
                col.Sortable = false;
            }

            olvScenarioLadder.RowHeight = 45;

            olvScenarioLadder.View = View.Details;
            olvScenarioLadder.FullRowSelect = true;
            olvScenarioLadder.GridLines = true;
            olvScenarioLadder.AllowDrop = true;
            olvScenarioLadder.ShowItemToolTips = false;

            // 💡 행 드래그 순서 변경: MouseDown에서 DoDragDrop 직접 호출
            // IsSimpleDragSource 대신 직접 제어 → AllowDrop과 충돌 없음
            olvScenarioLadder.MouseDown += (s, me) => _dragStartPoint = me.Location;
            olvScenarioLadder.MouseMove += olvScenarioLadder_MouseMove;

            olvScenarioLadder.DoubleClick += olvScenarioLadder_DoubleClick;

            InitializeScenarioLadderContextMenu();
        }

        private void InitializeDropZone()
        {
            pnlDropZone.AllowDrop = true;
            pnlDropZone.DragEnter += PnlDropZone_DragEnter;
            pnlDropZone.DragDrop += PnlDropZone_DragDrop;

            lblRawLogDrag.AllowDrop = true;
            lblRawLogDrag.DragEnter += PnlDropZone_DragEnter;
            lblRawLogDrag.DragDrop += PnlDropZone_DragDrop;

            pnlDropZone.BringToFront();
            pnlDropZone.Dock = DockStyle.Fill;

            // 💡 [빌더 UI 가독성 보정] 이벤트네임 텍스트박스의 폰트 크기를 키우고 박스 높이를 자연스럽게 확장
            txtEventName.Font = new Font("Malgun Gothic", 11.0f, FontStyle.Regular);
        }

        #endregion

        #region 📂 2. 좌측 Raw 로그 수신 및 클릭 파싱 핸들러 (순정 데이터 이관 및 100% 수동 마스킹 엔진)

        private void PnlDropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        private void PnlDropZone_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) return;

            try
            {
                // 💡 Raw 로그 파서에서 번호와 전체 텍스트만 깔끔하게 받아와 바인딩
                _rawLogList = _logParser.ParseLogFile(files[0]);
                pnlDropZone.Visible = false;
                olvScenarioRawLog.Visible = true;
                olvScenarioRawLog.SetObjects(_rawLogList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"로그 파일 파싱 오류:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void olvRawLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (olvScenarioRawLog.SelectedObject == null) return;

            var selectedRow = olvScenarioRawLog.SelectedObject as RawLogModel;
            if (selectedRow == null || string.IsNullOrEmpty(selectedRow.LogMessage) || selectedRow.LogMessage == "*") return;

            _selectedTemplateForEdit = null;
            txtEventName.Text = $"EVT_{selectedRow.LineNo}";

            // 💡 순정 원본 raw 데이터를 버퍼에 대피 및 바인딩
            _originalLogBackup = selectedRow.LogMessage;

            RenderHighlightLog(rtbMaskedPreview, selectedRow.LogMessage);
            // 💡 컨텍스트메뉴는 생성자에서 1회 생성 완료 - 여기서 재생성 불필요
        }

        private void RenderHighlightLog(RichTextBox rtb, string originalLog)
        {
            int currentSelectionStart = rtb.SelectionStart;
            int currentSelectionLength = rtb.SelectionLength;

            rtb.Clear();
            rtb.Text = originalLog;
            rtb.ForeColor = Color.White;

            // 💡 무분별한 오토 마스킹을 전면 폐기하고, 오직 사용자가 지정한 가변 기호('*') 구역만 무결하게 추적하여 노란색 블록으로 강조합니다.
            MatchCollection manualMatches = Regex.Matches(rtb.Text, @"\*+");
            foreach (Match match in manualMatches)
            {
                rtb.Select(match.Index, match.Length);
                rtb.SelectionBackColor = Color.Yellow;
                rtb.SelectionColor = Color.Black;
            }

            rtb.Select(currentSelectionStart, currentSelectionLength);
        }

        private void InitializeBuilderContextMenu()
        {
            var cms = new ContextMenuStrip();

            // 메뉴 1: 우클릭 마스킹 처리 인터락
            var menuMask = new ToolStripMenuItem("선택 영역 마스킹 (*)", null, (s, e) =>
            {
                if (string.IsNullOrEmpty(rtbMaskedPreview.SelectedText)) return;

                int selStart = rtbMaskedPreview.SelectionStart;
                int selLen = rtbMaskedPreview.SelectionLength;
                string currentText = rtbMaskedPreview.Text;

                string modifiedText = currentText.Remove(selStart, selLen).Insert(selStart, "*");
                RenderHighlightLog(rtbMaskedPreview, modifiedText);

                rtbMaskedPreview.Select(selStart + 1, 0);
            });

            // 메뉴 2: 완전 원본 데이터 복원 롤백 스위치
            var menuUndo = new ToolStripMenuItem("원본 로그로 되돌리기 (Undo)", null, (s, e) =>
            {
                if (string.IsNullOrEmpty(_originalLogBackup)) return;
                RenderHighlightLog(rtbMaskedPreview, _originalLogBackup);
            });

            cms.Opening += (s, e) =>
            {
                menuMask.Enabled = (rtbMaskedPreview.SelectionLength > 0);
                menuUndo.Enabled = !string.IsNullOrEmpty(_originalLogBackup);
            };

            cms.Items.Add(menuMask);
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(menuUndo);

            rtbMaskedPreview.ContextMenuStrip = cms;
        }

        #endregion

        #region ⚙️ 3. [REGISTER / UPDATE] 인터락 분기 제어 엔진

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string eventName = txtEventName.Text.Trim().ToUpper();
            string patternText = rtbMaskedPreview.Text.Trim();

            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(patternText))
            {
                MessageBox.Show("이벤트명과 마스킹 패턴이 입력되지 않았습니다.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedTemplateForEdit != null)
            {
                if (_unitTemplateList.Exists(u => u != _selectedTemplateForEdit && u.EventName == eventName))
                {
                    MessageBox.Show("이미 존재하는 다른 유닛 명칭입니다.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _selectedTemplateForEdit.EventName = eventName;
                _selectedTemplateForEdit.MaskingPattern = patternText;

                olvUnitRepository.RefreshObject(_selectedTemplateForEdit);
                _selectedTemplateForEdit = null;
            }
            else
            {
                if (_unitTemplateList.Exists(u => u.EventName == eventName))
                {
                    MessageBox.Show("이미 등록된 유닛 명칭입니다. 다른 이름을 사용하십시오.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newUnit = new UnitTemplateModel { EventName = eventName, MaskingPattern = patternText };
                _unitTemplateList.Add(newUnit);
                olvUnitRepository.SetObjects(_unitTemplateList);
            }

            txtEventName.Text = string.Empty;
            rtbMaskedPreview.Clear();
        }

        #endregion

        #region ⛓ 4. 사다리 조립 인터락 및 우클릭 편집 엔진 (화살표 컬럼 기준 드롭 제어)

        private void olvUnitRepository_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (olvUnitRepository.SelectedObject == null) return;

            var selectedTemplate = olvUnitRepository.SelectedObject as UnitTemplateModel;
            if (selectedTemplate == null) return;

            _selectedTemplateForEdit = selectedTemplate;
            txtEventName.Text = selectedTemplate.EventName;
            RenderHighlightLog(rtbMaskedPreview, selectedTemplate.MaskingPattern);
        }

        private void olvUnitRepository_MouseDown(object sender, MouseEventArgs e)
        {
            if (olvUnitRepository.SelectedObject == null) return;

            var selectedTemplate = olvUnitRepository.SelectedObject as UnitTemplateModel;
            if (selectedTemplate == null) return;

            olvUnitRepository.DoDragDrop(selectedTemplate, DragDropEffects.Copy);
        }

        private void olvScenarioLadder_MouseMove(object sender, MouseEventArgs e)
        {
            // 💡 왼쪽 버튼 누른 채로 일정 거리 이상 움직이면 드래그 시작
            if (e.Button != MouseButtons.Left) return;
            if (Math.Abs(e.X - _dragStartPoint.X) < 4 && Math.Abs(e.Y - _dragStartPoint.Y) < 4) return;

            var item = olvScenarioLadder.GetItemAt(e.X, e.Y) as OLVListItem;
            if (item == null) item = olvScenarioLadder.SelectedItem as OLVListItem;

            var step = item?.RowObject as ScenarioStepModel;
            if (step == null) return;

            _draggingStep = step;
            // ScenarioStepModel 자체를 DataObject에 담아 DoDragDrop 실행
            olvScenarioLadder.DoDragDrop(step, DragDropEffects.Move);
            _draggingStep = null;
        }

        private void olvScenarioLadder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(UnitTemplateModel)))
                e.Effect = DragDropEffects.Copy;        // 유닛 목록 → 스텝 추가
            else if (e.Data.GetDataPresent(typeof(ScenarioStepModel)))
                e.Effect = DragDropEffects.Move;        // 스텝 행 → 순서 변경
            else
                e.Effect = DragDropEffects.None;
        }

        private void olvScenarioLadder_DragDrop(object sender, DragEventArgs e)
        {
            // ── Case 1: 유닛 목록에서 드래그 → 스텝 추가 ──
            var droppedTemplate = e.Data.GetData(typeof(UnitTemplateModel)) as UnitTemplateModel;
            if (droppedTemplate != null)
            {
                Point clientPoint = olvScenarioLadder.PointToClient(new Point(e.X, e.Y));
                int flowLineLeftBoundary = olvScenarioLadder.Columns[0].Width + olvScenarioLadder.Columns[1].Width;
                string determinedDirection = (clientPoint.X < flowLineLeftBoundary) ? "TX" : "RX";

                var newStep = new ScenarioStepModel
                {
                    StepNo = _scenarioLadderList.Count + 1,
                    EventName = droppedTemplate.EventName,
                    MaskingPattern = droppedTemplate.MaskingPattern,
                    Direction = determinedDirection
                };

                _scenarioLadderList.Add(newStep);
                olvScenarioLadder.SetObjects(_scenarioLadderList);
                RefreshStepTooltips();
                return;
            }

            // ── Case 2: 스텝 행 드래그 → 순서 변경 ──
            var draggedStep = e.Data.GetData(typeof(ScenarioStepModel)) as ScenarioStepModel;
            if (draggedStep == null) return;

            Point dropPoint = olvScenarioLadder.PointToClient(new Point(e.X, e.Y));
            var targetItem = olvScenarioLadder.GetItemAt(dropPoint.X, dropPoint.Y) as OLVListItem;

            int fromIndex = _scenarioLadderList.IndexOf(draggedStep);
            int toIndex = targetItem != null
                ? _scenarioLadderList.IndexOf(targetItem.RowObject as ScenarioStepModel)
                : _scenarioLadderList.Count - 1;

            if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex) return;

            _scenarioLadderList.RemoveAt(fromIndex);
            _scenarioLadderList.Insert(toIndex, draggedStep);

            ReIndexScenarioSteps();
            RefreshStepTooltips();
        }

        private void olvScenarioLadder_DoubleClick(object sender, EventArgs e)
        {
            if (olvScenarioLadder.SelectedObject == null) return;

            var clickedStep = olvScenarioLadder.SelectedObject as ScenarioStepModel;
            if (clickedStep == null) return;

            clickedStep.Direction = (clickedStep.Direction == "TX") ? "RX" : "TX";
            olvScenarioLadder.RefreshObject(clickedStep);
        }

        // 💡 [우클릭 컨텍스트 메뉴] 사다리 단계 이동, 삭제, 타임아웃 설정, Optional 토글
        private void InitializeScenarioLadderContextMenu()
        {
            var cms = new ContextMenuStrip();

            var menuMoveUp = new ToolStripMenuItem("Move Up", null, (s, e) =>
            {
                var selected = olvScenarioLadder.SelectedObject as ScenarioStepModel;
                if (selected == null) return;
                int index = _scenarioLadderList.IndexOf(selected);
                if (index <= 0) return;
                _scenarioLadderList.RemoveAt(index);
                _scenarioLadderList.Insert(index - 1, selected);
                ReIndexScenarioSteps();
            });

            var menuMoveDown = new ToolStripMenuItem("Move Down", null, (s, e) =>
            {
                var selected = olvScenarioLadder.SelectedObject as ScenarioStepModel;
                if (selected == null) return;
                int index = _scenarioLadderList.IndexOf(selected);
                if (index < 0 || index >= _scenarioLadderList.Count - 1) return;
                _scenarioLadderList.RemoveAt(index);
                _scenarioLadderList.Insert(index + 1, selected);
                ReIndexScenarioSteps();
            });

            var menuDelete = new ToolStripMenuItem("Delete Selected", null, (s, e) =>
            {
                var selected = olvScenarioLadder.SelectedObject as ScenarioStepModel;
                if (selected == null) return;
                _scenarioLadderList.Remove(selected);
                ReIndexScenarioSteps();
            });

            // 💡 TimeOut Setting: 이 스텝 매칭 후 다음 스텝까지 허용 대기 시간(초) 입력
            var menuTimeout = new ToolStripMenuItem("TimeOut Setting (Sec)", null, (s, e) =>
            {
                var selected = olvScenarioLadder.SelectedObject as ScenarioStepModel;
                if (selected == null) return;

                int index = _scenarioLadderList.IndexOf(selected);
                if (index == _scenarioLadderList.Count - 1)
                {
                    MessageBox.Show("The last step has no next step. Timeout cannot be set.",
                        "TimeOut N/A", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string currentVal = selected.TimeoutSeconds > 0 ? selected.TimeoutSeconds.ToString() : "";
                string input = ShowInputDialog(
                    $"[Step {selected.StepNo}] {selected.EventName}\n\n" +
                    $"Enter the allowed wait time (seconds) until the next step.\n" +
                    $"(Enter 0 or leave blank to disable timeout)",
                    "TimeOut Setting", currentVal);

                if (input == null) return;

                if (string.IsNullOrWhiteSpace(input) || input.Trim() == "0")
                    selected.TimeoutSeconds = 0;
                else if (double.TryParse(input.Trim(), out double seconds) && seconds > 0)
                    selected.TimeoutSeconds = seconds;
                else
                {
                    MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                olvScenarioLadder.RefreshObject(selected);
                RefreshStepTooltips();
            });

            // 💡 Optional Step Toggle: 이 스텝이 없어도 사이클 계속 진행할지 여부 토글
            var menuOptional = new ToolStripMenuItem("Optional Step Toggle", null, (s, e) =>
            {
                var selected = olvScenarioLadder.SelectedObject as ScenarioStepModel;
                if (selected == null) return;

                // 첫 스텝은 Optional 불가 (사이클 시작점이므로)
                int index = _scenarioLadderList.IndexOf(selected);
                if (index == 0)
                {
                    MessageBox.Show("The first step cannot be optional.\nIt is the cycle start point.",
                        "Optional N/A", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                selected.IsOptional = !selected.IsOptional;
                olvScenarioLadder.RefreshObject(selected);
                RefreshStepTooltips();
            });

            cms.Opening += (s, e) =>
            {
                bool hasSelection = (olvScenarioLadder.SelectedObject != null);
                menuMoveUp.Enabled = hasSelection;
                menuMoveDown.Enabled = hasSelection;
                menuDelete.Enabled = hasSelection;
                menuTimeout.Enabled = hasSelection;
                menuOptional.Enabled = hasSelection;
            };

            cms.Items.Add(menuMoveUp);
            cms.Items.Add(menuMoveDown);
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(menuTimeout);
            cms.Items.Add(menuOptional);
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(menuDelete);

            olvScenarioLadder.ContextMenuStrip = cms;
            RefreshStepTooltips();
        }

        // 💡 Timeout + Optional 설정을 통합해서 툴팁으로 표시
        private void RefreshStepTooltips()
        {
            olvScenarioLadder.ShowItemToolTips = true;
            olvScenarioLadder.CellToolTipGetter = (col, rowObj) =>
            {
                var step = rowObj as ScenarioStepModel;
                if (step == null) return null;

                var parts = new System.Collections.Generic.List<string>();

                if (step.TimeoutSeconds > 0)
                {
                    int idx = _scenarioLadderList.IndexOf(step);
                    if (idx >= 0 && idx < _scenarioLadderList.Count - 1)
                    {
                        var next = _scenarioLadderList[idx + 1];
                        parts.Add($"⏱ Timeout: {step.TimeoutSeconds}s  ({step.EventName} → {next.EventName})");
                    }
                }

                if (step.IsOptional)
                    parts.Add("〇 Optional: this step may be skipped");

                return parts.Count > 0 ? string.Join("\n", parts) : null;
            };
        }

        // 💡 간단한 텍스트 입력 다이얼로그 (별도 Form 없이 인라인 구현)
        private string ShowInputDialog(string message, string title, string defaultValue = "")
        {
            Form dlg = new Form
            {
                Width = 420,
                Height = 200,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label { Text = message, Left = 12, Top = 10, Width = 380, Height = 100, AutoSize = false };
            var txt = new TextBox { Text = defaultValue, Left = 12, Top = 118, Width = 180 };
            var btnOk = new Button { Text = "확인", Left = 200, Top = 115, Width = 80, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "취소", Left = 290, Top = 115, Width = 80, DialogResult = DialogResult.Cancel };

            dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
        }

        // 💡 [사다리 단계 재인덱싱] 드롭, 이동, 삭제 등으로 인해 StepNo가 꼬일 수 있으므로 항상 최신화
        private void ReIndexScenarioSteps()
        {
            for (int i = 0; i < _scenarioLadderList.Count; i++)
            {
                _scenarioLadderList[i].StepNo = i + 1;
            }
            olvScenarioLadder.SetObjects(_scenarioLadderList);
        }

        #endregion

        #region 💾 5. 우측 하단 시나리오 매니징 커맨드 엔진 (+NEW, LOAD, SAVE)

        private string GetScenarioDirectoryPath()
        {
            string baseMetaPath = Util.CreateMetaFolderAndGetPath();
            string scenarioFolderPath = Path.Combine(baseMetaPath, "Scenarios");

            if (!Directory.Exists(scenarioFolderPath))
            {
                Directory.CreateDirectory(scenarioFolderPath);
            }
            return scenarioFolderPath;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            if (_scenarioLadderList.Count > 0)
            {
                var dialogResult = MessageBox.Show("작성 중인 시나리오 사다리가 완전히 초기화됩니다. 계속하시겠습니까?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.No) return;
            }

            _scenarioLadderList.Clear();
            olvScenarioLadder.SetObjects(_scenarioLadderList);
            txtScenarioName.Text = string.Empty;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_scenarioLadderList.Count == 0)
            {
                MessageBox.Show("저장할 사다리 시퀀스 데이터가 비어 있습니다.", "Empty Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string scenarioName = txtScenarioName.Text.Trim();

            if (string.IsNullOrEmpty(scenarioName))
            {
                MessageBox.Show("시나리오 명칭을 반드시 먼저 입력해 주십시오.", "Required Scenario Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtScenarioName.Focus();
                return;
            }

            try
            {
                string targetDir = GetScenarioDirectoryPath();
                string targetFilePath = Path.Combine(targetDir, $"{scenarioName}.json");

                // 💡 DefaultIgnoreCondition: TimeoutSeconds=0, IsOptional=false처럼
                // 기본값인 필드는 JSON에서 생략 → 기존 시나리오 파일과 포맷 혼재 방지
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
                };
                string jsonString = JsonSerializer.Serialize(_scenarioLadderList, jsonOptions);
                File.WriteAllText(targetFilePath, jsonString);

                MessageBox.Show($"시나리오가 자동으로 저장되었습니다.\n📂 위치: _meta/Scenarios/{scenarioName}.json", "Auto Save Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 💡 [실시간 크로스 폼 인터락] 저장이 완료되었음을 글로벌 브로커를 통해 검증기 창에 즉각 통지
                Core.ScenarioEventBroker.PublishScenarioSaved();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"시나리오 다이렉트 파일 출력 결함 발생:\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            // 💡 1. [기존 안전 인터락 보존] 데이터 덮어쓰기 사전 가드
            if (_scenarioLadderList.Count > 0)
            {
                var dialogResult = MessageBox.Show("불러오기를 진행하면 현재 사다리가 덮어써집니다. 진행하시겠습니까?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.No) return;
            }

            string targetDir = GetScenarioDirectoryPath();

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = targetDir;
                openFileDialog.Filter = "Scenario Files (*.json)|*.json";
                openFileDialog.Title = "Load Scenario Steps from Scenarios Repository";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 💡 2. 추출한 독립 로드 메서드로 파일 경로만 패스
                    LoadScenarioFile(openFileDialog.FileName);
                }
            }
        }

        /// <summary>
        /// 💡 [추출된 독립 로드 마스터 엔진]
        /// 주입된 파일 경로를 기반으로 JSON 시나리오 자산을 파싱하여 사다리 그리드 및 UI에 매핑합니다.
        /// 일반 버튼 로드와 검증기(Validator) 우클릭 자동 소환 양쪽에서 이 메서드를 공유합니다.
        /// </summary>
        public void LoadScenarioFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            try
            {
                string jsonString = File.ReadAllText(filePath);
                var loadedSteps = JsonSerializer.Deserialize<List<ScenarioStepModel>>(jsonString);

                if (loadedSteps != null)
                {
                    // 💡 3. 기존 순정 바인딩 알고리즘 및 텍스트박스 맵 명세 완벽 보존
                    _scenarioLadderList = loadedSteps;
                    olvScenarioLadder.SetObjects(_scenarioLadderList);
                    txtScenarioName.Text = Path.GetFileNameWithoutExtension(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"시나리오 파일 자동 로드 파싱 결함:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #endregion

        private void LogScenarioForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 💡 [트루타입 글꼴 예외 원천 봉쇄 마스터 인터락]
                // 폼이 파괴되기 전에 ObjectListView가 소유한 내부 툴팁 컨트롤 자원을 
                // 강제로 물리적 디스포즈 처리하여 무효한 폰트 핸들(HDC) 참조 연산을 전면 차단합니다.

                if (olvScenarioRawLog != null) olvScenarioRawLog.CellToolTipShowing -= null;
                if (olvUnitRepository != null) olvUnitRepository.CellToolTipShowing -= null;

                if (olvScenarioLadder != null)
                {
                    olvScenarioLadder.CellToolTipShowing -= null;
                    // 사다리 그리드의 컨텍스트 메뉴 바인딩을 끊어 해제 순서 보장
                    olvScenarioLadder.ContextMenuStrip = null;
                }

                // 💡 ObjectListView 내부의 가상 윈도우 핸들 파괴 시점 충돌을 막기 위해 
                // 컨트롤 자체의 상위 디스포즈 파이프라인을 선행 트리거합니다.
                olvScenarioRawLog?.Dispose();
                olvUnitRepository?.Dispose();
                olvScenarioLadder?.Dispose();
            }
            catch (Exception)
            {
                // 메인 스레드 종료 루프에 간섭을 주지 않기 위해 예외 흡수 가드
            }
        }

        // 💡 [신규 UX 고도화] 유닛 저장소 더블클릭 시 사다리 최하단 즉시 인입 인터락 (기본값: EQP)
        private void olvUnitRepository_DoubleClick(object sender, EventArgs e)
        {
            if (olvUnitRepository.SelectedObject == null) return;

            var clickedTemplate = olvUnitRepository.SelectedObject as UnitTemplateModel;
            if (clickedTemplate == null) return;

            // 더블클릭 인입은 마우스 좌표가 없으므로 공정 표준 선행 신호인 "TX (EQP)"를 기본 방향으로 확정합니다.
            var newStep = new ScenarioStepModel
            {
                StepNo = _scenarioLadderList.Count + 1,
                EventName = clickedTemplate.EventName,
                MaskingPattern = clickedTemplate.MaskingPattern,
                Direction = "TX"
            };

            _scenarioLadderList.Add(newStep);
            olvScenarioLadder.SetObjects(_scenarioLadderList);

            RefreshStepTooltips(); // 💡 컨텍스트메뉴 재생성 불필요, 툴팁만 갱신
        }

        private void rtbMaskedPreview_TextChanged(object sender, EventArgs e)
        {
            // 본인의 실제 RichTextBox 컨트롤 명칭으로 대입하십시오.
            rtbMaskedPreview.ForeColor = Color.Black;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ─────────────────────────────────────────────
        // 💡 우측 컨트롤 패널 슬라이드 토글
        // txtScenarioName, lblCurrentScenario 포함된 pnlControlButtons를 슬라이드로 열고 닫음
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

            // splitContainer2.Panel2 (olvScenarioLadder + pnlControlButtons가 있는 패널)에 추가
            splitContainer2.Panel2.Controls.Add(_btnSideToggle);
            _btnSideToggle.BringToFront();

            // 초기 상태: pnlControlButtons 숨김 (너비 0으로 시작)
            pnlControlButtons.Visible = false;
            pnlControlButtons.Width = 0;
            _sidePanelVisible = false;
        }

        private void ToggleSidePanel()
        {
            _btnSideToggle.Enabled = false;
            _sidePanelVisible = !_sidePanelVisible;
            _btnSideToggle.Text = _sidePanelVisible ? "〉" : "〈";

            if (_sidePanelVisible)
            {
                pnlControlButtons.Width = 0;
                pnlControlButtons.Visible = true;
                AnimateSidePanel(opening: true);
            }
            else
            {
                AnimateSidePanel(opening: false);
            }
        }

        private void AnimateSidePanel(bool opening)
        {
            const int STEP = 15;
            const int INTERVAL = 10;

            var timer = new System.Windows.Forms.Timer { Interval = INTERVAL };
            timer.Tick += (s, e) =>
            {
                if (opening)
                {
                    int next = pnlControlButtons.Width + STEP;
                    if (next >= SIDE_PANEL_WIDTH)
                    {
                        pnlControlButtons.Width = SIDE_PANEL_WIDTH;
                        timer.Stop();
                        timer.Dispose();
                        _btnSideToggle.Enabled = true;
                    }
                    else
                    {
                        pnlControlButtons.Width = next;
                    }
                }
                else
                {
                    int next = pnlControlButtons.Width - STEP;
                    if (next <= 0)
                    {
                        pnlControlButtons.Width = 0;
                        pnlControlButtons.Visible = false;
                        timer.Stop();
                        timer.Dispose();
                        _btnSideToggle.Enabled = true;
                    }
                    else
                    {
                        pnlControlButtons.Width = next;
                    }
                }
            };
            timer.Start();
        }
    }
}