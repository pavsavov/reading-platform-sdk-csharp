# Pagination

The SDK exposes two shared pagination primitives in `PublishingPlatform.SDK.Models.Common`:

- `PaginationRequest`: shared page settings (`PageSize`, `ContinuationToken`).
- `PagedResult<T>`: shared paged payload envelope (`Items`, `TotalCount`, `ContinuationToken`).

List request models for books, book access, audit logs, and webhooks reuse `PaginationRequest` so paging behavior stays consistent across modules.

For page-by-page control, call `ListAsync(...)` on each module and handle `ContinuationToken` manually.

For automatic cross-page iteration, call `ListAllAsync(...)`:

- `IBooksClient.ListAllAsync(...)`
- `IBookAccessClient.ListAllAsync(...)`
- `IBookAuditLogsClient.ListAllAsync(...)`
- `IWebhooksClient.ListAllAsync(...)`

`ListAllAsync(...)` returns `IAsyncEnumerable<T>` and continues until the backend response does not include a continuation token.
