param(
    [string]$InstallDir
)

$ErrorActionPreference = "Stop"

$dragDropMenuClassId = "{A9D60874-04A4-4962-8798-69D186A6E5E6}"

function Assert-Admin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Repair must be run from an elevated PowerShell session."
    }
}

function Get-RegValue {
    param(
        [string]$Key,
        [string]$Name = ""
    )

    $arguments = @("query", $Key, "/reg:64")
    if ($Name -eq "") {
        $arguments += "/ve"
    } else {
        $arguments += @("/v", $Name)
    }

    $output = & reg.exe @arguments 2>$null
    if ($LASTEXITCODE -ne 0) {
        return $null
    }

    foreach ($line in $output) {
        if ($line -match 'REG_\w+\s+(.+)$') {
            return $Matches[1].Trim()
        }
    }

    return $null
}

function Add-RegValue {
    param(
        [string]$Key,
        [string]$Name,
        [string]$Value
    )

    if ($Name -eq "") {
        & reg.exe add $Key /ve /t REG_SZ /d $Value /f /reg:64 | Out-Null
    } else {
        & reg.exe add $Key /v $Name /t REG_SZ /d $Value /f /reg:64 | Out-Null
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to write registry value '$Name' at '$Key'."
    }
}

function Resolve-InstallDir {
    param([string]$ExplicitInstallDir)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitInstallDir)) {
        return $ExplicitInstallDir
    }

    $registeredInstallDir = Get-RegValue "HKLM\Software\Kopeeer" "InstallDir"
    if (-not [string]::IsNullOrWhiteSpace($registeredInstallDir)) {
        return $registeredInstallDir
    }

    $scriptRootInstallDir = Resolve-Path (Join-Path $PSScriptRoot "..") -ErrorAction SilentlyContinue
    if ($scriptRootInstallDir -and (Test-Path -LiteralPath (Join-Path $scriptRootInstallDir "Kopeeer.App.exe"))) {
        return $scriptRootInstallDir.Path
    }

    return Join-Path $env:ProgramFiles "Kopeeer"
}

function Resolve-ShellExtensionPath {
    param(
        [string]$InstallDir,
        [string]$Version
    )

    $registeredShellPath = Get-RegValue "HKLM\Software\Kopeeer" "ShellExtensionPath"
    if (-not [string]::IsNullOrWhiteSpace($registeredShellPath) -and (Test-Path -LiteralPath $registeredShellPath)) {
        return $registeredShellPath
    }

    $shellRoot = Join-Path $InstallDir "Shell"
    if (Test-Path -LiteralPath $shellRoot) {
        $candidate = Get-ChildItem -LiteralPath $shellRoot -Filter "Kopeeer.ShellExtension.dll" -Recurse -File -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTimeUtc -Descending |
            Select-Object -First 1
        if ($candidate) {
            return $candidate.FullName
        }
    }

    return Join-Path $InstallDir "Shell\$Version\Kopeeer.ShellExtension.dll"
}

function Invoke-ShellRefresh {
    Add-Type -Namespace Kopeeer -Name ShellNotify -MemberDefinition @"
        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        public static extern void SHChangeNotify(int wEventId, uint uFlags, System.IntPtr dwItem1, System.IntPtr dwItem2);
"@
    [Kopeeer.ShellNotify]::SHChangeNotify(0x08000000, 0, [IntPtr]::Zero, [IntPtr]::Zero)
}

Assert-Admin

$InstallDir = Resolve-InstallDir $InstallDir
$appPath = Join-Path $InstallDir "Kopeeer.App.exe"
$version = Get-RegValue "HKLM\Software\Kopeeer" "Version"
if ([string]::IsNullOrWhiteSpace($version)) {
    $version = "1.0.1"
}

$shellPath = Resolve-ShellExtensionPath $InstallDir $version
if (-not (Test-Path -LiteralPath $appPath)) {
    throw "Kopeeer app executable was not found: $appPath"
}

