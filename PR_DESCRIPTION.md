## Summary
This PR implements release automation and contribution-policy alignment for `reading-platform-sdk-csharp`, shifting from manual version decisions to Release Please while keeping manual maintainer approval for actual releases.

## What Changed
- Added automated release/versioning configuration:
  - `release-please-config.json`
  - `.release-please-manifest.json`
  - `Directory.Build.props` as centralized version source
- Added contributor policy guide:
  - `CONTRIBUTE.md`
- Updated Codex skill policy source:
  - `.codex/skills/rps-versioning-releases/SKILL.md`
- Added/updated CI/CD workflows (renamed to `.yaml`):
  - `.github/workflows/build.yaml` (quality checks: build/test/docs/vulnerability/Sonar when token exists)
  - `.github/workflows/commit-convention.yaml` (Conventional Commits enforcement)
  - `.github/workflows/release-please.yaml` (release PR + tag automation)
  - `.github/workflows/publish-nuget.yaml` (tag-triggered NuGet publish)
- Added commitlint config:
  - `commitlint.config.mjs`

## Release Model After This PR
- Developers manually create and merge PRs.
- Release Please automatically determines version bumps and updates changelog/version files.
- Maintainer manually reviews and merges Release PRs.
- Tag creation is automated after Release PR merge.
- NuGet publish is automated from `v*.*.*` tags.

## Required Repository Secrets
- `RELEASE_PLEASE_TOKEN`
- `NUGET_API_KEY`
- `SONAR_TOKEN` (optional, enables Sonar step)

## Validation
- Release Please config JSON is valid.
- Workflow structure and triggers were aligned with the new policy.
- Local compile could not be fully verified in this environment because local SDK is .NET 9 while the solution targets `net10.0`.

## Notes
- Workflow files were renamed from `.yml` to `.yaml` per recommendation.
