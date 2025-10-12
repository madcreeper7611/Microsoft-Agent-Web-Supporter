<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AgentForm
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
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AgentForm))
        Me.ControlAxAgent = New AxAgentObjects.AxAgent
        Me.MAWSNotifyIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        CType(Me.ControlAxAgent, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ControlAxAgent
        '
        Me.ControlAxAgent.Enabled = True
        Me.ControlAxAgent.Location = New System.Drawing.Point(0, 0)
        Me.ControlAxAgent.Name = "ControlAxAgent"
        Me.ControlAxAgent.OcxState = CType(resources.GetObject("ControlAxAgent.OcxState"), System.Windows.Forms.AxHost.State)
        Me.ControlAxAgent.Size = New System.Drawing.Size(32, 32)
        Me.ControlAxAgent.TabIndex = 0
        '
        'MAWSNotifyIcon
        '
        Me.MAWSNotifyIcon.Text = "Microsoft Agent Web Support"
        Me.MAWSNotifyIcon.Visible = True
        '
        'AgentForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(32, 32)
        Me.ControlBox = False
        Me.Controls.Add(Me.ControlAxAgent)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "AgentForm"
        Me.Opacity = 0
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.Text = "Microsoft Agent Web Support"
        CType(Me.ControlAxAgent, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ControlAxAgent As AxAgentObjects.AxAgent
    Friend WithEvents MAWSNotifyIcon As System.Windows.Forms.NotifyIcon
End Class