if (-not (Test-Path -LiteralPath $shellPath)) {
    throw "Kopeeer shell extension DLL was not found: $shellPath"
}

Write-Host "Repairing Kopeeer shell integration..."
Write-Host "App: $appPath"
Write-Host "Shell extension: $shellPath"

Add-RegValue "HKLM\Software\Kopeeer" "InstallDir" $InstallDir
Add-RegValue "HKLM\Software\Kopeeer" "AppPath" $appPath
Add-RegValue "HKLM\Software\Kopeeer" "ShellExtensionPath" $shellPath
Add-RegValue "HKLM\Software\Kopeeer" "Version" $version
Add-RegValue "HKLM\Software\Microsoft\Windows\CurrentVersion\App Paths\Kopeeer.App.exe" "" $appPath
Add-RegValue "HKLM\Software\Microsoft\Windows\CurrentVersion\App Paths\Kopeeer.App.exe" "Path" $InstallDir

Add-RegValue "HKLM\Software\Classes\*\shell\Kopeeer.CopyWith" "MUIVerb" "Copy with Kopeeer..."
Add-RegValue "HKLM\Software\Classes\*\shell\Kopeeer.CopyWith" "Icon" (Join-Path $InstallDir "Assets\copy.ico")
Add-RegValue "HKLM\Software\Classes\*\shell\Kopeeer.CopyWith\command" "" "`"$appPath`" --enqueue --operation copy --pick-target --sources `"%1`""

Add-RegValue "HKLM\Software\Classes\*\shell\Kopeeer.MoveWith" "MUIVerb" "Move with Kopeeer..."
Add-RegValue "HKLM\Software\Classes\*\shell\Kopeeer.MoveWith" "Icon" (Join-Path $InstallDir "Assets\cut.ico")
Add-RegValue "HKLM\Software\Classes\*\shell\Kopeeer.MoveWith\command" "" "`"$appPath`" --enqueue --operation move --pick-target --sources `"%1`""

Add-RegValue "HKLM\Software\Classes\Directory\shell\Kopeeer.CopyWith" "MUIVerb" "Copy with Kopeeer..."
Add-RegValue "HKLM\Software\Classes\Directory\shell\Kopeeer.CopyWith" "Icon" (Join-Path $InstallDir "Assets\copy.ico")
Add-RegValue "HKLM\Software\Classes\Directory\shell\Kopeeer.CopyWith\command" "" "`"$appPath`" --enqueue --operation copy --pick-target --sources `"%1`""

Add-RegValue "HKLM\Software\Classes\Directory\shell\Kopeeer.MoveWith" "MUIVerb" "Move with Kopeeer..."
Add-RegValue "HKLM\Software\Classes\Directory\shell\Kopeeer.MoveWith" "Icon" (Join-Path $InstallDir "Assets\cut.ico")
Add-RegValue "HKLM\Software\Classes\Directory\shell\Kopeeer.MoveWith\command" "" "`"$appPath`" --enqueue --operation move --pick-target --sources `"%1`""

Add-RegValue "HKLM\Software\Classes\CLSID\$dragDropMenuClassId" "" "Kopeeer Right-Drag Menu"
Add-RegValue "HKLM\Software\Classes\CLSID\$dragDropMenuClassId" "AppPath" $appPath
Add-RegValue "HKLM\Software\Classes\CLSID\$dragDropMenuClassId\InprocServer32" "" $shellPath
Add-RegValue "HKLM\Software\Classes\CLSID\$dragDropMenuClassId\InprocServer32" "ThreadingModel" "Apartment"
Add-RegValue "HKLM\Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer" "" $dragDropMenuClassId
Add-RegValue "HKLM\Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer" "" $dragDropMenuClassId
Add-RegValue "HKLM\Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer" "" $dragDropMenuClassId
Add-RegValue "HKLM\Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" $dragDropMenuClassId "Kopeeer Right-Drag Menu"

Invoke-ShellRefresh

Write-Host "Repair complete. If Explorer still does not show Kopeeer, restart Explorer or sign out and back in once."
