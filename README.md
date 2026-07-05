# Kopeeer

A calm queue for Windows file operations.

Kopeeer is an early-stage Windows utility for queued copy and move operations.

The repository codename is `file-operation-queue`. The final public product name is still being evaluated, so branding should stay easy to rename.

The idea is simple: selected file operations started from Windows Explorer should be able to wait their turn. Kopeeer will collect chosen copy and move actions in a local queue, process one job at a time, and let the user continue working.

## What It Solves

Windows Explorer is fast and familiar, but large file operations can become noisy when several copy or move jobs start at once. Progress windows overlap. Disk load jumps. The user has to babysit operations that should have been orderly.

Kopeeer keeps Explorer as the planned starting point while moving selected operations into a small, predictable queue.

## What It Is Not

Kopeeer is not meant to be a full TeraCopy replacement.

It is not trying to be a faster copy engine, a clipboard manager, a file sharing app, a cloud sync tool, a file manager, or a dashboard full of transfer statistics. The first goal is narrower: a reliable handoff from Windows Explorer into a calm local queue.

## Planned Interaction

- Normal drag and drop stays unchanged.
- Explorer context menu commands should add jobs to the queue first.
- A later modifier workflow, such as `ALT + SHIFT` while dropping, may add jobs directly if Windows integration proves reliable.
- Jobs are processed one after another.
- A tray app shows the queue, current job, and basic settings.

## Status

Kopeeer is currently an early alpha prototype.

It is not ready for production use.

The current test app does not integrate with Windows Explorer yet. Shell integration on Windows is powerful, strict, and easy to get wrong, so the first runnable build is a manual local app: choose source, choose target, choose copy or move, add to queue, and process jobs sequentially.

## Planned Shape

Version 0.1 should focus on:

- Queue data model and worker behavior.
- Local worker for one-at-a-time operations.
- Minimal tray app / UI.
- Clear internal boundary between UI, queue, worker, and Windows shell integration.
- First Explorer context menu integration as a goal, not a completed feature.
- Research spike for drag-and-drop interception, not a shipped promise.

The preferred direction is a mixed architecture:

- C++/Win32 COM Shell Extension for the Explorer-facing parts.
- .NET tray app for the small user interface.
- .NET worker with a controlled Windows file-operation layer, unless the `IFileOperation` prototype shows a native worker is safer.
- WiX Toolset installer for clean registration and removal.

See [docs/architecture.md](docs/architecture.md), [docs/architecture-decision.md](docs/architecture-decision.md), [docs/concept.md](docs/concept.md), and [docs/windows-integration.md](docs/windows-integration.md).

## Current Code

The first local test version is intentionally small:

- `Kopeeer.sln`
- `src/Kopeeer.App`
- `src/Kopeeer.Core`
- `src/Kopeeer.Worker`
- Manual source file/folder selection.
- Manual target folder selection.
- Copy/move queue.
- Sequential processing.
- Basic status display.
- Basic local logging to `logs/kopeeer.log`.
- No Shell Extension.
- No Explorer hook.
- No installer.
- No context menu.
- No automatic shortcut handling.

## Build Requirements

- Windows 10 or Windows 11.
- .NET 8 SDK or newer.
- Optional: Visual Studio 2022.

## How To Build

```powershell
dotnet restore
dotnet build
```

Or:

```powershell
scripts\build.ps1
```

## How To Run

```powershell
dotnet run --project src/Kopeeer.App
```

Or:

```powershell
scripts\run.ps1
```

## What Works In 0.1.0-alpha

- Manual file/folder selection.
- Manual target folder selection.
- Copy/move queue.
- Sequential processing.
- Basic status display.
- Basic logging.

## What Does Not Work Yet

- No Explorer integration yet.
- No Shell Extension yet.
- No installer yet.
- No context menu yet.
- No drag-and-drop hook yet.
- No automatic shortcut handling yet.

## Known Alpha Limitations

- Existing target files or folders fail the job instead of prompting.
- Long paths and network drives are not deeply tested yet.
- Move is implemented as copy-then-delete after a successful copy.
- The UI is intentionally plain.

See [docs/windows-test-plan.md](docs/windows-test-plan.md) before treating Windows behavior as verified.

## Freeware

Kopeeer, or whatever final name is chosen, is planned as freeware.

No cloud account. No telemetry. No ads.

If the tool becomes useful to you, a voluntary donation link may be offered later. The app itself should stay calm about it: no nag screens, no artificial limits, no pressure.

## Language

The project is English-first for version 0.1: README, UI text, installer text, issue templates, changelog, and documentation should be written in English.

The architecture should still be localization-ready. User-facing strings should be centralized where possible so German can be added later without rewriting the app. See [docs/localization.md](docs/localization.md).

## Documentation

- [Concept](docs/concept.md)
- [Architecture](docs/architecture.md)
- [Architecture decision](docs/architecture-decision.md)
- [Windows integration](docs/windows-integration.md)
- [Shortcuts](docs/shortcuts.md)
- [Installer](docs/installer.md)
- [Branding](docs/branding.md)
- [Localization](docs/localization.md)
- [Roadmap](ROADMAP.md)

## License

The current license suggestion is MIT. See [LICENSE](LICENSE).
