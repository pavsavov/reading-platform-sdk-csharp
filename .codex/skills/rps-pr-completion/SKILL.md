---
name: rps-pr-completion
description: Generate final PR completion artifacts for this repo, including a ticket-prefixed commit message and a Markdown PR summary with a Release Please BEGIN_COMMIT_OVERRIDE block.
---

# rps-pr-completion

## Goal

Produce two final delivery artifacts for a completed ticket:

1. A proper commit message using the repository ticket format.
2. A `.md`-formatted PR summary that includes a Release Please override block.

## When to Use

Use this skill when implementation work is done and the user asks for:

- commit message generation,
- PR summary generation,
- release-note-ready completion text,
- Release Please override text.

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

The override section must contain this exact block format:

```text
BEGIN_COMMIT_OVERRIDE
feat!: expand SDK initialization/module documentation and add runnable basic usage examples
END_COMMIT_OVERRIDE
```

Replace only the middle line with the correct conventional commit for the current PR. Keep `BEGIN_COMMIT_OVERRIDE` and `END_COMMIT_OVERRIDE` unchanged.

## Release Please Mapping Rule

- For release-triggering changes, override line should start with:
  - `feat:`, `fix:`, `feat!:`, or `fix!:`.
- For non-releasable-only changes (`docs`, `chore`, `test`, `refactor`), still provide the section but explicitly state no override is required unless the team wants release-note inclusion.

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
