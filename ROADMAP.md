# Roadmap

Kopeeer should stay small, understandable, and Explorer-first. The path to `1.0` is not about adding many features. It is about making copy and move jobs feel reliable, clear, and boring in the best possible way.

## Current Release: 0.3.0-alpha

The current alpha can already be installed and tested on Windows:

- Explorer right-drag commands:
  - `Copy with Kopeeer`
  - `Move with Kopeeer`
- Classic right-click fallback commands.
- Sequential copy and move queue.
- Compact transfer window with file name, progress, speed, upcoming jobs, and cancel.
- Optional `Shut down when done`.
- Self-contained installer with native Explorer shell extension.

This is still an alpha. Use test files first.

## Next: 0.4.0-alpha

Focus: make failure cases obvious and safe.

- Add a clear dialog or decision flow when the target file already exists.
- Offer safe choices such as skip, rename, or cancel.
- Improve cancellation for very large files and folders.
- Show a clearer final state after a failed or canceled queue.
- Add a short troubleshooting section for Explorer integration.

## Then: 0.5.0-beta

Focus: make Kopeeer feel ready for everyday testing.

- Add a tiny settings view for defaults.
- Remember useful preferences such as shutdown behavior.
- Improve wording across the app and installer.
- Add screenshots or a short GIF to the README.
- Test uninstall/reinstall/update flows on Windows 10 and Windows 11.
- Decide whether the classic right-click fallback should stay enabled by default.

## Before 1.0

Focus: reliability, trust, and a simple first-user experience.

- Confirm copy and move behavior with folders, many files, large files, and removable drives.
- Confirm behavior on network paths and long paths, or document limitations clearly.
- Make logs useful without exposing confusing internal details.
- Add release checks so every installer is built the same way.
- Decide whether code signing is required before wider public use.
- Keep the first screen and README focused on download, install, and use.

## Not Planned For 1.0

These ideas may be useful later, but they should not distract the first stable release:

- Full file manager features.
- Cloud sync.
- Backup scheduling.
- Clipboard history.
- Complex transfer dashboards.
- Left-drag modifier interception, unless a reliable Windows Explorer path is proven.
