# Contribution Guide

This guide defines how changes are proposed, versioned, and released for the Publishing Platform SDK.

## Release and control model

This repository uses automated versioning with manual control:

- Developers control code changes through pull requests.
- Automation (Release Please) controls versioning and changelog generation.
- Maintainers control release approval by reviewing and merging release PRs.

## Pull request workflow

1. Create a feature branch.
2. Make code changes and tests.
3. Open a pull request to `main`.
4. Ensure CI checks pass.
5. Merge after review approval.

Do not manually:

- bump version numbers
- create release tags
- prepare manual release commits

## Commit convention (required)

This repository requires ticket-prefixed commit messages.

Use this commit format:

- `RPS-<number> type: short description`
- optional scope: `RPS-<number> type(scope): short description`
- optional breaking marker: `RPS-<number> type(scope)!: short description`

Supported types:

- `feat`: new feature (minor bump)
- `fix`: bug fix (patch bump)
- `feat!`: breaking change (major bump)
- `docs`: documentation change
- `test`: test changes
- `refactor`: internal refactor without behavior change
- `chore`: maintenance

Examples:

- `RPS-4 docs: update README examples`
- `RPS-21 feat!: rename PublishingPlatformClient`
- `RPS-8 fix(api): handle null API response`
- `RPS-11 chore(ci): adjust workflow permissions`

These will fail:

- `docs: update README examples` (missing ticket)
- `RPS-4 update README examples` (missing type and colon)

Release automation commits are exempt from this rule:

- `chore(main): release X.Y.Z`

## PR title convention (required)

To preserve Release Please bump detection, PR titles must use conventional format with ticket scope:

- `type(rps-<number>): short description`
- breaking changes: `type(rps-<number>)!: short description`

Examples:

- `feat(rps-4): add release workflow`
- `docs(rps-21): update contribution guide`

## Versioning and changelog

Versioning follows Semantic Versioning:

- `MAJOR.MINOR.PATCH`

Automated bump mapping:

- `fix` -> patch
- `feat` -> minor
- `feat!` or `BREAKING CHANGE` -> major

`CHANGELOG.md` is maintained by Release Please based on merged conventional commits.

## Release flow

1. Changes are merged to `main`.
2. Release Please analyzes commits.
3. Release Please creates or updates a release PR.
4. The release PR updates:
   - `CHANGELOG.md`
   - `Directory.Build.props` version
5. Maintainer reviews release PR correctness.
6. Maintainer merges release PR.
7. Release Please creates a `vX.Y.Z` tag.
8. Tag-triggered workflow publishes to NuGet.org.

## CI/CD responsibilities

CI/CD validates:

- build
- unit tests
- dependency vulnerability audit
- static analysis (Sonar when token is configured)
- documentation presence (`README.md`, `CHANGELOG.md`)
- commit convention on pull requests
- PR title convention on pull requests

CI/CD automates:

- release PR generation
- changelog updates
- version updates
- release tag creation
- NuGet publish on release tags

CI/CD does not:

- create feature commits
- merge pull requests
- bypass maintainer approval

## Enforcement timeline

- Grace period (warning only): until `2026-05-17`
- Hard enforcement (failing checks): starts on `2026-05-17`

## Required repository secrets

Configure these secrets in GitHub Actions:

- `RELEASE_PLEASE_TOKEN`: a PAT used by Release Please (required so downstream workflows trigger from automation-created tags/PRs).
- `NUGET_API_KEY`: NuGet.org API key for package publishing.
- `SONAR_TOKEN`: optional token for Sonar analysis.

## Notes for maintainers

- Keep branch protection enabled on `main`.
- Require CI checks before merge.
- Require squash merge.
- Enable "Default to PR title for squash merge commits."
- Keep PR titles conventional so release classification stays clean.
