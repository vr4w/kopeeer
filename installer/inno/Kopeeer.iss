; Kopeeer installer draft.
; Build on Windows through scripts\build-installer.ps1.

#ifndef AppVersion
#define AppVersion "1.0.0"
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
#define AppPublisher "vr4w"
#define AppExeName "Kopeeer.App.exe"
#define AppUrl "https://github.com/vr4w/kopeeer"

[Setup]
AppId={{8F7F8CF9-13CE-4E1C-8E8D-FA4D54BB4A47}
AppName={#AppName}
AppVerName={#AppName} {#AppVersion}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}/issues
AppUpdatesURL={#AppUrl}/releases
AppComments=Explorer-first copy and move queue for Windows.
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
WizardStyle=modern
OutputDir={#OutputDir}
OutputBaseFilename=Kopeeer-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed=x64compatible
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#AppExeName}
CloseApplications=no

[Messages]
WelcomeLabel1=Install Kopeeer
WelcomeLabel2=Kopeeer adds Copy with Kopeeer and Move with Kopeeer to Windows Explorer. Install it once, then use it from Explorer's right-drag menu. No separate app window needs to stay open.
FinishedHeadingLabel=Kopeeer is installed
FinishedLabel=Kopeeer is ready in Windows Explorer. Right-drag files or folders onto a target folder and choose Copy with Kopeeer or Move with Kopeeer.

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#DropHandlerDir}\Kopeeer.ShellExtension.dll"; DestDir: "{app}\Shell\{#AppVersion}"; Flags: ignoreversion

[Tasks]
Name: "explorercontext"; Description: "Add Kopeeer to Explorer right-drag menus"; GroupDescription: "Windows Explorer integration:"; Flags: checkedonce

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
Root: HKLM; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}\InprocServer32"; ValueType: string; ValueName: ""; ValueData: "{app}\Shell\{#AppVersion}\Kopeeer.ShellExtension.dll"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}\InprocServer32"; ValueType: string; ValueName: "ThreadingModel"; ValueData: "Apartment"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer"; ValueType: string; ValueName: ""; ValueData: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer"; ValueType: string; ValueName: ""; ValueData: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer"; ValueType: string; ValueName: ""; ValueData: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; Flags: uninsdeletekey; Tasks: explorercontext; Check: IsWin64
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"; ValueType: string; ValueName: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; ValueData: "Kopeeer Right-Drag Menu"; Flags: uninsdeletevalue; Tasks: explorercontext; Check: IsWin64

