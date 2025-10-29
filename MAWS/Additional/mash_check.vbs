Call Main

Sub Main() 
	On Error Resume Next
	
	Dim FSO, msg
	Dim ScrShell
	Dim MASHPath32
	Dim MASHPath64
	
	Set FSO = CreateObject("Scripting.FileSystemObject")
	Set ScrShell = WScript.CreateObject("WScript.Shell")
	MASHPath32 = ScrShell.ExpandEnvironmentStrings("%ProgramFiles%") & "\BellCraft.com\MASH\MASH.exe"
	MASHPath64 = ScrShell.ExpandEnvironmentStrings("%ProgramFiles(x86)%") & "\BellCraft.com\MASH\MASH.exe"

	If Not (FSO.FileExists(MASHPath32) Or FSO.FileExists(MASHPath64)) Then
		ScrShell.Exec(FSO.GetParentFolderName(WScript.ScriptFullName) & "\mash_full_setup.exe")
	End If
	
	Set ScrShell = Nothing
End Sub