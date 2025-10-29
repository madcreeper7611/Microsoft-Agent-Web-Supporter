Imports System.Net
Imports System.Text
Imports AgentObjects
Imports System.Text.RegularExpressions

Public Class AgentForm
    ' The URL to the script, first set to the command line arguments and changed later.
    Public ScriptURL As String = Command()
    ' Contains the IDs of each character in the script.
    Dim CharIDs As New List(Of String)
    ' Hide Requests to make sure each agent is hidden before the program closes.
    Dim HideReq As IAgentCtlRequest
    ' The client needed for downloading the data in the script.
    Dim client As New WebClient
    ' Variable for making requests
    Dim Req

    Public Sub New()
        InitializeComponent()
        Hide()
    End Sub

    Private Sub AgentForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ControlAxAgent.CreateControl()
        ' Detects if there's command line arguments, if there's not, shows the popup window to load a MASH file from the web.
        If ScriptURL = Nothing Then
            LoadFileDialog.Show()
        Else
            ReadScript()
        End If
    End Sub

    ' Extremely useful to prevent agents from speaking at the same time and giving things time to load.
    Private Sub WaitFor(ByVal Request)
        On Error Resume Next
        For Each Agent As IAgentCtlCharacter In ControlAxAgent.Characters
            Agent.Wait(Request)
        Next
    End Sub

    Public Sub ReadScript()
        ' Set's the group that new values will be loaded in.
        ' For example: If the current parse is [Characters] then it will start adding Characters to the group.
        Dim CurrentParse As String


        ScriptURL = ScriptURL.Replace("""", "")

        If ScriptURL.StartsWith("msagentweb://") Then
            ScriptURL = ScriptURL.Replace("msagentweb://", "http://")
        ElseIf Not ScriptURL.StartsWith("http://") AndAlso Not ScriptURL.StartsWith("https://") Then
            ScriptURL = "http://" & ScriptURL
        End If

        Try
            Dim ScriptText = client.DownloadString(ScriptURL)

            For Each RL In ScriptText.Split(Chr(10))
                Dim Line = RL.Trim
                If Not Line = String.Empty Then
                    ' Removes unnecessary characters such as spaces and such.
                    ' Checks if the line is a list, if it is, set it as the current parse.
                    If RL.StartsWith("[") Then
                        CurrentParse = Line
                    Else
                        ' Gets the value that comes after the equal sign.
                        Dim AfterEquals As String = Line.Substring(Line.LastIndexOf("=") + 1)
                        ' Detects the current parse and runs actions based off it.
                        If CurrentParse = "[Characters]" Then
                            ' Loads all of the characters in the MASH script.
                            Dim CharID As String = Line.Remove(Line.LastIndexOf("="))
                            ControlAxAgent.Characters.Load(CharID, AfterEquals)
                            ControlAxAgent.Characters(CharID).Get("State", "Showing, Hiding, Speaking, Moving, Gesturing, Idling, Hearing, Listening", True)
                            CharIDs.Add(CharID)
                        ElseIf CurrentParse = "[LanguageIDs]" Then
                            ' Sets the language ID for all of the characters.
                            ControlAxAgent.Characters(Line.Remove(Line.LastIndexOf("="))).LanguageID = AfterEquals
                        ElseIf CurrentParse = "[Script]" Then
                            If Line.ToLower.Contains("set req =") Then
                                ' Sets the current request to the action listed after the equal sign.
                                Req = GetActionFromLine(Line)
                            ElseIf Line.ToLower = "waitfor req" Then
                                ' Has the program wait for a specific request before running the next line.
                                WaitFor(Req)
                            Else
                                ' Causes a specific character to do an action from the line.
                                GetActionFromLine(Line)
                            End If
                        End If
                    End If

                End If
            Next

            HideAllCharacters()
        Catch ex As Exception
            MessageBox.Show("There was an error while loading the script:" & ex.Message)
            Close()
        End Try
    End Sub

    ' Parses the current line to check if it can run any actions on it.
    Private Function GetActionFromLine(ByVal Line As String)
        Dim ActionRegex As New Regex("[A-Za-z0-9]+\.[A-Za-z0-9]+")
        Dim QuotesRegex As New Regex("""(?:[^""]|"""")*""(\s+&\s+[A-Za-z0-9]+\(\)\s+&\s+)?")
        Dim PointRegex As New Regex("[a-zA-Z0-9]+, [a-zA-Z0-9]+")
        Dim IntRegex As New Regex("[0-9]+")

        If ActionRegex.IsMatch(Line) Then
            Dim ActionMatch = ActionRegex.Match(Line).ToString.ToLower
            Dim CharID = ActionMatch.Remove(ActionMatch.IndexOf("."))

            If QuotesRegex.IsMatch(Line) Then
                Dim FunctionRegex As New Regex("(""\s+&\s+)?[A-Za-z0-9]+\(\)(\s+&\s+"")?")
                Dim QuotesMatch = QuotesRegex.Match(Line).ToString.Replace("""", "")
                Dim SpeakString As String

                For Each M In QuotesRegex.Matches(Line)
                    SpeakString = SpeakString & M.ToString
                Next

                For Each M In FunctionRegex.Matches(SpeakString)
                    Dim Match = M.ToString.ToLower

                    If Match.Contains("gettimeofday") Then
                        SpeakString = SpeakString.Replace(M.ToString, GetTimeOfDay)
                    ElseIf Match.Contains("gettime") Then
                        SpeakString = SpeakString.Replace(M.ToString, GetTime)
                    ElseIf Match.Contains("getday") Then
                        SpeakString = SpeakString.Replace(M.ToString, GetDay)
                    ElseIf Match.Contains("getdate") Then
                        SpeakString = SpeakString.Replace(M.ToString, GetDate)
                    ElseIf Match.Contains("holiday") AndAlso Holiday() <> "" Then
                        SpeakString = SpeakString.Replace(M.ToString, Holiday)
                    End If
                Next

                SpeakString = SpeakString.Substring(1, SpeakString.Length - 2).Replace("""""", """")

                If ActionMatch.Contains(".ttsmodeid") Then
                    ControlAxAgent.Characters(CharID).TTSModeID = QuotesMatch
                    Return Nothing
                ElseIf ActionMatch.Contains(".play") Then
                    ControlAxAgent.Characters(CharID).Get("Animation", QuotesMatch)
                    Return ControlAxAgent.Characters(CharID).Play(QuotesMatch)
                ElseIf ActionMatch.Contains(".speak") Then
                    Return ControlAxAgent.Characters(CharID).Speak(SpeakString)
                ElseIf ActionMatch.Contains(".think") Then
                    MessageBox.Show(CharID)
                    Return ControlAxAgent.Characters(CharID).Think(SpeakString)
                End If

                Return Nothing
            ElseIf PointRegex.IsMatch(Line) Then
                Dim PointMatch As String = PointRegex.Match(Line).ToString
                Dim ScreenBottom As Integer = Screen.PrimaryScreen.Bounds.Bottom - ControlAxAgent.Characters(CharID).Height
                Dim ScreenLeft As Integer = Screen.PrimaryScreen.Bounds.Left
                Dim ScreenRight As Integer = Screen.PrimaryScreen.Bounds.Right - ControlAxAgent.Characters(CharID).Width
                Dim ScreenTop As Integer = Screen.PrimaryScreen.Bounds.Top
                Dim LocationX As String = PointMatch.Remove(PointMatch.IndexOf(","))
                Dim LocationY As String = PointMatch.Substring(PointMatch.IndexOf(",") + 1)
                Dim PointX As Integer
                Dim PointY As Integer

                If LocationX.ToLower.Contains("left") Then
                    PointX = ScreenLeft
                ElseIf LocationX.ToLower.Contains("center") Then
                    PointX = ScreenRight / 2
                ElseIf LocationX.ToLower.Contains("right") Then
                    PointX = ScreenRight
                ElseIf IntRegex.IsMatch(LocationX) Then
                    PointX = Convert.ToInt32(IntRegex.Match(LocationX).ToString)
                Else
                    Return 0
                End If

                If LocationY.ToLower.Contains("top") Then
                    PointY = ScreenTop
                ElseIf LocationY.ToLower.Contains("center") Then
                    PointY = ScreenBottom / 2
                ElseIf LocationY.ToLower.Contains("bottom") Then
                    PointY = ScreenBottom
                ElseIf IntRegex.IsMatch(LocationY) Then
                    PointY = Convert.ToInt32(IntRegex.Match(LocationY).ToString)
                Else
                    Return 0
                End If

                If ActionMatch.Contains(".moveto") Then
                    Return ControlAxAgent.Characters(CharID).MoveTo(PointX, PointY)
                ElseIf ActionMatch.Contains(".gestureat") Then
                    Return ControlAxAgent.Characters(CharID).GestureAt(PointX, PointY)
                End If

                Return Nothing
            ElseIf IntRegex.IsMatch(Line) Then
                Dim IntMatch As Integer = Convert.ToInt16(IntRegex.Match(Line).ToString)

                If ActionMatch.Contains(".width") Then
                    ControlAxAgent.Characters(CharID).Width = IntMatch
                ElseIf ActionMatch.Contains(".height") Then
                    ControlAxAgent.Characters(CharID).Height = IntMatch
                End If

                Return Nothing
            ElseIf ActionMatch.Contains(".show") Then
                Return ControlAxAgent.Characters(CharID).Show()
            ElseIf ActionMatch.Contains(".hide") Then
                Return ControlAxAgent.Characters(CharID).Hide()
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function

    Private Function GetTimeOfDay()
        If Date.Now.Hour < 12 Then
            Return "Morning"
        ElseIf Date.Now.Hour < 17 Then
            Return "Afternoon"
        Else
            Return "Evening"
        End If
    End Function

    Private Function GetTime() As String
        Return Date.Now.ToShortTimeString
    End Function

    ' Gets the name of the current Day of the Week.
    Private Function GetDay() As String
        Dim DayName As String() = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"}

        Return DayName(Date.Now.DayOfWeek)
    End Function

    Private Function GetDate() As String
        Return MonthName(Date.Now.Month) & " " & Date.Now.Day & ", " & Date.Now.Year
    End Function

    ' Checks if the current date is the holiday.
    Private Function Holiday() As String
        If Date.Now.Month = 1 And Date.Now.Day = 1 Then
            Return "Happy New Year!"
        End If
        If Date.Now.Month = 2 And Date.Now.Day = 14 Then
            Return "Happy Valentines Day!"
        End If
        If Date.Now.Month = 3 And Date.Now.Day = 14 Then
            Return "It's Gordon's Birthday!"
        End If
        If Date.Now.Month = 3 And Date.Now.Day = 17 Then
            Return "Happy Saint Patrick's Day!"
        End If
        If Date.Now.Month = 4 And Date.Now.Day = 1 Then
            Return "Happy April Fools Day!"
        End If
        If Date.Now.Month = 5 And Date.Now.Day = 5 Then
            Return "Happy Cinco de Mayo!"
        End If
        If Date.Now.Month = 5 And Date.Now.Day = 25 Then
            Return "It's TMAFE's Anniversary!"
        End If
        If Date.Now.Month = 7 And Date.Now.Day = 4 Then
            Return "Happy Fourth of July!"
        End If
        If Date.Now.Month = 7 And Date.Now.Day = 6 Then
            Return "It's MadCreeper's Birthday!"
        End If
        If Date.Now.Month = 9 And Date.Now.Day = 8 Then
            Return "It's Microsoft Agent's Anniversary!"
        End If
        If Date.Now.Month = 10 And Date.Now.Day = 31 Then
            Return "Happy Halloween!"
        End If
        If Date.Now.Month = 11 And Date.Now.Day <= 2 Then
            Return "Today is Day of the Dead!"
        End If
        If Date.Now.Month = 11 And Date.Now.Day <= 2 Then
            Return "Today is Christmas Eve!"
        End If
        If Date.Now.Month = 12 And Date.Now.Day = 25 Then
            Return "Merry Christmas!"
        End If
        If Date.Now.Month = 12 And Date.Now.Day = 31 Then
            Return "Today is New Years Eve!"
        End If

        Return Nothing
    End Function

    ' Gets all characters in the script and adds the options to show/hide them from the tray icon.
    Private Sub TrayCMS_Opening(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles TrayCMS.Opening
        ' Gets each character ID in the CharIDs list
        For Each CharID As String In CharIDs
            ' Checks if the character is currently visible then adds the option to show/hide the character.
            If ControlAxAgent.Characters(CharID).Visible Then
                TrayCMS.Items.Add("Hide " & CharID)
            Else
                TrayCMS.Items.Add("Show " & CharID)
            End If
        Next
    End Sub

    Private Sub TrayCMS_ItemClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles TrayCMS.ItemClicked
        Dim ItemText = e.ClickedItem.Text

        ' Checks if the text of the item starts with Show/Hide and preforms an action based on it.
        If ItemText.StartsWith("Hide") Then
            ControlAxAgent.Characters(ItemText.Substring(5)).Hide()
        ElseIf ItemText.StartsWith("Show") Then
            ControlAxAgent.Characters(ItemText.Substring(5)).Show()
        End If
    End Sub

    Private Sub ExitTSMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitTSMI.Click
        For Each CharID In CharIDs
            ControlAxAgent.Characters(CharID).StopAll()
        Next

        HideAllCharacters()
    End Sub

    Private Sub HideAllCharacters()
        For i = 0 To CharIDs.Count - 1
            Dim CharID = CharIDs(i)
            Dim AgentChar = ControlAxAgent.Characters(CharID)

            If Not AgentChar.HasOtherClients And AgentChar.Active Then
                HideReq = AgentChar.Hide
                WaitFor(HideReq)
            End If
        Next
    End Sub

    Private Sub ControlAxAgent_RequestComplete(ByVal sender As System.Object, ByVal e As AxAgentObjects._AgentEvents_RequestCompleteEvent) Handles ControlAxAgent.RequestComplete
        If e.request Is HideReq Then
            Dim UnloadedChars = 0

            For Each CharID In CharIDs
                Dim AgentChar = ControlAxAgent.Characters(CharID)

                If AgentChar.Visible = False Then
                    UnloadedChars = UnloadedChars + 1
                End If

                If UnloadedChars = CharIDs.Count Then
                    Application.Exit()
                End If
            Next
        End If
    End Sub
End Class