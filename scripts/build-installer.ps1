param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "0.2.0-alpha",
    [bool]$SelfContained = $true,
    [string]$InnoCompilerPath
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot "src\Kopeeer.App\Kopeeer.App.csproj"
$publishDir = Join-Path $repoRoot "artifacts\publish\Kopeeer.App"
$installerDir = Join-Path $repoRoot "artifacts\installer"
$innoScript = Join-Path $repoRoot "installer\inno\Kopeeer.iss"
$selfContainedValue = $SelfContained.ToString().ToLowerInvariant()

function Find-InnoCompiler {
    param([string]$ExplicitPath)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        if (Test-Path -LiteralPath $ExplicitPath) {
            return (Resolve-Path -LiteralPath $ExplicitPath).Path
        }

        throw "Inno Setup compiler was not found at: $ExplicitPath"
    }

    $pathCommand = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($pathCommand) {
        return $pathCommand.Source
    }

    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    return $null
}

$innoCompiler = Find-InnoCompiler $InnoCompilerPath
if ($null -eq $innoCompiler) {
    throw @"
Inno Setup compiler was not found.

Install Inno Setup 6 on Windows, then run this script again:
https://jrsoftware.org/isinfo.php

If ISCC.exe is installed in a custom location, pass:
scripts\build-installer.ps1 -InnoCompilerPath "C:\Path\To\ISCC.exe"
"@
}

New-Item -ItemType Directory -Force -Path $publishDir | Out-Null
New-Item -ItemType Directory -Force -Path $installerDir | Out-Null

Write-Host "Publishing Kopeeer.App..."
dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained $selfContainedValue `
    --output $publishDir

Write-Host "Building installer with Inno Setup..."
& $innoCompiler `
    "/DAppVersion=$Version" `
    "/DPublishDir=$publishDir" `
    "/DOutputDir=$installerDir" `
    $innoScript

if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup failed with exit code $LASTEXITCODE."
}

$installer = Join-Path $installerDir "Kopeeer-Setup-$Version.exe"
if (-not (Test-Path -LiteralPath $installer)) {
    throw "Installer build finished, but expected output was not found: $installer"
}

Write-Host "Installer created:"
Write-Host $installer
