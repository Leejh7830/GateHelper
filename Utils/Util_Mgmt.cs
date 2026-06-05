using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.IO;
using static GateHelper.LogManager;


namespace GateHelper.Utils
{
    public static class Util_Mgmt
    {
        /// <summary>
        /// 수집 항목을 선택받는 동적 팝업을 띄우고 결과를 반환합니다.
        /// 반환값: (SEM 선택 여부, Port 선택 여부)
        /// </summary>
        public static (bool isSem, bool isPort) ShowCollectionSelectDialog()
        {
            // 1. 메모리 상에서 즉석으로 폼(창) 생성
            Form prompt = new Form()
            {
                Width = 320,
                Height = 220,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "데이터 수집 옵션",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // 2. 체크박스 컨트롤 동적 생성
            CheckBox chkSem = new CheckBox()
            {
                Left = 40,
                Top = 30,
                Text = "StockerSEM 수집",
                // Checked = true,
                Width = 220
            };
            CheckBox chkPort = new CheckBox()
            {
                Left = 40,
                Top = 60,
                Text = "StockerPort 수집 (하위 항목 포함)",
                // Checked = true,
                Width = 220
            };

            // 3. 버튼 컨트롤 동적 생성
            Button btnOk = new Button()
            {
                Text = "수집 시작",
                Left = 40,
                Top = 120,
                Width = 100,
                DialogResult = DialogResult.OK
            };
            Button btnCancel = new Button()
            {
                Text = "취소",
                Left = 150,
                Top = 120,
                Width = 100,
                DialogResult = DialogResult.Cancel
            };

            // 4. 생성한 컨트롤들을 폼에 부착
            prompt.Controls.Add(chkSem);
            prompt.Controls.Add(chkPort);
            prompt.Controls.Add(btnOk);
            prompt.Controls.Add(btnCancel);

            // 엔터 키 누르면 확인, ESC 키 누르면 취소 작동
            prompt.AcceptButton = btnOk;
            prompt.CancelButton = btnCancel;

            // 5. 창을 띄우고(ShowDialog) 사용자의 응답 대기 및 결과 반환
            if (prompt.ShowDialog() == DialogResult.OK)
            {
                return (chkSem.Checked, chkPort.Checked);
            }

            // 취소 버튼을 누르거나 창을 그냥 끄면 둘 다 false 반환
            return (false, false);
        } // ShowCollectionSelectDialog END


        /// <summary>
        /// 수집된 표 데이터를 7열 규격으로 변환하여 지정된 라인/타입 시트에 실시간 누적 저장합니다.
        /// </summary>
        /// <param name="machineName">호기명 (예: FSTO_01)</param>
        /// <param name="itemName">수집항목명 (예: StockerSEM, STOCKERPORT:1)</param>
        /// <param name="tableData">순수 5열 데이터 리스트</param>
        public static void SaveDataToExcel(string machineName, string itemName, List<string[]> tableData)
        {
            if (tableData == null || tableData.Count == 0) return;

            // 1. 저장 경로 지정 (실행 파일이 있는 폴더에 생성)
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "전체_설비데이터_통합.xlsx");

            // 2. 동적 시트명 결정 로직 (예: FSTO_01 -> F_SEM 또는 F_PORT)
            string linePrefix = machineName.Substring(0, 1).ToUpper(); // 첫 글자 추출 (F, A, E, P)
            string typeSuffix = itemName.Contains("SEM") ? "SEM" : "PORT";
            string sheetName = $"{linePrefix}_{typeSuffix}";

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
                LogManager.LogMessage($"[엑셀 저장 실패] '{filePath}' 파일이 열려있습니다. 파일을 닫아주세요.", Level.Error);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error, "엑셀 누적 저장 중 예기치 않은 오류 발생");
            }
        } // SaveDataToExcel END

    } // Util.Mgmt.cs END
























} // namespace
