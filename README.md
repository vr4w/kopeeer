# Kopeeer

A calm copy and move queue for Windows Explorer.

Kopeeer is an early alpha Windows utility for people who want file operations to wait their turn. Add a copy or move job from Explorer, choose the target folder, and Kopeeer processes the queue one job at a time.

Status: `0.2.0-alpha`. Use test files first. Kopeeer is not ready for production data yet.

No cloud sync. No telemetry. No ads.

## Install

1. Open the latest GitHub Release.
2. Download `Kopeeer-Setup-*.exe`.
3. Run the installer.
4. Keep `Add Explorer context menu commands` enabled.
5. Right-click a file or folder in Explorer.
6. Choose `Copy with Kopeeer...` or `Move with Kopeeer...`.
7. Pick the target folder.

Kopeeer opens, adds the job to the queue, and starts processing pending jobs sequentially.

## Current Alpha Behavior

What works now:

- Current-user installer without admin rights.
- Explorer context menu entries for files and folders.
- Copy and move jobs.
- Target folder picker.
- Sequential queue processing.
- Safe conflict behavior: existing targets are not overwritten silently.
- Basic log file at `logs\kopeeer.log`.
- Single running Kopeeer window for Explorer enqueue requests.

What is still not finished:

- No production-ready Shell Extension yet.
- No COM registration yet.
- No automatic drag-and-drop interception yet.
- No `SHIFT`/modifier drop workflow yet.
- No signed public installer yet.

## Intended Workflow

The product goal is still the original one: Explorer-first file operations without a complex transfer manager.

The finished version should let a user copy or move files from Explorer into a calm queue with as little UI as possible. The context menu flow is the current safe alpha step. The deeper drag-and-drop workflow needs a native Explorer integration spike before it can be shipped responsibly.

Kopeeer is not trying to become TeraCopy, a clipboard manager, a file sharing app, cloud sync, backup software, or a file manager.

## Build From Source

Requirements:

- Windows 10 or Windows 11.
- .NET 8 SDK or newer.
- Inno Setup 6 for installer builds.

Build:

```powershell
dotnet restore
dotnet build
```

Run:

```powershell
dotnet run --project src\Kopeeer.App\Kopeeer.App.csproj
```

Build installer:

```powershell
scripts\build-installer.ps1
```

Expected installer output:

```text
artifacts\installer\Kopeeer-Setup-0.2.0-alpha.exe
```

## Test Safely

Create a throwaway test area:

```powershell
mkdir C:\Temp\KopeeerTest
mkdir C:\Temp\KopeeerTest\Source
mkdir C:\Temp\KopeeerTest\Target
"hello" | Set-Content C:\Temp\KopeeerTest\Source\example.txt
```

Then use Explorer:

1. Right-click `C:\Temp\KopeeerTest\Source\example.txt`.
2. Choose `Copy with Kopeeer...`.
3. Pick `C:\Temp\KopeeerTest\Target`.
4. Confirm the file appears in the target folder.
5. Repeat the same job to confirm conflict handling fails safely.

## Repository Shape

- `src/Kopeeer.App` - Windows desktop app.
- `src/Kopeeer.Core` - queue model.
- `src/Kopeeer.Worker` - sequential file operation worker.
- `installer/inno` - alpha installer definition.
- `scripts` - local build, run, installer, and context menu helper scripts.
- `docs` - architecture, Windows integration, branding, installer, and localization notes.

## Project Notes

The repository codename is `file-operation-queue`. The current working product name is `Kopeeer`, but branding should remain easy to rename until the public name is legally cleared.

See:

- [docs/architecture.md](docs/architecture.md)
- [docs/architecture-decision.md](docs/architecture-decision.md)
- [docs/windows-integration.md](docs/windows-integration.md)
- [docs/drag-drop-explorer-hook.md](docs/drag-drop-explorer-hook.md)
