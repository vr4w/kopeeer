$ErrorActionPreference = "Stop"

$dropHandlerClassId = "{6A8E31AF-39B4-4B10-88AD-8F2C1CFB95D4}"
$copyHookClassId = "{E9C3BBA3-CC7C-49E3-9A6D-0D76F0956602}"
$dragDropMenuClassId = "{A9D60874-04A4-4962-8798-69D186A6E5E6}"
$dropHandlerClassRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\CLSID\$dropHandlerClassId"
$copyHookClassRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\CLSID\$copyHookClassId"
$dragDropMenuClassRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\CLSID\$dragDropMenuClassId"
$dropHandlerRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\shellex\DropHandler"
$copyHookRoot = "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\shellex\CopyHookHandlers\Kopeeer"
$dragDropMenuRoots = @(
    "Registry::HKEY_CURRENT_USER\Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer",
    "Registry::HKEY_CURRENT_USER\Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer",
    "Registry::HKEY_CURRENT_USER\Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer"
)
$approvedRoot = "Registry::HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"
$backupRoot = "Registry::HKEY_CURRENT_USER\Software\Kopeeer\ShellExtension"
$previousDropHandler = $null

if (Test-Path -LiteralPath $backupRoot) {
    $previousDropHandler = (Get-ItemProperty -LiteralPath $backupRoot -Name "PreviousDirectoryDropHandler" -ErrorAction SilentlyContinue).PreviousDirectoryDropHandler
}

if (Test-Path -LiteralPath $dropHandlerRoot) {
    $current = (Get-Item -LiteralPath $dropHandlerRoot).GetValue("")
    if ($current -eq $dropHandlerClassId) {
        if ([string]::IsNullOrWhiteSpace($previousDropHandler)) {
            Remove-Item -LiteralPath $dropHandlerRoot -Recurse -Force
            Write-Host "Removed current-user Directory drop handler registration."
        } else {
            Set-Item -Path $dropHandlerRoot -Value $previousDropHandler
            Write-Host "Restored previous current-user Directory drop handler: $previousDropHandler"
        }
    } else {
        Write-Host "Directory drop handler is not Kopeeer; left untouched."
    }
} else {
    Write-Host "No current-user Directory drop handler registration found."
}

if (Test-Path -LiteralPath $copyHookRoot) {
    $currentCopyHook = (Get-Item -LiteralPath $copyHookRoot).GetValue("")
    if ($currentCopyHook -eq $copyHookClassId) {
        Remove-Item -LiteralPath $copyHookRoot -Recurse -Force
        Write-Host "Removed Kopeeer current-user Directory copy hook registration."
    }
}

foreach ($dragDropMenuRoot in $dragDropMenuRoots) {
    if (Test-Path -LiteralPath $dragDropMenuRoot) {
        $currentDragDropMenu = (Get-Item -LiteralPath $dragDropMenuRoot).GetValue("")
        if ($currentDragDropMenu -eq $dragDropMenuClassId) {
            Remove-Item -LiteralPath $dragDropMenuRoot -Recurse -Force
            Write-Host "Removed Kopeeer current-user right-drag menu registration: $dragDropMenuRoot"
        }
    }
}

if (Test-Path -LiteralPath $approvedRoot) {
    Remove-ItemProperty -Path $approvedRoot -Name $dropHandlerClassId -ErrorAction SilentlyContinue
    Remove-ItemProperty -Path $approvedRoot -Name $copyHookClassId -ErrorAction SilentlyContinue
    Remove-ItemProperty -Path $approvedRoot -Name $dragDropMenuClassId -ErrorAction SilentlyContinue
}

$machineRoots = @(
    "Registry::HKEY_LOCAL_MACHINE\Software\Classes\Directory\shellex\DragDropHandlers\Kopeeer",
    "Registry::HKEY_LOCAL_MACHINE\Software\Classes\Folder\shellex\DragDropHandlers\Kopeeer",
    "Registry::HKEY_LOCAL_MACHINE\Software\Classes\Drive\shellex\DragDropHandlers\Kopeeer",
    "Registry::HKEY_LOCAL_MACHINE\Software\Classes\Directory\shellex\CopyHookHandlers\Kopeeer"
)

foreach ($machineRoot in $machineRoots) {
    if (Test-Path -LiteralPath $machineRoot) {
        Remove-Item -LiteralPath $machineRoot -Recurse -Force
        Write-Host "Removed machine-wide Kopeeer shell registration: $machineRoot"
    }
}

$machineClassRoots = @(
    "Registry::HKEY_LOCAL_MACHINE\Software\Classes\CLSID\$dropHandlerClassId",
    "Registry::HKEY_LOCAL_MACHINE\Software\Classes\CLSID\$copyHookClassId",
    "Registry::HKEY_LOCAL_MACHINE\Software\Classes\CLSID\$dragDropMenuClassId"
)

foreach ($machineClassRoot in $machineClassRoots) {
    if (Test-Path -LiteralPath $machineClassRoot) {
        Remove-Item -LiteralPath $machineClassRoot -Recurse -Force
        Write-Host "Removed machine-wide Kopeeer COM class registration: $machineClassRoot"
    }
}

$machineApprovedRoot = "Registry::HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"
if (Test-Path -LiteralPath $machineApprovedRoot) {
    Remove-ItemProperty -Path $machineApprovedRoot -Name $dropHandlerClassId -ErrorAction SilentlyContinue
    Remove-ItemProperty -Path $machineApprovedRoot -Name $copyHookClassId -ErrorAction SilentlyContinue
    Remove-ItemProperty -Path $machineApprovedRoot -Name $dragDropMenuClassId -ErrorAction SilentlyContinue
}

if (Test-Path -LiteralPath $dropHandlerClassRoot) {
    Remove-Item -LiteralPath $dropHandlerClassRoot -Recurse -Force
    Write-Host "Removed Kopeeer drop handler COM class registration."
}

if (Test-Path -LiteralPath $copyHookClassRoot) {
    Remove-Item -LiteralPath $copyHookClassRoot -Recurse -Force
    Write-Host "Removed Kopeeer copy hook COM class registration."
}

if (Test-Path -LiteralPath $dragDropMenuClassRoot) {
    Remove-Item -LiteralPath $dragDropMenuClassRoot -Recurse -Force
    Write-Host "Removed Kopeeer right-drag menu COM class registration."
}

if (Test-Path -LiteralPath $backupRoot) {
    Remove-Item -LiteralPath $backupRoot -Recurse -Force
}

Write-Host "Kopeeer shell extension unregistered."
