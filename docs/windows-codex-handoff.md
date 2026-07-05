# Windows Codex Handoff: Kopeeer

This handoff is written for a fresh Codex thread on a Windows laptop.

## Project

Kopeeer is an early alpha Windows utility for queued copy and move operations from Windows Explorer.

Product idea:

- User starts file operations from Explorer.
- Kopeeer collects copy/move jobs in a calm queue.
- Jobs are processed sequentially, one after another.
- The app should stay much simpler than TeraCopy.
- No cloud sync, no telemetry, no ads, no clipboard manager, no file sharing.

Current public working name: `Kopeeer`.

Repository codename: `file-operation-queue`.

Repository: `https://github.com/carlhutson-ux/kopeeer.git`

## Current Git State

Branch: `main`

Remote: `origin https://github.com/carlhutson-ux/kopeeer.git`

Current verified commit before this handoff file was created:

```text
bce9c93e3636ad947d64a1fc0fbc43edacaa4301
```

Latest commit message before this handoff:

```text
bce9c93 Simplify Windows CI build
```

Existing tag:

```text
v0.2.0-alpha
```

The local repository was clean before this handoff file was added.

## What Has Been Implemented

Buildable .NET app:

- `Kopeeer.sln`
- `src/Kopeeer.App/Kopeeer.App.csproj`
- `src/Kopeeer.Core/Kopeeer.Core.csproj`
- `src/Kopeeer.Worker/Kopeeer.Worker.csproj`

App behavior:

- WinForms app targeting `net8.0-windows`.
- Manual source file/folder selection.
- Manual target folder selection.
- Copy or move operation selection.
- In-memory queue.
- Sequential queue processing.
- Basic status display.
- Basic log file at `logs\kopeeer.log`.
- Existing target files/folders are not overwritten silently.
- Failures are shown in the queue.

Explorer alpha behavior:

- Current-user context menu entries for files and folders.
- `Copy with Kopeeer...`
- `Move with Kopeeer...`
- Target folder picker after Explorer command.
- Queue starts processing after target selection.
- Copy and move context menu entries have separate icon assets:
  - `src/Kopeeer.App/Assets/copy.ico`
  - `src/Kopeeer.App/Assets/cut.ico`
- Single-instance handoff exists:
  - If Kopeeer is already running, new Explorer enqueue requests should be sent to the running app through a named pipe.

Installer alpha behavior:

- Inno Setup script exists at `installer/inno/Kopeeer.iss`.
- Builder script exists at `scripts/build-installer.ps1`.
- Installer installs into the current user's local app folder.
- Installer should not require admin rights.
- Installer has option `Add Explorer context menu commands`.
- Installer does not install a native Shell Extension.
- Installer does not register COM.
- Installer does not implement drag-and-drop interception.

CI:

- `.github/workflows/windows-build.yml` was simplified.
- It now only restores and builds:

```powershell
dotnet restore src\Kopeeer.App\Kopeeer.App.csproj
dotnet build src\Kopeeer.App\Kopeeer.App.csproj --configuration Release --no-restore
```

- Installer build/release publishing was removed from CI for now because it caused early-alpha failure noise.

## Important Decisions Already Made

Architecture:

- Robustness over quick hacks.
- Keep Explorer integration cautious.
- Do not ship untested deep Explorer hooks.
- Keep the app, queue model, worker, and Windows integration separated.

Current technical direction:

- Main app/UI: .NET WinForms for the alpha.
- Queue core: .NET.
- Worker: .NET.
- Explorer context menu alpha: current-user registry verbs.
- Future deep Explorer integration: likely native C++/Win32/COM Shell Extension spike.
- Shell Extension must do minimal work inside Explorer and hand off to the app/worker.
- Named pipes are the current IPC direction.

Branding/language:

- Current product display name: `Kopeeer`.
- Keep branding easy to rename.
- README and docs are English-first.
- Future localization should be prepared but not implemented yet.

Non-goals:

- No cloud sync.
- No clipboard manager.
- No file sharing.
- No TeraCopy-style complex transfer manager.
- No speed benchmark focus.
- No production-ready Shell Extension yet.
- No untested COM registration in the default installer.

## Files And Areas Changed So Far

Important project files:

- `README.md`
- `CHANGELOG.md`
- `ROADMAP.md`
- `Kopeeer.sln`
- `Directory.Build.props`
- `.github/workflows/windows-build.yml`

App:

