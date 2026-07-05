# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this project intends to follow semantic versioning once releases begin.

## 0.1.0 - Unreleased

### Added

- Initial public project foundation.
- Concept documentation for the queue-first Explorer workflow.
- Architecture overview for the recommended component split and technical risks.
- Architecture decision for the recommended version 0.1 mixed Windows-native architecture.
- Initial `FileOperationQueue.Core` project with queue model, JSON persistence, status transitions, and local worker boundary.
- Initial `FileOperationQueue.App` Windows tray UI scaffold for displaying the local queue.
- Command-line queue entry points for context menu handoff.
- Draft Inno Setup installer script and reversible current-user context menu dev scripts.
- Drag-and-drop Explorer hook prototype plan and Windows test checklist.
- Windows build workflow for GitHub Actions.
- First buildable Windows app skeleton for `0.1.0-alpha`.
- Manual copy/move queue prototype.
- Sequential job processing.
- Safe file/folder copy and move behavior that fails instead of overwriting existing targets.
- Basic local logging to `logs/kopeeer.log`.
- README build/run instructions and helper scripts.
- Alpha UI polish with clearer layout, button state handling, readable queue table, and status summary.
- Cleanup of obsolete duplicate `FileOperationQueue.*` prototype projects from `src/` and `tests/`.
- Windows integration notes covering Shell Extensions, context menus, drag-and-drop risk, copy hooks, and `IFileOperation`.
- Branding note for current working display name `Kopeeer` and repository codename `file-operation-queue`.
- English-first localization strategy for version 0.1.
- Initial roadmap, contribution, funding, security, and issue template files.

### Known Risks

- Deep drag-and-drop integration from Explorer is not solved yet.
- Copy Hook Handlers are not a full queueing solution because they can approve or block operations, but do not perform the operation.
- Shell Extension registration, unloading, crash isolation, and installer cleanup need dedicated Windows testing.
- The tray app targets Windows and still needs verification on a Windows machine with the .NET SDK installed.
- Context menu and drag-and-drop behavior must be verified on Windows before release.
- Long paths and network drives are known alpha limitations.
