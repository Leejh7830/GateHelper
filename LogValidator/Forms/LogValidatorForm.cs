using BrightIdeasSoftware;
using GateHelper.LogValidator.Core;
using GateHelper.LogValidator.Models;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GateHelper.LogValidator
{
    public partial class LogValidatorForm : MaterialForm
    {
        private readonly MaterialSkinManager _skinManager = MaterialSkinManager.Instance;
        private readonly LogParser _logParser = new LogParser();
        private readonly LogValidatorEngine _validatorEngine = new LogValidatorEngine();

        private List<RawLogModel> _validatorRawLogList = new List<RawLogModel>();
        private List<ScenarioEvaluator> _currentRepositoryList = new List<ScenarioEvaluator>();
        private readonly string _scenarioBaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_meta", "Scenarios");

        private int _currentSelectedStartLineNo = -1;

        public LogValidatorForm()
        {
            InitializeComponent();
            _skinManager.AddFormToManage(this);

            InitializeValidatorRawLogGridView();
            InitializeScenarioRepositoryGridView();
            InitializeValidationResultGridView();
            InitializeValidatorDropZone();

            InitializeScenarioTreeViewInterlock();

            // 💡 [실시간 리로드 인터락] 편집기에서 저장 버튼을 누르면, 검증기가 열려있는 상태에서도 목록이 자동 동기화됩니다.
            ScenarioEventBroker.OnScenarioSaved += OnRuntimeScenarioRefresh;
        }

        #region 🛠 1. 그리드 뼈대 셋업 및 우클릭 컨텍스트 메뉴 바인딩

        private void InitializeValidatorRawLogGridView()
        {
            olvValidatorRawLog.Columns.Clear();
            olvValidatorRawLog.Font = new System.Drawing.Font("Malgun Gothic", 10.5f, System.Drawing.FontStyle.Regular);

            var colLineNo = new OLVColumn("Line", "LineNo") { Width = 60, TextAlign = HorizontalAlignment.Center };
            var colMessage = new OLVColumn("Log Message", "LogMessage") { Width = 500 };

            olvValidatorRawLog.Columns.AddRange(new ColumnHeader[] { colLineNo, colMessage });
            olvValidatorRawLog.View = View.Details;
            olvValidatorRawLog.FullRowSelect = true;
            olvValidatorRawLog.GridLines = true;

            olvValidatorRawLog.ShowItemToolTips = false;
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

            var colProgress = new OLVColumn("Progress", "Progress") { Width = 110, TextAlign = HorizontalAlignment.Center };
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

            int targetStartLineNo = childReport.StartLineNo;
            _currentSelectedStartLineNo = targetStartLineNo;
            var matchedLinesSet = new HashSet<int>(childReport.MatchedLineNumbers ?? new List<int>());

            int targetJumpIndex = -1;
            for (int i = 0; i < _validatorRawLogList.Count; i++)
            {
                if (_validatorRawLogList[i] != null && _validatorRawLogList[i].LineNo == childReport.StartLineNo)
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
                        bool isMatchedLine = matchedLinesSet.Contains(logModel.LineNo);
                        bool isExplicitStartLine = (logModel.LineNo == targetStartLineNo);

                        rowObject.BackColor = (isMatchedLine || isExplicitStartLine) ? System.Drawing.Color.FromArgb(255, 243, 205) : olvValidatorRawLog.BackColor;
                        rowObject.ForeColor = (isMatchedLine || isExplicitStartLine) ? System.Drawing.Color.Black : olvValidatorRawLog.ForeColor;
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

            olvValidatorRawLog.DragDrop += (s, e) =>
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files == null || files.Length == 0) return;

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    // 💡 [정렬 위치 교정 가드] 루프 진입 전에는 순수 데이터 누적만 진행합니다.
                    foreach (string filePath in files)
                    {
                        if (!File.Exists(filePath)) continue;
                        string fileName = Path.GetFileName(filePath).ToUpper();

                        List<RawLogModel> parsedSegment = null;

                        if (fileName.Contains("VARIABLE_TRACE"))
                        {
                            parsedSegment = _logParser.ParseLogFile(filePath);
                            foreach (var log in parsedSegment) log.LogType = "PLC";
                        }
                        else if (fileName.Contains("ERROR") || fileName.Contains("ALARM"))
                        {
                            parsedSegment = _logParser.ParseLogFile(filePath);
                            foreach (var log in parsedSegment) log.LogType = "ALARM";
                        }
                        else if (fileName.Contains("TRANSFER"))
                        {
                            parsedSegment = _logParser.ParseLogFile(filePath);
                            foreach (var log in parsedSegment) log.LogType = "TRANSFER";
                        }
                        else
                        {
                            parsedSegment = _logParser.ParseLogFile(filePath);
                            foreach (var log in parsedSegment) log.LogType = "UNKNOWN";
                        }

                        if (parsedSegment != null && parsedSegment.Count > 0)
                        {
                            _validatorRawLogList.AddRange(parsedSegment);
                        }
                    }

                    // 마스터 리스트를 시간(LogTime) 정렬 구조로 확실하게 락인(Lock-in)합니다.
                    _validatorRawLogList = _validatorRawLogList.OrderBy(log => log.LogTime).ToList();

                    // 고속 마스터 렌더링 동기화
                    olvValidatorRawLog.BeginUpdate();
                    olvValidatorRawLog.SetObjects(_validatorRawLogList);
                    olvValidatorRawLog.EndUpdate();

                    // 💡 정렬이 완결된 무결한 마스터 리스트를 동적 탭 엔진에 주입 수송합니다.
                    BuildUnitTabsAndGrids(_validatorRawLogList);

                    Cursor.Current = Cursors.Default;
                }
                catch (Exception ex)
                {
                    Cursor.Current = Cursors.Default;
                    olvValidatorRawLog.EndUpdate();
                    MessageBox.Show($"현장 복합 로그 수신 및 타임라인 병합 결함:\n{ex.Message}", "Drop Interlock Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
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
            // 새 검증이 가동되는 순간, 좌측 대용량 뷰어에 걸려있던 RowFormatter 규칙을 완전히 무효화하고
            // 그래픽 카드를 새로고침하여 이전 사이클의 노란색 마스킹 흔적을 깨끗하게 청소합니다.
            olvValidatorRawLog.RowFormatter = null;
            _currentSelectedStartLineNo = -1; // 전역 필드 버퍼도 완전히 리셋
            olvValidatorRawLog.Refresh();

            if (_validatorRawLogList == null || _validatorRawLogList.Count == 0)
            {
                MessageBox.Show("검증을 진행할 현장 Raw 로그 파일이 로드되지 않았습니다.\n좌측 그리드에 로그 파일을 드롭해 주십시오.", "Validation Guard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var checkedObjects = olvScenarioRepository.CheckedObjects;
            if (checkedObjects == null || checkedObjects.Count == 0)
            {
                MessageBox.Show("검증을 실행할 시나리오 유닛이 선택되지 않았습니다.\n체크박스를 최소 1개 이상 선택해 주십시오.", "Validation Guard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (treeScenarioGroup.SelectedNode == null || treeScenarioGroup.SelectedNode.Tag == null) return;
            string currentActiveDirectory = treeScenarioGroup.SelectedNode.Tag.ToString();

            var targetEvaluators = new List<ScenarioEvaluator>();

            try
            {
                foreach (var obj in checkedObjects)
                {
                    var eval = obj as ScenarioEvaluator;
                    if (eval == null) continue;

                    string fullPath = Path.Combine(currentActiveDirectory, $"{eval.ScenarioName}.json");
                    if (!File.Exists(fullPath)) continue;

                    string jsonString = File.ReadAllText(fullPath);
                    var loadedSteps = JsonSerializer.Deserialize<List<ScenarioStepModel>>(jsonString);

                    if (loadedSteps != null)
                    {
                        eval.Steps = loadedSteps;
                        eval.CurrentStepIndex = 0;
                        eval.Status = EvaluationResultStatus.Ready;
                        targetEvaluators.Add(eval);
                    }
                }

                var reports = _validatorEngine.Validate(_validatorRawLogList, targetEvaluators);
                olvValidationResult.SetObjects(reports);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"일괄 검증 연산 중 아키텍처 크래시 발생:\n{ex.Message}", "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // 가드: 만약 세팅 파일이 비어있거나 로드가 안 되었다면 에러 방지용 기본 패턴 락인
                _dynamicUnitRegex = new Regex(@"J1[EAFP](?:STO|OHS|CNV)\d+-\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return;
            }

            // 세팅에서 조합된 리스트(J1ESTO, J1ASTO 등)를 정규식 그룹 패턴으로 안전하게 이식
            string escapedTypes = string.Join("|", combinedList.Select(Regex.Escape));

            // 명명 규칙 패턴 조립: (설비조합패턴)(호기번호)-(서브유닛번호)
            string finalPattern = $@"\b({escapedTypes})\d+-\d+\b";

            // Compiled 옵션을 주어 10만 줄이 넘는 대용량 로그 스캔 시에도 렉(Lag)이 걸리지 않도록 고속 연산 가드
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

            // 모든 UI 결합 공정 완료 후 레이아웃 락 해제 및 최종 출력
            tabControl1.ResumeLayout();
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
    }
}