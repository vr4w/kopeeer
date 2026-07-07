# Installer

The project needs an installer because deep Windows integration must be registered and removed cleanly.

## Installer Direction

Kopeeer currently uses Inno Setup. The installer should feel like a small Windows utility installer: install it once, keep the Explorer integration enabled, then forget about the app until a transfer starts.

The installer should not launch Kopeeer at the end. Kopeeer is started by Explorer when the user chooses `Copy with Kopeeer` or `Move with Kopeeer`.

The installer choice can be revisited later if MSI-style upgrade management becomes necessary, but it should not block the first stable release.

Current repository state:

- `installer/inno/Kopeeer.iss` is the active Inno Setup script.
- `scripts/build-installer.ps1` publishes the app and builds the installer EXE on Windows.
- It installs the app from `artifacts\publish\Kopeeer.App`.
- It writes the installer to `artifacts\installer`.
- It installs into `Program Files` and requires administrator approval.
- It registers the native Explorer right-drag Shell Extension machine-wide.
- It also registers classic right-click fallback commands for files and folders.
- It uses separate copy and cut icon assets for the Explorer menu entries.
- It does not start Kopeeer after installation.
- It installs the Explorer Shell Extension into a versioned folder so upgrades do not have to close Explorer just to replace a loaded DLL.

## Build Installer

Requirements:

- Windows 10 or Windows 11.
- .NET 8 SDK or newer.
- Inno Setup 6.

Build:

```powershell
scripts\build-installer.ps1
```

Optional custom version:

```powershell
scripts\build-installer.ps1 -Version "1.0.0"
```

Optional custom Inno Setup compiler path:

```powershell
scripts\build-installer.ps1 -InnoCompilerPath "C:\Path\To\ISCC.exe"
```

Expected output:

```text
artifacts\installer\Kopeeer-Setup-1.0.0.exe
```

Before publishing a release, install the generated EXE on a Windows test machine and confirm Explorer copy/move behavior, upgrade behavior, and uninstall cleanup.

## Installer Goals

- Install the app.
- Register 64-bit Shell Extension components.
- Register Explorer menu entries.
- Avoid closing Explorer during normal install and upgrade.
- Cleanly unregister everything on uninstall.
- Make integration choices visible instead of silently modifying Explorer.

## Installer Options

Current option:

- Add Kopeeer to Explorer right-drag menus.

## Language Strategy

The installer text is English only for now.

The installer should clearly explain:

- What Explorer integration means.
- Which context menu entries will be added.
- That no separate app window needs to stay open.
- How Explorer integration can be removed again.

Future multilingual installer support can be added later, but it should not complicate the first release.

## Uninstall Requirements

Uninstall must:

- Stop the app if it is running.
- Unregister Shell Extensions.
- Remove context menu entries.
- Leave no stale Explorer menu entries after Explorer is restarted.

## Risks

- Explorer may keep old Shell Extension DLLs loaded until Explorer is restarted.
- Uninstall may require Explorer restart, sign-out, or reboot to release an already-loaded DLL.
- Broken registration can leave stale menu entries.
- Windows 10 and Windows 11 behavior must both be tested.

## Release Signing

A public release should eventually be code-signed.

Unsigned Shell Extensions and installers can look alarming and may reduce trust. Signing is not required for the first stable release, but it should be planned before broader public distribution.

## References

- WiX Toolset documentation: [Using WiX](https://docs.firegiant.com/wix/using-wix/)
- Microsoft: [Creating Shell Extension Handlers](https://learn.microsoft.com/en-us/windows/win32/shell/handlers)
