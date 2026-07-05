param(
    [Parameter(Mandatory = $true)]
    [string]$AppExePath
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $AppExePath)) {
    throw "App executable was not found: $AppExePath"
}

function Set-KopeeerVerb {
    param(
        [string]$BasePath,
        [string]$Verb,
        [string]$Label,
        [string]$Argument
    )

    $keyPath = Join-Path $BasePath $Verb
    $commandPath = Join-Path $keyPath "command"

    New-Item -Path $keyPath -Force | Out-Null
    New-Item -Path $commandPath -Force | Out-Null

    New-ItemProperty -Path $keyPath -Name "MUIVerb" -Value $Label -PropertyType String -Force | Out-Null
    New-ItemProperty -Path $keyPath -Name "Icon" -Value $AppExePath -PropertyType String -Force | Out-Null
    Set-Item -Path $commandPath -Value "`"$AppExePath`" $Argument `"%1`""
}

Set-KopeeerVerb "HKCU:\Software\Classes\*\shell" "Kopeeer.CopyWith" "Copy with Kopeeer..." "--queue-copy"
Set-KopeeerVerb "HKCU:\Software\Classes\*\shell" "Kopeeer.MoveWith" "Move with Kopeeer..." "--queue-move"
Set-KopeeerVerb "HKCU:\Software\Classes\Directory\shell" "Kopeeer.CopyWith" "Copy with Kopeeer..." "--queue-copy"
Set-KopeeerVerb "HKCU:\Software\Classes\Directory\shell" "Kopeeer.MoveWith" "Move with Kopeeer..." "--queue-move"

Write-Host "Kopeeer context menu entries installed for current user."
