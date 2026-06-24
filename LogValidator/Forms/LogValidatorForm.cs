using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using GateHelper.LogValidator.Core;
using GateHelper.LogValidator.Models;

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
                if (files.Length == 0) return;

                try
                {
                    _validatorRawLogList = _logParser.ParseLogFile(files[0]);
                    olvValidatorRawLog.SetObjects(_validatorRawLogList);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"현장 로그 수신 파싱 결함:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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