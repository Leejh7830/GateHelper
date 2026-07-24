using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace GateHelper.Mgmt.ExcelAnalyzer
{
    public static class Util_ExcelAnalyzer
    {
        /// <summary>
        /// 엑셀 파일 내의 모든 시트 이름을 반환합니다.
        /// </summary>
        public static List<string> GetSheetNames(string filePath)
        {
            var sheetNames = new List<string>();
            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var ws in workbook.Worksheets)
                {
                    sheetNames.Add(ws.Name);
                }
            }
            return sheetNames;
        }

        /// <summary>
        /// 엑셀 파일의 지정된 시트에서 첫 번째 행(Header)을 읽어 반환합니다.
        /// </summary>
        public static List<string> GetHeaders(string filePath, string sheetName)
        {
            var headers = new List<string>();

            if (!File.Exists(filePath))
                throw new FileNotFoundException("엑셀 파일을 찾을 수 없습니다.");

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name == sheetName) ?? workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        throw new Exception("엑셀 파일에 워크시트가 존재하지 않습니다.");

                    var firstRow = worksheet.FirstRowUsed();
                    if (firstRow == null)
                        throw new Exception("엑셀 파일이 비어있습니다.");

                    foreach (var cell in firstRow.CellsUsed())
                    {
                        headers.Add(cell.GetString().Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"엑셀 헤더 읽기 실패: {ex.Message}");
            }

            return headers;
        }

        /// <summary>
        /// 지정된 컬럼명들을 기반으로 엑셀 전체 데이터를 비동기(Async) 파싱하여 반환합니다.
        /// </summary>
        public static async Task<List<ExcelRowData>> ParseExcelDataAsync(string filePath, string sheetName, string machineColName, string nameColName, string valueColName, string descColName)
        {
            return await Task.Run(() =>
            {
                var resultList = new List<ExcelRowData>();

                if (!File.Exists(filePath))
                    throw new FileNotFoundException("엑셀 파일을 찾을 수 없습니다.");

                try
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name == sheetName) ?? workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new Exception("엑셀 파일에 워크시트가 존재하지 않습니다.");

                        var firstRow = worksheet.FirstRowUsed();
                        if (firstRow == null)
                            throw new Exception("엑셀 파일이 비어있습니다.");

                        // 1. 헤더 인덱스 찾기 (1-based index)
                        int machineColIdx = -1;
                        int nameColIdx = -1;
                        int valueColIdx = -1;
                        int descColIdx = -1;

                        foreach (var cell in firstRow.CellsUsed())
                        {
                            string headerText = cell.GetString().Trim();
                            if (headerText == machineColName) machineColIdx = cell.Address.ColumnNumber;
                            if (headerText == nameColName) nameColIdx = cell.Address.ColumnNumber;
                            if (headerText == valueColName) valueColIdx = cell.Address.ColumnNumber;
                            if (!string.IsNullOrEmpty(descColName) && headerText == descColName) descColIdx = cell.Address.ColumnNumber;
                        }

                        if (machineColIdx == -1) throw new Exception($"호기명 컬럼 '{machineColName}'을(를) 찾을 수 없습니다.");
                        if (nameColIdx == -1) throw new Exception($"변수명 컬럼 '{nameColName}'을(를) 찾을 수 없습니다.");
                        if (valueColIdx == -1) throw new Exception($"설정값 컬럼 '{valueColName}'을(를) 찾을 수 없습니다.");

                        // 2. 데이터 추출 (두 번째 행부터)
                        var rows = worksheet.RowsUsed().Skip(1);
                        foreach (var row in rows)
                        {
                            var data = new ExcelRowData
                            {
                                MachineName = row.Cell(machineColIdx).GetString().Trim(),
                                VariableName = row.Cell(nameColIdx).GetString().Trim(),
                                Value = row.Cell(valueColIdx).GetString().Trim(),
                                Description = descColIdx != -1 ? row.Cell(descColIdx).GetString().Trim() : "",
                                RowIndex = row.RowNumber()
                            };

                            if (!string.IsNullOrEmpty(data.MachineName) && !string.IsNullOrEmpty(data.VariableName))
                            {
                                resultList.Add(data);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"엑셀 파싱 실패: {ex.Message}");
                }

                return resultList;
            });
        }

        /// <summary>
        /// 추출된 엑셀 데이터와 규칙(시나리오) 목록을 대조하여 무결성을 검증합니다.
        /// </summary>
        public static List<ValidationError> ValidateRules(List<ExcelRowData> excelData, List<RuleProfile> profiles)
        {
            var errors = new List<ValidationError>();

            // 1. 호기명 기준으로 데이터 그룹핑
            var machineDataGroups = excelData.GroupBy(x => x.MachineName).ToDictionary(g => g.Key, g => g.ToList());

            // 2. 각 시나리오(프로필)별로 검사 수행
            foreach (var profile in profiles)
            {
                // 이 프로필에 속한 호기들만 추출
                var mappedMachinesInExcel = profile.MappedMachines.Where(m => machineDataGroups.ContainsKey(m)).ToList();
                if (mappedMachinesInExcel.Count == 0) continue; // 이 규칙에 해당하는 호기가 엑셀에 없으면 패스

                // --- A. 공통값(Common) 검사 ---
                // 규칙에 지정된 공통 변수가 모든 맵핑된 호기에서 똑같은 값을 가지는지 확인
                foreach (var commonVar in profile.CommonVariables)
                {
                    // 기준값 설정 (첫 번째 호기의 값을 기준으로 삼음)
                    string baselineValue = null;
                    string baselineMachine = null;

                    foreach (var machine in mappedMachinesInExcel)
                    {
                        var row = machineDataGroups[machine].FirstOrDefault(x => x.VariableName == commonVar);
                        if (row == null) continue; // 해당 변수가 아예 없으면 일단 넘어감 (또는 에러 처리 가능)

                        if (baselineValue == null)
                        {
                            baselineValue = row.Value;
                            baselineMachine = machine;
                        }
                        else
                        {
                            if (row.Value != baselineValue)
                            {
                                errors.Add(new ValidationError
                                {
                                    MachineName = machine,
                                    RuleName = profile.RuleName,
                                    VariableName = commonVar,
                                    ErrorType = "CommonViolation",
                                    ExpectedValue = $"{baselineValue} ({baselineMachine} 기준)",
                                    ActualValue = row.Value,
                                    Description = $"공통값 규칙 위반: 다른 호기들과 설정값이 다릅니다."
                                });
                            }
                        }
                    }
                }

                // --- B. 고유값(Unique) 검사 ---
                // 규칙에 지정된 고유 변수가 호기들끼리 중복되지 않는지 확인
                foreach (var uniqueVar in profile.UniqueVariables)
                {
                    var valueToMachineMap = new Dictionary<string, string>(); // Value -> MachineName

                    foreach (var machine in mappedMachinesInExcel)
                    {
                        var row = machineDataGroups[machine].FirstOrDefault(x => x.VariableName == uniqueVar);
                        if (row == null || string.IsNullOrWhiteSpace(row.Value)) continue;

                        if (valueToMachineMap.ContainsKey(row.Value))
                        {
                            string conflictedMachine = valueToMachineMap[row.Value];
                            errors.Add(new ValidationError
                            {
                                MachineName = machine,
                                RuleName = profile.RuleName,
                                VariableName = uniqueVar,
                                ErrorType = "UniqueViolation",
                                ExpectedValue = "고유값 (중복 불가)",
                                ActualValue = row.Value,
                                Description = $"고유값 충돌 위반: {conflictedMachine} 호기와 값이 중복됩니다."
                            });
                        }
                        else
                        {
                            valueToMachineMap[row.Value] = machine;
                        }
                    }
                }
            }

            return errors;
        }
    }
}
