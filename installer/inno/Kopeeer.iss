; Kopeeer installer draft.
; Build on Windows through scripts\build-installer.ps1.

#ifndef AppVersion
#define AppVersion "0.3.0-alpha"
#endif

#ifndef PublishDir
#define PublishDir "..\..\artifacts\publish\Kopeeer.App"
#endif

#ifndef OutputDir
#define OutputDir "..\..\artifacts\installer"
#endif

#ifndef DropHandlerDir
#define DropHandlerDir "..\..\artifacts\publish\Kopeeer.Shell"
#endif

#define AppName "Kopeeer"
#define AppPublisher "file-operation-queue contributors"
#define AppExeName "Kopeeer.App.exe"

[Setup]
AppId={{8F7F8CF9-13CE-4E1C-8E8D-FA4D54BB4A47}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=Kopeeer-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed=x64compatible
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#AppExeName}

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#DropHandlerDir}\Kopeeer.ShellExtension.dll"; DestDir: "{app}\Shell"; Flags: ignoreversion

[Tasks]
Name: "explorercontext"; Description: "Add Explorer context menu commands"; GroupDescription: "Windows Explorer integration:"; Flags: checkedonce

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"

[Registry]
Root: HKLM; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Copy with Kopeeer..."; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKLM; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\copy.ico"; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKLM; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation copy --pick-target --sources ""%1"""; Flags: uninsdeletekey; Tasks: explorercontext

Root: HKLM; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Move with Kopeeer..."; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKLM; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\cut.ico"; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKLM; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation move --pick-target --sources ""%1"""; Flags: uninsdeletekey; Tasks: explorercontext

Root: HKLM; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Copy with Kopeeer..."; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKLM; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\copy.ico"; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKLM; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation copy --pick-target --sources ""%1"""; Flags: uninsdeletekey; Tasks: explorercontext

Root: HKLM; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Move with Kopeeer..."; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKLM; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\cut.ico"; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKLM; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation move --pick-target --sources ""%1"""; Flags: uninsdeletekey; Tasks: explorercontext

Root: HKLM; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; ValueType: string; ValueName: ""; ValueData: "Kopeeer Right-Drag Menu"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; ValueType: string; ValueName: "AppPath"; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}\InprocServer32"; ValueType: string; ValueName: ""; ValueData: "{app}\Shell\Kopeeer.ShellExtension.dll"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}\InprocServer32"; ValueType: string; ValueName: "ThreadingModel"; ValueData: "Apartment"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer"; ValueType: string; ValueName: ""; ValueData: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer"; ValueType: string; ValueName: ""; ValueData: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer"; ValueType: string; ValueName: ""; ValueData: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"; ValueType: string; ValueName: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; ValueData: "Kopeeer Right-Drag Menu"; Flags: uninsdeletevalue; Tasks: explorercontext; Check: IsWin64

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Start {#AppName}"; Flags: nowait postinstall skipifsilent
