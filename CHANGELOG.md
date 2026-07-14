# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this project intends to follow semantic versioning once releases begin.

## 1.0.1 - 2026-07-14

### Fixed

- Explorer integration now starts Kopeeer correctly when no instance is running.
- Queue list flicker with multiple pending jobs.
- Explorer shell integration registration on 64-bit Windows.
- Missing application icon in Windows Search and shortcuts.
- Incomplete destination files are cleaned up after cancellation.
- Installer upgrade handling when Explorer has loaded the shell extension.

### Changed

- Compact borderless transfer window.
- Improved installer validation and diagnostics.
- Safer temporary-file handling for copy and move operations.
- Escape key now cancels an active queue or closes an idle window.

### Security / Documentation

- Unsigned publisher / SmartScreen behavior documented.
- SHA256 checksum published for the installer.

## 1.0.0 - 2026-07-07

### Changed

- Promoted Kopeeer to the first stable Explorer-first release.
- Updated README, roadmap, installer notes, and release text for `1.0.0`.
- Reworked the roadmap around recurring release checks instead of a beta-to-1.0 checklist.
- Removed the confusing intermediate beta placeholder and simplified the stable-release scope notes.
- Updated installer documentation to match the current machine-wide Explorer integration.
- Changed installer shell-extension placement so upgrades no longer need to close Explorer just to replace the loaded DLL.

## 0.5.0-beta - 2026-07-07

### Added

- Target conflict choices when a file or folder already exists: rename, skip, or cancel queue.
- Transfer size display in the compact window, for example `20 MB of 200 MB`.
- Upcoming queue rows now show file size and copy/move action.
- Beta installer version for the next test cycle.

### Changed

- Made the compact transfer window fixed-size to avoid broken resizing.
- Removed the expandable manual input area from the compact transfer window.
- Changed the completed-state button from red `Cancel` to a quiet `Close`.
- Made the upcoming queue area grow with pending files before scrolling.
- Refined installer wording toward a set-it-and-forget-it Explorer integration.

## 0.3.0-alpha - 2026-07-06

### Added

- Native Explorer right-drag menu integration for:
  - `Copy with Kopeeer`
  - `Move with Kopeeer`
- Compact transfer window with current file name, progress, speed, upcoming jobs, and cancel.
- Optional `Shut down when done`.
- First public GitHub release with installer asset.

### Changed

- Updated the app path lookup for the native shell extension.
- Refined the transfer window to be smaller and more focused.
- Improved README installation notes for the public alpha.

### Fixed

- Fixed shell extension app path lookup after installation.
- Made cancel close the transfer window immediately.
- Removed the visible error column from the main transfer window.

### Known Risks

- This is still an alpha release. Use test files first.
- Existing target files and folders are not overwritten silently, but the user choice flow still needs polish.
- Cancellation can still leave partial files depending on the exact operation Windows is performing at the moment of canceling.

## 0.2.0-alpha - 2026-07-05

### Added

- Command-line enqueue support with `--enqueue`, `--operation`, `--target`, `--pick-target`, and `--sources`.
- Explorer context menu prototype using current-user registry entries.
- User-level register/unregister scripts for context menu testing.
- Target picker flow for Explorer-selected files/folders.
- More queue-focused app layout with manual job creation behind "Add job manually...".
- Installer builder script that publishes the app and creates an Inno Setup installer EXE.
- Copy and cut icon assets for Explorer context menu entries.
- Single-instance handoff so Explorer enqueue requests go to the running Kopeeer window.
- GitHub Actions installer artifact and prerelease publishing path.

### Changed

- Removed obsolete duplicate `FileOperationQueue.*` prototype projects from `src/` and `tests/`.
- Updated alpha UI to make the queue list the primary surface.
- Simplified README around the installer-first tester flow.
- Simplified the queue table by removing timestamp columns from the main view.
- Made Explorer-started queue jobs begin processing automatically after the target folder is selected.
- Added an installer option for Explorer context menu registration.

### Fixed

- Made context menu registration use `reg.exe` with explicit default command values so Explorer can launch Kopeeer reliably.
- Simplified GitHub Actions to build only the intended Kopeeer app project during alpha.

### Known Risks

- Single-instance handoff is new and still needs Windows validation with repeated Explorer requests.
- Windows 11 may place classic context menu entries behind "Show more options".
- Context menu integration is an alpha prototype and does not install a Shell Extension.

## 0.1.0-alpha - Unreleased

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
