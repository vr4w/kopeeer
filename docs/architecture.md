# Architecture

Kopeeer is planned as a local Windows file-operation queue. The architecture should stay robust before it becomes clever.

The repository codename is `file-operation-queue`. `Kopeeer` is the current working product display name, not a namespace, package ID, executable name, protocol name, or installer identity.

## Recommended Architecture

Version 0.1 should use a mixed Windows-native architecture:

- Native Shell Extension: C++/Win32/COM, or a carefully evaluated alternative only if it proves stable inside Explorer.
- Main app / tray UI: .NET.
- Worker: .NET behind a narrow file-operation abstraction, with room for a native component if testing shows that is safer.
- File operations: evaluate `IFileOperation` first, with fallback strategies for cases where progress, cancellation, conflicts, or path handling need more control.
- Installer: evaluate WiX Toolset and Inno Setup before choosing the release packaging path.

The Shell Extension must stay thin. It should gather Explorer selection context, validate input, and hand work to the app or worker. It should not perform copy or move operations itself.

## Development Baseline

- .NET 8 for the queue core, local worker boundary, and future tray app.
- Windows 10/11 64-bit for Explorer integration and installer validation.
- No production Shell Extension code until the queue core and UI boundary are stable.

## Components

### Shell Extension

Purpose:

- Add explicit Explorer context menu commands.
- Extract selected items and target context.
- Send a local queue request to the app or worker.

Preferred direction:

- Native C++/Win32/COM.
- 64-bit Windows 10 and Windows 11.
- Minimal logic inside Explorer.

### Main App / Tray UI

Purpose:

- Show current job and pending jobs.
- Offer minimal queue controls.
- Hold settings.
- Explain status and errors in plain English.

Preferred direction:

- .NET desktop app.
- Local-only behavior.
- Centralized user-facing strings for future localization.

### Queue Core

Purpose:

- Define job model and states.
- Persist queue state locally.
- Keep internal state language-neutral.
- Provide a stable contract between UI and worker.

Current implementation:

- `FileOperationQueue.Core` contains the first queue model and local worker boundary.
- `OperationQueue` owns enqueue and status transitions.
- `JsonFileQueueStore` persists queue snapshots locally.
- `LocalQueueWorker` processes a single active job through an executor abstraction.
- No Shell Extension or Explorer code is included.

### Current Tray UI Scaffold

The first app project is `FileOperationQueue.App`.

Current behavior:

- Windows Forms app targeting `net8.0-windows`.
- Tray icon with show, refresh, and exit commands.
- Main window showing the local queue snapshot.
- Centralized UI strings in `UiText`.
- Display branding isolated in `ProductBranding`.

It intentionally does not execute file operations yet. The production worker must be connected only after the file-operation executor is real and tested on Windows.

### Worker

Purpose:

- Process one job at a time.
- Execute copy and move operations.
- Report progress and errors.
- Keep cancellation and retry behavior conservative.

Preferred direction:

- .NET worker first.
- Hide actual file-operation implementation behind an interface.
- Evaluate a native worker only if Windows API behavior makes it necessary.

Current implementation:

- `IFileOperationExecutor` defines the execution boundary.
- `NoOpFileOperationExecutor` exists only as a safe smoke-test executor.
- Production copy/move behavior is not implemented yet.

### File Operation Layer

First API to evaluate:

- `IFileOperation`, because it is the modern Shell API for copy, move, rename, create, and delete operations and can provide progress/error callbacks.

Fallback strategies to evaluate:

- Lower-level Windows file APIs for tighter progress, retry, and cancellation behavior.
- Conservative failure handling for network drives, long paths, access denied errors, and partial moves.

### Installer

Candidates:

- WiX Toolset.
- Inno Setup.

The installer must explain Explorer integration clearly, register and unregister components cleanly, and avoid leaving broken context menu entries after uninstall.

## Technical risks

- Shell Extension complexity.
- Explorer stability.
- COM registration/de-registration.
- Installer reliability.
- Permission and UAC behavior.
- File conflict handling.
- Network drives and long paths.
- Antivirus false positives.

## Non-goals for 0.1

- No cloud sync.
- No clipboard manager.
- No file sharing.
- No speed benchmark focus.
- No replacement for backup tools.
- No production-ready Explorer hook yet.

## Repository Structure

Planned structure when implementation starts:

```text
src/
  FileOperationQueue.App/
  FileOperationQueue.Core/
  FileOperationQueue.Worker/
  FileOperationQueue.ShellExtension/
  FileOperationQueue.Installer/
tests/
  FileOperationQueue.Core.Tests/
  FileOperationQueue.Worker.Tests/
  FileOperationQueue.Integration.Tests/
tools/
```

These names are intentionally neutral. The product display name can change without renaming the architecture.
