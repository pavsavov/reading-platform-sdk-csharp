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

This repository requires conventional commit messages.

Use this commit format:

- `type: short description`
- optional scope: `type(scope): short description`
- optional breaking marker: `type(scope)!: short description`

Supported types:

- `feat`: new feature (minor bump)
- `fix`: bug fix (patch bump)
- `feat!`: breaking change (major bump)
- `docs`: documentation change
- `test`: test changes
- `refactor`: internal refactor without behavior change
- `chore`: maintenance

Examples:

- `docs: update README examples`
- `feat!: rename PublishingPlatformClient`
- `fix(api): handle null API response`
- `chore(ci): adjust workflow permissions`

These will fail:

- `RPS-4 docs: update README examples` (ticket prefix no longer allowed)
- `update README examples` (missing type and colon)

Release automation commits are exempt from this rule:

- `chore(main): release X.Y.Z`

## PR title convention (required)

PR titles must include the ticket id from the source branch name.

- `RPS-<number> type: short description`
- optional scope: `RPS-<number> type(scope): short description`
- breaking changes: `RPS-<number> type!: short description`
- the `RPS-<number>` in title must match the ticket in branch name (for example `feature/RPS-21`)

Examples:

- `RPS-21 feat: add release workflow`
- `RPS-21 docs: update contribution guide`

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
7. Release Please creates a `vX.Y.Z` tag and GitHub Release.
8. Release-published workflow publishes to NuGet.org.

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
- NuGet publish on published GitHub releases

CI/CD does not:

- create feature commits
- merge pull requests
- bypass maintainer approval

## Enforcement timeline

- Grace period (warning only): until `2026-05-17`
- Hard enforcement (failing checks): starts on `2026-05-17`

## Repository credentials

The release and quality workflows require repository credentials and external service configuration.

- Do not document credential names, values, or scopes in this public guide.
- Maintainers should configure required credentials in repository settings using least-privilege access.
- Trusted publishing is used for package release authentication.
- If workflow credential setup changes, update maintainer-only operational documentation.

Trusted publishing requirements:

- Configure a nuget.org Trusted Publishing policy for this repository.
- Workflow file name in policy must match `.github/workflows/publish-nuget.yaml`.
- Keep `permissions.id-token: write` in publish workflow.

## Notes for maintainers

- Keep branch protection enabled on `main`.
- Require CI checks before merge.
- Require squash merge.
- Enable "Default to PR title for squash merge commits."
- Keep PR titles in `RPS-<number> type: ...` or `RPS-<number> type(scope): ...` format.
