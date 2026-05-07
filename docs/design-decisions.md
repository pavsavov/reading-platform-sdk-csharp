# Design Decisions

This document records the major SDK design decisions that guide current implementation and planned work.

## Depth Over Breadth

Status: Accepted.

The SDK focuses on the book lifecycle before expanding into broader publishing domains. The core modules are books, content, publishing, distribution, access, analytics, audit logs, assets, and webhooks.

This keeps the SDK useful for real partner workflows instead of spreading effort across shallow generic API wrappers.

## REST-Only SDK Surface

Status: Accepted.

SDK communication with the platform is REST-oriented. The SDK should not add non-REST transport alternatives as public consumption paths.

This matches the current shared HTTP transport and keeps authentication, diagnostics, resilience, and error handling centralized.

## Shared Transport Instead Of Per-Client Pipelines

Status: Accepted.

All module clients should use the shared internal HTTP transport. This trades some per-client flexibility for consistency.

The benefit is a single place for correlation IDs, JSON request conventions, resilience, and error normalization. Module clients should focus on request validation, endpoint shape, and response mapping.

## Publishing Lifecycle And Distribution Workflow Separation

Status: Accepted.

Publishing and distribution are separate concerns in the SDK surface.

- Publishing is domain lifecycle state transition (`Publish`, `Unpublish`, `Schedule`, lifecycle status).
- Distribution is operational propagation workflow (`Start`, `GetStatus`, `Retry`, list operations/history).

Both concerns correlate through `bookId`, while distribution additionally introduces an `operationId` for long-running delivery tracking.

This avoids overloaded "publish" behavior, improves retry safety, and isolates downstream partner failures from lifecycle state management.

## Options Pattern And Direct Builder

Status: Accepted.

The SDK supports both hosted and non-hosted consumers.

Hosted applications should use `AddPublishingPlatformClient()` with `IOptions<PublishingPlatformClientOptions>`. Scripts, tests, and simple integrations can use `PublishingPlatformClientBuilder` with a direct options object.

Both paths should remain supported and behaviorally aligned.

## Opt-In Resilience

Status: Accepted.

Resilience is disabled by default. Consumers must explicitly enable retry, circuit breaker, and timeout behavior.

Retries must remain conservative because some operations can have side effects. Future unsafe operations should require idempotency-aware design before they are retried automatically.

## Central Error Mapping

Status: Accepted.

The SDK uses `IPublishingPlatformErrorMapper` as the single extension point for API error conversion.

The default mapper returns `ApiException` for generic API failures and typed book exceptions for common book-centric HTTP failures. Consumers can replace it when they need domain-specific exceptions, but the transport should still provide the same normalized context.

## Provider-Hidden Query Architecture

Status: Planned.

The query API should be platform-owned. Google Books API may be used internally as a search source, but Google-specific models and provider concepts should not appear in the public SDK surface.

This allows the SDK to combine external search data and local uploaded-file metadata behind one query experience. It also keeps the provider replaceable if the search strategy changes.

## Caller-Configured Upload Root

Status: Planned.

The upload workflow should require a caller-provided upload root directory. The recommended convention is `{projectRoot}/assets/uploads`, but the SDK should not silently infer the project root.

This is safer for a NuGet-distributed SDK because consumers may run from web apps, workers, tests, command-line tools, or hosted environments where the current directory is not the project root.

## Local Storage Only Upload MVP

Status: Planned.

The first upload implementation should store files locally only. It should accept a file, validate supported formats, copy the file into SDK-managed local storage, extract basic metadata, update a local index, and return a workflow result.

This lets the SDK prove value independently:

```csharp
await client.UploadAsync(file);
await client.SearchAsync("clean architecture");
```

Remote API upload behavior can be added later as a separate capability. Keeping local storage separate makes the first implementation testable and avoids mixing filesystem, metadata, indexing, and remote API failures too early.

## Safe Diagnostics Defaults

Status: Accepted direction.

Diagnostics should be safe by default. Correlation IDs and Activity-based tracing are useful, but sensitive data such as API keys, auth headers, book content, and user identifiers must not be logged unless an explicit future option allows it.

Diagnostics work is still evolving, so docs and implementation should avoid promising full logging, metrics, or per-module override behavior until those tickets are complete.

## Strong Typing And Manual Mapping

Status: Accepted direction.

Public SDK models should be strongly typed and platform-owned. Mapping should stay explicit and manual through focused code, not AutoMapper or hidden reflection-based mapping.

This keeps the public contract stable and makes future provider integrations, including query sources, easier to isolate.
