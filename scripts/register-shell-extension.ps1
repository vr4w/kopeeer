param(
    [string]$DllPath,
    [string]$AppExePath
)

$ErrorActionPreference = "Stop"

$dropHandlerClassId = "{6A8E31AF-39B4-4B10-88AD-8F2C1CFB95D4}"
$copyHookClassId = "{E9C3BBA3-CC7C-49E3-9A6D-0D76F0956602}"
$dragDropMenuClassId = "{A9D60874-04A4-4962-8798-69D186A6E5E6}"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

if ([string]::IsNullOrWhiteSpace($DllPath)) {
    $DllPath = Join-Path $repoRoot "native\Kopeeer.ShellExtension\bin\x64\Release\Kopeeer.ShellExtension.dll"
}

if ([string]::IsNullOrWhiteSpace($AppExePath)) {
    $AppExePath = Join-Path $repoRoot "artifacts\publish\Kopeeer.App\Kopeeer.App.exe"
}

$DllPath = (Resolve-Path -LiteralPath $DllPath).Path
$AppExePath = (Resolve-Path -LiteralPath $AppExePath).Path

$dropHandlerClassRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\CLSID\$dropHandlerClassId"
$dropHandlerInprocRoot = Join-Path $dropHandlerClassRoot "InprocServer32"
$copyHookClassRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\CLSID\$copyHookClassId"
$copyHookInprocRoot = Join-Path $copyHookClassRoot "InprocServer32"
$dragDropMenuClassRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\CLSID\$dragDropMenuClassId"
$dragDropMenuInprocRoot = Join-Path $dragDropMenuClassRoot "InprocServer32"
$dropHandlerRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\shellex\DropHandler"
$copyHookRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\shellex\CopyHookHandlers\Kopeeer"
$dragDropMenuRoots = @(
    "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer",
    "Registry::HKEY_CURRENT_USER\Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer",
    "Registry::HKEY_CURRENT_USER\Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer"
)
$approvedRoot = "Registry::HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"
$backupRoot = "Registry::HKEY_CURRENT_USER\Software\Kopeeer\ShellExtension"

New-Item -Path $dropHandlerClassRoot -Force | Out-Null
New-Item -Path $dropHandlerInprocRoot -Force | Out-Null
New-Item -Path $copyHookClassRoot -Force | Out-Null
New-Item -Path $copyHookInprocRoot -Force | Out-Null
New-Item -Path $dragDropMenuClassRoot -Force | Out-Null
New-Item -Path $dragDropMenuInprocRoot -Force | Out-Null
New-Item -Path $dropHandlerRoot -Force | Out-Null
New-Item -Path $copyHookRoot -Force | Out-Null
foreach ($dragDropMenuRoot in $dragDropMenuRoots) {
    New-Item -Path $dragDropMenuRoot -Force | Out-Null
}
New-Item -Path $approvedRoot -Force | Out-Null
New-Item -Path $backupRoot -Force | Out-Null

$existingDropHandler = (Get-Item -LiteralPath $dropHandlerRoot).GetValue("")
if (-not [string]::IsNullOrWhiteSpace($existingDropHandler) -and $existingDropHandler -ne $dropHandlerClassId) {
    Set-ItemProperty -Path $backupRoot -Name "PreviousDirectoryDropHandler" -Value $existingDropHandler
    Write-Host "Existing current-user Directory drop handler saved for restore: $existingDropHandler"
}

Set-Item -Path $dropHandlerClassRoot -Value "Kopeeer Drop Handler"
Set-ItemProperty -Path $dropHandlerClassRoot -Name "AppPath" -Value $AppExePath
Set-Item -Path $dropHandlerInprocRoot -Value $DllPath
Set-ItemProperty -Path $dropHandlerInprocRoot -Name "ThreadingModel" -Value "Apartment"
Set-Item -Path $dropHandlerRoot -Value $dropHandlerClassId

Set-Item -Path $copyHookClassRoot -Value "Kopeeer Copy Hook"
Set-ItemProperty -Path $copyHookClassRoot -Name "AppPath" -Value $AppExePath
Set-Item -Path $copyHookInprocRoot -Value $DllPath
Set-ItemProperty -Path $copyHookInprocRoot -Name "ThreadingModel" -Value "Apartment"
Set-Item -Path $copyHookRoot -Value $copyHookClassId

Set-Item -Path $dragDropMenuClassRoot -Value "Kopeeer Right-Drag Menu"
Set-ItemProperty -Path $dragDropMenuClassRoot -Name "AppPath" -Value $AppExePath
Set-Item -Path $dragDropMenuInprocRoot -Value $DllPath
Set-ItemProperty -Path $dragDropMenuInprocRoot -Name "ThreadingModel" -Value "Apartment"
foreach ($dragDropMenuRoot in $dragDropMenuRoots) {
    Set-Item -Path $dragDropMenuRoot -Value $dragDropMenuClassId
}

Set-ItemProperty -Path $approvedRoot -Name $dropHandlerClassId -Value "Kopeeer Drop Handler"
Set-ItemProperty -Path $approvedRoot -Name $copyHookClassId -Value "Kopeeer Copy Hook"
Set-ItemProperty -Path $approvedRoot -Name $dragDropMenuClassId -Value "Kopeeer Right-Drag Menu"

Write-Host "Kopeeer shell extension registered for current-user Explorer tests."
Write-Host "DropHandler ClassId: $dropHandlerClassId"
Write-Host "CopyHook ClassId: $copyHookClassId"
Write-Host "Right-drag menu ClassId: $dragDropMenuClassId"
Write-Host "DLL: $DllPath"
Write-Host "App: $AppExePath"
Write-Host "Right-drag menu registered for Directory, Folder, and Drive."
Write-Host ""
Write-Host "Explorer may need to be restarted, or you may need to sign out and back in."
Write-Host "Unregister with scripts\unregister-shell-extension.ps1"
