using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;

namespace GateHelper.Mgmt.ExcelAnalyzer
{
    public partial class ExcelAnalyzerForm : MaterialForm
    {
        private string _droppedFilePath = "";
        private ExcelAnalyzerConfig _config = new ExcelAnalyzerConfig();
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_meta", "excel_rules.json");
        
        // 파싱된 전체 데이터를 메모리에 캐싱
        private List<ExcelRowData> _parsedData = new List<ExcelRowData>();

        // 동적 툴팁 객체
        private ToolTip _checkboxToolTip = new ToolTip();

        public ExcelAnalyzerForm()
        {
            InitializeComponent();
            
            // 다크 테마 적용을 위해 MaterialSkinManager에 현재 폼 등록
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);

            InitUI();
            LoadConfig();
        }

        private void InitUI()
        {
            // 1. 드래그 앤 드롭 설정
            PnlDropFile.AllowDrop = true;
            PnlDropFile.DragEnter += PnlDropFile_DragEnter;
            PnlDropFile.DragDrop += PnlDropFile_DragDrop;

            // 1-1. 시트 콤보박스 이벤트 연결 (사용자가 디자이너에서 만든 CmbSheet)
            if (CmbSheet != null)
            {
                CmbSheet.SelectedIndexChanged += CmbSheet_SelectedIndexChanged;
            }

            // 2. 콤보박스 초기 상태
            CmbMachineCol.Items.Clear();
            CmbNameCol.Items.Clear();
            CmbValueCol.Items.Clear();
            if (CmbDescCol != null) CmbDescCol.Items.Clear();

            // 3. 버튼 이벤트 연결 (화면 전환)
            BtnStartRuleSetup.Click += BtnStartRuleSetup_Click;
            BtnStartAnalyze.Click += BtnStartAnalyze_Click;
            BtnBackToHome1.Click += (s, e) => PnlDropFile.BringToFront();
            BtnBackToHome2.Click += (s, e) => PnlDropFile.BringToFront();

            // 3-1. 규칙 설정 이벤트 연결
            LstScenarios.SelectedIndexChanged += LstScenarios_SelectedIndexChanged;
            BtnAddScenario.Click += BtnAddScenario_Click;
            BtnDeleteScenario.Click += BtnDeleteScenario_Click;
            BtnSaveRule.Click += BtnSaveRule_Click;

            // 4. 최초 패널 설정
            PnlDropFile.BringToFront();
        }

