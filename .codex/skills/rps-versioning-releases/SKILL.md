---
name: rps-versioning-releases
description: Apply semantic versioning, release classification, and changelog rules for this SDK. Use when preparing commits, planning releases, reviewing breaking changes, or updating CHANGELOG.md for user-visible changes.
---

# rps-versioning-releases

## Goal

Keep SDK versioning and release notes consistent, predictable, and consumer-focused with automated versioning and manual release approval.

Control model:

- Developers control code changes through PRs.
- Automation (Release Please) controls version calculation and changelog generation.
- Maintainer controls release approval by reviewing and merging release PRs.

## Versioning Strategy

Use Semantic Versioning:

- `MAJOR.MINOR.PATCH`

## Version Bump Rules

- `MAJOR` -> breaking changes
- `MINOR` -> backward-compatible features
- `PATCH` -> backward-compatible fixes

Conventional-commit mapping:

- `fix:` -> increase `PATCH`
- `feat:` -> increase `MINOR`
- `feat!:` or `BREAKING CHANGE:` -> increase `MAJOR`

Examples:

- `fix: handle 404 API response` -> patch bump
- `feat: add document listing` -> minor bump
- `feat!: rename public SDK client` -> major bump

## Breaking Change Criteria

Treat any of the following as breaking:

- renaming public classes
- renaming public methods
- changing method signatures
- removing public models or properties
- changing exception behavior
- changing default authentication behavior

If uncertain, classify as breaking and call it out explicitly.

## Commit Message Convention

Use repository-specific ticketed commit format for contributor commits.

Format:

- `RPS-<number> type: short description`
- optional scope: `RPS-<number> type(scope): short description`
- optional breaking marker: `RPS-<number> type(scope)!: short description`

Supported types:

- `feat`: new feature
- `fix`: bug fix
- `docs`: documentation only
- `test`: tests
- `refactor`: internal change with no behavior change
- `chore`: maintenance

Breaking change notation:

- `RPS-21 feat!: change API contract`
- or include:
  - `BREAKING CHANGE: <details>`

## PR Title Convention

Use repository ticket-first PR title format:

- `RPS-<number> type: short description`
- optional scope: `RPS-<number> type(scope): short description`
- breaking: `RPS-<number> type!: short description`

## Changelog Policy

`CHANGELOG.md` is maintained automatically by Release Please from conventional commits.

Write entries from the SDK consumer perspective:

- what changed
- why it matters
- how consumers might need to react (if applicable)

Avoid internal-only language that does not help SDK consumers.

Recommended structure:

- `## [Unreleased]`
- `### Added`
- `### Changed`
- `### Fixed`

Additional rules:

- Keep entries concise.
- Include only user-relevant internal work.
- Prefer clear commit messages so generated changelog entries are useful.

## Release Workflow

Release flow is hybrid: manual code review + automated release mechanics.

1. Developer opens PR with ticketed commits and ticket-first PR title.
2. Maintainer reviews and merges PR to `main`.
3. Release Please analyzes merged commits and opens/updates release PR.
4. Release PR updates:
   - `CHANGELOG.md`
   - central version file (`Directory.Build.props`)
5. Maintainer validates changelog + version bump and merges release PR.
6. Release Please creates version tag `vX.Y.Z`.
7. Tag-triggered pipeline publishes NuGet package.

## CI/CD Responsibilities

CI/CD should:

- build
- run unit tests
- run static analysis (for example Sonar)
- run dependency vulnerability checks
- validate key docs (`README`, `CHANGELOG`)
- enforce commit convention checks
- run Release Please on `main`
- publish NuGet only on version tags created by release automation

CI/CD must not:

- create feature commits
- bypass maintainer release approval

## Automation Policy

Use Release Please as the authoritative automation for:

- version bump calculation
- changelog generation
- release PR creation
- release tag creation after approved merge

Contributors must not manually bump versions, manually prepare release versions, or manually create release tags.
