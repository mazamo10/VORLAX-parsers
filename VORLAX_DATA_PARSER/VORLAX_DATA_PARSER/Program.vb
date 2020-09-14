
Imports System.IO


Module Program
    Sub Main()
        Read_CSV()

    End Sub

    Sub Read_CSV()
        Dim inputCSV As String

        Console.WriteLine("Input CSV File Path:")
        inputCSV = Console.ReadLine()

        File_Check(inputCSV)
    End Sub

    Sub File_Check(FName As String)
        Dim extension As String

        If String.IsNullOrWhiteSpace(FName) Then
            Throw New ArgumentException($"'{NameOf(FName)}' cannot be null or whitespace", NameOf(FName))
        End If

        Console.WriteLine(" User specified file path: " + FName)
        If System.IO.File.Exists(FName) Then
            extension = LCase(Path.GetExtension(FName))
            If extension <> ".csv" Then
                Console.WriteLine(" File must contian extension *.csv\n")
                Console.WriteLine()
                Read_CSV()
                Return
            End If

            Parser(FName)
        Else
            Console.WriteLine("Error: File does not exist as specified")
            Read_CSV()
            Return
        End If
    End Sub

    Sub Parser(FName)
        Dim FileLocation As String
        FileLocation = Path.GetDirectoryName(FName)

        FileCleaner(FName, FileLocation)

        Csv2txt(FName, FileLocation)

    End Sub

    Sub Csv2txt(Fname, FileLocation)
        Dim fileName As String
        fileName = FileLocation + "\temp.tmp"
        Dim lines As String() = File.ReadAllLines(fileName)

        Dim Columns As UInt16 = 18
        Dim Rows As UInt64 = lines.Length

        Dim VariableNames As String = ""
        Dim DataValues(Rows - 2, Columns) As Double
        Dim VarLine As Boolean = True

        Using MyReader As New FileIO.TextFieldParser(fileName)
            MyReader.TextFieldType = FileIO.FieldType.Delimited
            MyReader.SetDelimiters(",")

            Dim currentRow As String()
            Dim j As UInt32 = 0
            While Not MyReader.EndOfData
                Try
                    currentRow = MyReader.ReadFields()
                    Dim currentField As String

                    If VarLine Then
                        Dim i As UInt32 = 0

                        For Each currentField In currentRow
                            If i < Columns - 1 Then
                                VariableNames = VariableNames & currentField & ","
                                i += 1
                            ElseIf i < Columns Then
                                VariableNames = VariableNames & currentField
                            Else
                                Console.WriteLine("Error: Too many variable names provided")
                            End If
                        Next
                        VarLine = False
                    Else
                        Dim i As UInt32 = 0
                        For Each currentField In currentRow
                            If i < Columns Then
                                DataValues(j, i) = currentField
                                i += 1
                            Else
                                Console.WriteLine("Error: Too many variable values provided")
                            End If
                        Next
                        j += 1
                    End If

                Catch ex As FileIO.MalformedLineException
                    Console.WriteLine("Line " & ex.Message &
                    "is not valid and will be skipped.")
                End Try
            End While
        End Using


        Dim Mach_Old As Double = 0.0

        'Create first output file
        Dim fileTag As String = "_M" & DataValues(0, 0) & ".txt"
        Dim file_const_M As String = Replace(Fname, ".csv", fileTag)


        For j As UInt32 = 0 To Rows - 2

            Dim Mach As Double = DataValues(j, 0)

            If j = 0 Then
                Mach_Old = Mach
                Dim rowText As String = ""

                For i As UInt32 = 0 To Columns - 2
                    rowText = rowText & DataValues(j, i) & ","
                Next
                rowText = rowText & DataValues(j, Columns - 1)

                FileWrite(file_const_M, VariableNames, rowText, True)
            Else
                If (Mach <> Mach_Old) Then
                    Dim rowText As String = ""

                    'Create output file
                    For i As UInt32 = 0 To Columns - 2
                        rowText = rowText & DataValues(j, i) & ","
                    Next
                    rowText = rowText & DataValues(j, Columns - 1)

                    Mach_Old = Mach

                    'Create output file
                    fileTag = "_M" & Mach & ".txt"
                    file_const_M = Replace(Fname, ".csv", fileTag)

                    FileWrite(file_const_M, VariableNames, rowText, True)
                Else
                    Dim rowText As String = ""

                    'Create output file
                    For i As UInt32 = 0 To Columns - 2
                        rowText = rowText & DataValues(j, i) & ","
                    Next
                    rowText = rowText & DataValues(j, Columns - 1)

                    FileWrite(file_const_M, "", rowText, False)
                End If
            End If
        Next
        File.Delete(fileName)
    End Sub

    'Removes first line and all asterisks, spaces, and tabs from csv file
    Sub FileCleaner(Fname, FileLocation)
        Dim lineText As String
        Dim fileName As String
        Dim cleanStr As String

        Dim i As UInt32 = 0

        fileName = FileLocation + "\temp.tmp"

        Dim objWriter As New StreamWriter(fileName)

        Using objReader = File.OpenText(Fname)

            Do While objReader.Peek() <> -1

                lineText = objReader.ReadLine()

                cleanStr = Replace(lineText, "\t", "")
                cleanStr = Replace(cleanStr, " ", "")
                cleanStr = Replace(cleanStr, "*", "")
                cleanStr = Replace(cleanStr, ",,", ",")

                If i > 0 Then
                    objWriter.WriteLine(cleanStr)
                End If

                i += 1
            Loop
        End Using
        objWriter.Close()

    End Sub
    Sub FileWrite(name As String, varLine As String, rowText As String, firstLine As Boolean)
        Using SW As StreamWriter = New StreamWriter(name, True)
            If firstLine Then
                SW.WriteLine(varLine)
            End If

            SW.WriteLine(rowText)
        End Using
    End Sub
End Module
