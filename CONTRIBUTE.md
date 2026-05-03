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

This repository uses Conventional Commits.

Use:

- `type: short description`

Supported types:

- `feat`: new feature (minor bump)
- `fix`: bug fix (patch bump)
- `feat!`: breaking change (major bump)
- `docs`: documentation change
- `test`: test changes
- `refactor`: internal refactor without behavior change
- `chore`: maintenance

Examples:

- `feat: add issue analytics`
- `fix: handle null API response`
- `feat!: rename PublishingPlatformClient`
- `docs: update README examples`

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

## Required repository secrets

Configure these secrets in GitHub Actions:

- `RELEASE_PLEASE_TOKEN`: a PAT used by Release Please (required so downstream workflows trigger from automation-created tags/PRs).
- `NUGET_API_KEY`: NuGet.org API key for package publishing.
- `SONAR_TOKEN`: optional token for Sonar analysis.

## Notes for maintainers

- Keep branch protection enabled on `main`.
- Require CI checks before merge.
- Prefer squash-merge with conventional PR titles so release classification stays clean.
