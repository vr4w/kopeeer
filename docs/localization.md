# Localization

Version 0.1 should be English-first and localization-ready.

This means the project should not implement a full language system before it is useful, but it should avoid choices that make localization painful later.

## Version 0.1 Language

Use English for:

- README.
- UI text.
- Installer text.
- GitHub issue templates.
- Changelog.
- Documentation.

Do not create German documentation yet unless it is explicitly requested.

If a German README is added later, use `README.de.md` and link it from the top of `README.md`.

## Code Guidelines

Do not scatter user-facing strings randomly across the codebase.

Keep UI strings centralized where possible:

- Tray app labels.
- Button text.
- Status names shown to users.
- Error messages.
- Installer-facing descriptions where the installer stack allows it.
- Shortcut descriptions and help text.

Internal enum values, storage keys, and protocol messages should remain stable and language-neutral. User-facing labels can be mapped from those internal values.

## Future Languages

Preferred later direction:

- English as default.
- German as optional language.
- Language selector in settings.

Do not ask for language during every normal interaction.

Do not use flag icons for language selection. Language is not the same as country.

## Version 0.1 Non-goals

- No full language switcher unless it is trivial in the chosen stack.
- No German documentation set.
- No multilingual installer unless the installer stack makes it very low-risk.
- No runtime language architecture that distracts from the queue, worker, and Explorer integration.