        private void LoadConfig()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    string json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject<ExcelAnalyzerConfig>(json) ?? new ExcelAnalyzerConfig();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"설정 파일 로드 실패: {ex.Message}");
                }
            }
        }

        private void SaveConfig()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
                string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 저장 실패: {ex.Message}");
            }
        }

        #region [드래그 앤 드롭 & 헤더 추출]
        private void PnlDropFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void PnlDropFile_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                string file = files[0];
                if (!file.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("엑셀 파일(.xlsx)만 드롭할 수 있습니다.", "형식 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _droppedFilePath = file;
                
                // 시트 목록 불러오기
                try
                {
                    var sheets = Util_ExcelAnalyzer.GetSheetNames(_droppedFilePath);
                    // 디자이너에 생성된 CmbSheet 컨트롤에 시트 목록 추가
                    if (CmbSheet != null)
                    {
                        CmbSheet.Items.Clear();
                        foreach (var s in sheets) CmbSheet.Items.Add(s);
                        CmbSheet.Visible = true;

                        if (CmbSheet.Items.Count > 0)
                            CmbSheet.SelectedIndex = 0; // 시트가 선택되면 CmbSheet_SelectedIndexChanged 발생
                    }
                    else
                    {
                        MessageBox.Show("CmbSheet 콤보박스가 폼 디자인에 없습니다. 디자이너에서 추가해주세요.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("시트 목록을 읽어오는 데 실패했습니다: " + ex.Message);
                }
            }
        }

        private void CmbSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbSheet.SelectedItem != null)
            {
                LoadExcelHeaders(_droppedFilePath, CmbSheet.SelectedItem.ToString());
            }
        }

        private void LoadExcelHeaders(string filePath, string sheetName)
        {
            try
            {
                var headers = Util_ExcelAnalyzer.GetHeaders(filePath, sheetName);
                
                CmbMachineCol.Items.Clear();
                CmbNameCol.Items.Clear();
                CmbValueCol.Items.Clear();
                if (CmbDescCol != null) CmbDescCol.Items.Clear();

                foreach (var h in headers)
                {
                    CmbMachineCol.Items.Add(h);
                    CmbNameCol.Items.Add(h);
                    CmbValueCol.Items.Add(h);
                    if (CmbDescCol != null) CmbDescCol.Items.Add(h);
                }

                // 기존 매핑 기록(Config)이 있다면 자동 선택, 없으면 대충 유추해서 선택
                SelectComboItem(CmbMachineCol, _config.LastMappedMachineColumn, new[] { "Equipment", "설비", "호기", "EQP" });
                SelectComboItem(CmbNameCol, _config.LastMappedNameColumn, new[] { "Name", "변수명", "항목", "Item" });
                SelectComboItem(CmbValueCol, _config.LastMappedValueColumn, new[] { "Value", "값", "설정값", "데이터" });
                if (CmbDescCol != null) SelectComboItem(CmbDescCol, _config.LastMappedDescColumn, new[] { "Desc", "설명", "비고", "내용" });

                // 💡 [수정] MaterialSkin 렌더링 갱신 버그 방지 (강제 리프레시)
                CmbMachineCol.Invalidate(); CmbMachineCol.Refresh();
                CmbNameCol.Invalidate(); CmbNameCol.Refresh();
                CmbValueCol.Invalidate(); CmbValueCol.Refresh();

                // 버튼 표시
                BtnStartRuleSetup.Visible = true;
                BtnStartAnalyze.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "컬럼 로드 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectComboItem(MaterialComboBox cb, string lastSaved, string[] keywords)
        {
            if (cb.Items.Count == 0) return;

            // 1. 저장된 이력이 있으면 우선 선택
            if (!string.IsNullOrEmpty(lastSaved) && cb.Items.Contains(lastSaved))
            {
                cb.SelectedItem = lastSaved;
                return;
            }

            // 2. 키워드 기반 유추
            for (int i = 0; i < cb.Items.Count; i++)
            {
                string itemStr = cb.Items[i].ToString().ToLower();
                if (keywords.Any(k => itemStr.Contains(k.ToLower())))
                {
                    cb.SelectedIndex = i;
                    return;
                }
            }

            // 3. 없으면 0번째
            cb.SelectedIndex = 0;
        }
        #endregion

        #region [버튼 전환 및 규칙/분석 로직]
        
        private async void BtnStartRuleSetup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_droppedFilePath))
            {
                // 엑셀 없이 바로 진입 (저장된 설정 기반으로만 UI 로드)
                _parsedData.Clear();
                PnlRuleSetup.BringToFront();
                PopulateCheckboxes();
                PopulateScenarios();
                return;
            }

            if (CmbMachineCol.SelectedItem == null || CmbNameCol.SelectedItem == null || CmbValueCol.SelectedItem == null || CmbSheet.SelectedItem == null)
            {
                MessageBox.Show("시트 및 컬럼 매핑을 완료해주세요. (또는 엑셀 없이 진입하려면 프로그램을 재시작 후 드롭하기 전에 클릭하세요)");
                return;
            }

            // 매핑 기록 저장
            _config.LastMappedMachineColumn = CmbMachineCol.SelectedItem.ToString();
            _config.LastMappedNameColumn = CmbNameCol.SelectedItem.ToString();
            _config.LastMappedValueColumn = CmbValueCol.SelectedItem.ToString();
            _config.LastMappedDescColumn = CmbDescCol != null && CmbDescCol.SelectedItem != null ? CmbDescCol.SelectedItem.ToString() : "";
            SaveConfig();

            // 파싱 진행 (비동기)
            try
            {
                Cursor = Cursors.WaitCursor;
                string sheetName = CmbSheet.SelectedItem.ToString();
                
                _parsedData = await Util_ExcelAnalyzer.ParseExcelDataAsync(
                    _droppedFilePath, sheetName, 
                    _config.LastMappedMachineColumn, _config.LastMappedNameColumn, _config.LastMappedValueColumn, _config.LastMappedDescColumn);
                
                PnlRuleSetup.BringToFront();
                PopulateCheckboxes();
                PopulateScenarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void PopulateScenarios()
        {
            LstScenarios.Items.Clear();
            foreach (var profile in _config.Profiles)
            {
                LstScenarios.Items.Add(new MaterialSkin.MaterialListBoxItem(profile.RuleName));
            }
        }

        private void PopulateCheckboxes()
        {
            // MaterialCheckedListBox Items.Clear() 버그 방지 (Control 명시적 제거)
            ClbMachines.Items.Clear(); ClbMachines.Controls.Clear();
            ClbUniqueVars.Items.Clear(); ClbUniqueVars.Controls.Clear();
            ClbCommonVars.Items.Clear(); ClbCommonVars.Controls.Clear();
            
            _checkboxToolTip.RemoveAll();

            // 2. 호기명 목록 추출 (엑셀이 없으면 기존 프로필에서 추출)
            var distinctMachines = _parsedData.Count > 0 
                ? _parsedData.Select(x => x.MachineName).Distinct().OrderBy(x => x).ToList()
                : _config.Profiles.SelectMany(p => p.MappedMachines).Distinct().OrderBy(x => x).ToList();

            foreach (var m in distinctMachines) 
            {
                MaterialCheckbox cb = new MaterialCheckbox { Text = m, Tag = m, Checked = false };
                ClbMachines.Items.Add(cb);
            }

            // 3. 변수명 목록 추출 (엑셀이 없으면 기존 프로필에서 추출)
            var distinctVars = _parsedData.Count > 0 
                ? _parsedData.Select(x => x.VariableName).Distinct().OrderBy(x => x).ToList()
                : _config.Profiles.SelectMany(p => p.UniqueVariables.Concat(p.CommonVariables)).Distinct().OrderBy(x => x).ToList();

            foreach (var v in distinctVars)
            {
                // 변수에 해당하는 값과 설명을 샘플로 가져옴
                var rowData = _parsedData.FirstOrDefault(x => x.VariableName == v);
                string sampleValue = rowData?.Value ?? "N/A";
                string description = rowData != null && !string.IsNullOrWhiteSpace(rowData.Description) ? rowData.Description : "없음";
                
                // 긴 문자열은 지정된 길이마다 줄바꿈 처리하여 잘림 방지
                sampleValue = WrapText(sampleValue, 70);
                description = WrapText(description, 70);
                
                string tooltipMsg = $"변수명: {v}\n\n[설명]\n{description}\n\n[예시값]\n{sampleValue}";

                MaterialCheckbox cbUnique = new MaterialCheckbox { Text = v, Tag = v, Checked = false };
                _checkboxToolTip.SetToolTip(cbUnique, tooltipMsg);
                ClbUniqueVars.Items.Add(cbUnique);

                MaterialCheckbox cbCommon = new MaterialCheckbox { Text = v, Tag = v, Checked = false };
                _checkboxToolTip.SetToolTip(cbCommon, tooltipMsg);
                ClbCommonVars.Items.Add(cbCommon);
            }

            // 첫 번째 시나리오 자동 선택
            if (LstScenarios.Items.Count > 0)
                LstScenarios.SelectedIndex = 0;
        }

        /// <summary>
        /// 너무 긴 텍스트를 툴팁에서 표시하기 위해 N글자마다 줄바꿈을 삽입합니다.
        /// </summary>
        private string WrapText(string text, int maxLineLength = 70)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var sb = new System.Text.StringBuilder();
            int index = 0;
            while (index < text.Length)
            {
                int length = Math.Min(maxLineLength, text.Length - index);
                sb.AppendLine(text.Substring(index, length));
                index += length;
            }
            return sb.ToString().TrimEnd();
        }

        private void LstScenarios_SelectedIndexChanged(object sender, MaterialSkin.MaterialListBoxItem selectedItem)
        {
            if (LstScenarios.SelectedIndex < 0 || LstScenarios.SelectedIndex >= _config.Profiles.Count) return;

            var profile = _config.Profiles[LstScenarios.SelectedIndex];

            // TargetItem에 시트명 저장 로직 연동 (없으면 세팅)
            if (CmbSheet.SelectedItem != null && string.IsNullOrEmpty(profile.TargetItem))
            {
                profile.TargetItem = CmbSheet.SelectedItem.ToString();
            }

            // 체크박스 초기화
            foreach (MaterialCheckbox cb in ClbMachines.Items) cb.Checked = false;
            foreach (MaterialCheckbox cb in ClbUniqueVars.Items) cb.Checked = false;
            foreach (MaterialCheckbox cb in ClbCommonVars.Items) cb.Checked = false;

            // 기존 설정대로 체크 처리 (Text 대신 Tag로 비교)
            foreach (MaterialCheckbox cb in ClbMachines.Items)
                if (profile.MappedMachines.Contains(cb.Tag.ToString())) cb.Checked = true;

            foreach (MaterialCheckbox cb in ClbUniqueVars.Items)
                if (profile.UniqueVariables.Contains(cb.Tag.ToString())) cb.Checked = true;

            foreach (MaterialCheckbox cb in ClbCommonVars.Items)
                if (profile.CommonVariables.Contains(cb.Tag.ToString())) cb.Checked = true;
        }

        private string ShowInputBox(string promptText, string title, string defaultResponse)
        {
            using (Form promptForm = new Form()
            {
                Width = 400, Height = 150, FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title, StartPosition = FormStartPosition.CenterParent
            })
            {
                Label textLabel = new Label() { Left = 20, Top = 20, Text = promptText, AutoSize = true };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, Text = defaultResponse };
                Button confirmation = new Button() { Text = "확인", Left = 260, Width = 100, Top = 80, DialogResult = DialogResult.OK };
                promptForm.Controls.Add(textLabel); promptForm.Controls.Add(textBox); promptForm.Controls.Add(confirmation);
                promptForm.AcceptButton = confirmation;
                return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        private void BtnAddScenario_Click(object sender, EventArgs e)
        {
            string newName = ShowInputBox("새로운 시나리오(규칙) 이름을 입력하세요:", "규칙 추가", "새 규칙");
            if (!string.IsNullOrWhiteSpace(newName))
            {
                _config.Profiles.Add(new RuleProfile { RuleName = newName });
                SaveConfig();
                PopulateScenarios();
                LstScenarios.SelectedIndex = _config.Profiles.Count - 1; // 방금 추가한 항목 선택
            }
        }

        private void BtnDeleteScenario_Click(object sender, EventArgs e)
        {
            if (LstScenarios.SelectedIndex >= 0)
            {
                _config.Profiles.RemoveAt(LstScenarios.SelectedIndex);
                SaveConfig();
                PopulateScenarios();
            }
        }

        private void BtnSaveRule_Click(object sender, EventArgs e)
        {
            if (LstScenarios.SelectedIndex < 0)
            {
                MessageBox.Show("저장할 시나리오를 먼저 선택하세요.");
                return;
            }

            var profile = _config.Profiles[LstScenarios.SelectedIndex];

            // 새 체크 상태를 파일에 쓰기 (Text 대신 Tag 사용)
            profile.MappedMachines.Clear();
            foreach (MaterialCheckbox cb in ClbMachines.Items) if (cb.Checked) profile.MappedMachines.Add(cb.Tag.ToString());
            
            profile.UniqueVariables.Clear();
            foreach (MaterialCheckbox cb in ClbUniqueVars.Items) if (cb.Checked) profile.UniqueVariables.Add(cb.Tag.ToString());
            
            profile.CommonVariables.Clear();
            foreach (MaterialCheckbox cb in ClbCommonVars.Items) if (cb.Checked) profile.CommonVariables.Add(cb.Tag.ToString());

            SaveConfig();
            MessageBox.Show($"[{profile.RuleName}] 규칙이 성공적으로 저장되었습니다!", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnStartAnalyze_Click(object sender, EventArgs e)
        {
             if (CmbMachineCol.SelectedItem == null || CmbNameCol.SelectedItem == null || CmbValueCol.SelectedItem == null || CmbSheet.SelectedItem == null)
            {
                MessageBox.Show("시트 및 컬럼 매핑을 완료해주세요.");
                return;
            }

            // 매핑 기록 저장
            _config.LastMappedMachineColumn = CmbMachineCol.SelectedItem.ToString();
            _config.LastMappedNameColumn = CmbNameCol.SelectedItem.ToString();
            _config.LastMappedValueColumn = CmbValueCol.SelectedItem.ToString();
            _config.LastMappedDescColumn = CmbDescCol != null && CmbDescCol.SelectedItem != null ? CmbDescCol.SelectedItem.ToString() : "";
            SaveConfig();

            try
            {
                Cursor = Cursors.WaitCursor;
                string sheetName = CmbSheet.SelectedItem.ToString();
                
                _parsedData = await Util_ExcelAnalyzer.ParseExcelDataAsync(
                    _droppedFilePath, sheetName,
                    _config.LastMappedMachineColumn, _config.LastMappedNameColumn, _config.LastMappedValueColumn, _config.LastMappedDescColumn);
                    
                PnlAnalysis.BringToFront();
                
                // TODO: ValidateRules 실행 및 DataGridView 바인딩
                MessageBox.Show("분석 화면 진입 (데이터 추출 완료!)");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion
    }
}
