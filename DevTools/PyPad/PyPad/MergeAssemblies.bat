:: /targetplatform:v4,c:\Windows\Microsoft.NET\Framework64\v4.0.30319 
SETLOCAL
CD BIN\DEBUG
z:\dev\bin\ilmerge /lib:C:\Windows\Microsoft.NET\Framework\v4.0.30319 /closed /target:winexe /out:PyPadSingle.exe PyPad.exe Microsoft.Dynamic.dll Microsoft.Scripting.dll  ACSR.Controls.ThirdParty.dll ACSR.Core.dll ACSR.PythonScripting.dll ScintillaNet.dll
ENDLOCAL