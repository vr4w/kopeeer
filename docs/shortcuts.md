# Shortcuts

The product should use very few shortcuts.

The main interaction should stay inside Windows Explorer and the tray app.

## Planned Explorer Modifier

Preferred idea:

- `ALT + SHIFT` while dropping files or folders onto a destination folder queues the operation in the app.

Status:

- Not implemented.
- Not proven.
- Requires Windows Shell research.

Reasoning:

- Intentional enough to avoid accidents.
- Still close to normal drag-and-drop.
- Does not consume common Explorer shortcuts as aggressively as a single modifier.

## Tray App Shortcuts

Possible future shortcuts:

- `Delete`: remove selected queued job when it has not started.
- `Space`: pause or resume queue.
- `Enter`: open selected job details.
- `Esc`: close details panel.

These should only apply when the app window has focus.

The exact window title should use the current product display name placeholder, `Kopeeer`, until the final name is chosen.

## Shortcut Principles

- No global hotkeys in version 0.1.
- No shortcuts that change normal Explorer behavior unless the integration is explicitly enabled.
- Every destructive action needs visible confirmation or a clear undo-safe boundary.
- Shortcut labels and help text should be centralized with other UI strings so they can be localized later.
