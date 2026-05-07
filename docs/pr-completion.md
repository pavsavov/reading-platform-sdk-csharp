# Summary

Add strongly typed request models for the upcoming `BookAccess`, `BookAnalytics`, `BookAuditLogs`, `BookAssets`, and `Webhooks` module operations, and expand internal Google Books contract payloads with explicit static manual mapping helpers.

What changed:
- Added new public request models:
  - `BookAccessGrantRequest`, `BookAccessRevokeRequest`
  - `GetBookAnalyticsRequest`
  - `ListBookAuditLogsRequest`
  - `UploadBookAssetRequest`, `DeleteBookAssetRequest`
  - `RegisterWebhookRequest`, `UpdateWebhookRequest`
- Expanded internal Google Books wire contracts with broader payload shapes:
  - `GoogleBooksVolumesResponsePayload`
  - `GoogleBooksImageLinksPayload`
  - `GoogleBooksIndustryIdentifierPayload`
  - extended `GoogleBooksVolumeInfoPayload` fields
- Extended static manual mapping with collection mapping (`ToBooks`) and category-to-tag propagation, while keeping provider models internal.
- Updated architecture boundary and contract tests for the new internal payloads and mapping behavior.
- Updated README module status sections to document available strongly typed request models for upcoming operations.

API impact:
- Public SDK model surface expanded with new `*Request` types.
- No new public client methods were introduced in this ticket.
- Google Books provider contracts remain internal and non-leaking.

# Validation

- [x] `dotnet test tests/PublishingPlatform.SDK.Tests/PublishingPlatform.SDK.Tests.csproj -v minimal`
- [x] Added/updated tests for request model defaults, Google mapping behavior, and internal visibility boundaries

Test result evidence:
- Passed: 128
- Failed: 0
- Skipped: 0

# Release Please Override (Required for releasable changes)

```text
BEGIN_COMMIT_OVERRIDE
feat: add strongly typed request models and expand internal google books mappings
END_COMMIT_OVERRIDE
```

# Manual NuGet Publish

1. Confirm `Directory.Build.props` version equals the intended release version.
2. Create matching tag `v<version>` on the exact commit to publish.
3. Run the `Publish NuGet` workflow manually with the same tag.
4. Verify the published package version appears on NuGet.
