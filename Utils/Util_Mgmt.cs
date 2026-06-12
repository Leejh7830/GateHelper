using ClosedXML.Excel;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;


namespace GateHelper.Utils
{
    public static class Util_Mgmt
    {
        /// <summary>
        /// 수집 항목을 선택받는 동적 팝업을 띄우고 결과를 반환합니다.
        /// 반환값: (SEM 선택 여부, Port 선택 여부)
        /// </summary>
        public static (string eqpType, bool isSem, bool isPort) ShowCollectionSelectDialog()
        {
            Form prompt = new Form()
            {
                Width = 320,
                Height = 280,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "데이터 수집 옵션",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // 설비 타입 드롭다운
            Label lblEqpType = new Label() { Left = 40, Top = 20, Text = "대상 설비 타입:" };
            ComboBox cmbEqpType = new ComboBox()
            {
                Left = 40,
                Top = 45,
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEqpType.Items.AddRange(new string[] { "STO", "CNV", "DDA" });
            cmbEqpType.SelectedIndex = 0; // 기본값 STO

            // 수정필요한 부분
            CheckBox chkSem = new CheckBox() { Left = 40, Top = 90, Text = "StockerSEM 수집", Width = 220 };
            CheckBox chkPort = new CheckBox() { Left = 40, Top = 120, Text = "StockerPort 수집 (하위 항목 포함)", Width = 220 };

            Button btnOk = new Button() { Text = "수집 시작", Left = 40, Top = 180, Width = 100, DialogResult = DialogResult.OK };
            Button btnCancel = new Button() { Text = "취소", Left = 150, Top = 180, Width = 100, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(lblEqpType);
            prompt.Controls.Add(cmbEqpType);
            prompt.Controls.Add(chkSem);
            prompt.Controls.Add(chkPort);
            prompt.Controls.Add(btnOk);
            prompt.Controls.Add(btnCancel);

            prompt.AcceptButton = btnOk;
            prompt.CancelButton = btnCancel;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                return (cmbEqpType.SelectedItem.ToString(), chkSem.Checked, chkPort.Checked);
            }

            return (null, false, false);
        } // ShowCollectionSelectDialog END


        /// <summary>
        /// 수집된 표 데이터를 7열 규격으로 변환하여 지정된 라인/타입 시트에 실시간 누적 저장합니다.
        /// </summary>
        /// <param name="machineName">호기명 (예: FSTO_01)</param>
        /// <param name="itemName">수집항목명 (예: StockerSEM, STOCKERPORT:1)</param>
        /// <param name="tableData">순수 5열 데이터 리스트</param>
        public static void SaveDataToExcel(string targetEqpType, string machineName, string itemName, List<string[]> tableData)
        {
            if (tableData == null || tableData.Count == 0) return;

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "Integrated_Equipment_Data.xlsx");

            // ==========================================================
            // 💡 targetEqpType을 동적으로 검색 (STO/CNV/DDA)
            // ==========================================================
            string linePrefix = "UNKNOWN";
            int typeIndex = machineName.IndexOf(targetEqpType, StringComparison.OrdinalIgnoreCase);

            if (typeIndex > 0)
            {
                linePrefix = machineName.Substring(typeIndex - 1, 1).ToUpper();
            }
            else
            {
                linePrefix = machineName.Substring(0, 1).ToUpper();
            }

            string typeSuffix = itemName.Contains("SEM") ? "SEM" : "PORT";
            string sheetName = $"{linePrefix}_{typeSuffix}";
            // ==========================================================

            try
            {
                // 3. 파일이 있으면 열고, 없으면 새로 생성
                using (var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook())
                {
                    IXLWorksheet worksheet;

                    // 4. 타겟 시트가 없으면 생성하고 헤더(7열) 작성
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

                        // 헤더 디자인 적용
                        var headerRange = worksheet.Range("A1:G1");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    // 5. 시트의 데이터가 있는 마지막 줄을 찾아서 그 다음 줄(startRow) 계산
                    int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                    int startRow = lastRow + 1;

                    // 6. 메모리의 5열 데이터를 7열로 재조립하여 엑셀 셀에 입력
                    for (int i = 0; i < tableData.Count; i++)
                    {
                        var rowData = tableData[i];
                        int currentRow = startRow + i;

                        // 강제 주입: 1열(호기명), 2열(수집항목)
                        worksheet.Cell(currentRow, 1).Value = machineName;
                        worksheet.Cell(currentRow, 2).Value = itemName;

                        // 원본 삽입: 3열 ~ 7열
                        for (int j = 0; j < rowData.Length && j < 5; j++)
                        {
                            // Value 열(인덱스 3)이 숫자로만 되어있을 경우 엑셀 수식 변환 오류를 막기 위해 문자열 처리 방어
                            worksheet.Cell(currentRow, 3 + j).SetValue(rowData[j]);
                        }
                    }

                    // 7. 칼럼 너비 자동 맞춤 (옵션)
                    worksheet.Columns().AdjustToContents();

                    // 8. 물리적 디스크에 저장 (덮어쓰기)
                    workbook.SaveAs(filePath);
                }
            }
            catch (IOException)
            {
                // 사용자가 엑셀 파일을 띄워놓고 있어서 프로그램이 접근하지 못할 때의 치명적 에러 방어
                LogMessage($"[엑셀 저장 실패] '{filePath}' 파일이 열려있습니다. 파일을 닫아주세요.", Level.Error);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "엑셀 누적 저장 중 예기치 않은 오류 발생");
            }
        } // SaveDataToExcel END







