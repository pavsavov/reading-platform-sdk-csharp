# Architecture

This SDK is a reference C# client for a publishing and reading platform. Its public API should stay platform-owned and book-centric: consumers work with publishing-platform concepts, not with raw transport details or third-party provider models.

The current implementation provides the shared client foundation. The deeper book lifecycle operations, local upload library, query workflow, pagination helpers, and richer diagnostics are active design and implementation areas.

## Current State

`IPublishingPlatformClient` is the public facade. It exposes module clients for the book lifecycle:

- `Books`
- `BookContent`
- `BookPublishing`
- `BookDistribution`
- `BookAccess`
- `BookAnalytics`
- `BookAuditLogs`
- `BookAssets`
- `Webhooks`

The module clients are wired through dependency injection and the direct builder path. Their concrete classes currently share the same internal transport foundation, while detailed module operations are still evolving through the implementation backlog.

### Publishing And Distribution Separation

`BookPublishing` and `BookDistribution` are separate by design:

- `BookPublishing` owns lifecycle transitions and answers "is this book published?".
- `BookDistribution` owns downstream delivery workflows and answers "where is this published book delivered?".

Correlation rules:

- Shared `bookId` links lifecycle and distribution concerns.
- Distribution uses `operationId` as the long-running workflow identity.
- Publishing success is a business precondition for distribution at backend level, not a merged client state machine.

This split keeps lifecycle state changes isolated from partner/channel operational failures, enabling safer retries and clearer diagnostics.

Consumers can create the SDK in two supported ways:

- Hosted applications use `AddPublishingPlatformClient()` with `IOptions<PublishingPlatformClientOptions>`.
- Non-hosted applications, scripts, and tests use `PublishingPlatformClientBuilder.Create(options).Build()`.

Both paths use `PublishingPlatformClientOptions` as the central configuration object.

## Shared Transport

All SDK HTTP traffic should flow through a shared internal transport. This keeps request construction, correlation IDs, error normalization, timeout behavior, and resilience behavior consistent across modules.

The transport currently:

- Uses a named `HttpClient` configured from `PublishingPlatformClientOptions`.
- Sends JSON-oriented requests with `Accept: application/json`.
- Adds `X-Correlation-Id` to outgoing requests.
- Sends calls through `IPublishingPlatformResiliencePipeline`.
- Converts non-success HTTP responses through `IPublishingPlatformErrorMapper`.

This central transport is intentional. It reduces per-client drift and makes future behavior such as retry rules, diagnostics, and error policies easier to apply consistently.

## Resilience

Resilience is opt-in. The default behavior is a no-op pipeline so consumers do not get retries or circuit breaking unless they configure them.

When enabled, resilience can include retry, circuit breaker, attempt timeout, and total timeout behavior. Retry behavior must remain conservative: transient failures can be retried, while unsafe operations need idempotency-aware handling before retries are expanded.

## Error Mapping

Error handling is centralized through `IPublishingPlatformErrorMapper`. The default mapper returns an `ApiException` with the HTTP status code and normalized message. Consumers can provide a custom mapper for domain-specific exception types.

The error handling contract should stay module-independent so every client follows the same failure rules.

## Planned Query And Search Flow

The query surface is planned as an SDK-owned API, for example an `IBookQueryClient` or equivalent module. It should return publishing-platform query models, not Google Books API response models.

Google Books API may be used as an internal adapter or search source. It must not become a public provider abstraction or leak Google-specific wire shapes into the SDK surface.

Current implementation direction:

- `IBooksClient` remains the public platform-owned contract.
- Google Books integration is represented only by internal adapter seams and internal DTO-to-model mappings.
- API key oriented Google volume enrichment is prepared internally; OAuth-only mylibrary flows stay deferred.

The first SDK-focused search flow should combine local uploaded files with external book discovery data behind one SDK search API:

```text
Client app
  -> SDK UploadAsync(...)
  -> Local SDK storage
  -> Local metadata extraction and index
  -> SDK SearchAsync(...)
```

Planned search sources:

- Local uploaded files stored by the SDK.
- External search data from an internal Google Books API adapter.

The public search result should explain the source at a platform level when needed, but the provider-specific implementation should remain replaceable.

## Planned Upload And Local Storage Workflow

The first upload implementation is local storage only. It does not upload files to a remote API and does not require a backend service.

For the first implementation, upload means adding a local file into the SDK-managed library so it can be indexed and searched locally.

The upload workflow should be a multi-step SDK workflow:

1. Validate upload configuration.
2. Use a caller-provided upload root directory.
3. Ensure the upload root exists.
4. Validate the file and supported format.
5. Create a unique library item or batch folder.
6. Copy the file into SDK-managed local storage.
7. Extract basic metadata.
8. Update the local metadata index.
9. Return a workflow result that identifies the stored file and indexed metadata.

The recommended convention for consuming projects is `{projectRoot}/assets/uploads`, but the SDK should not silently infer the project root. The caller must configure the upload root path explicitly.

Future remote upload behavior can build on this local storage result, but it should be a separate capability from the first SDK-focused workflow.

## Scope Strategy

The SDK follows a Depth over Breadth strategy. The priority is to make the book lifecycle reliable and ergonomic before expanding into unrelated publishing domains.

This means new modules should be added only when they support the core book lifecycle or a clear extension of it.
