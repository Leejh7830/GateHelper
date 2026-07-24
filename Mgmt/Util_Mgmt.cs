using ClosedXML.Excel;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;



namespace GateHelper.Mgmt
{
    public static class Util_Mgmt
    {
        /// <summary>
        /// 실제 설비 리스트를 받아 설비별 체크박스로 선택하는 팝업을 표시합니다.
        /// SEM/Port 옵션은 기존과 동일하게 유지합니다.
        /// 반환값: (선택된 설비명 리스트, SEM선택, Port선택)
        /// </summary>
        public static (List<string> selectedMachines, bool isSem, bool isPort) ShowCollectionSelectDialog(List<string> machineList)
        {
            const int FORM_W = 420;
            const int PADDING = 12;
            const int BTN_H = 26;
            const int GAP = 6;
            const int CLIENT_W = FORM_W - 16;

            // 감지된 설비 타입 (버튼으로 표시)
            var typeKeywords = new[] { "STO", "OHS", "CNV", "AGV", "DDA" };
            var detectedTypes = typeKeywords.Where(t => machineList.Any(m => m.ToUpper().Contains(t))).ToList();

            // 타입버튼 줄 수 계산 (버튼 1개 56px)
            int btnsPerRow = Math.Max(1, (CLIENT_W - PADDING * 2) / 56);
            int typeRowCount = detectedTypes.Count == 0 ? 0 : (int)Math.Ceiling((double)detectedTypes.Count / btnsPerRow);

            // ── y 좌표 순차 계산 (줄 겹침 방지) ──
            int row1Y = PADDING;                              // 줄1: 전체선택 | 전체해제
            int row2Y = row1Y + BTN_H + GAP;                 // 줄2: 타입버튼 (없으면 0px)
            int typeH = detectedTypes.Count == 0 ? 0 : typeRowCount * (BTN_H + 4) - 4;
            int labelY = row2Y + (detectedTypes.Count > 0 ? typeH + GAP : 0);
            int listY = labelY + 18;
            int listH = Math.Max(150, Math.Min(320, machineList.Count * 16 + 4));
            int optionY = listY + listH + GAP;
            int chkY = optionY + 18;
            int actionY = chkY + 32;
            int totalH = actionY + BTN_H + PADDING;

            using (Form prompt = new Form()
            {
                Width = FORM_W,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "수집 대상 설비 선택",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                prompt.ClientSize = new Size(CLIENT_W, totalH);

                // ── 줄 1: 전체선택 | 전체해제 ──
                Button btnAll = new Button() { Left = PADDING, Top = row1Y, Width = 76, Height = BTN_H, Text = "전체 선택" };
                Button btnNone = new Button() { Left = PADDING + 82, Top = row1Y, Width = 76, Height = BTN_H, Text = "전체 해제" };
                prompt.Controls.Add(btnAll);
                prompt.Controls.Add(btnNone);

                // ── 줄 2: 타입 필터 버튼 (전체선택/해제와 완전히 분리된 줄) ──
                var typeButtons = new List<Button>();
                int tx = PADDING, ty = row2Y;
                foreach (var t in detectedTypes)
                {
                    if (tx + 56 > CLIENT_W - PADDING) { tx = PADDING; ty += BTN_H + 4; }
                    var btn = new Button() { Left = tx, Top = ty, Width = 52, Height = BTN_H, Text = t, Tag = t };
                    prompt.Controls.Add(btn);
                    typeButtons.Add(btn);
                    tx += 56;
                }

                // ── 설비 체크리스트 ──
                prompt.Controls.Add(new Label() { Left = PADDING, Top = labelY, Text = $"수집 대상 설비 ({machineList.Count}대):", Width = 300, Height = 18 });
                CheckedListBox chkMachines = new CheckedListBox()
                {
                    Left = PADDING,
                    Top = listY,
                    Width = CLIENT_W - PADDING * 2,
                    Height = listH,
                    CheckOnClick = true
                };
                foreach (var m in machineList) chkMachines.Items.Add(m);
                for (int i = 0; i < chkMachines.Items.Count; i++) chkMachines.SetItemChecked(i, true);
                prompt.Controls.Add(chkMachines);

                // ── 이벤트 ──
                btnAll.Click += (s, e) =>
                {
                    for (int i = 0; i < chkMachines.Items.Count; i++) chkMachines.SetItemChecked(i, true);
                    foreach (var b in typeButtons)
                    {
                        b.BackColor = SystemColors.Control;
                        var (keyword, _) = ((string, bool))b.Tag;
                        b.Tag = (keyword, false);
                    }
                };
                btnNone.Click += (s, e) =>
                {
                    for (int i = 0; i < chkMachines.Items.Count; i++) chkMachines.SetItemChecked(i, false);
                    foreach (var b in typeButtons)
                    {
                        b.BackColor = SystemColors.Control;
                        var (keyword, _) = ((string, bool))b.Tag;
                        b.Tag = (keyword, false);
                    }
                };

                // 타입 버튼: 토글 방식 — 켜면 해당 타입 추가 선택(다른 타입 유지), 끄면 해당 타입만 해제
                foreach (var btn in typeButtons)
                {
                    var kw = (string)btn.Tag;
                    btn.Tag = (kw, false); // (키워드, 활성화여부)
                    btn.Click += (s, e) =>
                    {
                        var (keyword, isActive) = ((string, bool))btn.Tag;
                        bool turnOn = !isActive;
                        btn.Tag = (keyword, turnOn);
                        btn.BackColor = turnOn ? Color.FromArgb(173, 216, 230) : SystemColors.Control; // 활성 시 하늘색 표시

                        for (int i = 0; i < chkMachines.Items.Count; i++)
                        {
                            if (chkMachines.Items[i].ToString().ToUpper().Contains(keyword))
                                chkMachines.SetItemChecked(i, turnOn);
                        }
                    };
                }

                // ── SEM / Port 옵션 ──
                prompt.Controls.Add(new Label() { Left = PADDING, Top = optionY, Text = "수집 항목:", Width = 200, Height = 18 });
                CheckBox chkSem = new CheckBox() { Left = PADDING, Top = chkY, Text = "SEM 수집", Width = 140, Checked = true };
                CheckBox chkPort = new CheckBox() { Left = PADDING + 150, Top = chkY, Text = "Port 수집", Width = 140, Checked = true };
                prompt.Controls.Add(chkSem);
                prompt.Controls.Add(chkPort);

                // ── 수집시작 / 취소 버튼 ──
                Button btnOk = new Button() { Text = "수집 시작", Left = PADDING, Top = actionY, Width = 100, Height = BTN_H };
                Button btnCancel = new Button() { Text = "취소", Left = CLIENT_W - PADDING - 100, Top = actionY, Width = 100, Height = BTN_H, DialogResult = DialogResult.Cancel };
                prompt.Controls.Add(btnOk);
                prompt.Controls.Add(btnCancel);

                btnOk.Click += (sender, e) =>
                {
                    if (chkMachines.CheckedItems.Count == 0)
                    {
                        MessageBox.Show("수집할 설비를 최소 하나 이상 선택해 주십시오.", "선택 누락", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!chkSem.Checked && !chkPort.Checked)
                    {
                        MessageBox.Show("SEM 또는 Port 중 하나 이상 선택해 주십시오.", "선택 누락", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    prompt.DialogResult = DialogResult.OK;
                };

                prompt.AcceptButton = btnOk;
                prompt.CancelButton = btnCancel;

                if (prompt.ShowDialog() == DialogResult.OK)
                    return (chkMachines.CheckedItems.Cast<string>().ToList(), chkSem.Checked, chkPort.Checked);

                return (new List<string>(), false, false);
            }
        } // ShowCollectionSelectDialog END


        /// <summary>
        /// 메모리의 workbook에 데이터를 누적합니다. 파일 저장은 호출자(MainUI)가 루프 완료 후 1회 수행합니다.
        /// </summary>
        public static void SaveDataToExcel(XLWorkbook workbook, string machineName, string itemName, List<string[]> tableData)
        {
            if (workbook == null || tableData == null || tableData.Count == 0) return;

            // 설비명 구조: [공장코드][공정코드][설비타입][순번]  예) J1ESTO12345
            // 설비타입 키워드(STO/OHS 등) 바로 앞 1글자 = 공정코드(E/A/F/P) → 시트 구분자
            // 예: J1ESTO12345 → STO 앞 1글자 = E → E_SEM
            string linePrefix = "X";
            string[] knownKeywords = { "STO", "OHS", "CNV", "AGV", "DDA" };
            foreach (var keyword in knownKeywords)
            {
                int idx = machineName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (idx > 0)
                {
                    linePrefix = machineName.Substring(idx - 1, 1).ToUpper();
                    break;
                }
                else if (idx == 0)
                {
                    linePrefix = machineName.Substring(0, 1).ToUpper();
                    break;
                }
            }

            string typeSuffix = itemName.Contains("SEM") ? "SEM" : "PORT";
            string sheetName = $"{linePrefix}_{typeSuffix}";

            IXLWorksheet worksheet;

            // 시트가 없으면 생성하고 헤더 작성
            if (!workbook.Worksheets.TryGetWorksheet(sheetName, out worksheet))
            {
                worksheet = workbook.Worksheets.Add(sheetName);

                worksheet.Cell(1, 1).Value = "호기명";
                worksheet.Cell(1, 2).Value = "수집항목";
                worksheet.Cell(1, 3).Value = "Name";
                worksheet.Cell(1, 4).Value = "Access";
                worksheet.Cell(1, 5).Value = "Type";
                worksheet.Cell(1, 6).Value = "Value";
                worksheet.Cell(1, 7).Value = "Description";

                var headerRange = worksheet.Range("A1:G1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // 마지막 행 다음에 데이터 추가
            int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            int startRow = lastRow + 1;

            for (int i = 0; i < tableData.Count; i++)
            {
                var rowData = tableData[i];
                int currentRow = startRow + i;

                worksheet.Cell(currentRow, 1).Value = machineName;
                worksheet.Cell(currentRow, 2).Value = itemName;

                for (int j = 0; j < rowData.Length && j < 5; j++)
                    worksheet.Cell(currentRow, 3 + j).SetValue(rowData[j]);
            }
        } // SaveDataToExcel END

        /// <summary>
        /// 수집 완료 후 workbook을 실제 파일로 저장합니다. (루프 완료 후 1회만 호출)
        /// </summary>
        public static bool FlushWorkbookToFile(XLWorkbook workbook, string filePath)
        {
            try
            {
                // 모든 시트 칼럼 너비 자동 맞춤
                foreach (var ws in workbook.Worksheets)
                    ws.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
                LogMessage($"[엑셀 저장 완료] {filePath}", Level.Info);
                return true;
            }
            catch (IOException)
            {
                LogMessage($"[엑셀 저장 실패] 파일이 열려있습니다: {filePath}", Level.Error);
                MessageBox.Show($"엑셀 파일이 열려있어 저장할 수 없습니다.\n파일을 닫은 후 다시 시도해 주십시오.\n\n{filePath}",
                    "저장 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "엑셀 최종 저장 중 오류 발생");
                return false;
            }
        }







        // 1. 수집 루프 시작 전에 고정하지 않고, 단일 호기명(FSTO_01, LOHS_02)을 보고 즉각적으로 구조를 판단합니다.
        public static (string semName, string portParentName, string childPortPrefix) GetEquipmentKeywords(string machineName)
        {
            string upperName = machineName.ToUpper();

            // 나중에 구조 추가를 대비한 모듈화
            if (upperName.Contains("CNV")) return ("CnvSEM", "CnvPorts", "CNVPORT:");
            if (upperName.Contains("AGV")) return ("AgvSEM", "AgvPorts", "AGVPORT:");

            // STO, OHS 등은 모두 StockerSEM 공통 규격 사용
            return ("StockerSEM", "StockerPorts", "STOCKERPORT:");
        }

        // 2. [서브루틴] SEM 데이터 수집
        public static async Task<int> CollectSemDataAsync(IWebDriver driver, XLWorkbook workbook, string machineName, string targetSemName, CancellationToken token)
        {
            var tableData = await Util_MgmtElement.GetTableDataBySmartScrollAsync(driver);

            if (tableData != null && tableData.Count > 0)
            {
                SaveDataToExcel(workbook, machineName, targetSemName, tableData);
                return 1;
            }
            return 0;
        }

        // 3. [서브루틴] Port 부모 전개 및 자식 다중 수집
        // 🔒 [격리] scopeElement(현재 호기의 SEM 노드) 기준 following:: 축으로만 탐색하여
        //    이전 호기의 잔존 StockerPorts/자식 포트 노드를 절대 잡지 않도록 강제 격리
        // 🔍 [검증용] discovered(발견된 개수) vs collected(실제 수집 성공 개수)를 함께 반환하여
        //    최종 결과와 비교, 데이터 유실 여부를 판단할 수 있게 함
        public static async Task<(int collected, int discovered)> CollectPortDataAsync(IWebDriver driver, IWebElement scopeElement, XLWorkbook workbook, string machineName, string targetPortParentName, string targetChildPortPrefix, CancellationToken token)
        {
            int count = 0;

            string portParentXPath = $"following::span[contains(@class, 'wj-node-text') and text()='{targetPortParentName}']";

            // 🕒 [폴링 대기] SEM 탐색과 동일한 기준(0.5초 간격, 최대 20초) 적용
            const int portParentPollIntervalMs = 500;
            const int portParentMaxWaitMs = 20000;
            IWebElement portParentElement = null;

            for (int waited = 0; waited <= portParentMaxWaitMs; waited += portParentPollIntervalMs)
            {
                token.ThrowIfCancellationRequested();
                portParentElement = scopeElement.FindElements(By.XPath(portParentXPath)).FirstOrDefault(el => el.Displayed);
                if (portParentElement != null) break;
                await Task.Delay(portParentPollIntervalMs, token);
            }

            if (portParentElement == null)
            {
                LogManager.LogMessage($"[{machineName}] {targetPortParentName} (Port 부모 노드)를 {portParentMaxWaitMs / 1000}초 내에 찾지 못했습니다.", LogManager.Level.Warning);
                return (0, 0);
            }

            bool parentClicked = await Util_Element.ScrollAndClickAsync(driver, portParentElement, 1000);
            if (!parentClicked) return (0, 0);

            string childPortXPath = $"following::span[contains(@class, 'wj-node-text') and contains(text(), '{targetChildPortPrefix}')]";

            // 💡 부모 폴더 클릭 직후 하위 포트가 0개로 뜨는 버그 방지 (최대 2.5초 감시 대기)
            List<IWebElement> visibleChildPorts = new List<IWebElement>();
            for (int retry = 0; retry < 5; retry++)
            {
                token.ThrowIfCancellationRequested();
                visibleChildPorts = portParentElement.FindElements(By.XPath(childPortXPath)).Where(el => el.Displayed).ToList();
                if (visibleChildPorts.Count > 0) break; // 나타나면 즉시 감시 종료 후 진행
                await Task.Delay(500, token);
            }

            int childPortCount = visibleChildPorts.Count;

            // 로깅 추가 (실패 시 원인 파악용)
            if (childPortCount == 0)
            {
                LogManager.LogMessage($"[{machineName}] {targetPortParentName} 하위에 '{targetChildPortPrefix}' 포트가 발견되지 않아 스킵합니다.", LogManager.Level.Warning);
                return (0, 0);
            }

            LogManager.LogMessage($"[{machineName}] 총 {childPortCount}개의 Port 발견. 수집을 시작합니다.", LogManager.Level.Info);

            for (int j = 0; j < childPortCount; j++)
            {
                token.ThrowIfCancellationRequested();
                var refreshedPorts = portParentElement.FindElements(By.XPath(childPortXPath)).Where(el => el.Displayed).ToList();
                if (j >= refreshedPorts.Count) break;

                var targetPort = refreshedPorts[j];
                string portName = targetPort.Text;

                bool portClicked = await Util_Element.ScrollAndClickAsync(driver, targetPort, 1500);
                if (portClicked)
                {
                    token.ThrowIfCancellationRequested();
                    var tableData = await Util_MgmtElement.GetTableDataBySmartScrollAsync(driver);


                    if (tableData != null && tableData.Count > 0)
                    {
                        SaveDataToExcel(workbook, machineName, portName, tableData);
                        count++;
                    }
                    else
                    {
                        LogManager.LogMessage($"[{machineName}] {portName}의 데이터를 읽지 못했습니다.", LogManager.Level.Warning);
                    }
                }
            }

            return (count, childPortCount);
        }

        // 4. [리포팅] 최종 결과 집계 및 사용자 알림 출력
        // 🔍 [검증] expectedSemCount/expectedPortCount(발견/기대 수량)와 실제 수집 수량을 비교하여
        //    불일치 시 데이터가 부정확할 수 있음을 경고
        public static void ShowFinalReport(string eqpType, int machineCount, int successMachineCount, int collectedSemCount, int collectedPortCount, TimeSpan elapsedTime, List<string> failedMachines, int expectedSemCount, int expectedPortCount)
        {
            // 💡 [수정] 60분이 넘어가면 '시간' 단위로 변환하여 문자열을 동적 조합
            int hours = (int)elapsedTime.TotalHours;
            int minutes = elapsedTime.Minutes;  // 0~59분만 반환
            int seconds = elapsedTime.Seconds;  // 0~59초만 반환

            // 시간이 1시간 이상일 때와 아닐 때를 구분하여 텍스트 포맷 결정
            string timeFormat = hours > 0
                ? $"{hours}시간 {minutes}분 {seconds}초"
                : $"{minutes}분 {seconds}초";

            // 🔍 [검증] 기대 수량 대비 실제 수집 수량 불일치 여부 확인
            bool semMismatch = expectedSemCount != collectedSemCount;
            bool portMismatch = expectedPortCount != collectedPortCount;
            bool hasMismatch = semMismatch || portMismatch;

            // 백그라운드 관리자 로그 — "ShowFinalReport ::" 프리픽스 줄 다음 공백 한 줄, 그 아래 위/아래 === 바로 감싼 블록
            var reportLines = new List<string>
            {
                "",
                "",
                "===================================================",
                $"[최종 요약] 전체 자동화 수집 루프 완료 ({eqpType})",
                $" - 총 소요 시간 : {timeFormat}",
                $" - 대상 설비 : 총 {machineCount}대 중 {successMachineCount}대 완료 (실패: {failedMachines.Count}대)",
                $" - 엑셀 누적 결과 : SEM ({collectedSemCount}건), Port ({collectedPortCount}건)"
            };

            if (hasMismatch)
            {
                reportLines.Add($" - [검증 경고] 수량 불일치 감지 : SEM 기대 {expectedSemCount}건 vs 실제 {collectedSemCount}건 / Port 기대 {expectedPortCount}건 vs 실제 {collectedPortCount}건");
            }

            if (failedMachines.Count > 0)
            {
                reportLines.Add($" - 실패 호기 목록 : {string.Join(", ", failedMachines)}");
            }

            reportLines.Add("===================================================");

            // 레벨은 심각도가 가장 높은 것으로 통일 (Error > Warning > Info)
            Level reportLevel = failedMachines.Count > 0 ? Level.Error
                               : hasMismatch ? Level.Warning
                               : Level.Info;

            LogMessage(string.Join(Environment.NewLine, reportLines), reportLevel);

            // 사용자 화면 팝업용 리스트 축약
            string failedListDisplay = "None";
            if (failedMachines.Count > 0)
            {
                var displayList = failedMachines.Take(5).ToList();
                failedListDisplay = string.Join(", ", displayList);
                if (failedMachines.Count > 5)
                {
                    failedListDisplay += $" and {failedMachines.Count - 5} more...";
                }
            }

            double avgTime = successMachineCount > 0 ? (elapsedTime.TotalSeconds / successMachineCount) : 0;

            // 🔍 [검증] 불일치 시 메시지박스에도 경고 문구 추가
            string mismatchWarning = hasMismatch
                ? "\n⚠️ [데이터 검증 경고]\n" +
                  $"SEM 기대 {expectedSemCount}건 / 실제 {collectedSemCount}건, Port 기대 {expectedPortCount}건 / 실제 {collectedPortCount}건로 수량이 일치하지 않습니다.\n" +
                  "데이터가 부정확할 수 있으니 로그를 확인해 주십시오.\n"
                : "";

            string reportMessage = $"🎉 모든 설비의 데이터 수집이 완료되었습니다!\n\n" +
                       "📊 [수집 요약]\n" +
                       $"• 처리된 설비: 총 {machineCount}대 중 {successMachineCount}대 성공\n" +
                       $"• 실패한 설비: {failedMachines.Count}대 ({failedListDisplay})\n" +
                       $"• 총 엑셀 저장 건수: {collectedSemCount + collectedPortCount}건 (SEM: {collectedSemCount}건 / Port: {collectedPortCount}건)\n" +
                       $"• 총 소요 시간: {timeFormat} (설비당 평균 {avgTime:F1}초)\n" +
                       mismatchWarning + "\n" +
                       "💾 [저장 위치]\n" +
                       "바탕화면 ➔ Integrated_Equipment_Data.xlsx";

            MessageBox.Show(reportMessage, "Data Collection Complete", MessageBoxButtons.OK,
                hasMismatch ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }


        /// <summary>
        /// 💡 [엑셀 인터락] 수집 시작 전, 바탕화면 엑셀 파일의 중복 및 잠금 상태를 사전 검증합니다.
        /// </summary>
        /// <param name="filePath">검증할 엑셀 파일 전체 경로</param>
        /// <returns>true(진행 가능), false(수집 중단)</returns>
        public static bool CheckExcelFileInterlock(string filePath)
        {
            if (!File.Exists(filePath)) return true; // 파일이 없으면 즉시 통과

            try
            {
                // 1. 파일이 다른 프로그램(엑셀 등)에 의해 열려 있는지 잠금 상태 체크
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
            }
            catch (IOException)
            {
                LogMessage("엑셀 파일이 열려 있어 수집을 시작할 수 없습니다.", Level.Error);
                MessageBox.Show("저장 대상 엑셀 파일이 현재 실행(열림) 중입니다.\n\n" +
                                "데이터 증발 및 충돌을 방지하기 위해 엑셀 파일을 완전히 닫은 후 다시 시도해 주십시오.",
                                "파일 잠김 에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // 진행 불가
            }

            // 2. 파일 초기화 여부 결정 팝업창 출력
            DialogResult fileCheckResult = MessageBox.Show(
                "바탕화면에 이미 수집된 엑셀 파일이 존재합니다.\n기존 데이터에 이어서 누적(Append) 하시겠습니까?\n\n" +
                "• [예(Yes)] : 기존 파일 유지 및 하단에 데이터 누적\n" +
                "• [아니요(No)] : 기존 파일 초기화(삭제) 후 새 파일로 시작\n" +
                "• [취소(Cancel)] : 수집 작업 중단",
                "중복 파일 인터락", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (fileCheckResult == DialogResult.Cancel)
            {
                LogMessage("중복 파일 인터락 - 사용자가 수집을 취소했습니다.", Level.Info);
                return false; // 진행 불가
            }
            else if (fileCheckResult == DialogResult.No)
            {
                try
                {
                    File.Delete(filePath);
                    LogMessage("중복 파일 인터락 - 기존 파일을 삭제하고 초기화했습니다.", Level.Info);
                }
                catch (Exception ex)
                {
                    LogException(ex, Level.Error, "기존 파일 초기화 중 예외 발생");
                    return false;
                }
            }
            else
            {
                LogMessage("중복 파일 인터락 - 기존 파일에 데이터를 누적합니다.", Level.Info);
            }

            return true; // 모든 검증 통과 (진행 가능)
        }

        /// <summary>
        /// 단일 호기를 이름 기반 XPath로 직접 찾아 클릭하고 데이터를 수집합니다.
        /// 인덱스 기반 탐색을 제거하여 StaleElementReferenceException을 원천 차단합니다.
        /// </summary>
        public static async Task<(bool isSuccess, string machineName, int semCount, int portCount, string errorMessage, int expectedPortCount)>
        ProcessSingleMachineAsync(IWebDriver driver, XLWorkbook workbook, string machineXPath, string currentMachineName,
                             (string semName, string portParentName, string childPortPrefix) keys,
                             bool isSemChecked, bool isPortChecked, CancellationToken token)
        {
            int semCount = 0;
            int portCount = 0;
            int expectedPortCount = 0;

            try
            {
                // 이름으로 직접 탐색 (인덱스 기반 제거)
                var targetMachine = driver.FindElements(By.XPath(machineXPath))
                    .Where(el => el.Displayed).FirstOrDefault();
                if (targetMachine == null) return (false, currentMachineName, 0, 0, "화면에서 호기 노드를 찾을 수 없습니다.", 0);

                // 1. 호기 폴더 펼치기
                bool machineClicked = await Util_Element.ScrollAndClickAsync(driver, targetMachine, 1000);
                if (!machineClicked) return (false, currentMachineName, 0, 0, "호기 노드 클릭 실패", 0);

                // =================================================================
                // 💡 [구조 복원 + 격리] UI 네비게이션 필수 경로 타격
                // Port 수집을 위해 길을 열어주려면 반드시 SEM 노드를 클릭해야 하는 물리적 종속성 반영
                // 🔒 [격리] 전역(//) 탐색 대신 targetMachine 기준 following:: 축으로 탐색하여
                //    이전 호기(위쪽에 위치)의 잔존 노드를 절대 잡지 않도록 강제 격리
                // =================================================================
                string semXPath = $"following::span[contains(@class, 'wj-node-text') and text()='{keys.semName}']";

                // 🕒 [폴링 대기] 렌더링 지연 대비 — 0.5초 간격으로 최대 20초까지 감시.
                //    나타나는 즉시 대기를 끝내고 진행하므로 정상 케이스의 속도 저하는 없음.
                const int semPollIntervalMs = 500;
                const int semMaxWaitMs = 20000;
                IWebElement semElement = null;

                for (int waited = 0; waited <= semMaxWaitMs; waited += semPollIntervalMs)
                {
                    token.ThrowIfCancellationRequested();
                    semElement = targetMachine.FindElements(By.XPath(semXPath)).FirstOrDefault(el => el.Displayed);
                    if (semElement != null) break;
                    await Task.Delay(semPollIntervalMs, token);
                }

                if (semElement == null)
                {
                    LogManager.LogMessage($"[{currentMachineName}] {keys.semName} 노드를 {semMaxWaitMs / 1000}초 내에 찾지 못해 하위 스캔을 중단합니다.", LogManager.Level.Warning);
                    // 🔧 [수정] 경로를 못 찾은 것은 명백한 실패이므로 true로 위장하지 않고 실패 처리
                    return (false, currentMachineName, 0, 0, $"{keys.semName} 노드를 {semMaxWaitMs / 1000}초 내에 찾을 수 없음 (렌더링 지연 또는 트리 상태 이상)", 0);
                }

                // 수집 여부와 무관하게 무조건 클릭하여 하위 트리(Port)를 렌더링시킴
                bool semClicked = await Util_Element.ScrollAndClickAsync(driver, semElement, 1000);
                if (!semClicked)
                {
                    return (false, currentMachineName, 0, 0, $"{keys.semName} 노드 클릭 실패", 0);
                }

                // 💡 [핵심] 길은 열어두었으나, 실제 데이터를 긁을지 말지는 체크박스에 따라 철저히 독립적으로 작동
                if (isSemChecked)
                {
                    token.ThrowIfCancellationRequested();
                    semCount += await CollectSemDataAsync(driver, workbook, currentMachineName, keys.semName, token);
                }
                else
                {
                    LogManager.LogMessage($"[{currentMachineName}] 옵션에 따라 {keys.semName} 데이터 수집은 스킵합니다.", LogManager.Level.Info);
                }

                // 위에서 SEM을 클릭해 길을 열었으므로, 이제 Port가 정상적으로 스캔됨
                // 🔍 [검증용] 발견된 Port 개수(expected)와 실제 수집 성공 개수(collected)를 함께 받아 상위로 전달
                if (isPortChecked)
                {
                    token.ThrowIfCancellationRequested();
                    var portResult = await CollectPortDataAsync(driver, semElement, workbook, currentMachineName, keys.portParentName, keys.childPortPrefix, token);
                    portCount += portResult.collected;
                    expectedPortCount += portResult.discovered;
                }

                // 💡 [삭제] 호기 폴더 접기 로직 제거됨.
                // following:: 축 격리 탐색으로 이전 호기 잔존 노드를 이미 구조적으로 배제하고 있어
                // 폴더를 닫지 않아도 데이터 정확성에 영향이 없음을 실측(82대 전량 일치)으로 확인함.
                // 호기당 약 0.9초씩 불필요하게 소요되던 구간을 제거.

                return (true, currentMachineName, semCount, portCount, string.Empty, expectedPortCount);
            }
            catch (OperationCanceledException)
            {
                return (false, currentMachineName, 0, 0, "사용자에 의해 수집이 취소되었습니다.", 0);
            }
            catch (WebDriverException ex) when (ex.Message.Contains("no such window") || ex.Message.Contains("disconnected") || ex.Message.Contains("closed"))
            {
                return (false, currentMachineName, 0, 0, "[BROWSER_CLOSED] 브라우저가 강제 종료되었습니다.", 0);
            }
            catch (Exception ex)
            {
                return (false, currentMachineName, 0, 0, ex.Message, 0);
            }
        }







    } // Util.Mgmt.cs END
} // namespace