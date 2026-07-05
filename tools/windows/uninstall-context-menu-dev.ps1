$ErrorActionPreference = "Stop"

$keys = @(
    "HKCU:\Software\Classes\*\shell\Kopeeer.CopyWith",
    "HKCU:\Software\Classes\*\shell\Kopeeer.MoveWith",
    "HKCU:\Software\Classes\Directory\shell\Kopeeer.CopyWith",
    "HKCU:\Software\Classes\Directory\shell\Kopeeer.MoveWith"
)

foreach ($key in $keys) {
    if (Test-Path $key) {
        Remove-Item -Path $key -Recurse -Force
    }
}

Write-Host "Kopeeer context menu entries removed for current user."