        // 1. [설비 확산 대비] 키워드 매핑 팩토리
        public static (string semName, string portParentName, string childPortPrefix) GetEquipmentKeywords(string eqpType)
        {
            // 기능확장 시 수정 필요
            if (eqpType == "CNV") return ("CnvSEM", "CnvPorts", "CNVPORT:");
            if (eqpType == "AGV") return ("AgvSEM", "AgvPorts", "AGVPORT:");

            // 기본값 STO
            return ("StockerSEM", "StockerPorts", "STOCKERPORT:");
        }

        // 2. [서브루틴] SEM 데이터 단일 수집 및 저장
        public static async Task<int> CollectSemDataAsync(IWebDriver driver, string eqpType, string machineName, string targetSemName)
        {
            var tableData = await Task.Run(() => Util_Element.GetTableDataByClipboardFast(driver));
            if (tableData != null && tableData.Count > 0)
            {
                await Task.Run(() => SaveDataToExcel(eqpType, machineName, targetSemName, tableData));
                return 1; // 수집 성공 건수 반환
            }
            return 0;
        }

        // 3. [서브루틴] Port 부모 전개 및 자식 다중 수집
        public static async Task<int> CollectPortDataAsync(IWebDriver driver, string eqpType, string machineName, string targetPortParentName, string targetChildPortPrefix)
        {
            int count = 0;
            string portParentXPath = $"//span[contains(@class, 'wj-node-text') and text()='{targetPortParentName}']";
            var portParentElement = driver.FindElements(By.XPath(portParentXPath)).FirstOrDefault(el => el.Displayed);

            if (portParentElement != null)
            {
                bool parentClicked = await Util_Element.ScrollAndClickAsync(driver, portParentElement, 1000);
                if (parentClicked)
                {
                    string childPortXPath = $"//span[contains(@class, 'wj-node-text') and contains(text(), '{targetChildPortPrefix}')]";
                    var visibleChildPorts = driver.FindElements(By.XPath(childPortXPath)).Where(el => el.Displayed).ToList();
                    int childPortCount = visibleChildPorts.Count;

                    for (int j = 0; j < childPortCount; j++)
                    {
                        var refreshedPorts = driver.FindElements(By.XPath(childPortXPath)).Where(el => el.Displayed).ToList();
                        if (j >= refreshedPorts.Count) break;

                        var targetPort = refreshedPorts[j];
                        string portName = targetPort.Text;

                        bool portClicked = await Util_Element.ScrollAndClickAsync(driver, targetPort, 1500);
                        if (portClicked)
                        {
                            var tableData = await Task.Run(() => Util_Element.GetTableDataByClipboardFast(driver));
                            if (tableData != null && tableData.Count > 0)
                            {
                                await Task.Run(() => SaveDataToExcel(eqpType, machineName, portName, tableData));
                                count++;
                            }
                        }
                    }
                }
            }
            return count; // 누적된 Port 수집 성공 건수 반환
        }

