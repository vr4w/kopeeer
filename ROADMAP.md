# Roadmap

Kopeeer should stay small, understandable, and Explorer-first. The path to `1.0` is not about adding many features. It is about making copy and move jobs feel reliable, clear, and boring in the best possible way.

## Current Release: 1.0.0

The current stable release can be installed and used on Windows:

- Explorer right-drag commands:
  - `Copy with Kopeeer`
  - `Move with Kopeeer`
- Classic right-click fallback commands.
- Sequential copy and move queue.
- Compact transfer window with file name, progress, transfer size, speed, upcoming jobs, and cancel.
- Target conflict choices: rename, skip, or cancel queue.
- Optional `Shut down when done`.
- Self-contained installer with native Explorer shell extension.

## Release Checks

Before each public release, Kopeeer should pass these checks:

- Copy one file, many files, folders, and mixed file/folder selections.
- Move one file, many files, folders, and mixed file/folder selections.
- Handle existing target files and folders with rename, skip, and cancel queue.
- Cancel an active queue without leaving the app window stuck open.
- Show useful progress: current filename, progress bar, speed, copied size, and upcoming files.
- Install cleanly.
- Upgrade cleanly from the previous installer without breaking Explorer or the taskbar.
- Uninstall cleanly and remove Explorer menu entries.
- Keep README, installer text, and release notes clear for non-technical users.

Known limitations can stay out of the app as long as they are documented clearly. Broad public trust work such as code signing can follow after the first stable release if needed.

## Test Notes

Major releases should be tested through Explorer, not only through command-line helpers, because Explorer integration is the product.

Record only what matters:

- Did the menu item appear?
- Did the right operation happen?
- Did the window show understandable progress?
- Did cancel/skip/rename behave as expected?
- Did anything feel confusing enough to block a normal user?
