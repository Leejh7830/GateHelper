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

            // [컬럼 2: 검증 상태 명세 보정 - 가시성을 위해 대문자 강제 정렬]
            var colStatus = new OLVColumn("Status", "Status") { Width = 100, TextAlign = HorizontalAlignment.Center };
            colStatus.AspectGetter = row =>
            {
                if (row is ScenarioEvaluator parent) return parent.Status.ToString().ToUpper();
                if (row is StepValidationReport child) return child.StepStatus?.ToUpper();
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
            // 💡 [Material Skin 탈취 방어 및 포맷 이벤트 엔진 강제 기동]
            // =========================================================================
            olvValidationResult.UseCellFormatEvents = true; // 👈 1. 컴포넌트의 포맷 이벤트 발생 잠금 해제 (핵심 필수)
            olvValidationResult.OwnerDraw = true;           // 👈 2. Material Skin의 테마 배경색 덮어쓰기를 찢고 나오는 자체 렌더링 권한 확보

            // =========================================================================
            // 💡 [인지 공학 UX 적용: Status 컬럼 한정 정밀 셀 타겟팅 뱃지 시스템]
            // =========================================================================
            // 행 전체를 물들이는 원시적 방식을 차단하고, 오직 Status 셀 1개만 위계별로 도색합니다.
            olvValidationResult.FormatCell += (s, e) =>
            {
                if (e.Column == colStatus)
                {
                    if (e.Model is ScenarioEvaluator parent)
                    {
                        // 💡 [CS0117 에러 원천 봉쇄] 실제 모델에 정의된 대문자 식별자(FAILED, SUCCESS)로 완벽 매핑
                        if (parent.Status == EvaluationResultStatus.FAILED)
                        {
                            // 🚨 위계 1 (부모 불량): 명시적인 알러트 레드 꽉 찬 뱃지
                            e.SubItem.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
                            e.SubItem.ForeColor = System.Drawing.Color.White;
                            e.SubItem.Font = new System.Drawing.Font(olvValidationResult.Font, System.Drawing.FontStyle.Bold);
                        }
                        else if (parent.Status == EvaluationResultStatus.SUCCESS)
                        {
                            // 🍃 위계 3 (부모 정상): 차분한 소프트 민트 텍스트 (정상의 침묵)
                            e.SubItem.BackColor = olvValidationResult.BackColor;
                            e.SubItem.ForeColor = System.Drawing.Color.MediumSeaGreen;
                            e.SubItem.Font = new System.Drawing.Font(olvValidationResult.Font, System.Drawing.FontStyle.Bold);
                        }
                    }
                    else if (e.Model is StepValidationReport child)
                    {
                        if (child.StepStatus == "FAILED")
                        {
                            // 🔍 위계 2 (자식 불량 스텝): 버건디 레드 텍스트 하이라이트
                            e.SubItem.BackColor = olvValidationResult.BackColor;
                            e.SubItem.ForeColor = System.Drawing.Color.FromArgb(176, 42, 55);
                            e.SubItem.Font = new System.Drawing.Font(olvValidationResult.Font, System.Drawing.FontStyle.Bold);
                        }
                        else if (child.StepStatus == "SUCCESS")
                        {
                            // 🔇 위계 4 (자식 정상 스텝): 순정 기본 텍스트 컬러 유지
                            e.SubItem.BackColor = olvValidationResult.BackColor;
                            e.SubItem.ForeColor = olvValidationResult.ForeColor;
                        }
                    }
                }
            };

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

                    // 💡 [클로저 캡처 락인] 가상 모드 재사용 스레드 오류를 차단하기 위해 값을 로컬 상수로 전이 보존
                    int targetStartLineNo = childReport.StartLineNo;
                    _currentSelectedStartLineNo = targetStartLineNo;

                    // 💡 [참조 복사 가드] 성공 행 번호 컬렉션을 해시셋(HashSet)으로 전환하여 검색 속도를 O(1)로 극대화합니다.
                    var matchedLinesSet = new HashSet<int>(childReport.MatchedLineNumbers ?? new List<int>());

                    // [고속 인덱스 역추적] 첫 번째 행의 물리 인덱스 추출
                    int targetJumpIndex = -1;
                    int actualLogicalStartLine = childReport.StartLineNo;

                    if (_validatorRawLogList != null && _validatorRawLogList.Count > 0)
                    {
                        for (int i = 0; i < _validatorRawLogList.Count; i++)
                        {
                            if (_validatorRawLogList[i] == null) continue;

                            if (_validatorRawLogList[i].LineNo == actualLogicalStartLine ||
                                _validatorRawLogList[i].LineNo == actualLogicalStartLine - 1 ||
                                _validatorRawLogList[i].LineNo == actualLogicalStartLine - 2)
                            {
                                if (_validatorRawLogList[i].LogMessage != null &&
                                    _validatorRawLogList[i].LogMessage.Contains("CreateLogIfNecessary"))
                                {
                                    targetJumpIndex = i;
                                    break;
                                }
                            }
                        }

                        if (targetJumpIndex == -1)
                        {
                            for (int i = 0; i < _validatorRawLogList.Count; i++)
                            {
                                if (_validatorRawLogList[i] != null && _validatorRawLogList[i].LineNo == childReport.StartLineNo)
                                {
                                    targetJumpIndex = i;
                                    break;
                                }
                            }
                        }
                    }

                    if (targetJumpIndex != -1)
                    {
                        // 💡 [1순위 원인 제거: OS 선택 박스의 렌더링 덮어쓰기 개입 원천 차단]
                        // olvValidatorRawLog.SelectedIndex = targetJumpIndex; 구문을 완전히 삭제합니다.
                        // 행을 '선택(Select)' 상태로 만들지 않아야 Windows 공통 컨트롤이 노란색 커스텀 배경을 흰색/회색으로 지우지 않습니다.
                        olvValidatorRawLog.SelectedObject = null;

                        int preferredTopIndex = targetJumpIndex - 3;
                        if (preferredTopIndex < 0) preferredTopIndex = 0;

                        olvValidatorRawLog.TopItemIndex = preferredTopIndex;
                        olvValidatorRawLog.EnsureVisible(targetJumpIndex);

                        // =========================================================================
                        // 💡 2. [핀포인트 마스킹 인터락 셋업 - 무결성 해시 매칭 명세]
                        // =========================================================================
                        olvValidatorRawLog.RowFormatter = rowObject =>
                        {
                            var logModel = rowObject.RowObject as RawLogModel;
                            if (logModel != null)
                            {
                                // A. 백엔드 엔진이 검증 완료한 '진짜 성공 라인 번호 세트'에 존재하는지 O(1) 대조
                                bool isMatchedLine = matchedLinesSet.Contains(logModel.LineNo);

                                // B. 파서 오차 가드: 사이클의 명시적 첫 기동 줄인지 대조
                                bool isExplicitStartLine = (logModel.LineNo == targetStartLineNo);

                                // 💡 시각적 다리 역할을 하던 이물질 로직(isSeparationLineNoise)을 전면 삭제했습니다.
                                // 이제 155(참) -> 156(거짓) -> 157(참) 순서로 데이터에 적힌 6개 행만 정직하게 물듭니다.
                                if (isMatchedLine || isExplicitStartLine)
                                {
                                    rowObject.BackColor = System.Drawing.Color.FromArgb(255, 243, 205);
                                    rowObject.ForeColor = System.Drawing.Color.Black;
                                }
                                else
                                {
                                    rowObject.BackColor = olvValidatorRawLog.BackColor;
                                    rowObject.ForeColor = olvValidatorRawLog.ForeColor;
                                }
                            }
                        };

                        // 뷰 레이어 즉시 리프레시 강제 명령
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