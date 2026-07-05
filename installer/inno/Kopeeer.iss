; Kopeeer installer draft.
; Build on Windows through scripts\build-installer.ps1.

#ifndef AppVersion
#define AppVersion "0.2.0-alpha"
#endif

#ifndef PublishDir
#define PublishDir "..\..\artifacts\publish\Kopeeer.App"
#endif

#ifndef OutputDir
#define OutputDir "..\..\artifacts\installer"
#endif

#define AppName "Kopeeer"
#define AppPublisher "file-operation-queue contributors"
#define AppExeName "Kopeeer.App.exe"

[Setup]
AppId={{8F7F8CF9-13CE-4E1C-8E8D-FA4D54BB4A47}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=Kopeeer-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed=x64
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#AppExeName}

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Tasks]
Name: "explorercontext"; Description: "Add Explorer context menu commands"; GroupDescription: "Windows Explorer integration:"; Flags: checkedonce

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"

[Registry]
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Copy with Kopeeer..."; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\copy.ico"; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation copy --pick-target --sources ""%1"""; Flags: uninsdeletekey; Tasks: explorercontext

Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Move with Kopeeer..."; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\cut.ico"; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation move --pick-target --sources ""%1"""; Flags: uninsdeletekey; Tasks: explorercontext

Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Copy with Kopeeer..."; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\copy.ico"; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation copy --pick-target --sources ""%1"""; Flags: uninsdeletekey; Tasks: explorercontext

Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Move with Kopeeer..."; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\cut.ico"; Flags: uninsdeletekey; Tasks: explorercontext
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation move --pick-target --sources ""%1"""; Flags: uninsdeletekey; Tasks: explorercontext

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Start {#AppName}"; Flags: nowait postinstall skipifsilent
