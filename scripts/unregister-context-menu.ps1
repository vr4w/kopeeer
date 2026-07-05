$ErrorActionPreference = "Stop"

$keys = @(
    "HKCU\Software\Classes\*\shell\Kopeeer.CopyWith",
    "HKCU\Software\Classes\*\shell\Kopeeer.MoveWith",
    "HKCU\Software\Classes\Directory\shell\Kopeeer.CopyWith",
    "HKCU\Software\Classes\Directory\shell\Kopeeer.MoveWith"
)

foreach ($key in $keys) {
    & reg.exe delete $key /f 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Removed $key"
    } else {
        Write-Host "Not present: $key"
    }
}

Write-Host "Kopeeer context menu entries removed for the current user."

