# Drag-and-drop Explorer Hook Prototype

The original product idea is still the goal: hold a modifier such as `ALT + SHIFT` while dropping files in Explorer, and queue the operation instead of letting Windows start another uncontrolled copy or move immediately.

This document defines the prototype track. It is not yet production-ready behavior.

The current alpha does not intercept live Explorer drops. It uses the safer context menu path while the native Explorer hook is researched and tested on Windows.

## Desired Behavior

- Normal Explorer drag and drop remains unchanged.
- If the user holds `ALT + SHIFT` while dropping onto a folder, Kopeeer should queue the operation.
- The user should get clear feedback that the job was queued.
- Kopeeer should process queued jobs sequentially.

## Prototype Questions

- Which Shell Extension type can reliably see source items, target folder, and modifier key state at drop time?
- Can Kopeeer take ownership only when `ALT + SHIFT` is held?
- Can it prevent Explorer's default operation after queueing?
- Does the behavior differ between Windows 10 and Windows 11?
- Does it work for files, folders, network drives, external drives, long paths, and protected folders?

## Recommended Prototype Direction

The recommended prototype is a native C++/Win32/COM Shell Extension spike. The first thing to prove is not copying files; it is whether Windows exposes enough reliable drop context for Kopeeer to queue the operation without breaking normal Explorer behavior.

The Shell Extension must:

- Stay 64-bit.
- Do almost no work inside Explorer.
- Inspect modifier state and data object contents.
- Identify the destination folder.
- Send a local queue request to the app or worker.
- Decline handling when the modifier is not held.
- Fail back to normal Explorer behavior when Kopeeer is unavailable.

The .NET app already has the beginning of the required handoff shape: enqueue requests can be sent to the running app through a local named pipe. A native Shell Extension should reuse that boundary instead of doing file work inside Explorer.

## Safety Rule

Do not ship this hook in the default installer until it has been tested on a Windows machine.

The first installer path should remain the explicit context menu integration. Drag-and-drop should be marked experimental until it proves stable.

## Windows Test Checklist

- Windows 10 64-bit.
- Windows 11 64-bit.
- File to local folder.
- Folder to local folder.
- Multiple files.
- External drive target.
- Network share target.
- Long path target.
- Existing file conflict.
- App not already running.
- App already running.
- Modifier not held.
- `ALT + SHIFT` held.
- Right-button drag.
- Uninstall and Explorer restart.
