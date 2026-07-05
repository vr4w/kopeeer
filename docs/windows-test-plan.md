# Windows Test Plan

Use this plan on a Windows 10 or Windows 11 laptop before treating Explorer integration as real.

## Prerequisites

- Windows 10 or Windows 11, 64-bit.
- .NET 8 SDK.
- Git.
- Optional later: Inno Setup.

## First Build Check

From the repository root:

```powershell
dotnet build src\FileOperationQueue.Core\FileOperationQueue.Core.csproj
dotnet build src\FileOperationQueue.App\FileOperationQueue.App.csproj
python tools\validate_project.py
```

Expected result:

- Core builds.
- App builds.
- Structure validation passes.

## Tray App Check

Run:

```powershell
dotnet run --project src\FileOperationQueue.App\FileOperationQueue.App.csproj
```

Expected result:

- Kopeeer tray icon appears.
- Double-click opens the queue window.
- Refresh works.
- Exit closes the tray app.

## Context Menu Dev Check

Publish the app:

```powershell
dotnet publish src\FileOperationQueue.App\FileOperationQueue.App.csproj -c Release -o artifacts\publish\FileOperationQueue.App
```

Install current-user context menu entries:

```powershell
powershell -ExecutionPolicy Bypass -File tools\windows\install-context-menu-dev.ps1 -AppExePath "$PWD\artifacts\publish\FileOperationQueue.App\FileOperationQueue.App.exe"
```

Test:

- Right-click a file.
- Choose "Copy with Kopeeer..."
- Select a destination folder.
- Open Kopeeer.
- Confirm the job appears in the queue.

Repeat for:

- "Move with Kopeeer..."
- A folder.
- A path with spaces.

Uninstall dev context menu entries:

```powershell
powershell -ExecutionPolicy Bypass -File tools\windows\uninstall-context-menu-dev.ps1
```

## Drag-and-drop Hook Check

Do not test this until a native hook prototype exists.

See [drag-drop-explorer-hook.md](drag-drop-explorer-hook.md) for the intended behavior and checklist.