- `src/Kopeeer.App/Program.cs`
- `src/Kopeeer.App/MainForm.cs`
- `src/Kopeeer.App/StartupQueueRequest.cs`
- `src/Kopeeer.App/SingleInstanceCoordinator.cs`
- `src/Kopeeer.App/Kopeeer.App.csproj`
- `src/Kopeeer.App/Assets/copy.ico`
- `src/Kopeeer.App/Assets/cut.ico`

Core:

- `src/Kopeeer.Core/InMemoryJobQueue.cs`
- `src/Kopeeer.Core/QueueJob.cs`
- `src/Kopeeer.Core/FileOperationType.cs`
- `src/Kopeeer.Core/JobStatus.cs`

Worker:

- `src/Kopeeer.Worker/FileOperationProcessor.cs`
- `src/Kopeeer.Worker/SequentialQueueProcessor.cs`
- `src/Kopeeer.Worker/FileJobLogger.cs`
- `src/Kopeeer.Worker/IJobLogger.cs`

Installer/scripts:

- `installer/inno/Kopeeer.iss`
- `scripts/build-installer.ps1`
- `scripts/build.ps1`
- `scripts/run.ps1`
- `scripts/register-context-menu.ps1`
- `scripts/unregister-context-menu.ps1`

Docs:

- `docs/architecture.md`
- `docs/architecture-decision.md`
- `docs/windows-integration.md`
- `docs/windows-test-plan.md`
- `docs/drag-drop-explorer-hook.md`
- `docs/installer.md`
- `docs/branding.md`
- `docs/localization.md`
- `docs/concept.md`
- `docs/shortcuts.md`

Validation:

- `tools/validate_project.py`

Obsolete duplicate projects:

- `src/FileOperationQueue.App`
- `src/FileOperationQueue.Core`
- `tests/FileOperationQueue.Core.Tests`

These were removed earlier. The intended structure is `Kopeeer.*` only.

## What Must Be Tested On Windows

Basic build:

```powershell
git clone https://github.com/carlhutson-ux/kopeeer.git
cd kopeeer
dotnet restore src\Kopeeer.App\Kopeeer.App.csproj
dotnet build src\Kopeeer.App\Kopeeer.App.csproj --configuration Release
```

Run app:

```powershell
dotnet run --project src\Kopeeer.App\Kopeeer.App.csproj
```

Manual app test:

- App opens.
- Manual source file selection works.
- Manual source folder selection works.
- Target folder selection works.
- Copy job can be added.
- Move job can be added.
- Queue processes jobs sequentially.
- Existing target conflicts fail clearly.
- App does not crash.
- Log file is written to `logs\kopeeer.log`.

Context menu dev script test:

```powershell
dotnet publish src\Kopeeer.App\Kopeeer.App.csproj -c Release -o artifacts\publish\Kopeeer.App
powershell -ExecutionPolicy Bypass -File scripts\unregister-context-menu.ps1
powershell -ExecutionPolicy Bypass -File scripts\register-context-menu.ps1 -AppExePath "artifacts\publish\Kopeeer.App\Kopeeer.App.exe"
```

Test:

- Right-click a file.
- `Copy with Kopeeer...` appears.
- `Move with Kopeeer...` appears.
- Copy entry shows copy icon.
- Move entry shows cut icon.
- Choosing either opens Kopeeer and asks for target folder.
- Job appears in queue.
- Job starts processing automatically.
- Repeat with folder.
- Repeat with path containing spaces.
- Repeat while Kopeeer is already running:
  - The new request should go to the existing window.
  - It should not create multiple unrelated app windows.

Installer test:

```powershell
scripts\build-installer.ps1
```

Then run:

```powershell
artifacts\installer\Kopeeer-Setup-0.2.0-alpha.exe
```

Test:

- Installer completes without admin rights.
- Installer shows `Add Explorer context menu commands`.
- With the option enabled, context menu entries appear.
- Copy/move entries work after install.
- Uninstall removes the app and context menu entries.

GitHub Actions:

- Check that the simplified CI passes after commit `bce9c93`.
- Node.js runtime warnings for GitHub actions may appear; treat them as platform warnings unless a step fails.

## Drag-and-drop / Original Product Goal

The user ultimately wants this behavior:

- Drag files/folders in Explorer.
- Hold a modifier key, originally discussed as `ALT + SHIFT`.
- Drop onto target folder.
- Kopeeer queues the operation instead of Explorer starting an uncontrolled copy/move.
- No extra chooser asking whether to use Windows or Kopeeer.
- Tool should feel almost invisible.

Current status:

- This is not implemented yet.
- The current alpha uses context menu integration only.
- Do not ship a blind Explorer hook.
- Do not add COM registration casually.
- Do not modify Windows Explorer behavior before a native Windows prototype proves the hook is safe.

