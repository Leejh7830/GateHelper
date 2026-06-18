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
        /// 💡 [개조] 수집 항목을 다중 선택받는 동적 팝업을 띄우고 결과를 반환합니다.
        /// 반환값: (선택된_설비리스트, SEM선택, Port선택)
        /// </summary>
        /// <summary>
        /// 💡 [개조] 수집 항목을 다중 선택받는 동적 팝업을 띄우고 결과를 반환합니다.
        /// 사용자가 옵션을 선택하지 않았을 경우 경고창을 띄우고 수집을 차단하는 인터락이 적용되었습니다.
        /// 반환값: (선택된_설비리스트, SEM선택, Port선택)
        /// </summary>
        public static (List<string> selectedTypes, bool isSem, bool isPort) ShowCollectionSelectDialog(List<string> scannedTypes)
        {
            Form prompt = new Form()
            {
                Width = 340,
                Height = 320,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "스마트 데이터 수집 옵션",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label lblEqpType = new Label() { Left = 40, Top = 20, Text = "대상 설비 타입 (다중 선택 가능):", Width = 250 };

            CheckedListBox chkEqpTypes = new CheckedListBox() { Left = 40, Top = 45, Width = 240, Height = 80, CheckOnClick = true };

            // 스캔된 항목이 없으면 기본값(폴백) 제공
            if (scannedTypes.Count == 0) scannedTypes.AddRange(new[] { "STO", "OHS", "CNV", "AGV" });
            foreach (var type in scannedTypes) chkEqpTypes.Items.Add(type);

            // 첫 번째 항목은 기본 체크
            if (chkEqpTypes.Items.Count > 0) chkEqpTypes.SetItemChecked(0, true);

            CheckBox chkSem = new CheckBox() { Left = 40, Top = 140, Text = "StockerSEM 수집 (OHS 호환)", Width = 240 };
            CheckBox chkPort = new CheckBox() { Left = 40, Top = 170, Text = "StockerPort 수집 (하위 항목 포함)", Width = 240 };

            // 💡 [핵심 수정 1] btnOk에서 'DialogResult = DialogResult.OK' 속성을 제거하여 창이 강제로 닫히는 것을 막습니다.
            Button btnOk = new Button() { Text = "수집 시작", Left = 40, Top = 220, Width = 100 };
            Button btnCancel = new Button() { Text = "취소", Left = 180, Top = 220, Width = 100, DialogResult = DialogResult.Cancel };

            // 💡 [핵심 수정 2] 클릭 이벤트를 직접 가로채어 예외 인터락(Validation)을 수행합니다.
            btnOk.Click += (sender, e) =>
            {
                // 방어막 1: 설비 타입 선택 누락 체크
                if (chkEqpTypes.CheckedItems.Count == 0)
                {
                    MessageBox.Show("수집할 대상 설비 타입을 최소 하나 이상 선택해 주십시오.", "선택 누락", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // return을 통해 함수를 빠져나가면 창이 닫히지 않고 그대로 유지됩니다.
                }

                // 방어막 2: 수집 항목 선택 누락 체크
                if (!chkSem.Checked && !chkPort.Checked)
                {
                    MessageBox.Show("수집할 데이터(SEM 또는 Port)를 최소 하나 이상 체크해 주십시오.", "선택 누락", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 모든 검증을 완벽히 통과했을 때만 창의 결과를 OK로 조작하여 다이얼로그를 닫습니다.
                prompt.DialogResult = DialogResult.OK;
            };

            prompt.Controls.AddRange(new Control[] { lblEqpType, chkEqpTypes, chkSem, chkPort, btnOk, btnCancel });
            prompt.AcceptButton = btnOk; prompt.CancelButton = btnCancel;

            // ShowDialog()가 반환될 때까지 코드는 여기서 멈춰서 대기합니다.
            if (prompt.ShowDialog() == DialogResult.OK)
            {
                var selected = chkEqpTypes.CheckedItems.Cast<string>().ToList();
                return (selected, chkSem.Checked, chkPort.Checked);
            }

            // 취소를 누르거나 X창을 닫은 경우
            return (new List<string>(), false, false);
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
            // 💡 [수정] 호기명 내부에서 설비 키워드를 독립적으로 역추적하여 정확한 라인명(Prefix) 추출
            // 상위에서 어떤 파라미터가 오든 의존하지 않고, machineName 자체를 스캔합니다.
            // ==========================================================
            string linePrefix = "UNKNOWN";
            string[] knownKeywords = { "STO", "OHS", "CNV", "AGV", "DDA" }; // 향후 설비 추가 시 여기에만 단어 추가

            foreach (var keyword in knownKeywords)
            {
                int idx = machineName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (idx > 0)
                {
                    // 키워드(OHS 등) 발견! 바로 앞 글자(1자리)를 라인명으로 추출
                    linePrefix = machineName.Substring(idx - 1, 1).ToUpper();
                    break;
                }
                else if (idx == 0)
                {
                    // 키워드가 맨 앞에 있는 예외 상황 (예: STO_01)
                    linePrefix = machineName.Substring(0, 1).ToUpper();
                    break;
                }
            }

            // 예외 방어: 어떤 키워드도 매칭되지 않았다면 기본적으로 맨 앞 글자를 사용
            if (linePrefix == "UNKNOWN" && !string.IsNullOrEmpty(machineName))
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
        public static async Task<int> CollectSemDataAsync(IWebDriver driver, string eqpType, string machineName, string targetSemName)
        {
            // [주의] 테스트용 절대 경로입니다. 구조가 바뀌면 깨질 수 있으니 추후 상대 경로로 변경을 권장합니다.
            string targetGridXPath = "//*[@id=\"uncontrolled-tab-example-tabpane-WEB030102\"]/div/div[2]/div/div/div[3]/div/div/div[1]/div/div[2]/div/div/div[2]";

            // 💡 [정정] 명시적으로 Util_Element 클래스의 메서드를 호출
            var tableData = await Util_Element.GetTableDataBySmartScrollAsync(driver, targetGridXPath);

            if (tableData != null && tableData.Count > 0)
            {
                await Task.Run(() => SaveDataToExcel(eqpType, machineName, targetSemName, tableData));
                return 1;
            }
            return 0;
        }

        // 3. [서브루틴] Port 부모 전개 및 자식 다중 수집
        public static async Task<int> CollectPortDataAsync(IWebDriver driver, string eqpType, string machineName, string targetPortParentName, string targetChildPortPrefix)
        {
            int count = 0;
            string targetGridXPath = "//*[@id=\"uncontrolled-tab-example-tabpane-WEB030102\"]/div/div[2]/div/div/div[3]/div/div/div[1]/div/div[2]/div/div/div[2]";

            string portParentXPath = $"//span[contains(@class, 'wj-node-text') and text()='{targetPortParentName}']";
            var portParentElement = driver.FindElements(By.XPath(portParentXPath)).Where(el => el.Displayed).LastOrDefault();

            if (portParentElement != null)
            {
                bool parentClicked = await Util_Element.ScrollAndClickAsync(driver, portParentElement, 1000);
                if (parentClicked)
                {
                    string childPortXPath = $"//span[contains(@class, 'wj-node-text') and contains(text(), '{targetChildPortPrefix}')]";

                    // 💡 [핵심 수정] 부모 폴더 클릭 직후 하위 포트가 0개로 뜨는 버그 방지 (최대 2.5초 감시 대기)
                    List<IWebElement> visibleChildPorts = new List<IWebElement>();
                    for (int retry = 0; retry < 5; retry++)
                    {
                        visibleChildPorts = driver.FindElements(By.XPath(childPortXPath)).Where(el => el.Displayed).ToList();
                        if (visibleChildPorts.Count > 0) break; // 나타나면 즉시 감시 종료 후 진행
                        await Task.Delay(500);
                    }

                    int childPortCount = visibleChildPorts.Count;

                    // 로깅 추가 (실패 시 원인 파악용)
                    if (childPortCount == 0)
                    {
                        LogManager.LogMessage($"[{machineName}] {targetPortParentName} 하위에 '{targetChildPortPrefix}' 포트가 발견되지 않아 스킵합니다.", LogManager.Level.Warning);
                        return 0;
                    }

                    LogManager.LogMessage($"[{machineName}] 총 {childPortCount}개의 Port 발견. 수집을 시작합니다.", LogManager.Level.Info);

                    for (int j = 0; j < childPortCount; j++)
                    {
                        var refreshedPorts = driver.FindElements(By.XPath(childPortXPath)).Where(el => el.Displayed).ToList();
                        if (j >= refreshedPorts.Count) break;

                        var targetPort = refreshedPorts[j];
                        string portName = targetPort.Text;

                        bool portClicked = await Util_Element.ScrollAndClickAsync(driver, targetPort, 1500);
                        if (portClicked)
                        {
                            var tableData = await Util_Element.GetTableDataBySmartScrollAsync(driver, targetGridXPath);

                            if (tableData != null && tableData.Count > 0)
                            {
                                await Task.Run(() => SaveDataToExcel(eqpType, machineName, portName, tableData));
                                count++;
                            }
                            else
                            {
                                LogManager.LogMessage($"[{machineName}] {portName}의 데이터를 읽지 못했습니다.", LogManager.Level.Warning);
                            }
                        }
                    }
                }
            }
            else
            {
                LogManager.LogMessage($"[{machineName}] {targetPortParentName} (Port 부모 노드)를 화면에서 찾을 수 없습니다.", LogManager.Level.Warning);
            }
            return count;
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
        ProcessSingleMachineAsync(IWebDriver driver, int index, string targetXPath, string currentMachineName,
                             (string semName, string portParentName, string childPortPrefix) keys,
                             bool isSemChecked, bool isPortChecked)
        {
            int semCount = 0;
            int portCount = 0;

            try
            {
                var currentMachines = driver.FindElements(By.XPath(targetXPath)).Where(el => el.Displayed).ToList();
                if (index >= currentMachines.Count) return (false, currentMachineName, 0, 0, "화면에서 호기 노드가 유실되었습니다.");

                var targetMachine = currentMachines[index];

                // 1. 호기 폴더 펼치기
                bool machineClicked = await Util_Element.ScrollAndClickAsync(driver, targetMachine, 1000);
                if (!machineClicked) return (false, currentMachineName, 0, 0, "호기 노드 클릭 실패");

                // =================================================================
                // 💡 [구조 복원] UI 네비게이션 필수 경로 타격
                // Port 수집을 위해 길을 열어주려면 반드시 SEM 노드를 클릭해야 하는 물리적 종속성 반영
                // =================================================================
                string semXPath = $"//span[contains(@class, 'wj-node-text') and text()='{keys.semName}']";
                var semElement = driver.FindElements(By.XPath(semXPath)).Where(el => el.Displayed).LastOrDefault();

                if (semElement != null)
                {
                    // 수집 여부와 무관하게 무조건 클릭하여 하위 트리(Port)를 렌더링시킴
                    bool semClicked = await Util_Element.ScrollAndClickAsync(driver, semElement, 1000);

                    if (semClicked)
                    {
                        // 💡 [핵심] 길은 열어두었으나, 실제 데이터를 긁을지 말지는 체크박스에 따라 철저히 독립적으로 작동
                        if (isSemChecked)
                        {
                            semCount += await CollectSemDataAsync(driver, currentMachineName, currentMachineName, keys.semName);
                        }
                        else
                        {
                            LogManager.LogMessage($"[{currentMachineName}] 옵션에 따라 {keys.semName} 데이터 수집은 스킵합니다.", LogManager.Level.Info);
                        }

                        // 위에서 SEM을 클릭해 길을 열었으므로, 이제 Port가 정상적으로 스캔됨
                        if (isPortChecked)
                        {
                            portCount += await CollectPortDataAsync(driver, currentMachineName, currentMachineName, keys.portParentName, keys.childPortPrefix);
                        }
                    }
                }
                else
                {
                    LogManager.LogMessage($"[{currentMachineName}] {keys.semName} 필수 경로를 찾을 수 없어 하위 스캔을 중단합니다.", LogManager.Level.Warning);
                }

                // =================================================================
                // 클린 DOM: 호기 폴더 접기 (이름 기반 절대 타격 유지)
                // =================================================================
                try
                {
                    string closeXPath = $"//span[contains(@class, 'wj-node-text') and text()='{currentMachineName}']";
                    var closeNode = driver.FindElements(By.XPath(closeXPath)).Where(el => el.Displayed).FirstOrDefault();
                    if (closeNode != null) await Util_Element.ScrollAndClickAsync(driver, closeNode, 500);
                }
                catch (Exception ex)
                {
                    LogManager.LogMessage($"[{currentMachineName}] 폴더 접기 무시됨: {ex.Message}", LogManager.Level.Warning);
                }

                return (true, currentMachineName, semCount, portCount, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, currentMachineName, 0, 0, ex.Message);
            }
        }







    } // Util.Mgmt.cs END
} // namespace
