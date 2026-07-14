# Kopeeer 1.0.1 Test Checklist

Use this checklist before publishing a public `1.0.1` release.

Do not update the public README download link or publish a GitHub release until the installer passes on at least two Windows machines.

## Installer Under Test

Expected installer:

```text
artifacts/installer/Kopeeer-Setup-1.0.1.exe
```

## Required Machines

- Windows 11 machine A.
- Windows 11 machine B.
- Optional: Windows 10 64-bit.

## Fresh Install

1. Uninstall any existing Kopeeer installation.
2. Restart Explorer or sign out and back in if a previous shell extension was loaded.
3. Install `Kopeeer-Setup-1.0.1.exe`.
4. Confirm the installer does not claim success if Explorer integration validation fails.
5. Confirm Windows Search shows Kopeeer with the Kopeeer icon.
6. Start Kopeeer from Windows Search.
7. Confirm the app opens.

## Explorer Integration

1. Close Kopeeer completely.
2. Right-drag a file onto a target folder.
3. Choose `Copy with Kopeeer`.
4. Confirm Kopeeer starts automatically and the job runs.
5. Close Kopeeer completely again.
6. Right-drag a file onto a target folder.
7. Choose `Move with Kopeeer`.
8. Confirm Kopeeer starts automatically and the job runs.
9. Start Kopeeer manually.
10. Right-drag another file or folder onto a target folder.
11. Confirm the job is added to the running Kopeeer instance.
12. Right-drag a folder onto a target folder.
13. Confirm `Copy with Kopeeer` and `Move with Kopeeer` appear.
14. Right-click a file normally.
15. Confirm `Copy with Kopeeer...` and `Move with Kopeeer...` appear.
16. Right-click a folder normally.
17. Confirm fallback entries appear.
18. Confirm an Explorer restart or sign-out is not required after a fresh install unless Windows keeps old shell state cached.

## File Operations

1. Copy one file.
2. Move one file.
3. Copy one folder.
4. Move one folder.
5. Queue several jobs and confirm they run one after another.
6. Trigger an existing-target conflict and confirm rename/skip/cancel behavior.
7. Press `Cancel` during a queue and confirm behavior is clear.
8. Queue 5, 10, and 20 jobs and confirm the upcoming-job list does not flicker while one transfer is running.
9. Add more jobs while a transfer is already running and confirm the visible queue remains stable.

## Borderless Window

1. Confirm the transfer window has no normal Windows title bar.
2. Confirm the top area can move the window.
3. Confirm the `x` button closes when idle.
4. Confirm close/cancel behavior is clear during an active transfer.
5. Confirm the frame is visible in dark mode.
6. Confirm the frame is visible in light mode.
7. Test DPI scaling at 100 %, 125 %, 150 %, and 200 %.
8. Test a multi-monitor setup.
9. Confirm the window appears correctly in Alt-Tab and the taskbar.
10. Confirm no text or controls are clipped.
11. Confirm no visible flicker during progress updates.

## Diagnostics And Repair

Run diagnostics:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Program Files\Kopeeer\Tools\diagnose-installation.ps1"
```

Expected result:

- PASS for app executable.
- PASS for shell extension DLL.
- PASS for version.
- PASS for CLSID registration.
- PASS for right-drag handlers.
- PASS for fallback context menu entries.

Runtime logs:

```text
%LOCALAPPDATA%\Kopeeer\logs\kopeeer.log
%LOCALAPPDATA%\Kopeeer\shell-extension.log
```

Expected result:

- Shell log records the selected copy/move command.
- Shell log records source count, target folder, resolved app path, working directory, and app-start result.
- App log records startup, running-instance detection, IPC success/failure, accepted queue requests, and job status.

Run repair from an elevated PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Program Files\Kopeeer\Tools\repair-shell-integration.ps1"
```

Expected result:

- Repair rewrites only Kopeeer shell integration keys.
- Explorer is refreshed.
- No unrelated registry keys are deleted.

## Uninstall / Reinstall / Upgrade

1. Uninstall Kopeeer.
2. Confirm the app is removed.
3. Confirm right-drag Kopeeer entries are removed.
4. Confirm normal right-click fallback entries are removed.
5. Reinstall `1.0.1`.
6. Confirm Explorer integration works again.
7. Install public `1.0.0`.
8. Upgrade to `1.0.1`.
9. Confirm app, icon, shell extension, diagnostics, and Explorer entries work after upgrade.

## Pass Criteria

`1.0.1` can be published only after:

- Fresh install passes on at least two Windows machines.
- Upgrade from `1.0.0` passes.
- Uninstall removes Kopeeer shell integration.
- Diagnostics pass after install.
- Repair can restore missing Kopeeer registry entries.
