# Summary

Describe what this PR changes and why.

# Validation

- [ ] `dotnet test tests/PublishingPlatform.SDK.Tests/PublishingPlatform.SDK.Tests.csproj -v minimal`
- [ ] Any additional checks relevant to this PR

# Release Please Override (Required for releasable changes)

This repository keeps PR/commit titles in `RPS-<id> type: subject` format.  
If this PR should trigger a Release Please release PR, add one override block below.

Use one of these prefixes in the override line:

- `feat: ...` (minor)
- `fix: ...` (patch)
- `feat!: ...` or `fix!: ...` (major/breaking)

Template:

```text
BEGIN_COMMIT_OVERRIDE
feat: short conventional summary
END_COMMIT_OVERRIDE
```

Breaking-change template:

```text
BEGIN_COMMIT_OVERRIDE
feat!: short conventional summary
BREAKING CHANGE: explain the breaking behavior change.
END_COMMIT_OVERRIDE
```

If this PR is docs/chore/test-only and should not trigger a release, leave this section unchanged.
