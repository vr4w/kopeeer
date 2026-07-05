#!/usr/bin/env python3
from pathlib import Path
import sys

required_files = [
    "README.md",
    "CHANGELOG.md",
    "ROADMAP.md",
    "LICENSE",
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
    "src/FileOperationQueue.Core/FileOperationQueue.Core.csproj",
    "src/FileOperationQueue.Core/Queue/FileOperationJob.cs",
    "src/FileOperationQueue.Core/Queue/OperationQueue.cs",
    "src/FileOperationQueue.Core/Worker/LocalQueueWorker.cs",
    "src/FileOperationQueue.App/FileOperationQueue.App.csproj",
    "src/FileOperationQueue.App/Program.cs",
    "src/FileOperationQueue.App/Commands/CommandLineQueueHandler.cs",
    "src/FileOperationQueue.App/Tray/QueueApplicationContext.cs",
    "src/FileOperationQueue.App/Ui/MainForm.cs",
    "src/Kopeeer.App/Kopeeer.App.csproj",
    "src/Kopeeer.App/MainForm.cs",
    "src/Kopeeer.App/Program.cs",
    "src/Kopeeer.App/StartupQueueRequest.cs",
    "src/Kopeeer.Core/Kopeeer.Core.csproj",
    "src/Kopeeer.Core/InMemoryJobQueue.cs",
    "src/Kopeeer.Core/QueueJob.cs",
    "src/Kopeeer.Worker/Kopeeer.Worker.csproj",
    "src/Kopeeer.Worker/FileOperationProcessor.cs",
    "src/Kopeeer.Worker/SequentialQueueProcessor.cs",
    "scripts/build.ps1",
    "scripts/run.ps1",
    "installer/inno/Kopeeer.iss",
    "tools/windows/install-context-menu-dev.ps1",
    "tools/windows/uninstall-context-menu-dev.ps1",
    "docs/drag-drop-explorer-hook.md",
]

missing = [path for path in required_files if not Path(path).is_file()]
if missing:
    print("Missing required files:")
    for path in missing:
        print(f"- {path}")
    sys.exit(1)

for path in Path(".").rglob("*"):
    if path.is_file() and ".git" not in path.parts:
        text = path.read_text(encoding="utf-8")
        if "\t" in text:
            print(f"Tab character found in {path}")
            sys.exit(1)

print("Project structure validation passed.")
