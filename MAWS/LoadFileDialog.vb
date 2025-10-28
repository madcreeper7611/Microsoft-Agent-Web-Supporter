Imports System.Text.RegularExpressions

Imports System.Windows.Forms

Public Class LoadFileDialog
    Dim MSHFileRegex As New Regex("^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)\/([-a-zA-Z0-9()@:%_\+.~#?&//=]*).msh$")

    Private Sub OKOpenButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OKOpenButton.Click
        If MSHFileRegex.IsMatch(FileURLTextBox.Text) Then
            AgentForm.ScriptURL = FileURLTextBox.Text
            AgentForm.ReadScript()
            Me.Close()
        Else
            MessageBox.Show("There was an error while loading the script: URL is invalid, or doesn't lead directly to a .MSH file.", "MAWS", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub CancelButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CancelOpenButton.Click
        Application.Exit()
    End Sub
End Class
