# Error Handling

The SDK centralizes error handling so consumers get consistent failures across all module clients. Module clients should not parse HTTP failures independently.

## Current Behavior

All shared HTTP calls pass through the internal transport. When the API returns a non-success HTTP status code, the transport:

1. Reads the response body as a structured transport error payload when possible.
2. Falls back to a generic message when the body is missing, malformed, or not JSON.
3. Creates a `PublishingPlatformErrorContext`.
4. Throws the exception returned by `IPublishingPlatformErrorMapper`.

The error context includes:

- `Method`
- `RelativePath`
- `StatusCode`
- `Message`
- `CorrelationId`

The default mapper returns an `ApiException` that includes the HTTP status code and normalized message.

## Fallback Messages

If the response body does not contain a usable error message, the SDK returns a generic message in this shape:

```text
HTTP {statusCode} returned by Publishing Platform API.
```

This avoids exposing raw, unstructured response bodies while still giving consumers enough information to understand the failure category.

## Custom Error Mapping

Consumers can provide a custom `IPublishingPlatformErrorMapper` through `PublishingPlatformClientOptions` or `PublishingPlatformClientBuilder.WithErrorMapper`.

Custom mappers should use `PublishingPlatformErrorContext` to convert API failures into application-specific exceptions. They should preserve important context such as status code, request path, method, and correlation ID.

## Cancellation

SDK methods should continue to accept and forward `CancellationToken` values. Cancellation should not be wrapped as an API error. Callers should be able to distinguish a cancelled operation from a failed API request.

## Planned Query And Search Errors

The planned search API may combine the local SDK-managed library and an internal Google Books API adapter. Public search errors should stay SDK-owned.

Expected categories include:

- Invalid search input.
- Local index unavailable, unreadable, or corrupt.
- Local metadata record unavailable or unreadable.
- External search source unavailable.
- Partial search results when one source fails but another succeeds.

Google Books API error payloads should not be exposed directly. The SDK should normalize them into platform search failures or partial-result metadata.

## Planned Upload And Local Storage Errors

The first upload implementation is local storage only. Upload errors should describe local library failures clearly before any remote API behavior is introduced.

Expected categories include:

- Missing or invalid upload root configuration.
- Upload root cannot be created or written.
- Source file does not exist.
- Source file cannot be read.
- Unsupported file format.
- Stored file, batch folder, or manifest cannot be written.
- Metadata extraction fails.
- Local index cannot be updated.
- Duplicate or invalid upload metadata.

The SDK should fail fast for obvious argument and configuration problems, then use workflow-specific exceptions for meaningful local storage, metadata, and indexing failures.

## Design Direction

Error handling should continue moving toward stronger typed exceptions while preserving one normalization path. Consumers should not need to parse raw HTTP responses, provider responses, local filesystem errors, metadata extraction errors, or index update failures to understand SDK failures.
