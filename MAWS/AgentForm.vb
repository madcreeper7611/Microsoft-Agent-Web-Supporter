Imports System.IO
Imports System.Net
Imports System.Text
Imports AgentObjects
Imports System.Text.RegularExpressions

Public Class AgentForm
    ' The URL to the script, first set to the command line arguments and changed later.
    Public ScriptURL As String = Command()
    ' Contains the IDs of each character in the script.
    Dim CharIDs As New List(Of String)
    ' Load Request to make sure each character is loaded before the script runs.
    Dim HideReq As IAgentCtlRequest
    ' The amount of commands that each character has.
    Dim CommandsCount As Integer
    ' The client needed for downloading the data in the script.
    Dim client As New WebClient
    ' The text contained in the script.
    Dim ScriptText As String
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

    ' Extremely useful to prevent agents from speaking/thinking/playing at the same time and giving things time to load.
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

        ' Removes quotation marks in the command line arguments.
        ScriptURL = ScriptURL.Replace("""", "")

        If ScriptURL.StartsWith("msagentweb://") Then
            ' Replaces the start of the script with http:// if it starts with msagentweb://.
            ScriptURL = ScriptURL.Replace("msagentweb://", "http://")
        ElseIf Not ScriptURL.StartsWith("http://") AndAlso Not ScriptURL.StartsWith("https://") Then
            ' Adds http:// to the start of the script if there's no protocol infront of the URL.
            ScriptURL = "http://" & ScriptURL
        End If

        Try
            ' Downloads the data from the URL of the MSH script.
            ScriptText = client.DownloadString(ScriptURL)
            MAWSNotifyIcon.BalloonTipIcon = ToolTipIcon.None
            MAWSNotifyIcon.BalloonTipText = Nothing

            For Each RL In ScriptText.Split(Chr(10))
                ' Removes unnecessary characters from the line like spaces that come after the line.
                Dim Line = RL.Trim
                If Not Line = String.Empty Then
                    ' Checks if the line is a list declaration, if it is, set it as the current parse.
                    If RL.StartsWith("[") Then
                        CurrentParse = Line
                    Else
                        ' Gets the value that comes after the equal sign.
                        Dim AfterEquals As String = Line.Substring(Line.LastIndexOf("=") + 1)
                        ' Detects the current parse and runs actions based off it.
                        If CurrentParse = "[Characters]" Then
                            ' Loads all of the characters in the MASH script.
                            Dim CharID As String = Line.Remove(Line.LastIndexOf("=")).ToLower

                            If LoadAgentChar(CharID, AfterEquals) Then
                                ControlAxAgent.Characters(CharID).Get("State", "Showing, Hiding, Speaking, Moving, Gesturing, Idling, Hearing, Listening", True)
                                CharIDs.Add(CharID)
                                Wait(100)
                            End If
                        ElseIf CurrentParse = "[LanguageIDs]" Then
                            ' Sets the language ID for all of the characters.
                            Dim CharID As String = Line.Remove(Line.LastIndexOf("=")).ToLower
                            If CharIDs.Contains(CharID) Then
                                Req = ControlAxAgent.Characters(Line.Remove(Line.LastIndexOf("="))).LanguageID = AfterEquals
                                WaitFor(Req)
                            End If
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
                        ElseIf CurrentParse = "[Commands]" Then
                            For Each CharID In CharIDs
                                If Line.ToLower.StartsWith("menuname") Then
                                    ' Sets the caption of the menu to the value the script declares it as.
                                    ControlAxAgent.Characters(CharID).Commands.Caption = AfterEquals
                                Else
                                    ' Adds each character the commands declared in the script and gets the amount of commands.
                                    ControlAxAgent.Characters(CharID).Commands.Add(Line.Remove(Line.LastIndexOf("=")), AfterEquals.Remove(AfterEquals.IndexOf("|")), AfterEquals.Substring(AfterEquals.LastIndexOf("|") + 1))
                                    CommandsCount = ControlAxAgent.Characters(CharID).Commands.Count
                                End If
                            Next
                        End If
                    End If
                End If
            Next

            For Each CharID In CharIDs
                Dim AgentChar = ControlAxAgent.Characters(CharID)

                AgentChar.Commands.Add("CLOSE", "Close", "Close")
            Next

            If CharIDs.Count < 1 Then
                ' Exits the program if the script has 0 loaded characters.
                Application.Exit()
            ElseIf CommandsCount < 3 Then
                ' Hides every character at the end of a script if the script has less than 2 commands.
                HideCharsTimer.Start()
            End If
        Catch ex As Exception
            MessageBox.Show("There was an error while loading the script:" & ex.Message)
            Close()
        End Try
    End Sub

    ' This function is probably the hardest part of making MAWS...
    ' Parses the current line to check if it has any requests on it.
    Private Function GetActionFromLine(ByVal Line As String)
        ' Regexes for determining categories of requests and if the specific line is a request.

        ' Regex for determining if the line is a request. (Example: Merlin.Speak)
        Dim RequestRegex As New Regex("[A-Za-z0-9]+\.[A-Za-z0-9]+(\.[A-Za-z0-9]+)?")
        ' Regex for determining if the line contains quotes. (Examples: "Welcome to the Microsoft Agent Web Supporter!", or "Greet")
        Dim QuotesRegex As New Regex("(("")?[A-Za-z0-9]+\(\)\s+&\s+)?""(?:[^""]|"""")*""(\s+&\s+[A-Za-z0-9]+\(\)""?(\s+&\s+)?)?")
        ' Regex for determining if the request is setting a characters balloon style. (Example: &H21C000F)
        Dim BalloonStyleRegex As New Regex("&([A-Za-z]+([0-9]+[A-Za-z]+)+)")
        ' Regex for determining if the line is a point on the screen (Examples: MerlinRightX MerlinBottomY, or 300, 240")
        Dim PointRegex As New Regex("[a-zA-Z0-9]+, [a-zA-Z0-9]+")
        ' Regex for determining if the line is a single integer (Example: 128)
        Dim IntRegex As New Regex("[0-9]+")

        ' Determines if the line is a request.
        If RequestRegex.IsMatch(Line) Then
            Dim RequestMatch = RequestRegex.Match(Line).ToString.ToLower
            ' Gets the ID of the character preforming the request.
            Dim CharID = RequestMatch.Remove(RequestMatch.IndexOf("."))

            If Not CharIDs.Contains(CharID) Then
                Return Nothing
            ElseIf QuotesRegex.IsMatch(Line) Then
                ' Regex to determine if the current line contains one or more time function. (Example: GetTimeOfDay())
                Dim TimeRegex As New Regex("(""\s+&\s+)?[A-Za-z0-9]+\(\)(\s+&\s+"")?")
                Dim QuotesMatch = QuotesRegex.Match(Line).ToString.Replace("""", "")
                ' The string that the agent character speaks/thinks in the script.
                Dim SpeakString As String

                For Each M In QuotesRegex.Matches(Line)
                    SpeakString = SpeakString & M.ToString
                Next

                ' Replaces time functions with the current date and time.
                For Each M In TimeRegex.Matches(SpeakString)
                    Dim Match = M.ToString.ToLower

                    If Match.Contains("gettimeofday") Then
                        SpeakString = SpeakString.Replace(M.ToString, GetTimeOfDay)
                    ElseIf Match.Contains("gettime") Then
                        SpeakString = SpeakString.Replace(M.ToString, GetTime)
                    ElseIf Match.Contains("getday") Then
                        SpeakString = SpeakString.Replace(M.ToString, GetDay)
                    ElseIf Match.Contains("getdate") Then
                        SpeakString = SpeakString.Replace(M.ToString, GetDate)
                    ElseIf Match.Contains("holiday") Then
                        SpeakString = SpeakString.Replace(M.ToString, Holiday)
                    End If
                Next

                ' Removes double quotes and the quotes at the beginning and end of the phrase.
                SpeakString = SpeakString.Substring(1, SpeakString.Length - 2).Replace("""""", """")

                If RequestMatch.Contains(".ttsmodeid") Then
                    ' Sets the TTS Voice of the character if it's a TTSModeID Declaration
                    Req = ControlAxAgent.Characters(CharID).TTSModeID = QuotesMatch
                    WaitFor(Req)
                    Return Nothing
                ElseIf RequestMatch.Contains(".play") Then
                    ' Makes the character play an animation.
                    ControlAxAgent.Characters(CharID).Get("Animation", QuotesMatch)
                    Return ControlAxAgent.Characters(CharID).Play(QuotesMatch)
                ElseIf RequestMatch.Contains(".speak") Then
                    ' Makes the character speak the current speakstring.
                    If Not SpeakString = Nothing Then
                        Return ControlAxAgent.Characters(CharID).Speak(SpeakString)
                    End If
                    Return Nothing
                ElseIf RequestMatch.Contains(".think") Then
                    ' Makes the character think the current speakstring.
                    If Not SpeakString = Nothing Then
                        Return ControlAxAgent.Characters(CharID).Think(SpeakString)
                    End If
                    Return Nothing
                ElseIf RequestMatch.Contains(".balloon") Then
                    ' Gets the subrequest inside of the main request.
                    If RequestMatch.Contains(".balloon.font") Then
                        ' Sets the font to the name of the font set by the subrequest.
                        ControlAxAgent.Characters(CharID).Balloon.FontName = QuotesMatch
                        Return Nothing
                    End If
                End If

                Return Nothing
            ElseIf BalloonStyleRegex.IsMatch(Line) Then
                ControlAxAgent.Characters(CharID).Balloon.Style = BalloonStyleRegex.Match(Line).ToString
                Return Nothing
            ElseIf PointRegex.IsMatch(Line) Then
                Dim PointMatch As String = PointRegex.Match(Line).ToString
                ' Character Height and Width
                Dim CharWidth = ControlAxAgent.Characters(CharID).Width
                Dim CharHeight = ControlAxAgent.Characters(CharID).Height
                ' Screen Locations
                Dim ScreenBottom As Integer = Screen.PrimaryScreen.Bounds.Bottom
                Dim ScreenLeft As Integer = Screen.PrimaryScreen.Bounds.Left
                Dim ScreenRight As Integer = Screen.PrimaryScreen.Bounds.Right
                Dim ScreenTop As Integer = Screen.PrimaryScreen.Bounds.Top
                ' Get's the X and Y location that the character is supposed to move to or gesture at. (Can be a string replacement like BottomX, or an integer like 300)
                Dim LocationX As String = PointMatch.Remove(PointMatch.IndexOf(","))
                Dim LocationY As String = PointMatch.Substring(PointMatch.IndexOf(",") + 1)
                ' Actual X and Y position on screen that the character is meant to move to or gesture at.
                Dim PointX As Integer
                Dim PointY As Integer

                ' Replacement values for X.
                If LocationX.ToLower.Contains("leftcenter") Then
                    PointX = (ScreenRight / 4) - (CharWidth / 2)
                ElseIf LocationX.ToLower.Contains("rightcenter") Then
                    PointX = ((ScreenRight / 4) * 3) - (CharWidth / 2)
                ElseIf LocationX.ToLower.Contains("left") Then
                    PointX = ScreenLeft
                ElseIf LocationX.ToLower.Contains("center") Then
                    PointX = (ScreenRight / 2) - (CharWidth / 2)
                ElseIf LocationX.ToLower.Contains("right") Then
                    PointX = ScreenRight - CharWidth
                ElseIf IntRegex.IsMatch(LocationX) Then
                    PointX = Convert.ToInt32(IntRegex.Match(LocationX).ToString)
                Else
                    Return 0
                End If

                ' Replacement values for Y.
                If LocationY.ToLower.Contains("topcenter") Then
                    PointY = (ScreenBottom / 4) - (CharHeight / 2)
                ElseIf LocationY.ToLower.Contains("bottomcenter") Then
                    PointY = ((ScreenBottom / 4) * 3) - (CharHeight / 2)
                ElseIf LocationY.ToLower.Contains("top") Then
                    PointY = ScreenTop
                ElseIf LocationY.ToLower.Contains("center") Then
                    PointY = (ScreenBottom / 2) - (CharHeight / 2)
                ElseIf LocationY.ToLower.Contains("bottom") Then
                    PointY = ScreenBottom - CharHeight
                ElseIf IntRegex.IsMatch(LocationY) Then
                    PointY = Convert.ToInt32(IntRegex.Match(LocationY).ToString)
                Else
                    Return 0
                End If

                ' Makes the character move to or gesture at a point on the screen.
                If RequestMatch.Contains(".moveto") Then
                    Return ControlAxAgent.Characters(CharID).MoveTo(PointX, PointY)
                ElseIf RequestMatch.Contains(".gestureat") Then
                    Return ControlAxAgent.Characters(CharID).GestureAt(PointX, PointY)
                End If

                Return Nothing
            ElseIf IntRegex.IsMatch(Line) Then
                Dim IntMatch As Integer = Convert.ToInt16(IntRegex.Match(Line).ToString)

                ' Sets the width, height, or font size of the character depending on the declaration.
                If RequestMatch.Contains(".width") Then
                    ControlAxAgent.Characters(CharID).Width = IntMatch
                ElseIf RequestMatch.Contains(".height") Then
                    ControlAxAgent.Characters(CharID).Height = IntMatch
                ElseIf RequestMatch.Contains(".balloon.fontsize") Then
                    ControlAxAgent.Characters(CharID).Balloon.FontSize = IntMatch
                End If

                Return Nothing
            ElseIf RequestMatch.Contains(".show") Then
                ' Shows the character if it's a show request.
                Return ControlAxAgent.Characters(CharID).Show()
            ElseIf RequestMatch.Contains(".hide") Then
                ' Hides the character if it's a show request. (Not to be confused with HideReq)
                Return ControlAxAgent.Characters(CharID).Hide()
            ElseIf RequestMatch.Contains(".stop") Then
                ' Stops the current requests of the character if it's a stop request.
                ControlAxAgent.Characters(CharID).Stop()
            ElseIf RequestMatch.Contains(".stopall") Then
                ' Stops every request of the character if it's a stopall request.
                ControlAxAgent.Characters(CharID).StopAll()
            End If

            Return Nothing
            Else
                Return Nothing
            End If
    End Function

    ' Source - https://stackoverflow.com/questions/15857893/wait-5-seconds-before-continuing-code-vb-net
    ' Original Posted by Ali
    ' Retrieved 11/5/2025, License - CC BY-SA 4.0

    ' Wait request for giving the program time to load the characters
    Private Sub Wait(ByVal Miliseconds As Integer)
        For i As Integer = 0 To Miliseconds
            System.Threading.Thread.Sleep(10)
            Application.DoEvents()
        Next
    End Sub

    Private Function LoadAgentChar(ByVal CharID As String, ByVal CharACS As String)
        Try
            ControlAxAgent.Characters.Load(CharID, CharACS)
        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function

    ' Gets the time of day. (For Example: Evening)
    Private Function GetTimeOfDay() As String
        If Date.Now.Hour < 12 Then
            Return "Morning"
        ElseIf Date.Now.Hour < 17 Then
            Return "Afternoon"
        Else
            Return "Evening"
        End If
    End Function

    ' Gets the current Time. (For Example: 12:00 PM)
    Private Function GetTime() As String
        Return Date.Now.ToShortTimeString
    End Function

    ' Gets the name of the current Day of the Week. (For Example: Tuesday)
    Private Function GetDay() As String
        Dim DayName As String() = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"}

        Return DayName(Date.Now.DayOfWeek)
    End Function

    ' Gets the current Date. (For Example: October 28th, 2025)
    Private Function GetDate() As String
        Dim DayStr As String = Date.Now.Day

        If DayStr = "11" Or DayStr = "12" Or DayStr = "13" Then
            DayStr = DayStr & "th"
        Else
            Select Case DayStr.Remove(0, DayStr.Length - 1)
                Case 1
                    DayStr = DayStr & "st"
                Case 2
                    DayStr = DayStr & "nd"
                Case 3
                    DayStr = DayStr & "rd"
                Case Else
                    DayStr = DayStr & "th"
            End Select
        End If

        Return MonthName(Date.Now.Month) & " " & DayStr.Replace(" ", "") & ", " & Date.Now.Year
    End Function

    ' Checks if the current date is the holiday and gets the holiday. (For Example: Christmas)
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
        TrayCMS.Items.Clear()

        ' Gets each character ID in the CharIDs list
        For Each CharID As String In CharIDs
            ' Checks if the character is currently visible then adds the option to show/hide the character.
            If ControlAxAgent.Characters(CharID).Visible Then
                TrayCMS.Items.Add("Hide " & CharID)
            Else
                TrayCMS.Items.Add("Show " & CharID)
            End If
        Next

        TrayCMS.Items.Add("Exit")
    End Sub

    Private Sub TrayCMS_ItemClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles TrayCMS.ItemClicked
        Dim ItemText = e.ClickedItem.Text

        ' Checks if the text of the item starts with Show/Hide and preforms an action based on it.
        If ItemText.StartsWith("Hide") Then
            ControlAxAgent.Characters(ItemText.Substring(5)).Hide()
        ElseIf ItemText.StartsWith("Show") Then
            ControlAxAgent.Characters(ItemText.Substring(5)).Show()
        ElseIf ItemText = "Exit" Then
            For Each CharID In CharIDs
                ControlAxAgent.Characters(CharID).StopAll()
            Next

            HideAllCharacters()
        End If
    End Sub

    ' Delays the HideAllCharacters function by 1 second to prevent errors from it running as the same time as the characters loading.
    Private Sub HideCharsTimer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HideCharsTimer.Tick
        HideAllCharacters()
        HideCharsTimer.Dispose()
    End Sub

    ' Hides every character visible.
    Private Sub HideAllCharacters()
        For Each CharID In CharIDs
            Dim AgentChar = ControlAxAgent.Characters(CharID)

            ' Checks if the character is visible or has other clients active.
            If Not AgentChar.HasOtherClients And AgentChar.Active Then
                HideReq = AgentChar.Hide
                WaitFor(HideReq)
            End If
        Next
    End Sub

    Private Sub ControlAxAgent_RequestComplete(ByVal sender As System.Object, ByVal e As AxAgentObjects._AgentEvents_RequestCompleteEvent) Handles ControlAxAgent.RequestComplete
        If e.request Is HideReq Then
            ' The amount of characters unloaded in the script.
            Dim UnloadedChars = 0

            For Each CharID In CharIDs
                Dim AgentChar = ControlAxAgent.Characters(CharID)

                ' Checks to see if each character is visible before the app closes. If they're not, unloads them and increments the UnloadedChars by 1.
                If AgentChar.Visible = False Then
                    ControlAxAgent.Characters.Unload(CharID)
                    UnloadedChars = UnloadedChars + 1
                End If

                ' Closes the app if the amount of characters hidden is equal to the amount of character IDs in the script.
                If UnloadedChars = CharIDs.Count Then
                    Application.Exit()
                End If
            Next
        End If
    End Sub

    ' Preforms an action if the agent script contains a command.
    Private Sub ControlAxAgent_Command(ByVal sender As System.Object, ByVal e As AxAgentObjects._AgentEvents_CommandEvent) Handles ControlAxAgent.Command
        Dim CommandName As String = e.userInput.name
        Dim CurrentParse As String

        ' Closes the program if the command has the name of Close.
        If CommandName = "CLOSE" Then
            For Each CharID In CharIDs
                ControlAxAgent.Characters(CharID).StopAll()
            Next

            HideAllCharacters()
        Else
            For Each RL In ScriptText.Split(Chr(10))
                ' Removes unnecessary characters from the line like spaces that come after the line.
                Dim Line = RL.Trim

                If Not Line = String.Empty Then
                    ' Checks if the line is a list declaration, if it is, set it as the current parse.
                    ' If the line isn't declaring a list, it checks to make sure the current parse is a commandscript for the selected command.
                    If Line.StartsWith("[") Then
                        CurrentParse = Line
                    ElseIf CurrentParse = "[CommandScript:" & CommandName & "]" Then
                        If Line.ToLower.Contains("propertysheet.visible = true") Then
                            ' Shows the Advanced Character Options window.
                            ControlAxAgent.PropertySheet.Visible = True
                        ElseIf Line.ToLower.Contains("set req =") Then
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
            Next
        End If
    End Sub
End Class