# Concept

The product is a queue for selected Windows Explorer file operations.

It should not replace Explorer. It should not become a heavy file manager. It should make a narrow workflow calmer: add copy and move jobs to one queue, process them in order, and keep the user moving.

It is also not intended to compete with classic copy accelerators on speed. Tools such as TeraCopy focus heavily on replacing or improving the copy operation itself. The first product boundary is different: controlled job intake from Explorer, a local queue, and predictable one-at-a-time execution.

## Product Boundary

The product should do:

- Accept selected copy and move jobs from Explorer.
- Store jobs in a local queue.
- Process one job at a time.
- Show queue state in a small tray app.
- Handle errors and conflicts clearly.
- Stay local.

The product should not do in the early versions:

- Replace Windows Explorer.
- Replace every normal Windows copy dialog.
- Sync files to cloud services.
- Monitor all file operations on the system.
- Claim faster transfers than Windows.
- Add broad automation rules.
- Become a two-pane file manager.

## Version 0.1 Architecture Proposal

Recommended direction for version 0.1:

- `FileOperationQueue.ShellExtension`: C++/Win32 in-process COM component for Explorer integration.
- `FileOperationQueue.App`: .NET tray app for queue display and settings.
- `FileOperationQueue.Core`: shared queue model, job state, settings, and IPC contracts.
- `FileOperationQueue.Worker`: .NET background worker that performs copy and move jobs through a controlled Windows file-operation layer.
- `FileOperationQueue.Installer`: WiX installer for registration, removal, and optional integration choices.

The Shell Extension should never become the worker. Explorer should only load a small integration component that validates input and hands work to the app or worker.

See [architecture.md](architecture.md) for the broader architecture overview and [architecture-decision.md](architecture-decision.md) for the current decision record.

## Tech Stack Comparison

### C#/.NET With SharpShell

Strengths:

- Faster development than raw COM.
- Familiar .NET ecosystem.
- SharpShell supports several Shell Extension types, including context menus and drop handlers.
- Good fit for prototypes and learning.

Risks:

- Managed in-process Shell Extensions can be more fragile inside Explorer.
- Runtime loading, versioning, and registration can be awkward.
- Project maturity and compatibility need current validation before relying on it.
- Deep drag-and-drop behavior may still require native-level understanding.

Best use:

- Prototype context menus.
- Learn data flow and UX.
- Not the first choice for the final Explorer-loaded component unless testing proves it stable.

### C++/Win32/COM

Strengths:

- Native fit for Windows Shell Extensions.
- Direct access to COM interfaces, Shell APIs, `IDataObject`, PIDLs, and registry registration.
- Small runtime footprint.
- Best long-term control over Explorer integration.

Risks:

- Slower development.
- More manual memory, COM lifetime, threading, and registration work.
- Testing and debugging Explorer-loaded DLLs requires discipline.

Best use:

- Production Shell Extension.
- Context menu and drag-and-drop research.
- Thin IPC bridge to the app or worker.

### Rust With Windows APIs

Strengths:

- Strong safety model.
- Can call Win32 and COM APIs through Windows bindings.
- Good for a robust worker or core library.

Risks:

- Shell Extension examples and operational knowledge are less common than C++.
- COM registration and Explorer debugging may become harder for contributors.
- More friction if the UI is .NET and the installer is WiX.

Best use:

- Possible future worker or core experiment.
- Not the lowest-risk path for first Shell integration.

### Mixed Architecture

Strengths:

- Uses native C++ where Explorer expects native COM.
- Uses .NET where UI development is simpler.
- Keeps dangerous Explorer-loaded code small.
- Allows the worker to evolve independently.

Risks:

- Requires clean IPC design.
- More projects to build and package.
- Installer must handle multiple components correctly.

Best use:

- Recommended for version 0.1.

## Queue Model

Initial job fields:

- Job ID.
- Source items.
- Destination folder.
- Operation: copy or move.
- Created time.
- Status: queued, active, completed, failed, canceled.
- Progress summary.
- Error details when available.

The queue should be persisted locally so the app can recover after restart. File operation recovery must be conservative. A partially completed move is not the same as a clean queued job.

## Integration Order

Version 0.1 should not depend on global drag-and-drop interception.

Preferred order:

1. Queue core and local persistence.
2. Tray app.
3. Explicit Explorer context menu.
4. Installer with clean registration and removal.
5. Drag-and-drop research prototype.

This keeps the first release useful without pretending the hardest Explorer behavior is solved.

## IPC Direction

The Shell Extension should pass requests to the app or worker through a narrow local channel.

Candidates:

- Named pipe.
- Local COM server.
- Loopback HTTP is possible, but less aligned with the local Windows-only nature.

Version 0.1 should prefer named pipes unless a prototype shows a better fit.

## Repository Structure

Proposed future structure:

```text
/
  README.md
  CHANGELOG.md
  ROADMAP.md
  LICENSE
  CONTRIBUTING.md
  SECURITY.md
  docs/
    concept.md
    architecture.md
    architecture-decision.md
    windows-integration.md
    shortcuts.md
    installer.md
    localization.md
    branding.md
  src/
    FileOperationQueue.App/
    FileOperationQueue.Core/
    FileOperationQueue.Worker/
    FileOperationQueue.ShellExtension/
    FileOperationQueue.Installer/
  samples/
  tests/
    FileOperationQueue.Core.Tests/
    FileOperationQueue.Worker.Tests/
    FileOperationQueue.Integration.Tests/
  tools/
  .github/
    FUNDING.yml
    ISSUE_TEMPLATE/
```

No `src/` code is created yet because the current task is project foundation, not implementation.

## Open Technical Questions

- Can modifier-based Explorer drop interception be implemented reliably without breaking normal drops?
- Should drag-and-drop become a later feature after context menu integration?
- Should the worker call `IFileOperation` directly, or should it use lower-level file APIs for more predictable progress and retry handling?
- How should the app represent partial failures in a multi-file job?
- Which operations can be safely canceled?

## References

- Microsoft: [Creating Shell Extension Handlers](https://learn.microsoft.com/en-us/windows/win32/shell/handlers)
- Microsoft: [IFileOperation interface](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ifileoperation)
- Microsoft: [Copy Hook Handlers](https://learn.microsoft.com/en-us/windows/win32/shell/how-to-create-copy-hook-handlers)
- SharpShell: [GitHub project](https://github.com/dwmkerr/sharpshell)
