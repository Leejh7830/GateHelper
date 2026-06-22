using BrightIdeasSoftware;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GateHelper.LogValidator.Core;   // 💡 Core 주입
using GateHelper.LogValidator.Models; // 💡 Models 주입

namespace GateHelper.LogValidator
{
    public partial class LogScenarioForm : MaterialForm
    {
        private readonly MaterialSkinManager _skinManager = MaterialSkinManager.Instance;

        // 비즈니스 로직 연산 엔진 클래스 선언
        private readonly LogParser _logParser = new LogParser();
        private readonly LogMasker _logMasker = new LogMasker();

        // 그리드 연동 전용 마스터 컬렉션 리스트
        private List<RawLogModel> _rawLogList = new List<RawLogModel>();
        private List<ScenarioUnitModel> _scenarioLadderList = new List<ScenarioUnitModel>();

        public LogScenarioForm()
        {
            InitializeComponent();
            _skinManager.AddFormToManage(this);

            InitializeRawLogGridView();
            InitializeScenarioLadderGridView(); // 💡 사다리 그리드 초기화
            InitializeDropZone();
        }

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
        }

        private void InitializeScenarioLadderGridView()
        {
            var colStep = new OLVColumn("Step", "StepNo") { Width = 60, TextAlign = HorizontalAlignment.Center };
            var colName = new OLVColumn("Event Name", "EventName") { Width = 150 };
            var colPattern = new OLVColumn("Masking Pattern", "MaskingPattern") { Width = 450 };

            olvScenarioLadder.Columns.AddRange(new ColumnHeader[] { colStep, colName, colPattern });
            olvScenarioLadder.View = View.Details;
            olvScenarioLadder.FullRowSelect = true;
            olvScenarioLadder.GridLines = true;

            // 💡 사다리 그리드 드롭 허용 처리 명시
            olvScenarioLadder.AllowDrop = true;
        }

        private void InitializeDropZone()
        {
            pnlDropZone.AllowDrop = true;
            pnlDropZone.DragEnter += PnlDropZone_DragEnter;
            pnlDropZone.DragDrop += PnlDropZone_DragDrop;
            pnlDropZone.BringToFront();
            pnlDropZone.Dock = DockStyle.Fill;
        }

        private void PnlDropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void PnlDropZone_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) return;

            try
            {
                // 💡 분리된 Parser 인스턴스를 통해 무결성 데이터 수신
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

            txtEventName.Text = $"EVT_{selectedRow.LineNo}";
            RenderHighlightLog(rtbMaskedPreview, selectedRow.LogMessage);
        }

        private void RenderHighlightLog(RichTextBox rtb, string originalLog)
        {
            rtb.Clear();
            rtb.Text = originalLog;
            rtb.ForeColor = Color.White;

            // 💡 분리된 Masker 인스턴스에서 매칭 포인트를 일괄 질의받음
            MatchCollection matches = _logMasker.GetMaskingMatches(rtb.Text);
            foreach (Match match in matches)
            {
                rtb.Select(match.Index, match.Length);
                rtb.SelectionBackColor = Color.Yellow;
                rtb.SelectionColor = Color.Black;
            }

            rtb.Select(0, 0);
        }

        // 💡 [인터락] 중앙 박스 마우스 드래그 시작 
        private void rtbMaskedPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(rtbMaskedPreview.Text)) return;

            var dragData = new ScenarioUnitModel
            {
                EventName = txtEventName.Text,
                MaskingPattern = rtbMaskedPreview.Text
            };

            rtbMaskedPreview.DoDragDrop(dragData, DragDropEffects.Copy);
        }

        // 💡 [인터락] 우측 사다리 그리드 진입 영역 감시
        private void olvScenarioLadder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ScenarioUnitModel)))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        // 💡 [인터락] 우측 사다리 그리드 최종 안착 적재 연산
        private void olvScenarioLadder_DragDrop(object sender, DragEventArgs e)
        {
            var droppedUnit = e.Data.GetData(typeof(ScenarioUnitModel)) as ScenarioUnitModel;
            if (droppedUnit == null) return;

            droppedUnit.StepNo = _scenarioLadderList.Count + 1;
            _scenarioLadderList.Add(droppedUnit);

            olvScenarioLadder.SetObjects(_scenarioLadderList);
        }
    }
}