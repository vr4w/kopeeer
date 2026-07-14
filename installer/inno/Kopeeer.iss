; Kopeeer installer draft.
; Build on Windows through scripts\build-installer.ps1.

#ifndef AppVersion
#define AppVersion "1.0.1"
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

#ifndef ShellExtensionBuildId
#define ShellExtensionBuildId "local"
#endif

#define AppName "Kopeeer"
#define AppPublisher "vr4w"
#define AppExeName "Kopeeer.App.exe"
#define AppIconName "app.ico"
#define ShellExtensionName "Kopeeer.ShellExtension.dll"
#define ShellExtensionRelativeDir "Shell\" + AppVersion + "\" + ShellExtensionBuildId
#define DragDropMenuClassId "{A9D60874-04A4-4962-8798-69D186A6E5E6}"
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
SetupIconFile=..\..\src\Kopeeer.App\Assets\{#AppIconName}
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed=x64compatible
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\Assets\{#AppIconName}
CloseApplications=no

[Messages]
WelcomeLabel1=Install Kopeeer
WelcomeLabel2=Kopeeer adds Copy with Kopeeer and Move with Kopeeer to Windows Explorer. Install it once, then use it from Explorer's right-drag menu. No separate app window needs to stay open.
FinishedHeadingLabel=Kopeeer is installed
FinishedLabel=Kopeeer is ready in Windows Explorer. Right-drag files or folders onto a target folder and choose Copy with Kopeeer or Move with Kopeeer.

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#DropHandlerDir}\{#ShellExtensionName}"; DestDir: "{app}\{#ShellExtensionRelativeDir}"; Flags: ignoreversion restartreplace uninsrestartdelete
Source: "..\..\scripts\diagnose-installation.ps1"; DestDir: "{app}\Tools"; Flags: ignoreversion
Source: "..\..\scripts\repair-shell-integration.ps1"; DestDir: "{app}\Tools"; Flags: ignoreversion

[Registry]
Root: HKLM64; Subkey: "Software\Kopeeer"; ValueType: string; ValueName: "InstallDir"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Kopeeer"; ValueType: string; ValueName: "AppPath"; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Kopeeer"; ValueType: string; ValueName: "ShellExtensionPath"; ValueData: "{app}\{#ShellExtensionRelativeDir}\{#ShellExtensionName}"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Kopeeer"; ValueType: string; ValueName: "ShellExtensionBuildId"; ValueData: "{#ShellExtensionBuildId}"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Kopeeer"; ValueType: string; ValueName: "Version"; ValueData: "{#AppVersion}"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\{#AppExeName}"; ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\{#AppExeName}"; ValueType: string; ValueName: "Path"; ValueData: "{app}"; Flags: uninsdeletekey

Root: HKLM64; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Copy with Kopeeer..."; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\copy.ico"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Classes\*\shell\Kopeeer.CopyWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation copy --pick-target --sources ""%1"""; Flags: uninsdeletekey

Root: HKLM64; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Move with Kopeeer..."; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\cut.ico"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Classes\*\shell\Kopeeer.MoveWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation move --pick-target --sources ""%1"""; Flags: uninsdeletekey

Root: HKLM64; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Copy with Kopeeer..."; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\copy.ico"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Classes\Directory\shell\Kopeeer.CopyWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation copy --pick-target --sources ""%1"""; Flags: uninsdeletekey

Root: HKLM64; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "MUIVerb"; ValueData: "Move with Kopeeer..."; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\Assets\cut.ico"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\Classes\Directory\shell\Kopeeer.MoveWith\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" --enqueue --operation move --pick-target --sources ""%1"""; Flags: uninsdeletekey

Root: HKLM64; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; ValueType: string; ValueName: ""; ValueData: "Kopeeer Right-Drag Menu"; Flags: uninsdeletekey; Check: IsWin64
Root: HKLM64; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; ValueType: string; ValueName: "AppPath"; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey; Check: IsWin64
Root: HKLM64; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}\InprocServer32"; ValueType: string; ValueName: ""; ValueData: "{app}\{#ShellExtensionRelativeDir}\{#ShellExtensionName}"; Flags: uninsdeletekey; Check: IsWin64
Root: HKLM64; Subkey: "Software\Classes\CLSID\{{A9D60874-04A4-4962-8798-69D186A6E5E6}\InprocServer32"; ValueType: string; ValueName: "ThreadingModel"; ValueData: "Apartment"; Flags: uninsdeletekey; Check: IsWin64
Root: HKLM64; Subkey: "Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer"; ValueType: string; ValueName: ""; ValueData: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; Flags: uninsdeletekey; Check: IsWin64
Root: HKLM64; Subkey: "Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer"; ValueType: string; ValueName: ""; ValueData: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; Flags: uninsdeletekey; Check: IsWin64
Root: HKLM64; Subkey: "Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer"; ValueType: string; ValueName: ""; ValueData: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; Flags: uninsdeletekey; Check: IsWin64
Root: HKLM64; Subkey: "Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"; ValueType: string; ValueName: "{{A9D60874-04A4-4962-8798-69D186A6E5E6}"; ValueData: "Kopeeer Right-Drag Menu"; Flags: uninsdeletevalue; Check: IsWin64

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"; IconFilename: "{app}\Assets\{#AppIconName}"
Name: "{autoprograms}\Kopeeer Diagnostics"; Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\Tools\diagnose-installation.ps1"""; WorkingDir: "{app}"; IconFilename: "{app}\Assets\{#AppIconName}"

[Code]
const
  SHCNE_ASSOCCHANGED = $08000000;
  SHCNF_IDLIST = $0000;

procedure SHChangeNotify(wEventId: Longint; uFlags: Longint; dwItem1: Longint; dwItem2: Longint);
  external 'SHChangeNotify@shell32.dll stdcall';

function IsExpectedRegValue(RootKey: Integer; Subkey: String; ValueName: String; Expected: String): Boolean;
var
  Actual: String;
begin
  Result := RegQueryStringValue(RootKey, Subkey, ValueName, Actual) and (Actual = Expected);
  if not Result then
  begin
    Log('Kopeeer validation failed for registry value: ' + Subkey + ' [' + ValueName + '] expected=' + Expected + ' actual=' + Actual);
  end;
end;

function ValidateShellIntegration(): Boolean;
var
  AppPath: String;
  ShellPath: String;
begin
  AppPath := ExpandConstant('{app}\{#AppExeName}');
  ShellPath := ExpandConstant('{app}\{#ShellExtensionRelativeDir}\{#ShellExtensionName}');

  Result :=
    FileExists(AppPath) and
    FileExists(ShellPath) and
    IsExpectedRegValue(HKLM64, 'Software\Classes\CLSID\{#DragDropMenuClassId}', 'AppPath', AppPath) and
    IsExpectedRegValue(HKLM64, 'Software\Classes\CLSID\{#DragDropMenuClassId}\InprocServer32', '', ShellPath) and
    IsExpectedRegValue(HKLM64, 'Software\Classes\CLSID\{#DragDropMenuClassId}\InprocServer32', 'ThreadingModel', 'Apartment') and
    IsExpectedRegValue(HKLM64, 'Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer', '', '{#DragDropMenuClassId}') and
    IsExpectedRegValue(HKLM64, 'Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer', '', '{#DragDropMenuClassId}') and
    IsExpectedRegValue(HKLM64, 'Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer', '', '{#DragDropMenuClassId}') and
    IsExpectedRegValue(HKLM64, 'Software\Classes\*\shell\Kopeeer.CopyWith', 'MUIVerb', 'Copy with Kopeeer...') and
    IsExpectedRegValue(HKLM64, 'Software\Classes\*\shell\Kopeeer.MoveWith', 'MUIVerb', 'Move with Kopeeer...');

  if not FileExists(AppPath) then
  begin
    Log('Kopeeer validation failed: app executable missing: ' + AppPath);
  end;

  if not FileExists(ShellPath) then
  begin
    Log('Kopeeer validation failed: shell extension missing: ' + ShellPath);
  end;
end;

procedure RefreshExplorerShell();
begin
  SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, 0, 0);
end;

procedure RemoveShellIntegrationRegistration();
begin
  Log('Removing existing Kopeeer shell integration registration before file changes.');
  RegDeleteKeyIncludingSubkeys(HKLM64, 'Software\Classes\*\shell\Kopeeer.CopyWith');
  RegDeleteKeyIncludingSubkeys(HKLM64, 'Software\Classes\*\shell\Kopeeer.MoveWith');
  RegDeleteKeyIncludingSubkeys(HKLM64, 'Software\Classes\Directory\shell\Kopeeer.CopyWith');
  RegDeleteKeyIncludingSubkeys(HKLM64, 'Software\Classes\Directory\shell\Kopeeer.MoveWith');
  RegDeleteKeyIncludingSubkeys(HKLM64, 'Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer');
  RegDeleteKeyIncludingSubkeys(HKLM64, 'Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer');
  RegDeleteKeyIncludingSubkeys(HKLM64, 'Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer');
  RegDeleteKeyIncludingSubkeys(HKLM64, 'Software\Classes\CLSID\{#DragDropMenuClassId}');
  RegDeleteValue(HKLM64, 'Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved', '{#DragDropMenuClassId}');
  RefreshExplorerShell();
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    RemoveShellIntegrationRegistration();
  end
  else if CurStep = ssPostInstall then
  begin
    if not ValidateShellIntegration() then
    begin
      MsgBox('Kopeeer was installed, but Explorer integration could not be verified. The installation will stop so this can be fixed instead of silently leaving Kopeeer unusable.', mbError, MB_OK);
      RaiseException('Kopeeer Explorer integration validation failed.');
    end;

    RefreshExplorerShell();
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    RemoveShellIntegrationRegistration();
  end
  else if CurUninstallStep = usPostUninstall then
  begin
    RefreshExplorerShell();
  end;
end;
