; Kopeeer installer draft.
; Build this on Windows after publishing FileOperationQueue.App.

#define AppName "Kopeeer"
#define AppVersion "0.1.0"
#define AppPublisher "file-operation-queue contributors"
#define AppExeName "FileOperationQueue.App.exe"
#define PublishDir "..\..\artifacts\publish\FileOperationQueue.App"

[Setup]
AppId={{8F7F8CF9-13CE-4E1C-8E8D-FA4D54BB4A47}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputBaseFilename=Kopeeer-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"

[Registry]
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Copy with Kopeeer..."; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --queue-copy ""%1"""; Flags: uninsdeletekey

Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Move with Kopeeer..."; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --queue-move ""%1"""; Flags: uninsdeletekey

Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Copy with Kopeeer..."; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --queue-copy ""%1"""; Flags: uninsdeletekey

Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Move with Kopeeer..."; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --queue-move ""%1"""; Flags: uninsdeletekey

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Start {#AppName}"; Flags: nowait postinstall skipifsilent

