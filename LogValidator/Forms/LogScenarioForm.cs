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

        public LogScenarioForm()
        {
            InitializeComponent();
            _skinManager.AddFormToManage(this);

            InitializeRawLogGridView();
            InitializeUnitRepositoryGridView();
            InitializeScenarioLadderGridView();
            InitializeDropZone();
        }

        #region 🛠 1. 그리드 뼈대 셋업 및 드롭존 레이어 교정

        private void InitializeRawLogGridView()
        {
            var colLineNo = new OLVColumn("Line", "LineNo") { Width = 60, TextAlign = HorizontalAlignment.Center };
            var colTime = new OLVColumn("Timestamp", "Timestamp") { Width = 150 };
            var colMessage = new OLVColumn("Log Message", "LogMessage") { Width = 600 };

            olvRawLog.Columns.AddRange(new ColumnHeader[] { colLineNo, colTime, colMessage });
            olvRawLog.View = View.Details;
            olvRawLog.FullRowSelect = true;
            olvRawLog.GridLines = true;
            olvRawLog.Visible = false;
            olvRawLog.ShowItemToolTips = false;
            olvUnitRepository.ShowItemToolTips = false;
            olvScenarioLadder.ShowItemToolTips = false;
        }

        private void InitializeUnitRepositoryGridView()
        {
            var colName = new OLVColumn("Unit Name", "EventName") { Width = 280 };

            olvUnitRepository.Columns.AddRange(new ColumnHeader[] { colName });
            olvUnitRepository.View = View.Details;
            olvUnitRepository.FullRowSelect = true;
            olvUnitRepository.GridLines = true;

            olvUnitRepository.SelectedIndexChanged += olvUnitRepository_SelectedIndexChanged;
        }

        private void InitializeScenarioLadderGridView()
        {
            var colStep = new OLVColumn("Step", "StepNo") { Width = 60, TextAlign = HorizontalAlignment.Center };
            var colName = new OLVColumn("Assigned Unit", "EventName") { Width = 150 };
            var colPattern = new OLVColumn("Applied Pattern", "MaskingPattern") { Width = 450 };

            olvScenarioLadder.Columns.AddRange(new ColumnHeader[] { colStep, colName, colPattern });
            olvScenarioLadder.View = View.Details;
            olvScenarioLadder.FullRowSelect = true;
            olvScenarioLadder.GridLines = true;
            olvScenarioLadder.AllowDrop = true;
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
        }

        #endregion

        #region 📂 2. 좌측 Raw 로그 수신 및 클릭 파싱 핸들러

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
                _rawLogList = _logParser.ParseLogFile(files[0]);
                pnlDropZone.Visible = false;
                olvRawLog.Visible = true;
                olvRawLog.SetObjects(_rawLogList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"로그 파일 파싱 오류:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void olvRawLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (olvRawLog.SelectedObject == null) return;

            var selectedRow = olvRawLog.SelectedObject as RawLogModel;
            if (selectedRow == null || string.IsNullOrEmpty(selectedRow.LogMessage) || selectedRow.LogMessage == "*") return;

            _selectedTemplateForEdit = null;

            // 💡 이제 일반 TextBox(txtEventName)이므로 실시간 값 강제 대입 시에도 한글/영문 렌더링이 꼬이지 않습니다.
            txtEventName.Text = $"EVT_{selectedRow.LineNo}";
            RenderHighlightLog(rtbMaskedPreview, selectedRow.LogMessage);
        }

        private void RenderHighlightLog(RichTextBox rtb, string originalLog)
        {
            rtb.Clear();
            rtb.Text = originalLog;
            rtb.ForeColor = Color.White;

            MatchCollection matches = _logMasker.GetMaskingMatches(rtb.Text);
            foreach (Match match in matches)
            {
                rtb.Select(match.Index, match.Length);
                rtb.SelectionBackColor = Color.Yellow;
                rtb.SelectionColor = Color.Black;
            }
            rtb.Select(0, 0);
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

        #region ⛓ 4. [역연동 및 드래그] 대기소 클릭 수정 및 사다리 조립 인터락

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

            var newStep = new ScenarioStepModel
            {
                StepNo = _scenarioLadderList.Count + 1,
                EventName = droppedTemplate.EventName,
                MaskingPattern = droppedTemplate.MaskingPattern
            };

            _scenarioLadderList.Add(newStep);
            olvScenarioLadder.SetObjects(_scenarioLadderList);
        }

        #endregion

        #region 💾 5. 우측 하단 고도화된 시나리오 자동 저장 및 폴더 분리 매니징 엔진

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

                            // 💡 일반 텍스트 박스에 무결하게 로드된 파일명을 동기화합니다.
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
            _rawLogList?.Clear();
            _unitTemplateList?.Clear();
            _scenarioLadderList?.Clear();

            olvRawLog?.SetObjects(null);
            olvUnitRepository?.SetObjects(null);
            olvScenarioLadder?.SetObjects(null);
        }

    }
}