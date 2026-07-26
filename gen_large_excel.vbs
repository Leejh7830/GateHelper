Set objExcel = CreateObject("Excel.Application")
objExcel.Visible = False
objExcel.DisplayAlerts = False
Set objWorkbook = objExcel.Workbooks.Add()
Set objWorksheet = objWorkbook.Worksheets(1)
objWorksheet.Name = "MassData"

Const numEQP = 80
Const numVars = 100
Dim totalRows
totalRows = numEQP * numVars

' Create an array to hold the data (0 to totalRows means totalRows + 1 elements)
Dim data()
ReDim data(totalRows, 3)

' Headers
data(0, 0) = "EQP_Name"
data(0, 1) = "Param_Name"
data(0, 2) = "Current_Value"
data(0, 3) = "Description"

Dim r
r = 1
For e = 1 To numEQP
    Dim eqpName
    eqpName = "EQP-" & Right("000" & e, 3)
    
    Dim v
    For v = 1 To numVars
        Dim varName, varValue
        varName = "Var_" & Right("000" & v, 3)
        varValue = "Value_" & e & "_" & v
        
        ' 1. Unique 위반 테스트용 변수 (IPAddress)
        If v = 1 Then
            varName = "IPAddress"
            If e = 80 Then
                varValue = "192.168.100.1" ' EQP-001과 중복 발생
            Else
                varValue = "192.168.100." & e
            End If
        End If
        
        ' 2. Common 위반 테스트용 변수 (Timeout)
        If v = 2 Then
            varName = "Timeout"
            varValue = "3000"
            If e = 45 Then
                varValue = "5000" ' EQP-045만 값이 달라서 Common 에러 발생
            End If
        End If

        ' 3. Normal 패턴 매칭 (에러 없음)
        If v = 3 Then
            varName = "Port"
            varValue = "8080"
        End If

        data(r, 0) = eqpName
        data(r, 1) = varName
        data(r, 2) = varValue
        data(r, 3) = "Auto generated description for " & varName

        r = r + 1
    Next
Next

' Write to range in one go for fast performance
Set objRange = objWorksheet.Range("A1").Resize(totalRows + 1, 4)
objRange.Value = data

Dim filePath
filePath = CreateObject("Scripting.FileSystemObject").GetAbsolutePathName(".") & "\Test_MassData_8000.xlsx"
objWorkbook.SaveAs filePath, 51
objWorkbook.Close False
objExcel.Quit

WScript.Echo "Created " & filePath & " with " & totalRows & " rows."
