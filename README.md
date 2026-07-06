# Kopeeer

Kopeeer is a small Windows transfer queue for Explorer copy and move jobs.

It lets you right-drag files or folders onto a destination folder, choose `Copy with Kopeeer` or `Move with Kopeeer`, and then processes the jobs one after another in a compact transfer window.

Status: `0.3.0-alpha`. Use test files first.

## Install

Download the latest installer from the GitHub Releases page:

```text
Kopeeer-Setup-0.3.0-alpha.exe
```

Run the installer and keep `Add Explorer context menu commands` enabled.

Source code ZIP files do not contain the installer. Use the release asset instead.

The installer asks for administrator approval because Windows Explorer loads the right-drag shell extension reliably when it is registered machine-wide.

## What Works

- Explorer right-drag menu commands:
  - `Copy with Kopeeer`
  - `Move with Kopeeer`
- Classic right-click fallback commands for files and folders.
- Sequential copy/move queue.
- Compact transfer window with:
  - current file name
  - overall progress bar
  - transfer speed
  - upcoming job list
  - copy/move status per job
- Windows dark-mode aware UI.
- Optional `Shut down when done`.
- Existing target files and folders are not overwritten silently.
- Self-contained Windows build; no separate .NET runtime is required for the installed app.

## Use

Preferred workflow:

1. In Explorer, drag one or more files or folders with the right mouse button.
2. Drop them onto a target folder.
3. Choose `Copy with Kopeeer` or `Move with Kopeeer`.
4. Watch progress in the small Kopeeer transfer window.

Fallback workflow:

1. Right-click a file or folder.
2. Choose `Copy with Kopeeer...` or `Move with Kopeeer...`.
3. Pick a destination folder.

## Test Safely

Use throwaway files first:

```powershell
mkdir C:\Temp\KopeeerTest
mkdir C:\Temp\KopeeerTest\Source
mkdir C:\Temp\KopeeerTest\Target
"hello" | Set-Content C:\Temp\KopeeerTest\Source\example.txt
```

Then right-drag `example.txt` onto `Target` and choose `Copy with Kopeeer`.

## Build From Source

Requirements:

- Windows 10 or Windows 11, 64-bit.
- .NET 8 SDK or newer.
- Visual Studio Build Tools with the C++ desktop workload.
- Inno Setup 6.

Build:

```powershell
dotnet build
```

Build installer:

```powershell
scripts\build-installer.ps1
```

Expected output:

```text
artifacts\installer\Kopeeer-Setup-0.3.0-alpha.exe
```

## Repository Shape

- `src/Kopeeer.App` - Windows transfer window.
- `src/Kopeeer.Core` - queue model.
- `src/Kopeeer.Worker` - sequential copy/move worker.
- `native/Kopeeer.ShellExtension` - native Explorer right-drag shell extension.
- `installer/inno` - Inno Setup installer.
- `scripts` - build, registration, and test helpers.
- `docs` - architecture and Windows integration notes.

## Notes

Kopeeer is intentionally small. It is not a file manager, cloud sync tool, backup app, clipboard manager, or TeraCopy clone.

The current Explorer integration is based on a native shell extension. Early experiments showed that left-drag modifier interception was not reliable on the tested Windows 11 Explorer path, while right-drag menu integration worked.
