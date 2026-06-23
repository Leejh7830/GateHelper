using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GateHelper.LogValidator.Core;
using GateHelper.LogValidator.Models;

namespace GateHelper.LogValidator
{
    public partial class LogScenarioForm : MaterialForm
    {
        private readonly MaterialSkinManager _skinManager = MaterialSkinManager.Instance;

        private readonly LogParser _logParser = new LogParser();
        private readonly LogMasker _logMasker = new LogMasker();

        private List<RawLogModel> _rawLogList = new List<RawLogModel>();
        private List<UnitTemplateModel> _unitTemplateList = new List<UnitTemplateModel>();
        private List<ScenarioStepModel> _scenarioLadderList = new List<ScenarioStepModel>();

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
            colEQP.AspectGetter = row => ((ScenarioStepModel)row).Direction == "TX" ? ((ScenarioStepModel)row).EventName : string.Empty;

            // 💡 [화살표 왜곡 교정] 픽셀이 비뚤어지던 유니코드 문자대신 정중앙 대칭이 완벽히 유지되는 지시선 기호로 교체
            var colLine = new OLVColumn("Flow Line", "Direction") { Width = 90, TextAlign = HorizontalAlignment.Center };
            colLine.AspectGetter = row => ((ScenarioStepModel)row).Direction == "TX" ? "───▶" : "◀───";

            var colServer = new OLVColumn("SERVER", "EventName") { Width = 130, TextAlign = HorizontalAlignment.Center };
            colServer.AspectGetter = row => ((ScenarioStepModel)row).Direction == "RX" ? ((ScenarioStepModel)row).EventName : string.Empty;

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

            // 💡 자동 마스킹 분절 간섭 없이 순정 원본 raw 데이터를 다이렉트로 버퍼에 대피 및 바인딩
            _originalLogBackup = selectedRow.LogMessage;

            RenderHighlightLog(rtbMaskedPreview, selectedRow.LogMessage);
            InitializeBuilderContextMenu();
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

        private void olvScenarioLadder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(UnitTemplateModel))) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        private void olvScenarioLadder_DragDrop(object sender, DragEventArgs e)
        {
            var droppedTemplate = e.Data.GetData(typeof(UnitTemplateModel)) as UnitTemplateModel;
            if (droppedTemplate == null) return;

            // 💡 [드롭 좌표 정밀 제어 인터락] 전체 화면 절반이 아니라 'Flow Line(화살표)' 컬럼의 물리적 경계 좌표를 계산합니다.
            Point clientPoint = olvScenarioLadder.PointToClient(new Point(e.X, e.Y));

            // Step + EQP 컬럼의 폭 합산하여 화살표 시작 X좌표 산출 (50 + 130 = 180)
            int flowLineLeftBoundary = olvScenarioLadder.Columns[0].Width + olvScenarioLadder.Columns[1].Width;

            // 💡 마우스 커서가 화살표 기준 왼쪽이면 TX(EQP), 화살표를 포함한 오른쪽 영역이면 RX(SERVER)로 강제 확정
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

            InitializeScenarioLadderContextMenu();
        }

        private void olvScenarioLadder_DoubleClick(object sender, EventArgs e)
        {
            if (olvScenarioLadder.SelectedObject == null) return;

            var clickedStep = olvScenarioLadder.SelectedObject as ScenarioStepModel;
            if (clickedStep == null) return;

            clickedStep.Direction = (clickedStep.Direction == "TX") ? "RX" : "TX";
            olvScenarioLadder.RefreshObject(clickedStep);
        }

        // 💡 [우클릭 컨텍스트 메뉴] 사다리 단계 이동, 삭제 등 편집 기능 제공
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

            cms.Opening += (s, e) =>
            {
                bool hasSelection = (olvScenarioLadder.SelectedObject != null);
                menuMoveUp.Enabled = hasSelection;
                menuMoveDown.Enabled = hasSelection;
                menuDelete.Enabled = hasSelection;
            };

            cms.Items.Add(menuMoveUp);
            cms.Items.Add(menuMoveDown);
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(menuDelete);

            olvScenarioLadder.ContextMenuStrip = cms;
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

                string jsonString = JsonSerializer.Serialize(_scenarioLadderList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(targetFilePath, jsonString);

                MessageBox.Show($"시나리오가 자동으로 저장되었습니다.\n📂 위치: _meta/Scenarios/{scenarioName}.json", "Auto Save Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"시나리오 다이렉트 파일 출력 결함 발생:\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
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
                    try
                    {
                        string jsonString = File.ReadAllText(openFileDialog.FileName);
                        var loadedSteps = JsonSerializer.Deserialize<List<ScenarioStepModel>>(jsonString);

                        if (loadedSteps != null)
                        {
                            _scenarioLadderList = loadedSteps;
                            olvScenarioLadder.SetObjects(_scenarioLadderList);
                            txtScenarioName.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"시나리오 로드 결함 발생:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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

            // 우클릭 컨텍스트 가드 리프레시
            InitializeScenarioLadderContextMenu();
        }
    }
}