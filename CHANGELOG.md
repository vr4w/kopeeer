# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this project intends to follow semantic versioning once releases begin.

## 0.1.0 - Unreleased

### Added

- Initial public project foundation.
- Concept documentation for the queue-first Explorer workflow.
- Architecture overview for the recommended component split and technical risks.
- Architecture decision for the recommended version 0.1 mixed Windows-native architecture.
- Windows integration notes covering Shell Extensions, context menus, drag-and-drop risk, copy hooks, and `IFileOperation`.
- Branding note for current working display name `Kopeeer` and repository codename `file-operation-queue`.
- English-first localization strategy for version 0.1.
- Initial roadmap, contribution, funding, security, and issue template files.

### Known Risks

- Deep drag-and-drop integration from Explorer is not solved yet.
- Copy Hook Handlers are not a full queueing solution because they can approve or block operations, but do not perform the operation.
- Shell Extension registration, unloading, crash isolation, and installer cleanup need dedicated Windows testing.
