param(
    [string]$AppExePath
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$debugExe = Join-Path $repoRoot "src\Kopeeer.App\bin\Debug\net8.0-windows\Kopeeer.App.exe"
$releaseExe = Join-Path $repoRoot "src\Kopeeer.App\bin\Release\net8.0-windows\Kopeeer.App.exe"

if ([string]::IsNullOrWhiteSpace($AppExePath)) {
    if (Test-Path -LiteralPath $releaseExe) {
        $AppExePath = $releaseExe
    } elseif (Test-Path -LiteralPath $debugExe) {
        $AppExePath = $debugExe
    } else {
        throw "Kopeeer.App.exe was not found. Run 'dotnet build src\Kopeeer.App\Kopeeer.App.csproj' first, or pass -AppExePath."
    }
}

$AppExePath = (Resolve-Path -LiteralPath $AppExePath).Path

function Add-RegistryValue {
    param(
        [string]$Key,
        [string]$Name,
        [string]$Value
    )

    if ($Name -eq "") {
        & reg.exe add $Key /ve /t REG_SZ /d $Value /f | Out-Null
    } else {
        & reg.exe add $Key /v $Name /t REG_SZ /d $Value /f | Out-Null
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to write registry value '$Name' at '$Key'."
    }
}

function Set-KopeeerVerb {
    param(
        [string]$Key,
        [string]$Label,
        [string]$Operation
    )

    $commandKey = "$Key\command"
    $command = "`"$AppExePath`" --enqueue --operation $Operation --pick-target --sources `"%1`""

    Add-RegistryValue $Key "MUIVerb" $Label
    Add-RegistryValue $Key "Icon" $AppExePath
    Add-RegistryValue $commandKey "" $command
}

Set-KopeeerVerb "HKCU\Software\Classes\*\shell\Kopeeer.CopyWith" "Copy with Kopeeer..." "copy"
Set-KopeeerVerb "HKCU\Software\Classes\*\shell\Kopeeer.MoveWith" "Move with Kopeeer..." "move"
Set-KopeeerVerb "HKCU\Software\Classes\Directory\shell\Kopeeer.CopyWith" "Copy with Kopeeer..." "copy"
Set-KopeeerVerb "HKCU\Software\Classes\Directory\shell\Kopeeer.MoveWith" "Move with Kopeeer..." "move"

Write-Host "Kopeeer context menu entries registered for the current user."
Write-Host "Executable: $AppExePath"
Write-Host ""
Write-Host "Registry paths:"
$registeredKeys = @(
    "HKCU\Software\Classes\*\shell\Kopeeer.CopyWith",
    "HKCU\Software\Classes\*\shell\Kopeeer.MoveWith",
    "HKCU\Software\Classes\Directory\shell\Kopeeer.CopyWith",
    "HKCU\Software\Classes\Directory\shell\Kopeeer.MoveWith"
)

foreach ($key in $registeredKeys) {
    Write-Host "  $key"
}

Write-Host ""
Write-Host "Registered commands:"
foreach ($key in $registeredKeys) {
    & reg.exe query "$key\command" /ve
}

