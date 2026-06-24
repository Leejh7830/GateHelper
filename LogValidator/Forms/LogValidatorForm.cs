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
            olvValidationResult.ShowGroups = false;       // 그룹화 분절 노이즈 완전 제거 락
            olvValidationResult.ShowItemToolTips = false;  // 폰트 크래시 예외 봉쇄 가드

            // 💡 1. [트리 데이터 맵 정렬 명세]
            // 부모 객체와 자식 객체의 데이터 구조가 다르므로, 각 컬럼이 노드 타입에 맞게 가변 분기 처리되도록 AspectGetter를 락인(Lock-in)합니다.

            // [컬럼 1: 시나리오명 / 세부스텝명 명세]
            var colName = new OLVColumn("Scenario Name", "ScenarioName") { Width = 220 };
            colName.AspectGetter = row =>
            {
                if (row is ScenarioEvaluator parent) return parent.ScenarioName;
                if (row is StepValidationReport child) return child.StepDisplayHeader; // 계단식 하위 노드명
                return string.Empty;
            };

            // [컬럼 2: 검증 상태 명세]
            var colStatus = new OLVColumn("Status", "Status") { Width = 100, TextAlign = HorizontalAlignment.Center };
            colStatus.AspectGetter = row =>
            {
                if (row is ScenarioEvaluator parent) return parent.Status.ToString();
                if (row is StepValidationReport child) return child.StepStatus;
                return string.Empty;
            };

            // [컬럼 3: 진척도 명세 보정]
            var colProgress = new OLVColumn("Progress", "Progress") { Width = 110, TextAlign = HorizontalAlignment.Center };
            colProgress.AspectGetter = row =>
            {
                if (row is ScenarioEvaluator parent) return parent.Progress;       // "성공 12건 / 총 14건" 노출
                if (row is StepValidationReport child) return child.StepProgress;   // "5 / 5" 또는 "2 / 5" 노출
                return string.Empty;
            };

            // [컬럼 4: 메시지 명세 보정]
            var colMsg = new OLVColumn("Validation Message", "Message") { Width = 450 };
            colMsg.AspectGetter = row =>
            {
                if (row is ScenarioEvaluator parent) return parent.Message;       // "총 14회 발생 시퀀스가..." 노출
                if (row is StepValidationReport child) return child.StepMessage;   // 세부 유실 요약 내용 노출
                return string.Empty;
            };

            olvValidationResult.Columns.AddRange(new ColumnHeader[] { colName, colStatus, colProgress, colMsg });
            olvValidationResult.View = View.Details;
            olvValidationResult.FullRowSelect = true;
            olvValidationResult.GridLines = true;

            // =========================================================================
            // 💡 2. [TreeListView 핵심 계층형 트리 파이프라인 구성]
            // =========================================================================

            // [트리 가드 1] 특정 행 왼쪽에 확장 화살표(▶)를 띄울지 여부 결정 규칙
            olvValidationResult.CanExpandGetter = row =>
            {
                return (row is ScenarioEvaluator parent) && parent.StepReports.Count > 0;
            };

            // [트리 가드 2] 사용자가 확장을 트리거했을 때 하위로 뿜어낼 자식 리스트 전달 파이프라인
            olvValidationResult.ChildrenGetter = row =>
            {
                if (row is ScenarioEvaluator parent) return parent.StepReports;
                return null;
            };

            // =========================================================================
            // 💡 4. [더블클릭 정밀 인터락: 첫 행 이동 + 성공 행만 선택적 핀포인트 마스킹]
            // =========================================================================
            olvValidationResult.DoubleClick += (s, e) =>
            {
                if (olvValidationResult.SelectedObject == null) return;

                if (olvValidationResult.SelectedObject is ScenarioEvaluator)
                {
                    olvValidationResult.ToggleExpansion(olvValidationResult.SelectedObject);
                    return;
                }

                if (olvValidationResult.SelectedObject is StepValidationReport childReport)
                {
                    if (childReport.StartLineNo <= 0) return;

                    // 💡 [전역 락 전이] 더블클릭한 사이클의 시작 줄 번호를 클래스 멤버 변수에 확실하게 박아둡니다.
                    _currentSelectedStartLineNo = childReport.StartLineNo;

                    // [고속 인덱스 역추적] 첫 번째 행의 물리 인덱스 추출
                    int targetJumpIndex = -1;
                    int actualLogicalStartLine = childReport.StartLineNo;

                    for (int i = 0; i < _validatorRawLogList.Count; i++)
                    {
                        // 리스트를 돌며 헤더 번호와 일치하거나, 혹은 파서 오차로 밀린 진짜 시작 줄(CreateLogIfNecessary) 객체를 인덱스로 선점
                        if (_validatorRawLogList[i].LineNo == actualLogicalStartLine ||
                            _validatorRawLogList[i].LineNo == actualLogicalStartLine - 1 ||
                            _validatorRawLogList[i].LineNo == actualLogicalStartLine - 2)
                        {
                            if (_validatorRawLogList[i].LogMessage != null && _validatorRawLogList[i].LogMessage.Contains("CreateLogIfNecessary"))
                            {
                                targetJumpIndex = i; // 진짜 기동 행 인덱스 강탈 확정
                                break;
                            }
                        }
                    }

                    // 만약 위 정밀 필터로도 못 잡았다면 순정 동기화 백업 처리
                    if (targetJumpIndex == -1)
                    {
                        for (int i = 0; i < _validatorRawLogList.Count; i++)
                        {
                            if (_validatorRawLogList[i].LineNo == childReport.StartLineNo)
                            {
                                targetJumpIndex = i;
                                break;
                            }
                        }
                    }

                    if (targetJumpIndex != -1)
                    {
                        // =========================================================================
                        // 💡 1. [스크롤 센터링 / 최상단 리포지셔닝 정밀 제어 인터락]
                        // =========================================================================
                        olvValidatorRawLog.SelectedObject = null;
                        olvValidatorRawLog.SelectedIndex = targetJumpIndex; // 타겟 행 파랗게 선택

                        // 단순 EnsureVisible을 사용하면 타겟 행이 화면 최하단에 간신히 걸치게 됩니다.
                        // 이를 방지하기 위해 가상 렌더링 엔진 내부의 TopItemIndex(화면 맨 위에 보여줄 인덱스)를 
                        // 우리가 찾은 진짜 첫 행 인덱스로 강제 고정(Override)해 버립니다.

                        // 첫 행을 화면 맨 위보다 살짝 아래(예: 위에서 3~4번째 줄)에 배치하여 
                        // 문맥을 더 쉽게 인지하고 싶다면 `targetJumpIndex - 3` 정도로 오프셋을 뺄 수 있습니다.
                        int preferredTopIndex = targetJumpIndex - 3;
                        if (preferredTopIndex < 0) preferredTopIndex = 0;

                        // 가상 리스트 뷰의 스크롤 위치를 물리적으로 강제 점프시킵니다.
                        olvValidatorRawLog.TopItemIndex = preferredTopIndex;

                        // 백업 가드로 컴포넌트 렌더링 영역 가시성 확정
                        olvValidatorRawLog.EnsureVisible(targetJumpIndex);

                        // =========================================================================
                        // 💡 2. [핀포인트 마스킹 인터락 셋업]
                        // =========================================================================
                        olvValidatorRawLog.RowFormatter = rowObject =>
                        {
                            var logModel = rowObject.RowObject as RawLogModel;
                            if (logModel != null && childReport.MatchedLineNumbers != null)
                            {
                                // A. 엔진이 수집한 성공 목록에 정확히 포함되어 있는지 검사
                                bool isMatchedLine = childReport.MatchedLineNumbers.Contains(logModel.LineNo);

                                // B. 헤더에 표시된 공식 시작 줄 번호와 완벽히 일치하는지 검사
                                bool isExplicitStartLine = (logModel.LineNo == _currentSelectedStartLineNo);

                                // C. [정밀 범위 가드] 파서 오차로 인해 누수된 '바로 직전 1~2줄 범위' 내에 존재하는지 검사
                                bool isWithinOffsetRange = (logModel.LineNo == _currentSelectedStartLineNo - 1 || logModel.LineNo == _currentSelectedStartLineNo - 2);

                                // D. 해당 행이 실제 최초 공정 기동 메시지 포맷인지 인지 검사
                                bool isActualStartMessage = logModel.LogMessage != null && logModel.LogMessage.Contains("CreateLogIfNecessary");

                                // 💡 [최종 인터락 조건] 
                                // 매칭 성공 라인이거나, 공식 시작 줄이거나, 
                                // '오프셋 범위 안(상방 2줄 이내)에 있으면서 동시에 기동 메시지인 경우'만 핀포인트 마스킹 허용
                                if (isMatchedLine || isExplicitStartLine || (isWithinOffsetRange && isActualStartMessage))
                                {
                                    rowObject.BackColor = System.Drawing.Color.FromArgb(255, 243, 205);
                                    rowObject.ForeColor = System.Drawing.Color.Black;
                                }
                                else
                                {
                                    // 조건에 맞지 않는 상단의 무관한 행(RenderButton 등)들은 확실하게 화이트/다크 순정 스타일로 환원
                                    rowObject.BackColor = olvValidatorRawLog.BackColor;
                                    rowObject.ForeColor = olvValidatorRawLog.ForeColor;
                                }
                            }
                        };

                        // 뷰 레이어 즉시 강제 리프레시
                        olvValidatorRawLog.Refresh();
                    }
                }
            };
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