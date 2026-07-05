# Installer

The project needs an installer because deep Windows integration must be registered and removed cleanly.

## Recommended Installer Direction

Evaluate WiX Toolset and Inno Setup for the first serious Windows installer.

WiX strengths:

- Good fit for MSI-based Windows installation.
- Handles registry entries and component ownership explicitly.
- Can model upgrades and uninstallation.
- Suitable for Shell Extension registration when authored carefully.
- More appropriate for this project than a quick archive-based release because Explorer integration must be reversible.

Inno Setup strengths:

- Simpler authoring model.
- Friendly for small Windows utilities.
- Good candidate if the first installer does not need complex MSI behavior.
- Good first candidate for current-user context menu registration while the native Shell Extension remains unproven.

The final choice should be based on registration reliability, uninstall cleanliness, upgrade behavior, and how clearly the installer can explain Explorer integration.

Current repository state:

- `installer/inno/Kopeeer.iss` is a draft Inno Setup script.
- It installs the app from a future publish output folder.
- It registers current-user context menu commands for files and folders.
- It does not install a native Shell Extension or drag-and-drop hook.
- The current alpha context menu registration is tested first through `scripts/register-context-menu.ps1` and `scripts/unregister-context-menu.ps1`.

## Installer Goals

- Install the app and worker.
- Optionally register Explorer integration.
- Register 64-bit Shell Extension components.
- Add start menu entry if useful.
- Optionally start the app with Windows.
- Cleanly unregister everything on uninstall.
- Make integration choices visible instead of silently modifying Explorer.

## Installer Options

Initial options:

- Install tray app and worker.
- Enable Explorer context menu integration.
- Enable experimental drag-and-drop integration only when it is proven and clearly marked.
- Start the app after install.
- Start the app with Windows.

The drag-and-drop hook must remain opt-in and experimental until validated on Windows 10 and Windows 11.

## Language Strategy

Version 0.1 installer text should be English only.

The installer should clearly explain:

- What Explorer integration means.
- Which context menu entries will be added.
- Which shortcuts or modifier workflows are available, if any.
- Whether the app will start with Windows.
- How Explorer integration can be removed again.

Future multilingual installer support can be added later, but it should not complicate the first release.

## Uninstall Requirements

Uninstall must:

- Stop the app and worker.
- Unregister Shell Extensions.
- Remove context menu entries.
- Remove scheduled startup entry if used.
- Leave user logs/settings only if the user chooses to keep them.

## Risks

- Explorer may keep Shell Extension DLLs loaded.
- Uninstall may require Explorer restart, sign-out, or reboot.
- Broken registration can leave stale menu entries.
- Windows 10 and Windows 11 behavior must both be tested.

## Release Signing

A public release should eventually be code-signed.

Unsigned Shell Extensions and installers can look alarming and may reduce trust. Signing is not required for the concept phase, but it should be planned before broad public distribution.

## References

- WiX Toolset documentation: [Using WiX](https://docs.firegiant.com/wix/using-wix/)
- Microsoft: [Creating Shell Extension Handlers](https://learn.microsoft.com/en-us/windows/win32/shell/handlers)
