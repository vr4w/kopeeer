# Architecture Decision

## Decision

For version 0.1, the project will use a mixed Windows-native architecture:

- Native C++/Win32 COM Shell Extension for Explorer integration.
- .NET tray app for queue display, settings, and user-facing state.
- .NET worker for queue execution behind a narrow file-operation abstraction.
- Shared .NET core library for job model, persistence, settings, and IPC contracts.
- Installer strategy to be selected after evaluating WiX Toolset and Inno Setup.

The Shell Extension must stay thin. It should collect Explorer selection data, validate the request, and hand it to the app or worker through local IPC. It should not copy files, maintain the queue, open long-running UI, or make complex policy decisions inside Explorer.

## Why This Is Preferred

The project's risk is concentrated in Explorer integration. Windows Shell Extensions are loaded into Explorer, so robustness matters more than quick development.

C++/Win32 is the safest long-term choice for the Explorer-loaded component because it matches the native COM model directly and avoids managed runtime loading inside Explorer. .NET remains a good fit for the tray app, queue model, persistence, and worker code where development speed and maintainability matter more.

This split keeps the most sensitive component small while letting the rest of the app stay productive and testable.

## Version 0.1 Scope

Version 0.1 should ship only what can be made reliable:

- Local queue model.
- One active job at a time.
- Minimal tray app.
- Explicit Explorer context menu integration.
- Installer with optional Explorer integration.
- Worker prototype with file-operation implementation hidden behind an internal interface.

Modifier-based drag-and-drop should remain a research spike until proven on Windows 10 and Windows 11. It should not be required for the first usable release.

## Current Implementation Boundary

The first code step created a neutral queue prototype. After the first Windows alpha launched successfully, the active buildable path was simplified to `Kopeeer.sln`:

- `Kopeeer.App`
- `Kopeeer.Core`
- `Kopeeer.Worker`
- No Shell Extension.
- No COM code.
- No Explorer hook.

This keeps the alpha easy to build and test before returning to deeper Explorer integration.

## Rejected As Primary Architecture

### Pure .NET With SharpShell

This is attractive for prototypes, but not preferred as the production Explorer-loaded layer. Managed in-process Shell Extensions introduce runtime and registration concerns inside Explorer. The project can still use SharpShell for experiments, but the robust path is native C++ for the final Shell Extension.

### Pure C++ Application

This would keep all Windows integration native, but it slows UI and queue development without enough benefit. The tray UI should remain small, but it still benefits from modern .NET tooling and testability.

### Rust-first Shell Extension

Rust may become interesting for a worker or shared library, but it adds friction for COM Shell Extension examples, contributor familiarity, packaging, and debugging. It is not the lowest-risk first path.

### Copy Hook Handler

A Copy Hook Handler is not a queue architecture. It can approve or block certain Shell operations, but it does not perform the copy or move operation and does not report final success. It should not be the foundation of this project.

## Integration Order

Recommended build order:

1. Define job model, queue persistence, and worker interface.
2. Build tray app around real queue state.
3. Add explicit context menu Shell Extension.
4. Add WiX installer and uninstaller.
5. Run Windows 10 and Windows 11 integration tests.
6. Research modifier-based drag-and-drop after the context menu path is stable.

## Open Questions

- Should the production worker stay fully .NET if `IFileOperation` interop is reliable enough?
- Does `IFileOperation` provide the right progress and conflict behavior for the minimal UI?
- Which IPC mechanism is most reliable under Explorer startup, app-not-running, and upgrade scenarios?
- Can modifier-based drag-and-drop be implemented without surprising Explorer users?

## Consequences

This architecture is slower to start than a pure .NET prototype, but it reduces the risk of unstable Explorer behavior later.

It also creates a clear public story: this is not a quick drop-zone app. It is a careful Windows utility that keeps Explorer integration narrow, reversible, and honest.
