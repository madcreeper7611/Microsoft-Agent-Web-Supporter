Imports System.Net

Public Class AgentForm
    Public ScriptURL As String = Command()
    Dim client As New WebClient

    Public Sub New()
        InitializeComponent()
        Hide()
    End Sub

    Private Sub AgentForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If ScriptURL = Nothing Then
            LoadFileDialog.Show()
        Else
            ReadScript()
        End If
    End Sub

    Public Sub ReadScript()
        Try
            Dim ScriptText = client.DownloadString(ScriptURL)
        Catch ex As Exception
            MessageBox.Show("There was an error while loading the script:" & ex.Message)
        End Try
    End Sub
End Class