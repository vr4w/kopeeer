# Windows Test Plan

Use this plan on a Windows 10 or Windows 11 laptop before treating Explorer integration as real.

## Prerequisites

- Windows 10 or Windows 11, 64-bit.
- .NET 8 SDK.
- Git.
- Inno Setup 6 for installer builds.

## First Build Check

From the repository root:

```powershell
dotnet restore
dotnet build
python tools\validate_project.py
```

Expected result:

- Core builds.
- App builds.
- Structure validation passes.

## Tray App Check

Run:

```powershell
dotnet run --project src\Kopeeer.App
```

Expected result:

- Kopeeer window appears.
- You can select a source file.
- You can select a source folder.
- You can select a target folder.
- You can choose Copy or Move.
- You can add a job to the queue.
- You can start the queue.
- Jobs run one at a time.
- `logs\kopeeer.log` is written.

The app should keep "Add to queue" disabled until both source and target are selected. "Start queue" should stay disabled until at least one pending job exists.

## Manual Copy/Move Checks

Create a temporary test folder with throwaway files. Do not use important files for the first test.

Check:

- Copy one file to an empty target folder.
- Copy one folder to an empty target folder.
- Move one file to an empty target folder.
- Move one folder to an empty target folder.
- Try copying to a target where the destination already exists.

Expected result:

- Existing targets are not overwritten.
- Failed jobs show an error message.
- The app does not crash.
- Status summary updates Pending, Running, Completed, and Failed counts.

## Context Menu Dev Check

This is not part of the first manual app test. Use it only after the basic app works.

Publish the app:

```powershell
dotnet publish src\Kopeeer.App\Kopeeer.App.csproj -c Release -o artifacts\publish\Kopeeer.App
```

Install current-user context menu entries:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\unregister-context-menu.ps1
powershell -ExecutionPolicy Bypass -File scripts\register-context-menu.ps1
```

Test:

- Right-click a file.
- Choose "Copy with Kopeeer..."
- Select a destination folder.
- Confirm Kopeeer opens automatically.
- Confirm the job appears in the queue.
- Confirm processing starts without pressing "Start queue".
- Confirm the menu entry shows the copy icon.

Repeat for:

- "Move with Kopeeer..."
- A folder.
- A path with spaces.
- A second Explorer request while Kopeeer is already open.

Expected result:

- The second request should go to the existing Kopeeer window.
- The move entry should show the cut icon.
- The installer and script registrations should both use the same labels and command behavior.

Uninstall dev context menu entries:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\unregister-context-menu.ps1
```

## Installer Check

Build the installer:

```powershell
scripts\build-installer.ps1
```

Run:

```powershell
artifacts\installer\Kopeeer-Setup-0.2.0-alpha.exe
```

Expected result:

- Installer finishes without admin rights.
- `Add Explorer context menu commands` is visible as an option.
- With the option enabled, copy and move entries appear in Explorer.
- Uninstall removes the installed app and the current-user context menu entries.

## Drag-and-drop Hook Check

Do not test this until a native hook prototype exists.

See [drag-drop-explorer-hook.md](drag-drop-explorer-hook.md) for the intended behavior and checklist.
