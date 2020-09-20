
Imports System.IO


Module Program
    Sub Main()
        Read_CSV()

    End Sub

    Sub Read_CSV()
        Dim inputCSV As String

        Console.WriteLine("Input File Path to Log File:")
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
            If extension <> ".log" Then
                Console.WriteLine(" File must contian extension *.log\n")
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

        LogRead(FName, FileLocation)
    End Sub

    Sub LogRead(Fname As String, FileLocation As String)
        Dim fileName As String = ""
        Dim TextLine As String
        Dim TextLine_ns As String
        Dim TextNoNumerial As String

        Dim flag As Boolean = False
        Dim dataZone As Integer = 0

        Dim Mach As String = ""
        Dim Alpha As String = ""
        Dim Psi As String = ""

        Using SR As New StreamReader(Fname)
            While SR.Peek <> -1

                TextLine = SR.ReadLine()

                TextLine_ns = Replace(TextLine, " ", "")

                If StrComp(TextLine_ns, "VORL") = 0 And Not flag Then
                    flag = True
                End If

                If flag And StrComp(TextLine_ns, "VORL") <> 0 Then
                    TextNoNumerial = ReplaceNumeral(TextLine_ns)

                    If StrComp(TextNoNumerial, "MACH=ALPHA=DGPSI=DG") = 0 Then

                        Mach = ExtractVal(TextLine_ns, "MACH=", "ALPHA=")
                        Alpha = ExtractVal(TextLine_ns, "ALPHA=", "DGPSI=")
                        Psi = Mid(TextLine_ns, InStr(TextLine_ns, "DGPSI=") + 6, Len(TextLine_ns) - InStr(TextLine_ns, "DGPSI=") - 7)

                        dataZone += 1
                    End If
                    flag = False
                End If

                If dataZone > 0 And dataZone < 4 Then
                    dataZone += 1
                ElseIf dataZone = 4 Then
                    Dim pan As String = Replace(TextLine_ns, "PANELNO.", "")
                    fileName = CreateFileName(FileLocation, Mach, Alpha, Psi, pan)
                    Console.WriteLine(fileName)

                    dataZone = 5
                ElseIf dataZone = 5 Then
                    If StrComp(TextLine_ns, "ENDOFCASE") = 0 Or InStr(TextLine_ns, "PANELNO.") <> 0 Then
                        dataZone = 0
                    ElseIf InStr(TextLine_ns, "PANELNO.") <> 0 Then
                        pan = Replace(TextLine_ns, "PANELNO.", "")
                        fileName = CreateFileName(FileLocation, Mach, Alpha, Psi, pan)
                        Console.WriteLine(fileName)
                    Else
                        FileWrite(fileName, TextLine)
                    End If
                End If
            End While
        End Using
    End Sub

    Sub FileWrite(name As String, Line As String)
        Using SW As StreamWriter = New StreamWriter(name, True)
            SW.WriteLine(Line)
        End Using
    End Sub

    Function ReplaceNumeral(string1 As String)
        Dim string2 As String

        string2 = Replace(string1, "0", "")
        string2 = Replace(string2, "1", "")
        string2 = Replace(string2, "2", "")
        string2 = Replace(string2, "3", "")
        string2 = Replace(string2, "4", "")
        string2 = Replace(string2, "5", "")
        string2 = Replace(string2, "6", "")
        string2 = Replace(string2, "7", "")
        string2 = Replace(string2, "8", "")
        string2 = Replace(string2, "9", "")
        string2 = Replace(string2, ".", "")

        Return string2
    End Function

    Function CreateFileName(fileLocation As String, Mach As String,
                            Alpha As String, Psi As String, pan As String)
        Dim fileName As String

        fileName = $"{fileLocation}\VorlaxLog_M{Mach}_A{Alpha}_Psi{Psi}_Panel{pan}.txt"
        Return fileName
    End Function

    Function ExtractVal(FullString As String, LBoundString As String, UBoundString As String)
        Dim BoundedString As String

        Dim Lower As Integer = InStr(FullString, LBoundString) + Len(LBoundString)
        Dim Upper As Integer = InStr(FullString, UBoundString)

        BoundedString = Mid(FullString, Lower, Upper - Lower)
        Return BoundedString
    End Function
End Module
