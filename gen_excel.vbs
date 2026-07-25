Set objExcel = CreateObject("Excel.Application")
objExcel.Visible = False
Set objWorkbook = objExcel.Workbooks.Add()
Set objWorksheet = objWorkbook.Worksheets(1)
objWorksheet.Name = "DataSheet1"

objWorksheet.Cells(1, 1).Value = "EQP_Name"
objWorksheet.Cells(1, 2).Value = "Param_Name"
objWorksheet.Cells(1, 3).Value = "Current_Value"
objWorksheet.Cells(1, 4).Value = "Description"

' 호기 1 (정상)
objWorksheet.Cells(2, 1).Value = "EQP-A01"
objWorksheet.Cells(2, 2).Value = "Timeout"
objWorksheet.Cells(2, 3).Value = "3000"
objWorksheet.Cells(2, 4).Value = "Common timeout"

objWorksheet.Cells(3, 1).Value = "EQP-A01"
objWorksheet.Cells(3, 2).Value = "IPAddress"
objWorksheet.Cells(3, 3).Value = "192.168.1.10"
objWorksheet.Cells(3, 4).Value = "Unique IP"

' 호기 2 (공통 변수 불일치 에러 테스트용)
objWorksheet.Cells(4, 1).Value = "EQP-A02"
objWorksheet.Cells(4, 2).Value = "Timeout"
objWorksheet.Cells(4, 3).Value = "5000"
objWorksheet.Cells(4, 4).Value = "Common timeout"

objWorksheet.Cells(5, 1).Value = "EQP-A02"
objWorksheet.Cells(5, 2).Value = "IPAddress"
objWorksheet.Cells(5, 3).Value = "192.168.1.11"
objWorksheet.Cells(5, 4).Value = "Unique IP"

' 호기 3 (고유 변수 중복 에러 테스트용)
objWorksheet.Cells(6, 1).Value = "EQP-A03"
objWorksheet.Cells(6, 2).Value = "Timeout"
objWorksheet.Cells(6, 3).Value = "3000"
objWorksheet.Cells(6, 4).Value = "Common timeout"

objWorksheet.Cells(7, 1).Value = "EQP-A03"
objWorksheet.Cells(7, 2).Value = "IPAddress"
objWorksheet.Cells(7, 3).Value = "192.168.1.10"
objWorksheet.Cells(7, 4).Value = "Unique IP"

objWorkbook.SaveAs CreateObject("Scripting.FileSystemObject").GetAbsolutePathName(".") & "\Test_Validation_Data.xlsx", 51
objWorkbook.Close False
objExcel.Quit
