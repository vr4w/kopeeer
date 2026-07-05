# Security

The project is not ready for production use.

## Supported Versions

No released version is supported yet.

| Version | Supported |
| --- | --- |
| 0.1.0 Unreleased | No |

## Reporting A Vulnerability

Please open a private security advisory on GitHub once the repository is public, or contact the maintainer through the published project contact.

Do not post exploit details in a public issue.

## Security Principles

- The app should not send telemetry.
- The app should not require a cloud account.
- Shell Extension code should do as little as possible inside Explorer.
- File operations should be explicit and auditable.
- Installer and uninstaller behavior must be clean and reversible.

## Areas That Need Extra Care

- Shell Extension COM registration.
- IPC between Shell Extension, tray app, and worker.
- Handling paths from Explorer.
- Privilege boundaries.
- File overwrite and conflict handling.
- Cancellation and partial-copy recovery.
