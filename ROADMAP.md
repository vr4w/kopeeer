# Roadmap

The project should grow in small, testable phases. Each phase should leave the tool understandable.

## Phase 0: Concept And Public Project Foundation

- Define the core product boundary.
- Compare Shell Extension and app architecture options.
- Document risks before implementation.
- Choose first repository structure.
- Prepare public project files.
- Keep product naming configurable while candidates are evaluated.

## Phase 1: Queue Core And Local Worker

- Implement a local queue model.
- Add one active job at a time.
- Add pause, resume, cancel, and retry decisions where technically safe.
- Add local worker boundaries.
- Evaluate `IFileOperation` and fallback strategies.
- Store settings locally.
- Keep file-operation guarantees conservative until real Windows tests prove them.
- Centralize user-facing strings so the app is localization-ready.

Current status:

- First queue model, JSON persistence, and local worker boundary are in place.
- `Kopeeer.Core` and `Kopeeer.Worker` now provide the first manual alpha queue path.
- Jobs run sequentially in memory.
- First safe copy/move execution exists for manual alpha testing.
- Existing targets fail instead of being overwritten.
- Obsolete `FileOperationQueue.*` prototype projects have been removed from the build path.

## Phase 2: Tray App / Minimal UI

- Build a minimal tray app with current job and pending jobs.
- Add clear status labels.
- Add queue controls only when they are technically safe.
- Keep UI copy English-first and centralized.
- Avoid dashboard-style complexity.

Current status:

- First Windows Forms tray UI scaffold is in place.
- A plain runnable WinForms alpha app exists under `src/Kopeeer.App`.
- Manual file/folder source selection, target selection, copy/move selection, queue display, and queue start are in place.
- It writes a test log to `logs/kopeeer.log`.
- Buttons now enable only when the current action is valid.
- The queue table and status summary are more readable.
- Windows runtime verification is still required.

## Phase 3: Explorer Context Menu Integration

- Add explicit Explorer commands:
  - "Copy with Kopeeer..."
  - "Move with Kopeeer..."
  - "Add to Kopeeer queue..."
- Pass selected items and target folder to the app or worker.
- Keep the Shell Extension small and defensive.
- Validate registration and uninstallation on Windows 10 and Windows 11.
- Treat this as the first production Explorer integration path.

Current status:

- App command-line queue entry points are in place for context menu handoff.
- Reversible current-user registry dev scripts are in place.
- Draft Inno Setup installer registration is in place.
- Windows verification is required before this becomes a release feature.

## Phase 4: Drag-and-drop / Shell Integration Research

- Research the safest Shell mechanism for the modifier-based drop workflow.
- Prototype modifier detection such as `ALT + SHIFT`.
- Confirm whether the app can reliably take over a drop without breaking normal Explorer behavior.
- Assume this belongs after 0.1 unless the prototype is exceptionally clean.
- Do not ship a production-ready Explorer hook until stability is proven.

Current status:

- Prototype requirements and Windows test checklist are documented.
- No native drop hook is installed by default.

## Phase 5: Installer And Release Packaging

- Evaluate WiX Toolset and Inno Setup.
- Register and unregister Shell Extension components cleanly.
- Offer installer options for Explorer integration.
- Add release signing plan.
- Keep the version 0.1 installer English only, with clear Explorer integration wording.
- Build first public alpha release.

## Phase 6: Conflict Handling, Logs, Polish, Localization

- Improve conflict handling for existing files.
- Add clear, local-only logs.
- Add favorite target folders.
- Add better error recovery.
- Add future localization support if the centralized string model is ready.
- Refine wording, icons, and empty states.
- Keep the UI quiet.
