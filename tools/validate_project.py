#!/usr/bin/env python3
from pathlib import Path
import sys

required_files = [
    "README.md",
    "CHANGELOG.md",
    "ROADMAP.md",
    "LICENSE",
    "TESTING-1.0.1.md",
    "Kopeeer.sln",
    "CONTRIBUTING.md",
    "SECURITY.md",
    "docs/concept.md",
    "docs/architecture.md",
    "docs/architecture-decision.md",
    "docs/windows-integration.md",
    "docs/shortcuts.md",
    "docs/installer.md",
    "docs/localization.md",
    "docs/branding.md",
    "docs/windows-test-plan.md",
    ".github/workflows/windows-build.yml",
    "src/Kopeeer.App/Kopeeer.App.csproj",
    "src/Kopeeer.App/MainForm.cs",
    "src/Kopeeer.App/Program.cs",
    "src/Kopeeer.App/SingleInstanceCoordinator.cs",
    "src/Kopeeer.App/StartupQueueRequest.cs",
    "src/Kopeeer.App/Assets/copy.ico",
    "src/Kopeeer.App/Assets/cut.ico",
    "src/Kopeeer.App/Assets/app.ico",
    "src/Kopeeer.Core/Kopeeer.Core.csproj",
    "src/Kopeeer.Core/InMemoryJobQueue.cs",
    "src/Kopeeer.Core/QueueJob.cs",
    "src/Kopeeer.Worker/Kopeeer.Worker.csproj",
    "src/Kopeeer.Worker/FileOperationProcessor.cs",
    "src/Kopeeer.Worker/SequentialQueueProcessor.cs",
    "scripts/build.ps1",
    "scripts/build-installer.ps1",
    "scripts/run.ps1",
    "scripts/diagnose-installation.ps1",
    "scripts/repair-shell-integration.ps1",
    "scripts/register-context-menu.ps1",
    "scripts/unregister-context-menu.ps1",
    "installer/inno/Kopeeer.iss",
    "docs/drag-drop-explorer-hook.md",
]

obsolete_paths = [
    "src/FileOperationQueue.App",
    "src/FileOperationQueue.Core",
    "tests/FileOperationQueue.Core.Tests",
    "tools/windows/install-context-menu-dev.ps1",
    "tools/windows/uninstall-context-menu-dev.ps1",
]

present_obsolete_paths = [path for path in obsolete_paths if Path(path).exists()]
if present_obsolete_paths:
    print("Obsolete duplicate project paths found:")
    for path in present_obsolete_paths:
        print(f"- {path}")
    sys.exit(1)

missing = [path for path in required_files if not Path(path).is_file()]
if missing:
    print("Missing required files:")
    for path in missing:
        print(f"- {path}")
    sys.exit(1)

for path in Path(".").rglob("*"):
    if path.is_file() and ".git" not in path.parts:
        try:
            text = path.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            continue

        if "\t" in text:
            print(f"Tab character found in {path}")
            sys.exit(1)

print("Project structure validation passed.")
