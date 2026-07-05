param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64",
    [string]$MSBuildPath
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot "native\Kopeeer.ShellExtension\Kopeeer.ShellExtension.vcxproj"

function Find-MSBuild {
    param([string]$ExplicitPath)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        if (Test-Path -LiteralPath $ExplicitPath) {
            return (Resolve-Path -LiteralPath $ExplicitPath).Path
        }

        throw "MSBuild was not found at: $ExplicitPath"
    }

    $pathCommand = Get-Command "MSBuild.exe" -ErrorAction SilentlyContinue
    if ($pathCommand) {
        return $pathCommand.Source
    }

    $candidates = @(
        "C:\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe",
        "C:\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    throw "MSBuild.exe was not found. Install Visual Studio Build Tools with the C++ workload."
}

$msbuild = Find-MSBuild $MSBuildPath

Write-Host "Building Kopeeer.ShellExtension..."
& $msbuild $projectPath `
    /m `
    /p:Configuration=$Configuration `
    /p:Platform=$Platform `
    /p:OutDir="$repoRoot\native\Kopeeer.ShellExtension\bin\$Platform\$Configuration\\" `
    /p:IntDir="$repoRoot\native\Kopeeer.ShellExtension\obj\$Platform\$Configuration\\"

if ($LASTEXITCODE -ne 0) {
    throw "MSBuild failed with exit code $LASTEXITCODE."
}

$dllPath = Join-Path $repoRoot "native\Kopeeer.ShellExtension\bin\$Platform\$Configuration\Kopeeer.ShellExtension.dll"
if (-not (Test-Path -LiteralPath $dllPath)) {
    throw "Build finished, but expected output was not found: $dllPath"
}

Write-Host "Shell extension built:"
Write-Host $dllPath
