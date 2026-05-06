---
name: rps-pr-completion
description: Generate final PR completion artifacts for this repo, including ticket-prefixed commit messages, PR summary markdown, Release Please override guidance, and manual tag-based NuGet publish notes.
---

# rps-pr-completion

## Goal

Produce final delivery artifacts for a completed ticket:

1. A proper commit message using the repository ticket format.
2. A `.md`-formatted PR summary that includes a Release Please override block.
3. A short manual publish note that keeps tag and package version aligned.

## When to Use

Use this skill when implementation work is done and the user asks for:

- commit message generation,
- PR summary generation,
- release-note-ready completion text,
- Release Please override text.
- manual publish checklist text.

## Current Pipeline Assumptions

This skill assumes the repository currently uses:

- `build.yaml` for CI quality checks.
- `pr-title-convention.yaml` for PR title validation.
- `release-please.yaml` as manual-only (`workflow_dispatch`) for changelog/version PR generation.
- `publish-nuget.yaml` as manual-only (`workflow_dispatch`) with `tag` input.

Important release rule:

- Tag and package version must match (for example tag `v0.0.6` requires `Directory.Build.props` version `0.0.6` at that tagged commit).

## Required Inputs

- Ticket number (for example `47`).
- Commit type (`feat`, `feat!`, `fix`, `fix!`, `docs`, `test`, `refactor`, `chore`).
- Short change subject.
- Summary of what changed.
- Test evidence (commands + result).

If any input is missing, infer from branch/issue context when possible. If not inferable, ask the user.

## Output 1: Commit Message

Use this exact header format:

`RPS-<ticket-number> <type>: <subject>`

Examples:

- `RPS-47 feat: add DI initialization examples`
- `RPS-47 feat!: rename module contracts for consistency`
- `RPS-47 docs: expand README module documentation`

Rules:

- Keep subject concise and action-oriented.
- Use `!` only for breaking changes.
- Keep type lowercase.

## Output 2: PR Summary Markdown

Create a single Markdown document containing:

- `## Title`
- `## Summary`
- `## What Changed`
- `## Testing`
- `## API Impact`
- `## Release Please Override`
- `## Manual NuGet Publish`

If the change should be releasable through Release Please, include this exact block format:

```text
BEGIN_COMMIT_OVERRIDE
fix: short conventional summary
END_COMMIT_OVERRIDE
```

Replace only the middle line with the correct conventional commit for the current PR. Keep `BEGIN_COMMIT_OVERRIDE` and `END_COMMIT_OVERRIDE` unchanged.

If the PR is docs/chore/test/refactor-only and should not affect release semantics, explicitly state that override is optional and omitted by default.

## Release Please Mapping Rule

- For release-triggering changes, override line should start with:
  - `feat:`, `fix:`, `feat!:`, or `fix!:`.
- For non-releasable-only changes (`docs`, `chore`, `test`, `refactor`), still provide the section but explicitly state no override is required unless the team wants release-note inclusion.

## Manual NuGet Publish Guidance (Required in PR Summary)

Always include a short checklist:

1. Confirm `Directory.Build.props` version equals intended release version.
2. Create matching tag `v<version>` on that exact commit.
3. Run `Publish NuGet` workflow manually with that tag.
4. Verify package version appears on NuGet.

## Default Output File

Unless the user specifies another path, write:

`docs/pr-completion-RPS-<ticket-number>.md`

## Quality Checklist

Before finalizing, verify:

- Commit message matches `RPS-<id> <type>: <subject>`.
- PR summary is valid Markdown.
- Override block exists and uses exact delimiters.
- Override conventional line matches intended SemVer impact.
- Testing section contains at least one concrete command/result.
- Manual publish note is present and enforces tag/version match.