Next technical step:

Create a small native Windows spike to answer only this:

- Can a Shell Extension reliably see source items, target folder, and modifier state at drop time?
- Can it queue into Kopeeer through IPC?
- Can it prevent the default Explorer operation only when the modifier is held?
- Does it behave consistently on Windows 10 and Windows 11?

Recommended first spike:

- Native C++/Win32/COM Shell Extension prototype.
- No real file copying inside Explorer.
- No default installer integration yet.
- Use named pipe handoff to Kopeeer.
- Keep it reversible and isolated.

Relevant doc:

- `docs/drag-drop-explorer-hook.md`
- `docs/windows-integration.md`

## Known Blockers / Problems So Far

Mac development limitation:

- The current Mac environment does not have `dotnet`.
- The current Mac environment does not have PowerShell Core.
- The current Mac environment does not have Inno Setup.
- Therefore the app, installer, and Explorer integration cannot be fully built/tested on the Mac.

Commands attempted on Mac:

```zsh
dotnet restore src/Kopeeer.App/Kopeeer.App.csproj
```

Result:

```text
zsh:1: command not found: dotnet
```

Previous GitHub Actions problem:

- CI was too ambitious.
- It restored/built solution, validated project structure, installed Inno Setup, built installer, uploaded artifact, and tried to publish release on tags.
- This caused failure emails during early alpha.
- Fixed in commit `bce9c93` by reducing CI to restore/build of `src/Kopeeer.App/Kopeeer.App.csproj`.

Earlier context menu problem:

- Explorer showed only `Copy with Kopeeer...` and no move command.
- Clicking produced a Windows association error.
- Fixed by using `reg.exe` with explicit default command values.

## Last Commands Executed In This Thread

Verification/read commands:

```zsh
git status --short --branch
git log --oneline -8
git remote -v
rg --files
sed -n '1,180p' CHANGELOG.md
sed -n '1,130p' .github/workflows/windows-build.yml
git rev-parse HEAD
git tag --points-at HEAD
git tag --list
sed -n '1,220p' docs/windows-test-plan.md
sed -n '1,180p' README.md
sed -n '1,180p' docs/drag-drop-explorer-hook.md
```

Important previous workflow-fix commands:

```zsh
python3 tools/validate_project.py
git diff --check
ruby -e 'require "yaml"; Dir[".github/**/*.yml"].each { |p| YAML.load_file(p) }; puts "YAML ok"'
dotnet restore src/Kopeeer.App/Kopeeer.App.csproj
git add .github/workflows/windows-build.yml README.md CHANGELOG.md
git commit -m "Simplify Windows CI build"
git push
```

The `dotnet restore` command failed only because `dotnet` is not installed on the Mac.

## Open TODOs

Immediate Windows TODOs:

- Pull latest `main`.
- Run the simple build.
- Check whether GitHub Actions now passes.
- Test context menu icons.
- Test installer option for context menu registration.
- Test single-instance handoff with repeated Explorer requests.
- Test uninstall cleanup.

Product TODOs:

- Make UI smaller and less "tool-like".
- Improve queue persistence beyond in-memory.
- Improve logs and user-facing failure messages.
- Add progress reporting.
- Add cancellation/pause only after safe worker behavior is solid.
- Decide whether installer should start Kopeeer automatically after install.
- Plan code signing before broad public release.

Major next step:

- Start the native Windows drag-and-drop/Shell Extension spike.
- Keep it experimental.
- Do not ship it in the default installer until tested.

## Recommended Prompt For The New Windows Codex Thread

Use this prompt:

```text
We are continuing Kopeeer on a Windows laptop.

Repository:
https://github.com/carlhutson-ux/kopeeer.git

Current branch:
main

Current known commit before this handoff file:
bce9c93e3636ad947d64a1fc0fbc43edacaa4301

Please first run:
git status
dotnet restore src\Kopeeer.App\Kopeeer.App.csproj
dotnet build src\Kopeeer.App\Kopeeer.App.csproj --configuration Release

Then test:
dotnet run --project src\Kopeeer.App\Kopeeer.App.csproj

After that, test the context menu and installer according to docs\windows-test-plan.md.

Do not implement deep Explorer drag-and-drop yet until the current alpha build, installer, context menu icons, single-instance handoff, and uninstall cleanup are verified on Windows.

Once those are verified, start a small native Windows spike for the original drag-and-drop goal:
hold a modifier while dropping in Explorer, queue the operation in Kopeeer, and do not break normal Explorer behavior.
```