        // 4. [리포팅] 최종 결과 집계 및 사용자 알림 출력
        public static void ShowFinalReport(string eqpType, int machineCount, int successMachineCount, int collectedSemCount, int collectedPortCount, TimeSpan elapsedTime, List<string> failedMachines)
        {
            // 💡 [수정] 60분이 넘어가면 '시간' 단위로 변환하여 문자열을 동적 조합
            int hours = (int)elapsedTime.TotalHours;
            int minutes = elapsedTime.Minutes;  // 0~59분만 반환
            int seconds = elapsedTime.Seconds;  // 0~59초만 반환

            // 시간이 1시간 이상일 때와 아닐 때를 구분하여 텍스트 포맷 결정
            string timeFormat = hours > 0
                ? $"{hours}시간 {minutes}분 {seconds}초"
                : $"{minutes}분 {seconds}초";

            // 백그라운드 관리자 로그
            LogMessage("===================================================", Level.Info);
            LogMessage($"[최종 요약] 전체 자동화 수집 루프 완료 ({eqpType})", Level.Info);
            LogMessage($" - 총 소요 시간 : {timeFormat}", Level.Info);
            LogMessage($" - 대상 설비 : 총 {machineCount}대 중 {successMachineCount}대 완료 (실패: {failedMachines.Count}대)", Level.Info);
            LogMessage($" - 엑셀 누적 결과 : SEM ({collectedSemCount}건), Port ({collectedPortCount}건)", Level.Info);

            if (failedMachines.Count > 0)
            {
                LogMessage($" - 실패 호기 목록 : {string.Join(", ", failedMachines)}", Level.Error);
            }
            LogMessage("===================================================", Level.Info);

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
            string reportMessage = $"🎉 모든 {eqpType} 설비의 데이터 수집이 완료되었습니다!\n\n" +
                       "📊 [수집 요약]\n" +
                       $"• 처리된 설비: 총 {machineCount}대 중 {successMachineCount}대 성공\n" +
                       $"• 실패한 설비: {failedMachines.Count}대 ({failedListDisplay})\n" +
                       $"• 총 엑셀 저장 건수: {collectedSemCount + collectedPortCount}건 (SEM: {collectedSemCount}건 / Port: {collectedPortCount}건)\n" +
                       $"• 총 소요 시간: {timeFormat} (설비당 평균 {avgTime:F1}초)\n\n" +
                       "💾 [저장 위치]\n" +
                       "바탕화면 ➔ Integrated_Equipment_Data.xlsx";

            MessageBox.Show(reportMessage, "Data Collection Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        /// 루프 내부의 단일 호기(Machine)에 대한 DOM 탐색, 클릭, 데이터 수집을 전담합니다.
        /// </summary>
        public static async Task<(bool isSuccess, string machineName, int semCount, int portCount, string errorMessage)>
            ProcessSingleMachineAsync(IWebDriver driver, int index, string targetXPath, string eqpType,
                                     (string semName, string portParentName, string childPortPrefix) keys,
                                     bool isSemChecked, bool isPortChecked)
        {
            int semCount = 0;
            int portCount = 0;
            string machineName = "Unknown";

            try
            {
                // 1. 최신 DOM에서 대상 호기 탐색 (Stale 방지)
                var currentMachines = driver.FindElements(By.XPath(targetXPath)).Where(el => el.Displayed).ToList();

                if (index >= currentMachines.Count)
                    return (false, machineName, 0, 0, "화면에서 호기 노드가 유실되었습니다.");

                var targetMachine = currentMachines[index];
                machineName = targetMachine.Text;

                // 2. 호기 클릭
                bool machineClicked = await Util_Element.ScrollAndClickAsync(driver, targetMachine, 1000);
                if (!machineClicked) return (false, machineName, 0, 0, "호기 노드 클릭 실패");

                // 3. SEM 폴더 탐색 및 클릭
                string semXPath = $"//span[contains(@class, 'wj-node-text') and text()='{keys.semName}']";
                var semElement = driver.FindElements(By.XPath(semXPath)).FirstOrDefault(el => el.Displayed);

                if (semElement != null)
                {
                    bool semClicked = await Util_Element.ScrollAndClickAsync(driver, semElement, 1000);
                    if (!semClicked) return (false, machineName, 0, 0, $"{keys.semName} 노드 클릭 실패");

                    // 4. 실제 수집 엔진 위임
                    if (isSemChecked)
                        semCount += await CollectSemDataAsync(driver, eqpType, machineName, keys.semName);

                    if (isPortChecked)
                        portCount += await CollectPortDataAsync(driver, eqpType, machineName, keys.portParentName, keys.childPortPrefix);

                    // 정상 처리 완료 반환
                    return (true, machineName, semCount, portCount, string.Empty);
                }
                else
                {
                    return (false, machineName, 0, 0, $"{keys.semName} 탐색 실패");
                }
            }
            catch (Exception ex)
            {
                return (false, machineName, 0, 0, ex.Message); // 예기치 못한 에러 반환
            }
        }







    } // Util.Mgmt.cs END
} // namespace
