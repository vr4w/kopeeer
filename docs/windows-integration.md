# Windows Integration

The hardest part is not the queue. It is the requirement that Explorer remains normal while selected operations can be handed to the app.

This document is intentionally cautious. It describes the likely path and the unresolved risk.

## Shell Extension Basics

Windows Shell Extensions are usually in-process COM DLLs loaded by Explorer. That makes them powerful and sensitive.

A Shell Extension for this project should:

- Be 64-bit.
- Do minimal work inside Explorer.
- Avoid long-running operations.
- Avoid blocking UI threads.
- Validate incoming paths and data objects.
- Hand off to the app or worker through IPC.
- Fail quietly back to normal Explorer behavior when the app is unavailable.

## Context Menu Integration

Context menu integration is the safest first Explorer feature and the recommended version 0.1 integration target.

Planned commands:

- "Copy with Kopeeer..."
- "Move with Kopeeer..."
- "Add to Kopeeer queue..."

Implementation direction:

- Start with reversible current-user Explorer verbs that invoke the app command line.
- App command-line entry points ask the user for a destination folder and enqueue the selected file or folder.
- Keep native C++ Shell Extension work for the later stage where richer multi-select and deeper Explorer context is required.

Why first:

- Clear user intent.
- Easier to test than modifier-based drag-and-drop.
- Does not need to intercept normal left-button drag behavior.
- Reversible without COM registration during early Windows testing.

Current prototype:

- `Kopeeer.App.exe --enqueue --operation copy --pick-target --sources "<path>"`
- `Kopeeer.App.exe --enqueue --operation move --pick-target --sources "<path>"`
- `scripts/register-context-menu.ps1`
- `scripts/unregister-context-menu.ps1`
- `installer/inno/Kopeeer.iss`

Registry paths:

- `HKCU\Software\Classes\*\shell\Kopeeer.CopyWith`
- `HKCU\Software\Classes\*\shell\Kopeeer.MoveWith`
- `HKCU\Software\Classes\Directory\shell\Kopeeer.CopyWith`
- `HKCU\Software\Classes\Directory\shell\Kopeeer.MoveWith`

Known limitation:

- Single-instance IPC is not implemented yet. If Kopeeer is already running, a context menu request launches a new process for now.

## Drag-and-drop Integration

The long-term desired product interaction is:

- User drags files or folders in Explorer.
- User holds `ALT + SHIFT` while dropping onto a target folder.
- The app queues the operation if Windows exposes a reliable and safe handoff point.
- Normal drops without the modifier behave exactly as before.

This is technically risky.

Windows has Shell Extension mechanisms around drop handlers and drag-and-drop handlers, but they do not automatically mean the app can globally replace Explorer's normal drop behavior for every folder in a safe, clean way.

Research questions:

- Which handler type sees the source items, target folder, and modifier state at the required moment?
- Can the app take over only when the modifier is held?
- Can it prevent the normal Explorer operation after queueing?
- Does behavior differ between Windows 10 and Windows 11?
- How does this interact with left-button drag, right-button drag, network locations, libraries, and virtual folders?

Until a Windows prototype proves this, drag-and-drop integration is planned research, not a version 0.1 dependency.

See [drag-drop-explorer-hook.md](drag-drop-explorer-hook.md).

## Copy Hook Handler Is Not The Solution

A Copy Hook Handler can be called before folder or printer operations such as copy, move, delete, or rename. It can approve or veto an operation.

It does not perform the operation itself. It is global. It is not informed about success or failure. That makes it unsuitable as the main queue mechanism.

Possible limited use:

- Research only.
- Maybe protective behavior in a very narrow future scenario.

Not suitable for:

- Queueing arbitrary file operations.
- Monitoring copy progress.
- Replacing Explorer's operation with the worker.

## File Operation API

`IFileOperation` is the preferred Windows Shell API to evaluate for the worker. It can copy, move, rename, create, and delete Shell items. It supports progress and error notification through an advise sink, and it is newer and safer than older `SHFileOperation` patterns.

Important detail:

- `IFileOperation` is intended for single-threaded apartment usage.
- The worker design must respect COM apartment rules.
- Progress and error behavior must be tested with real folders, large files, conflicts, permissions, and network paths.

Current direction:

- Prototype `IFileOperation` first for user-like Shell behavior.
- Keep the worker abstraction narrow enough to replace the implementation if lower-level file APIs prove safer for progress, retry, and cancellation.

Version 0.1 should not expose advanced guarantees such as verification, acceleration, or perfect resume behavior until the worker proves them.

## Shell Extension To App Handoff

The Shell Extension should not copy files.

Recommended flow:

1. Explorer invokes the Shell Extension.
2. Shell Extension extracts source items, destination, and requested operation.
3. Shell Extension sends a local IPC message to the app or worker.
4. The app or worker validates and persists the job.
5. Worker processes queued jobs one at a time.

Candidate IPC:

- Named pipes.
- Local COM out-of-process server.

Named pipes are the simpler first candidate. The protocol should be small, versioned, and local-only.

## Registration And Installer Risk

Shell Extensions require clean registration and clean removal.

The installer must:

- Register the 64-bit COM component.
- Offer optional Explorer integration.
- Unregister all components on uninstall.
- Avoid leaving broken context menu entries.
- Handle upgrades.
- Document when Explorer restart or sign-out may be needed.

## Testing Matrix

Minimum Windows test matrix:

- Windows 10 64-bit.
- Windows 11 64-bit.
- Local NTFS folders.
- External drive.
- Network share.
- Long paths.
- Read-only destination.
- Existing file conflicts.
- Large folder with many files.
- App not running.
- App starts on demand.
- Uninstall while Explorer was running.

## References

- Microsoft: [Creating Shell Extension Handlers](https://learn.microsoft.com/en-us/windows/win32/shell/handlers)
- Microsoft: [Creating Shortcut Menu Handlers](https://learn.microsoft.com/en-us/windows/win32/shell/context-menu-handlers)
- Microsoft: [Copy Hook Handlers](https://learn.microsoft.com/en-us/windows/win32/shell/how-to-create-copy-hook-handlers)
- Microsoft: [IFileOperation interface](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ifileoperation)
