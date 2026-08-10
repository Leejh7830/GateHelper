using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
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
        private bool _isUpdatingUI = false;
        private int _previousScenarioIndex = -1;
        private bool _isDirty = false;
        private List<ValidationError> _validationResults = new List<ValidationError>();

        public ExcelAnalyzerForm()
        {
            InitializeComponent();
            
            // 다크 테마 적용을 위해 MaterialSkinManager에 현재 폼 등록
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);

            InitUI();
            LoadConfig();
            
            DgvVariables.CurrentCellDirtyStateChanged += DgvVariables_CurrentCellDirtyStateChanged;
            DgvVariables.CellValueChanged += DgvVariables_CellValueChanged;
            DgvVariables.CellPainting += DgvVariables_CellPainting;
            DgvVariables.CellClick += DgvVariables_CellClick;
        }

        private Panel _pnlDropOverlay;
        private readonly Size _smallSize = new Size(700, 420);
        private readonly Size _largeSize = new Size(1250, 650);

        private void SwitchToPanel(Panel panel)
        {
            panel.BringToFront();
            Size targetSize = (panel == PnlDropFile) ? _smallSize : _largeSize;
            
            if (this.Size != targetSize)
            {
                // 화면 세로(Y)는 중앙을 유지하되, 가로(X)는 좌측을 고정하여
                // 크기가 커질 때 모니터 왼쪽 밖으로 빠져나가는 현상을 방지합니다.
                int diffY = (targetSize.Height - this.Height) / 2;
                this.Size = targetSize;
                this.Location = new Point(this.Location.X, this.Location.Y - diffY);
            }
        }

        private void InitUI()
        {
            // 1. 드래그 앤 드롭 설정
            PnlDropFile.AllowDrop = true;
            PnlDropFile.DragEnter += PnlDropFile_DragEnter;
            PnlDropFile.DragDrop += PnlDropFile_DragDrop;

            CreateDropOverlay();

            ApplyMaterialDesignToDataGridView(DgvVariables);
            ApplyMaterialDesignToDataGridView(DgvResults);

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
            BtnBackToHome1.Click += (s, e) => SwitchToPanel(PnlDropFile);
            BtnBackToHome2.Click += (s, e) => SwitchToPanel(PnlDropFile);

            // 3-1. 규칙 설정 이벤트 연결
            LstScenarios.SelectedIndexChanged += LstScenarios_SelectedIndexChanged;
            BtnAddScenario.Click += BtnAddScenario_Click;
            BtnDeleteScenario.Click += BtnDeleteScenario_Click;
            BtnSaveRule.Click += BtnSaveRule_Click;

            InitContextMenu();

            // 3-2. 분석(Validation) 결과 확인 이벤트 연결
            if (BtnGoToAnalyze != null) BtnGoToAnalyze.Click += BtnGoToAnalyze_Click;
            if (BtnRunValidation != null) BtnRunValidation.Click += BtnRunValidation_Click;
            if (BtnExport != null) BtnExport.Click += BtnExport_Click;
            if (chkShowErrorsOnly != null) 
            {
                chkShowErrorsOnly.Checked = true; // 기본적으로 오류만 보도록 설정
                chkShowErrorsOnly.CheckedChanged += (s, e) => DisplayValidationResults();
            }
            
            if (DgvResults != null) DgvResults.RowHeadersVisible = false;

            // 4. 최초 패널 설정
            SwitchToPanel(PnlDropFile);
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

        private void CreateDropOverlay()
        {
            _pnlDropOverlay = new Panel();
            _pnlDropOverlay.Dock = DockStyle.Fill;
            _pnlDropOverlay.AllowDrop = true;
            
            var skin = MaterialSkinManager.Instance;
            bool isDark = skin.Theme == MaterialSkinManager.Themes.DARK;
            _pnlDropOverlay.BackColor = isDark ? Color.FromArgb(240, 50, 50, 50) : Color.FromArgb(240, 245, 245, 245);

            Label lblDesc = new Label();
            lblDesc.Text = "💡 MGMT로 수집된 Variable Data를 분석해주는 기능입니다.";
            lblDesc.Font = new Font("맑은 고딕", 14f, FontStyle.Regular);
            lblDesc.AutoSize = false;
            lblDesc.Dock = DockStyle.Top;
            lblDesc.Height = 150;
            lblDesc.TextAlign = ContentAlignment.BottomCenter;
            lblDesc.ForeColor = isDark ? Color.LightGray : Color.DimGray;
            lblDesc.AllowDrop = true;
            lblDesc.Cursor = Cursors.Hand;

            Label lbl = new Label();
            lbl.Text = "Drag & Drop 📁 또는 여기를 클릭하여\n엑셀 파일을 선택하세요";
            lbl.Font = new Font("맑은 고딕", 24f, FontStyle.Bold);
            lbl.AutoSize = false;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.ForeColor = isDark ? Color.LightGray : Color.DimGray;
            lbl.AllowDrop = true;
            
            // 클릭 시 커서 모양 변경
            lbl.Cursor = Cursors.Hand;
            _pnlDropOverlay.Cursor = Cursors.Hand;

            // 이벤트 연결
            lblDesc.DragEnter += PnlDropFile_DragEnter;
            lblDesc.DragDrop += PnlDropFile_DragDrop;
            lblDesc.Click += Overlay_Click;

            lbl.DragEnter += PnlDropFile_DragEnter;
            lbl.DragDrop += PnlDropFile_DragDrop;
            lbl.Click += Overlay_Click;
            
            _pnlDropOverlay.DragEnter += PnlDropFile_DragEnter;
            _pnlDropOverlay.DragDrop += PnlDropFile_DragDrop;
            _pnlDropOverlay.Click += Overlay_Click;

            _pnlDropOverlay.Controls.Add(lbl);
            _pnlDropOverlay.Controls.Add(lblDesc); // DockStyle.Top이 제대로 동작하려면 나중에 추가해야 할 수 있음. (Z-order)
            PnlDropFile.Controls.Add(_pnlDropOverlay);
            _pnlDropOverlay.BringToFront();
        }

        #region [Drag & Drop & Header Extraction]
        private void PnlDropFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Overlay_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files (*.xlsx)|*.xlsx";
                ofd.Title = "분석할 엑셀 파일 선택";
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ProcessExcelFile(ofd.FileName);
                }
            }
        }

        private void PnlDropFile_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                ProcessExcelFile(files[0]);
            }
        }

        private void ProcessExcelFile(string file)
        {
            if (!file.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("엑셀 파일(.xlsx)만 선택할 수 있습니다.", "형식 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _droppedFilePath = file;
            
            // 시트 목록 불러오기
            try
            {
                var sheets = Util_ExcelAnalyzer.GetSheetNames(_droppedFilePath);
                
                // 파일 파싱을 성공적으로 완료했을 때만 오버레이를 숨겨 다음 단계로 전환
                if (_pnlDropOverlay != null) _pnlDropOverlay.Visible = false;

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
                _droppedFilePath = ""; // 실패 시 리셋
                MessageBox.Show("시트 목록을 읽어오는 데 실패했습니다: " + ex.Message);
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

                // 콤보박스 선택 렌더링 버그 방지 (강제 리프레시)
                CmbMachineCol.Invalidate(); CmbMachineCol.Refresh();
                CmbNameCol.Invalidate(); CmbNameCol.Refresh();
                CmbValueCol.Invalidate(); CmbValueCol.Refresh();
                if (CmbDescCol != null) { CmbDescCol.Invalidate(); CmbDescCol.Refresh(); }

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

        #region [Button Events & Logic]
        
        private async void BtnStartRuleSetup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_droppedFilePath))
            {
                // 엑셀 없이 바로 진입 (저장된 설정 기반으로만 UI 로드)
                _parsedData.Clear();
                SwitchToPanel(PnlRuleSetup);
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
                
                // 3. 같은 설비 내에 동일한 변수명이 중복으로 존재하는지 검사 후 알림
                var duplicates = _parsedData.GroupBy(x => new { x.MachineName, x.VariableName }).Where(g => g.Count() > 1).ToList();
                if (duplicates.Count > 0)
                {
                    string dupMsg = string.Join("\n", duplicates.Take(5).Select(g => $"- {g.Key.MachineName} : {g.Key.VariableName}"));
                    if (duplicates.Count > 5) dupMsg += "\n... 외 다수";
                    MessageBox.Show($"동일한 설비 내에 중복된 변수명이 발견되었습니다.\n엑셀 데이터가 올바른지 확인해주세요.\n\n[중복 항목 예시]\n{dupMsg}", "변수 중복 알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                SwitchToPanel(PnlRuleSetup);
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
            DgvVariables.Rows.Clear();
            
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

            // 3. 변수명 목록 추출 (엑셀이 없어도 과거 프로필에 잘못 저장된 고스트 변수를 찾기 위해 합침)
            var distinctVarsSet = new HashSet<string>();
            if (_parsedData.Count > 0)
                distinctVarsSet.UnionWith(_parsedData.Select(x => x.VariableName));
            
            distinctVarsSet.UnionWith(_config.Profiles.SelectMany(p => p.UniqueVariables.Concat(p.CommonVariables)));
            
            var distinctVars = distinctVarsSet.OrderBy(x => x).ToList();

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

                int rowIndex = DgvVariables.Rows.Add(v, false, false);
                DgvVariables.Rows[rowIndex].Cells["ColVarName"].ToolTipText = tooltipMsg;
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
            if (_isUpdatingUI) return;

            // 저장 안하고 다른 시나리오로 넘어갈 때 알림
            if (_isDirty && _previousScenarioIndex != -1 && _previousScenarioIndex != LstScenarios.SelectedIndex && _previousScenarioIndex < _config.Profiles.Count)
            {
                var res = MessageBox.Show("저장하지 않은 변경사항이 있습니다. 변경사항을 저장하시겠습니까?\n\n'예' : 현재 변경사항을 저장하고 이동\n'아니오' : 변경사항 취소하고 이동\n'취소' : 이동하지 않음", "변경사항 확인", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Cancel)
                {
                    _isUpdatingUI = true;
                    LstScenarios.SelectedIndex = _previousScenarioIndex;
                    _isUpdatingUI = false;
                    return;
                }
                else if (res == DialogResult.Yes)
                {
                    var prevProfile = _config.Profiles[_previousScenarioIndex];
                    prevProfile.MappedMachines.Clear();
                    foreach (MaterialCheckbox cb in ClbMachines.Items) if (cb.Checked) prevProfile.MappedMachines.Add(cb.Tag.ToString());
                    SaveConfig();
                }
                else
                {
                    int targetIndex = LstScenarios.SelectedIndex;
                    LoadConfig(); // 원래 데이터로 덮어쓰기 (메모리 원복)
                    _isUpdatingUI = true;
                    PopulateScenarios();
                    LstScenarios.SelectedIndex = targetIndex;
                    _isUpdatingUI = false;
                }
            }

            _isDirty = false;
            _previousScenarioIndex = LstScenarios.SelectedIndex;

            if (LstScenarios.SelectedIndex < 0 || LstScenarios.SelectedIndex >= _config.Profiles.Count) return;

            var profile = _config.Profiles[LstScenarios.SelectedIndex];

            // TargetItem에 시트명 저장 로직 연동 (없으면 세팅)
            if (CmbSheet.SelectedItem != null && string.IsNullOrEmpty(profile.TargetItem))
            {
                profile.TargetItem = CmbSheet.SelectedItem.ToString();
            }

            _isUpdatingUI = true;

            // 기기 목록 갱신
            foreach (MaterialCheckbox cb in ClbMachines.Items)
                cb.Checked = profile.MappedMachines.Contains(cb.Tag.ToString());

            // 변수 목록 갱신 (DataGridView)
            foreach (DataGridViewRow row in DgvVariables.Rows)
            {
                string varName = row.Cells["ColVarName"].Value?.ToString();
                if (string.IsNullOrEmpty(varName)) continue;
                row.Cells["ColUnique"].Value = profile.UniqueVariables.Contains(varName);
                row.Cells["ColCommon"].Value = profile.CommonVariables.Contains(varName);
            }

            _isUpdatingUI = false;
        }

        private void DgvVariables_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (DgvVariables.IsCurrentCellDirty)
            {
                DgvVariables.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvVariables_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdatingUI || e.RowIndex < 0) return;
            if (e.ColumnIndex != DgvVariables.Columns["ColUnique"].Index && e.ColumnIndex != DgvVariables.Columns["ColCommon"].Index) return;
            if (LstScenarios.SelectedIndex < 0 || LstScenarios.SelectedIndex >= _config.Profiles.Count) return;

            var profile = _config.Profiles[LstScenarios.SelectedIndex];
            string varName = DgvVariables.Rows[e.RowIndex].Cells["ColVarName"].Value?.ToString();
            if (string.IsNullOrEmpty(varName)) return;

            bool isChecked = Convert.ToBoolean(DgvVariables.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            bool isUniqueCol = e.ColumnIndex == DgvVariables.Columns["ColUnique"].Index;

            _isUpdatingUI = true; 
            
            if (isUniqueCol)
            {
                if (isChecked)
                {
                    if (!profile.UniqueVariables.Contains(varName)) profile.UniqueVariables.Add(varName);
                    if (profile.CommonVariables.Contains(varName)) profile.CommonVariables.Remove(varName);
                    DgvVariables.Rows[e.RowIndex].Cells["ColCommon"].Value = false; // 공통 체크 해제 (겹침 방지)
                }
                else
                {
                    profile.UniqueVariables.Remove(varName);
                }
            }
            else
            {
                if (isChecked)
                {
                    if (!profile.CommonVariables.Contains(varName)) profile.CommonVariables.Add(varName);
                    if (profile.UniqueVariables.Contains(varName)) profile.UniqueVariables.Remove(varName);
                    DgvVariables.Rows[e.RowIndex].Cells["ColUnique"].Value = false; // 고유 체크 해제 (겹침 방지)
                }
                else
                {
                    profile.CommonVariables.Remove(varName);
                }
            }
            
            _isUpdatingUI = false;
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
            
            // 변수들은 DgvVariables_CellValueChanged 에서 실시간으로 profile 객체에 동기화되므로 
            // 여기서 순회하며 갱신할 필요가 없습니다.

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
                    
                SwitchToPanel(PnlAnalysis);
                
                // 검증 자동 실행
                RunValidation();
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

        private void ApplyMaterialDesignToDataGridView(DataGridView dgv)
        {
            if (dgv == null) return;
            var materialSkinManager = MaterialSkinManager.Instance;
            bool isDark = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK;
            
            // 사용자 요청에 따라 한층 더 연한 그레이 배경색 적용
            Color bgColor = isDark ? Color.FromArgb(90, 90, 90) : materialSkinManager.BackgroundColor;
            // 배경이 연해졌으므로 선을 좀 더 밝게 하여 시인성 유지
            Color gridColor = isDark ? Color.FromArgb(145, 145, 145) : Color.FromArgb(200, 200, 200);
            
            dgv.BackgroundColor = bgColor;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single; // 헤더 부분도 세로선 표시
            dgv.ReadOnly = true; // 단일 클릭 즉각 반응을 위해 ReadOnly 처리 (CellClick에서 수동 토글)
            
            // 폰트 설정 (한글 가독성을 위해 맑은 고딕 사용 및 크기 확대)
            Font headerFont = new Font("맑은 고딕", 11f, FontStyle.Bold);
            Font rowFont = new Font("맑은 고딕", 11f, FontStyle.Regular);

            // Header Style
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = materialSkinManager.ColorScheme.PrimaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = materialSkinManager.ColorScheme.PrimaryColor;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 45;
            
            // Row Style
            dgv.DefaultCellStyle.BackColor = bgColor;
            dgv.DefaultCellStyle.ForeColor = materialSkinManager.TextHighEmphasisColor;
            dgv.DefaultCellStyle.Font = rowFont;
            dgv.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            Color selectionColor = isDark ? Color.FromArgb(100, 100, 100) : Color.FromArgb(220, 220, 220);
            dgv.DefaultCellStyle.SelectionBackColor = selectionColor;
            dgv.DefaultCellStyle.SelectionForeColor = materialSkinManager.TextHighEmphasisColor;
            
            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 40;
            dgv.GridColor = gridColor;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void DgvVariables_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.ColumnIndex == DgvVariables.Columns["ColUnique"].Index || e.ColumnIndex == DgvVariables.Columns["ColCommon"].Index))
            {
                bool currentState = false;
                var cellValue = DgvVariables.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (cellValue != null && bool.TryParse(cellValue.ToString(), out bool b))
                    currentState = b;

                // 클릭 즉시 수동으로 값 반전
                DgvVariables.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = !currentState;
                
                // ReadOnly 상태에서는 프로그래밍 방식으로 값을 변경해도 CellValueChanged가 자동 발생하지 않음.
                // 상호 배타(Mutual Exclusion) 로직과 Profile 저장을 수행하기 위해 수동으로 강제 호출합니다.
                DgvVariables_CellValueChanged(sender, e);
                
                // 표를 즉시 다시 그려서 상호 배타 처리된 체크박스 상태를 UI에 즉시 반영
                DgvVariables.InvalidateRow(e.RowIndex);
            }
        }

        private void DgvVariables_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && (e.ColumnIndex == DgvVariables.Columns["ColUnique"].Index || e.ColumnIndex == DgvVariables.Columns["ColCommon"].Index))
            {
                e.PaintBackground(e.CellBounds, true);
                bool isChecked = false;
                if (e.Value != null && bool.TryParse(e.Value.ToString(), out bool b))
                    isChecked = b;

                // 20x20 크기의 커스텀 체크박스 영역 계산
                int size = 20; 
                Rectangle rect = new Rectangle(
                    e.CellBounds.X + (e.CellBounds.Width - size) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - size) / 2,
                    size, size);

                var skin = MaterialSkinManager.Instance;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                if (isChecked)
                {
                    using (SolidBrush brush = new SolidBrush(skin.ColorScheme.AccentColor))
                    {
                        e.Graphics.FillRectangle(brush, rect);
                    }
                    using (Pen pen = new Pen(Color.White, 2.5f))
                    {
                        e.Graphics.DrawLine(pen, rect.X + 4, rect.Y + 10, rect.X + 8, rect.Y + 15);
                        e.Graphics.DrawLine(pen, rect.X + 8, rect.Y + 15, rect.X + 16, rect.Y + 6);
                    }
                }
                else
                {
                    using (Pen pen = new Pen(skin.TextMediumEmphasisColor, 2f))
                    {
                        e.Graphics.DrawRectangle(pen, rect);
                    }
                }
                e.Handled = true; // 기본 Windows 체크박스 렌더링 방지
            }
        }

        #endregion

        #region [Validation & Result Output]

        private void BtnGoToAnalyze_Click(object sender, EventArgs e)
        {
            if (_isDirty)
            {
                var res = MessageBox.Show("저장하지 않은 변경사항이 있습니다. 이대로 분석 화면으로 넘어가시겠습니까?\n\n'예' : 변경사항 저장 후 이동\n'아니오' : 변경사항 취소 후 이동\n'취소' : 이동하지 않음", "저장 확인", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Cancel) return;
                
                if (res == DialogResult.Yes)
                {
                    BtnSaveRule_Click(null, null); // 강제 저장
                }
                else
                {
                    LoadConfig(); // 메모리 롤백
                }
            }
            
            SwitchToPanel(PnlAnalysis);
            RunValidation();
        }

        private void BtnRunValidation_Click(object sender, EventArgs e)
        {
            RunValidation();
        }

        private void RunValidation()
        {
            if (_parsedData == null || _parsedData.Count == 0 || _config.Profiles == null || _config.Profiles.Count == 0)
            {
                MessageBox.Show("분석할 엑셀 데이터나 설정된 규칙(시나리오)이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                // 코어 엔진 호출
                _validationResults = Util_ExcelAnalyzer.ValidateRules(_parsedData, _config.Profiles);
                DisplayValidationResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"검증 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DisplayValidationResults()
        {
            if (DgvResults == null) return;
            DgvResults.Rows.Clear();
            
            bool showErrorsOnly = chkShowErrorsOnly != null && chkShowErrorsOnly.Checked;
            
            // 엔진이 기본적으로 에러 내역만 반환하지만, 확장을 고려하여 필터링
            var displayList = _validationResults;
            if (showErrorsOnly)
            {
                displayList = _validationResults.Where(x => x.ErrorType != "PASS").ToList();
            }

            var materialSkinManager = MaterialSkinManager.Instance;
            bool isDark = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK;
            
            // 다크/라이트 모드 배경색(명도)에 맞춰 시인성이 확보되는 색상으로 조정
            Color errorColor = isDark ? Color.FromArgb(255, 100, 100) : Color.Red;
            Color passColor = isDark ? Color.FromArgb(150, 255, 150) : Color.ForestGreen;

            Font rowFont = new Font("맑은 고딕", 11f, FontStyle.Regular);

            foreach (var err in displayList)
            {
                int rowIndex = DgvResults.Rows.Add();
                var row = DgvResults.Rows[rowIndex];

                bool isPass = err.ErrorType == "PASS";
                string statusIcon = isPass ? "✔" : "❌";
                
                row.DefaultCellStyle.ForeColor = isPass ? passColor : errorColor;
                row.DefaultCellStyle.Font = rowFont;
                
                row.Cells["ColStatus"].Value = statusIcon;
                row.Cells["ColRule"].Value = err.RuleName;
                row.Cells["ColMachine"].Value = err.MachineName;
                row.Cells["ColVariable"].Value = err.VariableName;
                row.Cells["ColValue"].Value = err.ActualValue;
                row.Cells["ColDesc"].Value = err.Description;
            }

            DgvResults.ClearSelection();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (DgvResults == null || DgvResults.Rows.Count == 0)
            {
                MessageBox.Show("추출할 분석 결과가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV 파일 (*.csv)|*.csv";
                sfd.FileName = $"검증결과_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var csvContent = new System.Text.StringBuilder();
                        
                        // 1. 헤더 추출
                        var headers = DgvResults.Columns.Cast<DataGridViewColumn>().Select(col => $"\"{col.HeaderText}\"");
                        csvContent.AppendLine(string.Join(",", headers));

                        // 2. 데이터 추출
                        foreach (DataGridViewRow row in DgvResults.Rows)
                        {
                            if (row.IsNewRow) continue;
                            var cells = row.Cells.Cast<DataGridViewCell>().Select(cell => $"\"{cell.Value?.ToString().Replace("\"", "\"\"")}\"");
                            csvContent.AppendLine(string.Join(",", cells));
                        }

                        // UTF8 BOM을 넣어 엑셀에서 열 때 한글 깨짐 방지
                        File.WriteAllText(sfd.FileName, csvContent.ToString(), new System.Text.UTF8Encoding(true));
                        MessageBox.Show("분석 결과가 성공적으로 추출되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"파일 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion

        #region 우클릭 컨텍스트 메뉴 (Context Menu)

        private ContextMenuStrip _scenarioContextMenu;

        private void InitContextMenu()
        {
            _scenarioContextMenu = new ContextMenuStrip();
            _scenarioContextMenu.Items.Add("위로 이동", null, ContextMenu_MoveUp_Click);
            _scenarioContextMenu.Items.Add("아래로 이동", null, ContextMenu_MoveDown_Click);
            _scenarioContextMenu.Items.Add(new ToolStripSeparator());
            _scenarioContextMenu.Items.Add("이름 변경", null, ContextMenu_Rename_Click);
            _scenarioContextMenu.Items.Add("복제", null, ContextMenu_Duplicate_Click);
            _scenarioContextMenu.Items.Add(new ToolStripSeparator());
            _scenarioContextMenu.Items.Add("삭제", null, ContextMenu_Delete_Click);

            LstScenarios.ContextMenuStrip = _scenarioContextMenu;
            _scenarioContextMenu.Opening += _scenarioContextMenu_Opening;
        }

        private void _scenarioContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (LstScenarios.SelectedIndex < 0 || LstScenarios.SelectedIndex >= _config.Profiles.Count)
            {
                e.Cancel = true;
            }
        }

        private void ContextMenu_MoveUp_Click(object sender, EventArgs e)
        {
            int index = LstScenarios.SelectedIndex;
            if (index <= 0 || index >= _config.Profiles.Count) return;

            var profile = _config.Profiles[index];
            _config.Profiles.RemoveAt(index);
            _config.Profiles.Insert(index - 1, profile);
            
            SaveConfig();
            
            _isUpdatingUI = true;
            PopulateScenarios();
            LstScenarios.SelectedIndex = index - 1;
            _isUpdatingUI = false;
            LstScenarios_SelectedIndexChanged(LstScenarios, null);
        }

        private void ContextMenu_MoveDown_Click(object sender, EventArgs e)
        {
            int index = LstScenarios.SelectedIndex;
            if (index < 0 || index >= _config.Profiles.Count - 1) return;

            var profile = _config.Profiles[index];
            _config.Profiles.RemoveAt(index);
            _config.Profiles.Insert(index + 1, profile);
            
            SaveConfig();
            
            _isUpdatingUI = true;
            PopulateScenarios();
            LstScenarios.SelectedIndex = index + 1;
            _isUpdatingUI = false;
            LstScenarios_SelectedIndexChanged(LstScenarios, null);
        }

        private void ContextMenu_Rename_Click(object sender, EventArgs e)
        {
            int index = LstScenarios.SelectedIndex;
            if (index < 0 || index >= _config.Profiles.Count) return;

            string oldName = _config.Profiles[index].RuleName;
            string newName = PromptForInput("시나리오 이름 변경", "새 이름을 입력하세요:", oldName);

            if (!string.IsNullOrWhiteSpace(newName) && newName != oldName)
            {
                _config.Profiles[index].RuleName = newName;
                SaveConfig();
                
                _isUpdatingUI = true;
                PopulateScenarios();
                LstScenarios.SelectedIndex = index;
                _isUpdatingUI = false;
            }
        }

        private void ContextMenu_Duplicate_Click(object sender, EventArgs e)
        {
            int index = LstScenarios.SelectedIndex;
            if (index < 0 || index >= _config.Profiles.Count) return;

            var profile = _config.Profiles[index];
            var json = JsonConvert.SerializeObject(profile);
            var newProfile = JsonConvert.DeserializeObject<RuleProfile>(json);
            
            newProfile.RuleName = profile.RuleName + "_복제";
            
            _config.Profiles.Insert(index + 1, newProfile);
            SaveConfig();
            
            _isUpdatingUI = true;
            PopulateScenarios();
            LstScenarios.SelectedIndex = index + 1;
            _isUpdatingUI = false;
            LstScenarios_SelectedIndexChanged(LstScenarios, null);
        }

        private void ContextMenu_Delete_Click(object sender, EventArgs e)
        {
            int index = LstScenarios.SelectedIndex;
            if (index < 0 || index >= _config.Profiles.Count) return;

            var res = MessageBox.Show($"'{_config.Profiles[index].RuleName}' 시나리오를 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                _config.Profiles.RemoveAt(index);
                SaveConfig();
                
                _isUpdatingUI = true;
                PopulateScenarios();
                if (_config.Profiles.Count > 0)
                {
                    LstScenarios.SelectedIndex = Math.Min(index, _config.Profiles.Count - 1);
                    _isUpdatingUI = false;
                    LstScenarios_SelectedIndexChanged(LstScenarios, null);
                }
                else
                {
                    _isDirty = false;
                    _previousScenarioIndex = -1;
                    _isUpdatingUI = false;
                }
            }
        }

        private string PromptForInput(string title, string promptText, string defaultValue = "")
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = promptText, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 45, Width = 340, Text = defaultValue };
            Button confirmation = new Button() { Text = "확인", Left = 260, Top = 75, Width = 100, DialogResult = DialogResult.OK };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;
            
            return prompt.ShowDialog(this) == DialogResult.OK ? textBox.Text : null;
        }

        #endregion
    }
}
