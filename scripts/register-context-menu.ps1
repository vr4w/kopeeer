$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$defaultExe = Join-Path $repoRoot "src\Kopeeer.App\bin\Debug\net8.0-windows\Kopeeer.App.exe"
$releaseExe = Join-Path $repoRoot "src\Kopeeer.App\bin\Release\net8.0-windows\Kopeeer.App.exe"

if (Test-Path -LiteralPath $releaseExe) {
    $appExePath = $releaseExe
} elseif (Test-Path -LiteralPath $defaultExe) {
    $appExePath = $defaultExe
} else {
    throw "Kopeeer.App.exe was not found. Run 'dotnet build src\Kopeeer.App\Kopeeer.App.csproj' first."
}

function Set-KopeeerVerb {
    param(
        [string]$BasePath,
        [string]$Verb,
        [string]$Label,
        [string]$Operation
    )

    $keyPath = Join-Path $BasePath $Verb
    $commandPath = Join-Path $keyPath "command"

    New-Item -Path $keyPath -Force | Out-Null
    New-Item -Path $commandPath -Force | Out-Null

    New-ItemProperty -Path $keyPath -Name "MUIVerb" -Value $Label -PropertyType String -Force | Out-Null
    New-ItemProperty -Path $keyPath -Name "Icon" -Value $appExePath -PropertyType String -Force | Out-Null
    Set-Item -Path $commandPath -Value "`"$appExePath`" --enqueue --operation $Operation --pick-target --sources `"%1`""
}

Set-KopeeerVerb "HKCU:\Software\Classes\*\shell" "Kopeeer.CopyWith" "Copy with Kopeeer..." "copy"
Set-KopeeerVerb "HKCU:\Software\Classes\*\shell" "Kopeeer.MoveWith" "Move with Kopeeer..." "move"
Set-KopeeerVerb "HKCU:\Software\Classes\Directory\shell" "Kopeeer.CopyWith" "Copy with Kopeeer..." "copy"
Set-KopeeerVerb "HKCU:\Software\Classes\Directory\shell" "Kopeeer.MoveWith" "Move with Kopeeer..." "move"

Write-Host "Kopeeer context menu entries registered for the current user."
Write-Host "Executable: $appExePath"
Write-Host "Registry paths:"
Write-Host "  HKCU:\Software\Classes\*\shell\Kopeeer.CopyWith"
Write-Host "  HKCU:\Software\Classes\*\shell\Kopeeer.MoveWith"
Write-Host "  HKCU:\Software\Classes\Directory\shell\Kopeeer.CopyWith"
Write-Host "  HKCU:\Software\Classes\Directory\shell\Kopeeer.MoveWith"

