# Contributing

Thank you for caring about this project.

The product display name is currently `Kopeeer`, but the final name is still undecided. Contributions should avoid hardcoding the temporary name where a neutral internal term works.

The project is intentionally small. Contributions should protect that feeling: calm, reliable, minimal, and deeply respectful of normal Windows behavior.

## Current Project Stage

The project is in concept and architecture phase.

Good early contributions:

- Windows Shell Extension research.
- Small architecture notes with sources.
- Queue and worker design discussion.
- Installer and registration cleanup notes.
- Clear bug reports from real Windows tests.

Please avoid large feature proposals before the core integration is proven.

## Principles

- Normal Explorer behavior must remain normal.
- Shell Extension code should stay small and defensive.
- No telemetry, ads, cloud requirement, or account system.
- User-facing wording should be plain and calm.
- Logs should be local and useful.
- Errors should be honest.

## Development Notes

The expected long-term repository shape is documented in [docs/concept.md](docs/concept.md).

Before implementation starts, decisions around C++/COM, .NET UI, worker boundaries, and installer registration should be validated on Windows 10 and Windows 11.

## Pull Requests

For future code changes:

- Keep pull requests focused.
- Describe behavior changes clearly.
- Include manual Windows test notes when Shell integration is touched.
- Do not mix formatting-only changes with behavior changes.
