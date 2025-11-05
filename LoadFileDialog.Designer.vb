<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class LoadFileDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.ButtonDialogTLP = New System.Windows.Forms.TableLayoutPanel
        Me.OKOpenButton = New System.Windows.Forms.Button
        Me.CancelOpenButton = New System.Windows.Forms.Button
        Me.FileURLTextBox = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.ButtonDialogTLP.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ButtonDialogTLP
        '
        Me.ButtonDialogTLP.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.ButtonDialogTLP.ColumnCount = 2
        Me.ButtonDialogTLP.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.ButtonDialogTLP.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.ButtonDialogTLP.Controls.Add(Me.OKOpenButton, 0, 0)
        Me.ButtonDialogTLP.Controls.Add(Me.CancelOpenButton, 1, 0)
        Me.ButtonDialogTLP.Location = New System.Drawing.Point(72, 141)
        Me.ButtonDialogTLP.Name = "ButtonDialogTLP"
        Me.ButtonDialogTLP.RowCount = 1
        Me.ButtonDialogTLP.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.ButtonDialogTLP.Size = New System.Drawing.Size(146, 29)
        Me.ButtonDialogTLP.TabIndex = 0
        '
        'OKOpenButton
        '
        Me.OKOpenButton.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.OKOpenButton.Location = New System.Drawing.Point(3, 3)
        Me.OKOpenButton.Name = "OKOpenButton"
        Me.OKOpenButton.Size = New System.Drawing.Size(67, 23)
        Me.OKOpenButton.TabIndex = 0
        Me.OKOpenButton.Text = "OK"
        '
        'CancelOpenButton
        '
        Me.CancelOpenButton.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.CancelOpenButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.CancelOpenButton.Location = New System.Drawing.Point(76, 3)
        Me.CancelOpenButton.Name = "CancelOpenButton"
        Me.CancelOpenButton.Size = New System.Drawing.Size(67, 23)
        Me.CancelOpenButton.TabIndex = 1
        Me.CancelOpenButton.Text = "Cancel"
        '
        'FileURLTextBox
        '
        Me.FileURLTextBox.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.FileURLTextBox.Location = New System.Drawing.Point(6, 19)
        Me.FileURLTextBox.Name = "FileURLTextBox"
        Me.FileURLTextBox.Size = New System.Drawing.Size(252, 20)
        Me.FileURLTextBox.TabIndex = 1
        Me.FileURLTextBox.Text = "http://"
        '
        'Label1
        '
        Me.Label1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(267, 58)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Enter the direct file URL of the script you want to open and then click OK."
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Controls.Add(Me.FileURLTextBox)
        Me.GroupBox1.Location = New System.Drawing.Point(15, 70)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(264, 50)
        Me.GroupBox1.TabIndex = 3
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Script URL"
        '
        'LoadFileDialog
        '
        Me.AcceptButton = Me.OKOpenButton
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.CancelOpenButton
        Me.ClientSize = New System.Drawing.Size(291, 182)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ButtonDialogTLP)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "LoadFileDialog"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Load MASH Script from URL"
        Me.ButtonDialogTLP.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ButtonDialogTLP As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OKOpenButton As System.Windows.Forms.Button
    Friend WithEvents CancelOpenButton As System.Windows.Forms.Button
    Friend WithEvents FileURLTextBox As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox

End Class
