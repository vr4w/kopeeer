# Roadmap

The project should grow in small, testable phases. Each phase should leave the tool understandable.

## Phase 0: Concept And Architecture

- Define the core product boundary.
- Compare Shell Extension and app architecture options.
- Document risks before implementation.
- Choose first repository structure.
- Prepare public project files.
- Keep product naming configurable while candidates are evaluated.

## Phase 1: Queue Core And Tray App

- Implement a local queue model.
- Add one active job at a time.
- Add pause, resume, cancel, and retry decisions where technically safe.
- Build a minimal tray app with current job and pending jobs.
- Store settings locally.
- Keep file-operation guarantees conservative until real Windows tests prove them.
- Centralize user-facing strings so the app is localization-ready.

## Phase 2: Explorer Context Menu

- Add explicit Explorer commands:
  - "Copy with Kopeeer..."
  - "Move with Kopeeer..."
  - "Add to Kopeeer queue..."
- Pass selected items and target folder to the app or worker.
- Keep the Shell Extension small and defensive.
- Validate registration and uninstallation on Windows 10 and Windows 11.
- Treat this as the first production Explorer integration path.

## Phase 3: Drag-and-drop Integration

- Research the safest Shell mechanism for the modifier-based drop workflow.
- Prototype modifier detection such as `ALT + SHIFT`.
- Confirm whether the app can reliably take over a drop without breaking normal Explorer behavior.
- Assume this belongs after 0.1 unless the prototype is exceptionally clean.

## Phase 4: Installer And Release

- Create a WiX-based installer.
- Register and unregister Shell Extension components cleanly.
- Offer installer options for Explorer integration.
- Add release signing plan.
- Keep the version 0.1 installer English only, with clear Explorer integration wording.
- Build first public alpha release.

## Phase 5: Polishing, Logs, Conflict Handling, Favorites

- Improve conflict handling for existing files.
- Add clear, local-only logs.
- Add favorite target folders.
- Add better error recovery.
- Add future localization support if the centralized string model is ready.
- Refine wording, icons, and empty states.
- Keep the UI quiet.
