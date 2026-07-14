param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0.1",
    [bool]$SelfContained = $true,
    [string]$InnoCompilerPath
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot "src\Kopeeer.App\Kopeeer.App.csproj"
$dropHandlerProjectPath = Join-Path $repoRoot "native\Kopeeer.ShellExtension\Kopeeer.ShellExtension.vcxproj"
$publishDir = Join-Path $repoRoot "artifacts\publish\Kopeeer.App"
$installerDir = Join-Path $repoRoot "artifacts\installer"
$dropHandlerDir = Join-Path $repoRoot "artifacts\publish\Kopeeer.Shell"
$innoScript = Join-Path $repoRoot "installer\inno\Kopeeer.iss"
$selfContainedValue = $SelfContained.ToString().ToLowerInvariant()
$shellExtensionBuildId = $null

try {
    $shellExtensionBuildId = (& git -C $repoRoot rev-parse --short=12 HEAD 2>$null).Trim()
} catch {
    $shellExtensionBuildId = $null
}

if ([string]::IsNullOrWhiteSpace($shellExtensionBuildId)) {
    $shellExtensionBuildId = Get-Date -Format "yyyyMMddHHmmss"
}

$shellExtensionBuildId = $shellExtensionBuildId -replace "[^A-Za-z0-9_.-]", "-"

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
        "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe",
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

function Find-MSBuild {
    $pathCommand = Get-Command "MSBuild.exe" -ErrorAction SilentlyContinue
    if ($pathCommand) {
        return $pathCommand.Source
    }

    $candidates = @(
        "C:\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe",
        "C:\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    throw "MSBuild.exe was not found. Install Visual Studio Build Tools with the C++ workload."
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
New-Item -ItemType Directory -Force -Path $dropHandlerDir | Out-Null

Write-Host "Publishing Kopeeer.App..."
dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained $selfContainedValue `
    --output $publishDir

Write-Host "Building Kopeeer shell extension..."
$msbuild = Find-MSBuild
& $msbuild $dropHandlerProjectPath `
    /m `
    /p:Configuration=$Configuration `
    /p:Platform=x64 `
    /p:OutDir="$dropHandlerDir\\" `
    /p:IntDir="$repoRoot\native\Kopeeer.ShellExtension\obj\x64\$Configuration\\"

if ($LASTEXITCODE -ne 0) {
    throw "MSBuild failed with exit code $LASTEXITCODE."
}

Write-Host "Building installer with Inno Setup..."
& $innoCompiler `
    "/DAppVersion=$Version" `
    "/DShellExtensionBuildId=$shellExtensionBuildId" `
    "/DPublishDir=$publishDir" `
    "/DDropHandlerDir=$dropHandlerDir" `
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
