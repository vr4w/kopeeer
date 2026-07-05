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

Earlier prototype note:

The initial neutral `FileOperationQueue.*` prototype projects were removed after the first Windows alpha started successfully. The active buildable path is now `Kopeeer.sln` with `Kopeeer.App`, `Kopeeer.Core`, and `Kopeeer.Worker`.

### Current Alpha UI

The first manually testable app is `Kopeeer.App`.

Current behavior:

- Windows Forms app targeting `net8.0-windows`.
- Manual source file or folder selection.
- Manual target folder selection.
- Copy or move operation selection.
- In-memory queue display.
- Sequential queue processing.
- Local logging to `logs/kopeeer.log`.

It has a first safe Explorer context menu integration path. It still does not install a native Shell Extension or intercept live drag-and-drop.

### Experimental Context Menu Path

The first Explorer integration path should avoid COM while the product is still unverified on Windows:

- Current-user registry verbs call the app executable.
- The app receives `--enqueue --operation copy|move --pick-target --sources "<path>"`.
- The app prompts for a destination folder.
- The running app receives the request through command-line startup or single-instance named pipe handoff.
- The queue core stores the job in memory for the current alpha session.

This path is less powerful than a native Shell Extension, but it is reversible and safer for Windows testing.

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

- `FileOperationProcessor` performs safe file/folder copy and move operations.
- Existing targets fail clearly instead of being overwritten silently.
- Move currently uses copy-then-delete behavior after a successful copy.
- Progress remains coarse and should be improved later.

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
  Kopeeer.App/
  Kopeeer.Core/
  Kopeeer.Worker/
  Kopeeer.ShellExtension/
  Kopeeer.Installer/
tests/
  Kopeeer.Core.Tests/
  Kopeeer.Worker.Tests/
  Kopeeer.Integration.Tests/
tools/
```

The product name can still change later, but the active alpha code path should not carry duplicate project families.
