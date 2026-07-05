# Branding

## Name

The final public product name is not decided yet.

Current repository/folder codename:

- `file-operation-queue`

Current product display name placeholder:

- `Kopeeer`

Current preferred candidates:

- Kopeeer
- Kopeer
- Movuq
- Draylo
- Drailo

Other active candidates:

- FileRail
- MoveQ

Branding must stay easy to rename. Code, documentation, installer text, UI strings, and issue templates should avoid assuming that `Kopeeer` is permanent.

## Naming Notes

`Kopeeer` is the recommended current working display name. Spell it exactly as `Kopeeer`: capital K, lowercase remaining letters, triple-e. Do not stylize it as `KoPeeer`, `KOPeeer`, or `KopeeR`.

First-pass public search did not find an obvious direct Windows file copy, file queue, Explorer integration, or software utility using the exact name `Kopeeer`. Related results mostly returned `Kope`, `Kopper`, `Kopieer`, and unrelated GitHub or app results. This makes `Kopeeer` currently stronger than `Kopeer` from a searchability and public branding perspective.

This is not legal trademark clearance. Before public 1.0 release, perform a proper trademark, domain, GitHub, package registry, app store, and trademark database check.

`Kopeer` remains a candidate, but it appears less distinctive than `Kopeeer`.

`Movuq`, `Draylo`, and `Drailo` remain active candidates. They should be evaluated for clarity, pronunciation, searchability, and whether they imply the right Windows file-operation queue.

`FileRail` suggests a simple path for files: not rushed, not scattered, but guided along a track. It has a warmer product feel, but it may be slightly less immediate.

`MoveQ` is short, direct, and close to the queue idea. It may understate copy operations.

## Rejected Candidates

- `Qopy`: Rejected because existing Qopy projects/services already occupy clipboard, file sharing, and file copy-related territory.
- `F.Paste`: Rejected for now because "paste" is strongly associated with clipboard actions, and it may collide conceptually with pastebin, fpaste, and clipboard-related Windows tools.

## Tagline

English:

> Queued copy and move for Windows.

Alternative:

> A calm queue for Windows file operations.

German direction:

> Dateien rein. Reihenfolge drin. Ruhe.

This German line is directional only. Version 0.1 documentation, UI, installer, and issue templates should be English.

## Tone

The product should sound:

- calm
- practical
- friendly
- precise
- honest

Position `Kopeeer` as a Windows file operation queue, not as a clipboard manager. Avoid language that suggests peer-to-peer transfer, cloud sync, or file sharing. The core product promise is local, calm, sequential file operations from Windows Explorer.

It should not sound:

- loud
- enterprise-heavy
- overly technical
- cute at the cost of clarity
- like a performance booster making promises it cannot keep

## UI Feeling

The interface should be small and quiet.

Expected tray app feeling:

- One clear current job.
- A short queue list.
- Plain status labels.
- Useful actions only.
- No dashboards for the sake of dashboards.

The product should not feel like a control room for three PDFs.

## Icon Direction

First icon idea:

- File shape.
- Rail or track line.
- Small arrow.
- Queue hint through two or three subtle stacked lines.

Style:

- Minimal.
- Recognizable at tray size.
- Works in light and dark taskbars.
- Avoid too much detail.

Potential visual language:

- A document outline sitting on a single horizontal rail.
- A small forward arrow aligned with the rail.
- A second faint document behind it to imply queueing.

## Rename Rules

- Keep the product display name in one obvious place once code exists.
- Do not scatter user-facing strings across the codebase.
- Avoid naming internal namespaces, protocols, or storage formats after a temporary public name when a neutral term will work.
- Prefer neutral internal terms such as `FileOperationQueue`, `Queue`, `Job`, `Worker`, and `ShellExtension`.
- Treat `Kopeeer` as display text, not architecture.
- Do not rename code namespaces, packages, executable names, or installer identifiers to `Kopeeer` until the name is finally approved.
