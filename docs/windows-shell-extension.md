# Windows Shell Extension

This is an experimental Windows Explorer integration for the original Kopeeer goal:
hold `ALT + SHIFT` while dropping files or folders onto a folder and enqueue the operation in Kopeeer.

It is intentionally not part of the normal installer yet.

## Current Behavior

- Registers a current-user native 64-bit COM drop handler for folder targets.
- Registers a current-user native 64-bit copy hook for folder copy/move operations.
- Registers a current-user right-drag menu handler for folder targets.
- Watches for `ALT + SHIFT` during Explorer drop.
- Extracts dropped file and folder paths from `CF_HDROP`.
- Uses the target folder passed by Explorer through `IPersistFile`.
- Launches `Kopeeer.App.exe --enqueue --operation copy --target "<folder>" --sources ...`.
- Writes a diagnostic log to `%LOCALAPPDATA%\Kopeeer\shell-extension.log`.

The first spike uses copy only. Move behavior should be added only after the drop interception behavior is proven safe.

First Windows finding:

- `Directory\shellex\DropHandler` did not load for normal left-button folder drops on the first Windows 11 test.
- `Directory\shellex\CopyHookHandlers\Kopeeer` also did not appear to load for the tested left-button Explorer copy path.
- The next spike path adds `Directory\shellex\DragDropHandlers\Kopeeer`.
- This is expected to appear when files are dragged with the right mouse button and dropped onto a folder.
- The menu item is `Copy with Kopeeer`.
- Follow-up finding: On the test laptop, existing right-drag handlers are registered under `Folder` and `Drive`, so Kopeeer now registers the right-drag handler under `Directory`, `Folder`, and `Drive`.

## Build

```powershell
dotnet publish src\Kopeeer.App\Kopeeer.App.csproj -c Release -o artifacts\publish\Kopeeer.App
powershell -ExecutionPolicy Bypass -File scripts\build-shell-extension.ps1
```

## Register

```powershell
powershell -ExecutionPolicy Bypass -File scripts\register-shell-extension.ps1
```

Explorer may need a restart, sign-out, or reboot before it loads the handler.

## Unregister

```powershell
powershell -ExecutionPolicy Bypass -File scripts\unregister-shell-extension.ps1
```

## Test

Use throwaway files first.

- Drag a file onto a folder without `ALT + SHIFT`.
- Confirm normal Explorer behavior still works.
- Drag a file onto a folder while holding `ALT + SHIFT`.
- Confirm Kopeeer queues and processes the copy into the target folder.
- Right-drag a file onto a folder.
- Confirm the Explorer drop menu contains `Copy with Kopeeer`.
- Choose `Copy with Kopeeer`.
- Confirm Kopeeer queues and processes the copy into the target folder.
- Repeat with a folder.
- Repeat with multiple files.
- Check `%LOCALAPPDATA%\Kopeeer\shell-extension.log` after each attempt.

If normal Explorer drag and drop breaks while the modifier is not held, immediately unregister the spike and restart Explorer.
