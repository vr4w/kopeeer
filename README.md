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

Kopeeer is early-stage and experimental.

It is not ready for production use.

No deep Explorer integration is implemented yet. Shell integration on Windows is powerful, strict, and easy to get wrong. Version 0.1 should start with the safer path: queue core, local worker, minimal tray app, and explicit Explorer context menu integration. Modifier-based drag-and-drop remains planned research until a Windows prototype proves it can be done without breaking normal Explorer behavior.

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
