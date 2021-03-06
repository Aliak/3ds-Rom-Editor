﻿Imports System.IO

Public Class Header_editor
    'Declare global variables
    Dim romFile, filename2, headerfile, savefile, rstring, writeS1, wstring2, wstring4 As String
    Const wstring3 As String = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" 'Necessary to work
    Dim genCheck As Boolean
    Private Sub btnUid_Click(sender As Object, e As EventArgs) Handles btnUid.Click
        Writefile()
    End Sub
    Private Sub Writefile() 'Prepare data to write to the 3ds rom file
        combineText()

        Dim b((writeS1.Length \ 2) - 1) As Byte
        'convert to bytes  
        Dim idx As Integer = 0
        For x As Integer = 0 To writeS1.Length - 1 Step 2
            b(idx) = Convert.ToByte(writeS1.Substring(x, 2), 16)
            idx += 1
        Next
        '1200 is the ofset in the rom we are writing to
        WriteRom(&H1200, romFile, b)

        Dim b2((wstring3.Length \ 2) - 1) As Byte
        'convert to bytes  
        Dim idx2 As Integer = 0
        For x As Integer = 0 To wstring3.Length - 1 Step 2
            b2(idx2) = Convert.ToByte(wstring3.Substring(x, 2), 16)
            idx2 += 1
        Next

        WriteRom(&H1210, romFile, b2)
        WriteRom(&H1220, romFile, b2)
        WriteRom(&H1230, romFile, b2)

        Dim b3((wstring2.Length \ 2) - 1) As Byte
        'convert to bytes  
        Dim idx3 As Integer = 0
        For x2 As Integer = 0 To wstring2.Length - 1 Step 2
            b3(idx3) = Convert.ToByte(wstring2.Substring(x2, 2), 16)
            idx3 += 1
        Next

        WriteRom(&H1240, romFile, b3)

        lblcom.Visible = True
    End Sub
    Private Sub combineText() 'Combine text feilds for writing as bianary data 
        writeS1 = txtUid1.Text + txtUid2.Text + txtUid3.Text + txtUid4.Text + txtUid5.Text + txtUid6.Text + txtUid7.Text + txtUid8.Text + txtUid9.Text + txtUid10.Text + txtUid11.Text + txtUid12.Text + txtUid13.Text + txtUid14.Text + txtUid15.Text + txtUid16.Text
        wstring2 = txtCard1.Text + txtCard2.Text + txtCard3.Text + txtCard4.Text + "00000000" + wstring4
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim dialog As New OpenFileDialog() 'select file to load
        dialog.Filter = "3ds Rom|*.3ds; *.3dz"
        If DialogResult.OK = dialog.ShowDialog Then
            romFile = dialog.FileName
            txtRom.Text = romFile
            lblcom.Visible = False
            lblcom2.Visible = False
            GetRomInfo()
            groupheader.Enabled = True
            GroupOp.Enabled = True
            GenChipID()
            ReadRom()
        End If
    End Sub
    Private Sub GenChipID() 'Generate and fix chipID
        If txtCap.Text = "128 MB" Then
            txtCard2.Text = "7F"
        ElseIf txtCap.Text = "256 MB" Then
            txtCard2.Text = "FF"
        ElseIf txtCap.Text = "512 MB" Then
            txtCard2.Text = "FE"
        ElseIf txtCap.Text = "1 GB" Then
            txtCard2.Text = "FA"
        ElseIf txtCap.Text = "2 GB" Then
            txtCard2.Text = "F8"
        ElseIf txtCap.Text = "4 GB" Then
            txtCard2.Text = "F0"
        End If

        If txtCardT.Text = "Card 1" Then
            txtCard4.Text = "90"
        ElseIf txtCardT.Text = "Card 2" Then
            txtCard4.Text = "98"
        End If
        genCheck = True
    End Sub
    Private Sub GetRomInfo()
        Dim reader As New IO.BinaryReader(New IO.FileStream(romFile, IO.FileMode.Open, IO.FileAccess.Read)) 'open file
        reader.BaseStream.Position = &H1150 'Read from position "1150" in the file
        Dim c As String = reader.ReadChars(10)
        txtSer.Text = c
        reader.Dispose()

        Using readfile As New IO.FileStream(romFile, IO.FileMode.Open)
            readfile.Seek(&H108, SeekOrigin.Current)
            Dim value As Integer = readfile.ReadByte()
            Dim counter As Integer = 0
            Dim show As String = ""
            Dim revString As String
            Do Until counter = 8
                rstring = value.ToString("X2")
                value = readfile.ReadByte()
                revString = StrReverse(rstring)
                show = show + revString
                counter += 1
            Loop
            txtMainTitle.Text = StrReverse(show)
            readfile.Dispose()
        End Using
        'Finds the type of the Rom from the data at ofset "18D"
        Using readfile As New IO.FileStream(romFile, IO.FileMode.Open)
            readfile.Seek(&H18D, SeekOrigin.Current)
            Dim value As Integer = readfile.ReadByte()
            Dim counter As Integer = 0
            Dim show As String = ""

            Do Until counter = 1
                rstring = value.ToString("X2")
                value = readfile.ReadByte()

                show = rstring
                counter += 1
            Loop
            If show = &H2 Then
                txtCardT.Text = "Card 2"
            ElseIf show = &H1 Then
                txtCardT.Text = "Card 1"
            End If
            readfile.Dispose()
        End Using
        'Finds the size of the Rom from the data at ofset "104"
        Using readfile As New IO.FileStream(romFile, IO.FileMode.Open)
            readfile.Seek(&H104, SeekOrigin.Current)
            Dim value As Integer = readfile.ReadByte()
            Dim counter As Integer = 0
            Dim show As String = ""

            Do Until counter = 4
                rstring = value.ToString("X2")
                value = readfile.ReadByte()

                show = show + rstring
                counter += 1
            Loop

            If show = "00000400" Then
                txtCap.Text = "128 MB"
            ElseIf show = "00000800" Then
                txtCap.Text = "256 MB"
            ElseIf show = "00001000" Then
                txtCap.Text = "512 MB"
            ElseIf show = "00002000" Then
                txtCap.Text = "1 GB"
            ElseIf show = "00004000" Then
                txtCap.Text = "2 GB"
            ElseIf show = "00008000" Then
                txtCap.Text = "4 GB"
            End If
            readfile.Dispose()
        End Using
        'Finds the manufaturer of the flash chip from the data at ofset "1240"
        Using readfile As New IO.FileStream(romFile, IO.FileMode.Open)
            readfile.Seek(&H1240, SeekOrigin.Current)
            Dim value As Integer = readfile.ReadByte()
            Dim counter As Integer = 0
            Dim id As String = ""

            Do Until counter = 1
                rstring = value.ToString("X2")
                value = readfile.ReadByte()

                id = id + rstring
                counter += 1
            Loop
            txtCard1.Text = id
            readfile.Dispose()
        End Using

        If txtCard1.Text = "C2" Then
            comMan.Text = "Macronix"
        ElseIf txtCard1.Text = "45" Then
            comMan.Text = "SanDisk"
        ElseIf txtCard1.Text = "AE" Then
            comMan.Text = "OKI Semiconductor"
        End If
    End Sub
    Public Sub WriteRom(ByVal address As Long, ByVal fileName As String, ByVal data() As Byte) 'Writes the data to 3ds rom file
        Dim writer As New IO.BinaryWriter(New IO.FileStream(fileName, IO.FileMode.OpenOrCreate, IO.FileAccess.Write))
        writer.BaseStream.Position = address
        writer.Write(data)
        writer.Dispose()
    End Sub
    Public Sub ReadBin() ' Open .bin file to import data
        Using readfile As New IO.FileStream(headerfile, IO.FileMode.Open)
            readfile.Seek(&H0, SeekOrigin.Current)
            Dim value As Integer = readfile.ReadByte()
            Dim counter As Integer = 0

            Do Until counter = 16
                rstring = value.ToString("X2")
                value = readfile.ReadByte()
                Select Case counter
                    Case 0
                        txtUid1.Text = rstring
                    Case 1
                        txtUid2.Text = rstring
                    Case 2
                        txtUid3.Text = rstring
                    Case 3
                        txtUid4.Text = rstring
                    Case 4
                        txtUid5.Text = rstring
                    Case 5
                        txtUid6.Text = rstring
                    Case 6
                        txtUid7.Text = rstring
                    Case 7
                        txtUid8.Text = rstring
                    Case 8
                        txtUid9.Text = rstring
                    Case 9
                        txtUid10.Text = rstring
                    Case 10
                        txtUid11.Text = rstring
                    Case 11
                        txtUid12.Text = rstring
                    Case 12
                        txtUid13.Text = rstring
                    Case 13
                        txtUid14.Text = rstring
                    Case 14
                        txtUid15.Text = rstring
                    Case 15
                        txtUid16.Text = rstring
                End Select
                counter += 1
            Loop
            readfile.Dispose()
        End Using

        If genCheck = False Then
            Using readfile2 As New IO.FileStream(headerfile, IO.FileMode.Open)

                readfile2.Seek(&H40, SeekOrigin.Current)
                Dim value As Integer = readfile2.ReadByte()
                Dim counter As Integer = 0
                rstring = 0

                Do Until counter = 4
                    rstring = value.ToString("X2")
                    value = readfile2.ReadByte()
                    Select Case counter
                        Case 0
                            txtCard1.Text = rstring
                        Case 1
                            txtCard2.Text = rstring
                        Case 2
                            txtCard3.Text = rstring
                        Case 3
                            txtCard4.Text = rstring
                    End Select
                    counter += 1
                Loop
                readfile2.Dispose()
            End Using
        End If
    End Sub
    Public Sub ReadRom()
        Using readfile As New IO.FileStream(romFile, IO.FileMode.Open)
            readfile.Seek(&H1200, SeekOrigin.Current)
            Dim value As Integer = readfile.ReadByte()
            Dim counter As Integer = 0
            'Check if there is a cardID in the rom
            Do Until counter = 16
                rstring = value.ToString("X2")
                value = readfile.ReadByte()
                'If ther is no CardID, load all 0
                If rstring = "FF" Then
                    MsgBox("No ID in Rom")
                    txtUid1.Text = "00"
                    txtUid2.Text = "00"
                    txtUid3.Text = "00"
                    txtUid4.Text = "00"
                    txtUid5.Text = "00"
                    txtUid6.Text = "00"
                    txtUid7.Text = "00"
                    txtUid8.Text = "00"
                    txtUid9.Text = "00"
                    txtUid10.Text = "00"
                    txtUid11.Text = "00"
                    txtUid12.Text = "00"
                    txtUid13.Text = "00"
                    txtUid14.Text = "00"
                    txtUid15.Text = "00"
                    txtUid16.Text = "00"
                    counter = 16
                Else
                    'If ther is a CardID, load it
                    Select Case counter
                        Case 0
                            txtUid1.Text = rstring
                        Case 1
                            txtUid2.Text = rstring
                        Case 2
                            txtUid3.Text = rstring
                        Case 3
                            txtUid4.Text = rstring
                        Case 4
                            txtUid5.Text = rstring
                        Case 5
                            txtUid6.Text = rstring
                        Case 6
                            txtUid7.Text = rstring
                        Case 7
                            txtUid8.Text = rstring
                        Case 8
                            txtUid9.Text = rstring
                        Case 9
                            txtUid10.Text = rstring
                        Case 10
                            txtUid11.Text = rstring
                        Case 11
                            txtUid12.Text = rstring
                        Case 12
                            txtUid13.Text = rstring
                        Case 13
                            txtUid14.Text = rstring
                        Case 14
                            txtUid15.Text = rstring
                        Case 15
                            txtUid16.Text = rstring
                    End Select
                    counter += 1
                End If
            Loop
            readfile.Dispose()
        End Using
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        'Select .bin file to open
        Dim dialog As New OpenFileDialog()
        dialog.Filter = "Rom Header|*.bin"
        If DialogResult.OK = dialog.ShowDialog Then
            headerfile = dialog.FileName
            ReadBin()
        End If
    End Sub
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        combineText()
        Dim dialog As New SaveFileDialog
        dialog.Filter = "Rom Header|*.bin"
        If DialogResult.OK = dialog.ShowDialog Then
            filename2 = dialog.FileName

            Dim b((writeS1.Length \ 2) - 1) As Byte
            'convert to bytes  
            Dim idx As Integer = 0
            For x As Integer = 0 To writeS1.Length - 1 Step 2
                b(idx) = Convert.ToByte(writeS1.Substring(x, 2), 16)
                idx += 1
            Next

            WriteRom(&H0, filename2, b)


            Dim b2((wstring3.Length \ 2) - 1) As Byte
            'convert to bytes  
            Dim idx2 As Integer = 0
            For x As Integer = 0 To wstring3.Length - 1 Step 2
                b2(idx2) = Convert.ToByte(wstring3.Substring(x, 2), 16)
                idx2 += 1
            Next

            WriteRom(&H10, filename2, b2)
            WriteRom(&H20, filename2, b2)
            WriteRom(&H30, filename2, b2)

            Dim b3((wstring2.Length \ 2) - 1) As Byte
            'convert to bytes  
            Dim idx3 As Integer = 0
            For x2 As Integer = 0 To wstring2.Length - 1 Step 2
                b3(idx3) = Convert.ToByte(wstring2.Substring(x2, 2), 16)
                idx3 += 1
            Next

            WriteRom(&H40, filename2, b3)

            lblcom2.Visible = True
        Else
            Me.Refresh()
        End If


    End Sub
    Private Sub comMan_SelectedIndexChanged(sender As Object, e As EventArgs) Handles comMan.SelectedIndexChanged
        If comMan.Text = "Macronix" Then
            txtCard1.Text = "C2"
        ElseIf comMan.Text = "SanDisk" Then
            txtCard1.Text = "45"
        ElseIf comMan.Text = "OKI Semiconductor" Then
            txtCard1.Text = "AE"
        End If
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        comMan.SelectedIndex = "0"
    End Sub
End Class