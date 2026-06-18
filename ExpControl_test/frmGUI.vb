Imports System.Text.RegularExpressions

Public Class frmGUI
    Public WithEvents dgv As DataGridView
    Public dt As DataTable
    Public WithEvents dgvloop As DataGridView
    Public dtloop As DataTable

    ' Extra buttons added programmatically so the Designer file does not need to be edited.
    Public WithEvents RunFLoopButton As Button
    Public WithEvents LogFeedbackButton As Button
    Private runFLoopReadyBackColor As Color
    Private runFLoopReadyForeColor As Color
    Private runFLoopReadyUseVisualStyleBackColor As Boolean

    Public inter As clsInteractive
    Dim gui_autofill As New frmGUI_autofill(Me)
    Public runningState As Integer
    Public currentExpNo As Integer = 0
    Public stopping As Integer = 0
    Public stopped As Integer = 1
    Public continuous As Integer = 2
    Public batch As Integer = 3
    Public batchstopping As Integer = 4

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        dgv = New DataGridView()
        dgv.Dock = DockStyle.Fill
        ExpSeqPanel.Controls.Add(dgv)
        dgv.AllowUserToAddRows = True
        dgv.AllowUserToDeleteRows = True

        dgvloop = New DataGridView()
        dgvloop.Dock = DockStyle.Fill
        LoopingPanel.Controls.Add(dgvloop)
        dgvloop.AllowUserToAddRows = False
        dgvloop.AllowUserToDeleteRows = False

        inter = New clsInteractive()

        dt = New DataTable()
        Dim dataset As DataSet
        dataset = New DataSet()
        dataset.Tables.Add(dt)
        dgv.DataSource = dt
        dgv.AutoGenerateColumns = True
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells

        dtloop = New DataTable()
        Dim datasetloop As DataSet
        datasetloop = New DataSet()
        datasetloop.Tables.Add(dtloop)
        dgvloop.DataSource = dtloop
        dgvloop.AutoGenerateColumns = True
        dgvloop.AutoResizeColumns()
        dgvloop.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells

        noToAddField.Text = "1"
        startSeqField.Text = "1"
        noPointsField.Text = "1"
        stopSeqField.Text = "1"
        linearRadio.Select()

        runningState = stopped

        AddRunFLoopAndLogFeedbackButtons()
    End Sub

    Private Sub AddRunFLoopAndLogFeedbackButtons()
        ' Put "Run f-loop" where "Load program" used to be, then move "Load program" to the right.
        If RunFLoopButton Is Nothing Then
            Dim loadProgramOriginalLocation As Point
            loadProgramOriginalLocation = ChooseProgramButton.Location

            RunFLoopButton = New Button()
            RunFLoopButton.Name = "RunFLoopButton"
            RunFLoopButton.Text = "Run f-loop"
            RunFLoopButton.Size = ChooseProgramButton.Size
            RunFLoopButton.Location = loadProgramOriginalLocation
            RunFLoopButton.Anchor = ChooseProgramButton.Anchor

            ' Match the Run Batch button style, but use a red-square F icon for f-loop.
            RunFLoopButton.Image = CreateRunFLoopIcon()
            RunFLoopButton.ImageAlign = BatchButton.ImageAlign
            RunFLoopButton.TextAlign = BatchButton.TextAlign
            RunFLoopButton.TextImageRelation = BatchButton.TextImageRelation
            RunFLoopButton.FlatStyle = BatchButton.FlatStyle
            RunFLoopButton.Font = BatchButton.Font
            RunFLoopButton.BackColor = BatchButton.BackColor
            RunFLoopButton.ForeColor = BatchButton.ForeColor
            RunFLoopButton.UseVisualStyleBackColor = BatchButton.UseVisualStyleBackColor

            runFLoopReadyBackColor = RunFLoopButton.BackColor
            runFLoopReadyForeColor = RunFLoopButton.ForeColor
            runFLoopReadyUseVisualStyleBackColor = RunFLoopButton.UseVisualStyleBackColor
            SetRunFLoopProgramLoaded(False)

            ChooseProgramButton.Location = New Point(RunFLoopButton.Right + 6, RunFLoopButton.Top)

            ChooseProgramButton.Parent.Controls.Add(RunFLoopButton)
            RunFLoopButton.BringToFront()
            ChooseProgramButton.BringToFront()
        End If

        ' Keep the original "Log Experiment" button and add "Log feedback" immediately
        ' to the LEFT of the existing "Load log file" button. Do not move the
        ' Load-log button; moving it to the right can push it off the visible form.
        If LogFeedbackButton Is Nothing Then
            Dim loadLogOriginalLocation As Point
            loadLogOriginalLocation = loadLog_button.Location

            LogFeedbackButton = New Button()
            LogFeedbackButton.Name = "LogFeedbackButton"
            LogFeedbackButton.Text = "Log feedback"
            LogFeedbackButton.Size = loadLog_button.Size
            LogFeedbackButton.Location = New Point(loadLogOriginalLocation.X - LogFeedbackButton.Width - 6, loadLogOriginalLocation.Y)
            LogFeedbackButton.Anchor = loadLog_button.Anchor
            LogFeedbackButton.Visible = True
            LogFeedbackButton.Enabled = True

            loadLog_button.Visible = True
            loadLog_button.Enabled = True
            loadLog_button.Location = loadLogOriginalLocation

            loadLog_button.Parent.Controls.Add(LogFeedbackButton)
            LogFeedbackButton.BringToFront()
            loadLog_button.BringToFront()

            logExp_Button.Visible = True
            logExp_Button.Enabled = True
            logExp_Button.BringToFront()
        End If
    End Sub

    Public Sub SetRunFLoopProgramLoaded(ByVal programLoaded As Boolean)
        If RunFLoopButton Is Nothing Then
            Return
        End If

        If programLoaded Then
            RunFLoopButton.Enabled = True
            RunFLoopButton.UseVisualStyleBackColor = runFLoopReadyUseVisualStyleBackColor
            RunFLoopButton.BackColor = runFLoopReadyBackColor
            RunFLoopButton.ForeColor = runFLoopReadyForeColor
        Else
            RunFLoopButton.Enabled = False
            RunFLoopButton.UseVisualStyleBackColor = False
            RunFLoopButton.BackColor = Color.FromArgb(64, 64, 64)
            RunFLoopButton.ForeColor = Color.LightGray
        End If
    End Sub

    Public Sub SetRunFLoopRunning(ByVal running As Boolean)
        If RunFLoopButton Is Nothing Then
            Return
        End If

        If running Then
            RunFLoopButton.Enabled = False
            RunFLoopButton.UseVisualStyleBackColor = False
            RunFLoopButton.BackColor = Color.FromArgb(0, 90, 180)
            RunFLoopButton.ForeColor = Color.White
        Else
            SetRunFLoopProgramLoaded(True)
        End If
    End Sub

    Private Function CreateRunFLoopIcon() As Image
        ' Create a small red square icon with a white "F", in the same role as the Batch button icon.
        Dim bmp As New Bitmap(16, 16)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)

            Using redBrush As New SolidBrush(Color.Red)
                g.FillRectangle(redBrush, 0, 0, 15, 15)
            End Using

            Using borderPen As New Pen(Color.DarkRed)
                g.DrawRectangle(borderPen, 0, 0, 15, 15)
            End Using

            Using letterFont As New Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel)
                Using whiteBrush As New SolidBrush(Color.White)
                    Dim format As New StringFormat()
                    format.Alignment = StringAlignment.Center
                    format.LineAlignment = StringAlignment.Center
                    g.DrawString("F", letterFont, whiteBrush, New RectangleF(0, 0, 16, 16), format)
                End Using
            End Using
        End Using
        Return bmp
    End Function

    Public Sub buildDataTables()

        'reset dt and dtloop
        dt.Columns.Clear()
        dtloop.Columns.Clear()
        dt.Rows.Clear()
        dtloop.Rows.Clear()

        'generate new dt and dtloop
        Dim arrList As ArrayList
        arrList = modUtilities.GetExpVariables()

        Dim var As Object
        For Each var In arrList
            Dim column As DataColumn
            column = New DataColumn()
            column.DataType = System.Type.GetType("System.Double")
            column.ColumnName = var.ToString()
            dt.Columns.Add(column)
        Next

        For Each var In arrList
            Dim column As DataColumn
            column = New DataColumn()
            column.DataType = System.Type.GetType("System.Double")
            column.ColumnName = var.ToString()
            dtloop.Columns.Add(column)
        Next

        appendNRows(1, dtloop)
        appendNRows(1, dt)

    End Sub

    Private Sub appendNRows(ByVal noRows As Integer, ByVal dtable As DataTable)
        Dim i As Integer
        For i = 1 To noRows Step 1
            Dim row As DataRow
            row = dtable.NewRow()
            Dim arrList As ArrayList
            arrList = modUtilities.GetExpVariablesDefVals()
            Dim var As Object
            Dim j As Integer
            j = 0
            For Each var In arrList
                row(j) = var
                j = j + 1
            Next
            dtable.Rows.Add(row)
        Next i
    End Sub

    Private Sub AddExpsButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddExpsButton.Click
        Dim k As Integer
        k = Integer.Parse(noToAddField.Text)
        appendNRows(k, dt)
    End Sub

    Private Sub writeSeqButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles writeSeqButton.Click
        Dim p As Point
        p = dgv.CurrentCellAddress
        Dim noExtraRows, currentNoRows, noPoints As Integer
        currentNoRows = dt.Rows.Count
        noPoints = Integer.Parse(noPointsField.Text)
        noExtraRows = p.Y + noPoints - currentNoRows
        If (noExtraRows > 0) Then
            appendNRows(noExtraRows, dt)
        End If
        Dim arr As ArrayList
        arr = New ArrayList()
        Dim i As Integer
        Dim startSeq, stopSeq, incSeq As Double
        startSeq = Double.Parse(startSeqField.Text)
        stopSeq = Double.Parse(stopSeqField.Text)
        If linearRadio.Checked() Then
            incSeq = (stopSeq - startSeq) / (noPoints - 1)
            For i = 0 To noPoints - 1
                arr.Add(startSeq + i * incSeq)
            Next
        Else
            incSeq = (Math.Log10(stopSeq) - Math.Log10(startSeq)) / (noPoints - 1)
            For i = 0 To noPoints - 1
                arr.Add(Math.Pow(10, (Math.Log10(startSeq) + i * incSeq)))
            Next
        End If
        For i = 0 To noPoints - 1
            dt.Rows(p.Y + i)(p.X) = arr(i)
        Next
    End Sub


    Private Sub randomizeButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles randomizeButton.Click
        Dim dt2 As DataTable
        dt2 = New DataTable()
        dt2 = dt.Clone()
        Dim noRows, i, rand As Integer
        Dim r As New Random(System.DateTime.Now.Millisecond)
        noRows = dt.Rows.Count
        Dim row As DataRow
        For i = 0 To noRows - 1
            rand = r.Next(0, dt.Rows.Count)
            row = dt.Rows(rand)
            dt2.ImportRow(row)
            dt.Rows.RemoveAt(rand)
        Next
        For i = 0 To noRows - 1
            row = dt2.Rows(i)
            dt.ImportRow(row)
        Next
    End Sub

    Private Sub LoopCellEdited(ByVal sender As Object, ByVal e As DataGridViewCellCancelEventArgs) Handles dgvloop.CellBeginEdit
        dgvloop.Rows(0).DefaultCellStyle.BackColor = Color.Yellow
    End Sub

    Private Sub ChooseProgramButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChooseProgramButton.Click
        With OpenFileDialog
            If .ShowDialog = System.Windows.Forms.DialogResult.OK Then
                prepareToRun(.FileName)
            End If
        End With
    End Sub

    Private Sub interactiveCmdText_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles interactiveCmdText.KeyPress
        If e.KeyChar = ControlChars.Cr Then
            inter.parse(interactiveCmdText.Text)
            interactiveCmdText.Text = ""
            e.Handled = True
        End If
    End Sub

    Private Sub RunButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RunButton.Click
        Dim cb As New AsyncCallback(AddressOf modMain.experimentCompleted)
        Dim del As New runExperimentDelegate(AddressOf runExperiment)
        del.BeginInvoke(cb, del)
    End Sub

    'added oct 2011, run virtual experiment
    Private Sub RunVirtualButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RunVirtualButton.Click

        Dim cb As New AsyncCallback(AddressOf modMain.experimentCompleted)
        Dim del As New runExperimentDelegate(AddressOf runVirtualExperiment)
        del.BeginInvoke(cb, del)
    End Sub

    Private Sub StopButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StopButton.Click
        runningState = stopping
        StopButton.Enabled = False
        BatchButton.Enabled = False
        StopBatchButton.Enabled = False
        StatusLabel.Text = "Waiting for current experiment to end..."
        Refresh()
    End Sub

    Private Sub BatchButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BatchButton.Click
        modMain.runBatch()
    End Sub

    Private Sub StopBatchButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StopBatchButton.Click
        runningState = batchstopping
        StopBatchButton.Enabled = False
        StatusLabel.Text = "Waiting for current experiment to end..."
        Refresh()
    End Sub

    Private Sub InteractiveGuiButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles interGUI_Button.Click
        If gui_inter.IsLoaded() = False Then
            gui_inter.LoadMacros()
        End If

        'gui_inter.ShowDialog() 'modal
        gui_inter.Show() 'modeless
        gui_inter.Refresh()

    End Sub

    Private Sub AutoFill_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AutoFill.Click
        'Dim gui_autofill As New frmGUI_autofill(Me)
        'gui_autofill.ShowDialog()
        'gui_autofill.Refresh()
        gui_autofill.ShowAutofill(Me.currentExpFileLabel.Text)

    End Sub


    Private Sub logExp_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles logExp_Button.Click
        Dim intResponse As Integer
        intResponse = MsgBox("Only BATCH parameters are logged. Continue?", vbYesNo + vbExclamation, "Attention")

        If intResponse = vbYes Then
            With FolderBrowserDialog
                FolderBrowserDialog.Description = "Select folder to save BATCH experiment logs:"
                'FolderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer
                If .ShowDialog = System.Windows.Forms.DialogResult.OK Then
                    logExperiment(.SelectedPath)
                End If
            End With

        End If

        'If runningState = batch Or runningState = batchstopping Then
        'End If

    End Sub

    Private Sub RunFLoopButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RunFLoopButton.Click
        ' Run f-loop is separate from the normal Run button.
        ' It enables nextExpParameters.txt reading in modMain.
        ' Make sure the feedback/parameter directory is chosen on the GUI thread.
        If Not EnsureFeedbackLogDirectory() Then
            Return
        End If

        Dim del As New runExperimentDelegate(AddressOf runFLoopExperiment)
        del.BeginInvoke(AddressOf experimentCompleted, del)
    End Sub

    Private Sub LogFeedbackButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LogFeedbackButton.Click
        ' Feedback logging is intentionally separate from batch experiment-parameter logging.
        LogFeedback()
    End Sub

    Private Sub LogFeedback()
        ' Select the folder used by Run f-loop.
        ' Run f-loop will read nextExpParameters.txt from this folder and will write
        ' nextExpParameters_debug.txt, currentExpParameters.txt, and
        ' ExpParametersRecord.txt to this same folder.
        Using feedbackFolderDialog As New System.Windows.Forms.FolderBrowserDialog()
            feedbackFolderDialog.Description = "Select f-loop feedback / parameter directory:"
            feedbackFolderDialog.ShowNewFolderButton = True

            Dim currentDir As String
            currentDir = GetFeedbackLogDirectory()
            If currentDir IsNot Nothing AndAlso currentDir.Trim().Length > 0 AndAlso System.IO.Directory.Exists(currentDir) Then
                feedbackFolderDialog.SelectedPath = currentDir
            End If

            If feedbackFolderDialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                SetFeedbackLogDirectory(feedbackFolderDialog.SelectedPath)
                MsgBox("F-loop feedback directory set to:" & vbCrLf & feedbackFolderDialog.SelectedPath, MsgBoxStyle.Information, "Log feedback")
            End If
        End Using
    End Sub

    Private Sub frmGUI_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        ' Determine if text has changed in the textbox by comparing to original text.
        'MsgBox("frmGUI_Closing")
        'deallocate memory
        analogdata.release_data()
        'analogdata2.release_data()
        'analogdata3.release_data()

        'disconnect from the host for SpectronService
        analogdata.Close()
        'analogdata2.Close()
        'analogdata3.Close()
        'If textBox1.Text <> strMyOriginalText Then
        '    ' Display a MsgBox asking the user to save changes or abort.
        '    If MessageBox.Show("Do you want to save changes to your text?", "My Application", MessageBoxButtons.YesNo) = DialogResult.Yes Then
        '        ' Cancel the Closing event from closing the form.
        '        e.Cancel = True
        '    End If ' Call method to save file...
        'End If
    End Sub 'Form1_ClosingEnd Class'Form1

    Private Sub frmGUI_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub loadLog_button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles loadLog_button.Click
        With OpenFileDialog
            If .ShowDialog = System.Windows.Forms.DialogResult.OK Then
                ' Clear batch params
                dt.Rows.Clear()

                ' Load parameters from log file
                LoadLogParams(.FileName)
            End If
        End With
    End Sub

    Private Sub LoadLogParams(ByVal log_file_location As String)
        Dim s As String
        Dim t As String()
        Dim varList As ArrayList = modUtilities.GetExpVariables()
        Dim var As Object
        s = My.Computer.FileSystem.ReadAllText(log_file_location)
        t = Regex.Split(s, "--------------------------")

        'clean up
        Dim param_table As String() = Regex.Split(t(1).Trim(), "\n")
        Dim param_row As String() = Regex.Split(param_table(0), ",")

        If varList.Count() = param_row.GetLength(0) - 1 Then
            For ii As Integer = 0 To (param_table.Length() - 1)
                Dim row As DataRow = dt.NewRow()

                param_row = Regex.Split(param_table(ii), ",")

                Dim jj As Integer = 0
                For Each var In varList
                    row(jj) = param_row(jj)
                    jj = jj + 1
                Next
                dt.Rows.Add(row)
            Next
        Else
            MsgBox("Number of arguments in log file does not match the number required by loaded program.")
        End If
    End Sub

End Class