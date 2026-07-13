param(
    [string]$InstallDir
)

$ErrorActionPreference = "Stop"

$dragDropMenuClassId = "{A9D60874-04A4-4962-8798-69D186A6E5E6}"
$failures = New-Object System.Collections.Generic.List[string]

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

function Test-Check {
    param(
        [string]$Name,
        [bool]$Condition,
        [string]$Failure
    )

    if ($Condition) {
        Write-Host "PASS $Name"
        return
    }

    Write-Host "FAIL $Name - $Failure"
    $failures.Add($Failure)
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

    return Join-Path $env:ProgramFiles "Kopeeer"
}

$InstallDir = Resolve-InstallDir $InstallDir
$appPath = Join-Path $InstallDir "Kopeeer.App.exe"
$version = Get-RegValue "HKLM\Software\Kopeeer" "Version"
$shellPath = Get-RegValue "HKLM\Software\Kopeeer" "ShellExtensionPath"
if ([string]::IsNullOrWhiteSpace($shellPath)) {
    $shellPath = Join-Path $InstallDir "Shell\$version\Kopeeer.ShellExtension.dll"
}

Write-Host "Kopeeer installation diagnostics"
Write-Host "InstallDir: $InstallDir"
Write-Host "Version: $version"
Write-Host ""

Test-Check "App executable exists" (Test-Path -LiteralPath $appPath) "Missing app executable: $appPath"
Test-Check "Shell extension DLL exists" (Test-Path -LiteralPath $shellPath) "Missing shell extension DLL: $shellPath"
Test-Check "Installed version registered" (-not [string]::IsNullOrWhiteSpace($version)) "HKLM\Software\Kopeeer Version is missing"

$registeredAppPath = Get-RegValue "HKLM\Software\Classes\CLSID\$dragDropMenuClassId" "AppPath"
$registeredDllPath = Get-RegValue "HKLM\Software\Classes\CLSID\$dragDropMenuClassId\InprocServer32"
$threadingModel = Get-RegValue "HKLM\Software\Classes\CLSID\$dragDropMenuClassId\InprocServer32" "ThreadingModel"

Test-Check "CLSID AppPath registered" ($registeredAppPath -eq $appPath) "Expected AppPath '$appPath', got '$registeredAppPath'"
Test-Check "CLSID DLL path registered" ($registeredDllPath -eq $shellPath) "Expected DLL '$shellPath', got '$registeredDllPath'"
Test-Check "ThreadingModel is Apartment" ($threadingModel -eq "Apartment") "Expected Apartment, got '$threadingModel'"

$dragDropRoots = @(
    "HKLM\Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer",
    "HKLM\Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer",
    "HKLM\Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer"
)

foreach ($root in $dragDropRoots) {
    $value = Get-RegValue $root
    Test-Check "Right-drag handler $root" ($value -eq $dragDropMenuClassId) "Expected $dragDropMenuClassId at $root, got '$value'"
}

$fallbackEntries = @{
    "HKLM\Software\Classes\*\shell\Kopeeer.CopyWith" = "Copy with Kopeeer..."
    "HKLM\Software\Classes\*\shell\Kopeeer.MoveWith" = "Move with Kopeeer..."
    "HKLM\Software\Classes\Directory\shell\Kopeeer.CopyWith" = "Copy with Kopeeer..."
    "HKLM\Software\Classes\Directory\shell\Kopeeer.MoveWith" = "Move with Kopeeer..."
}

foreach ($entry in $fallbackEntries.GetEnumerator()) {
    $value = Get-RegValue $entry.Key "MUIVerb"
    Test-Check "Fallback entry $($entry.Key)" ($value -eq $entry.Value) "Expected '$($entry.Value)' at $($entry.Key), got '$value'"
}

Write-Host ""
if ($failures.Count -eq 0) {
    Write-Host "Kopeeer diagnostics passed."
    exit 0
}

Write-Host "Kopeeer diagnostics failed with $($failures.Count) issue(s)."
Write-Host "Run repair-shell-integration.ps1 from an elevated PowerShell to rewrite Kopeeer shell registration."
exit 1
