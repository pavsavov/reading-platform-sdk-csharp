# reading-platform-sdk-csharp

A reference C# SDK for a reading platform, designed to simplify integration for external publishers and partners.

## Configure the client

`PublishingPlatformClientOptions` is the central configuration object.

```csharp
var options = new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
    Timeout = TimeSpan.FromSeconds(30),

    // Optional: resilience is disabled by default.
    Resilience = new PublishingPlatformResilienceOptions
    {
        Enabled = true,
        Retry = new RetryResilienceOptions
        {
            Enabled = true,
            MaxRetryAttempts = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            RetryNonIdempotentMethods = false
        },
        CircuitBreaker = new CircuitBreakerResilienceOptions
        {
            Enabled = true,
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(30)
        }
    }
};
```

## Choose a configuration style

- Use the Options pattern in hosted apps (ASP.NET Core, worker services) when configuration comes from `appsettings`, environment variables, or secrets providers and you want startup-time validation.
- Use direct POCO/builder configuration in non-hosted apps, scripts, tests, or quick integrations where you construct options in code.

## Create a client (no host, direct POCO)

```csharp
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key"
}).Build();
```

## Initialization examples

The root client exposes resource-style modules such as:

- `platformClient.Books`
- `platformClient.BookContent`
- `platformClient.BookPublishing`
- `platformClient.Webhooks`

Planned local upload/search workflow capabilities described in architecture docs are future-facing and are not currently exposed as separate public SDK modules.

### Direct builder initialization

```csharp
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Options;

var platformClient = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
    Timeout = TimeSpan.FromSeconds(30)
}).Build();

var books = platformClient.Books;
var content = platformClient.BookContent;
var publishing = platformClient.BookPublishing;
var webhooks = platformClient.Webhooks;
```

### DI initialization (ASP.NET Core / host apps)

```csharp
using Microsoft.Extensions.DependencyInjection;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Extensions;

var services = new ServiceCollection();

services.AddPublishingPlatformClient(options =>
{
    options.BaseUrl = "https://api.books.example";
    options.ApiKey = "your-api-key";
    options.Timeout = TimeSpan.FromSeconds(30);
});

using var provider = services.BuildServiceProvider();
var platformClient = provider.GetRequiredService<IPublishingPlatformClient>();

var books = platformClient.Books;
var content = platformClient.BookContent;
var publishing = platformClient.BookPublishing;
var webhooks = platformClient.Webhooks;
```

Runnable scripts are available in:

- `examples/BasicUsage/BuilderInitializationExample.csx`
- `examples/BasicUsage/DiInitializationExample.csx`
- `examples/ArchitectureTradeoffs/ArchitectureTradeoffsExample.csx`
- `examples/Diagnostics/README.md`
- `examples/BookPublishing/BookPublishingExample.csx`
- `examples/BookDistribution/BookDistributionExample.csx`
- `examples/BookAnalytics/BookAnalyticsExample.csx`
- `examples/Pagination/PaginationExample.csx`
- `examples/MockApiCoreFlowExample.csx`
- `examples/MockPublishingPlatform.Api/README.md`
- `examples/ResumableUpload/ResumableUploadExample.csx`

## Client modules: available properties and use cases

`IPublishingPlatformClient` is the SDK root and exposes module clients as properties.

### `Books` (`IBooksClient`)

Primary module for active book lifecycle operations.

Available operations:

- `CreateAsync(...)`: create a new book record.
- `GetByIdAsync(...)`: retrieve a single book by ID.
- `ListAsync(...)`: list books with filtering, sorting, and paging.
- `ListAllAsync(...)`: stream books across all pages as `IAsyncEnumerable<Book>`.
- `UpdateMetadataAsync(...)`: replace mutable metadata.
- `PatchMetadataAsync(...)`: partially update mutable metadata.
- `DeleteAsync(...)`: remove a book by ID.

Typical use cases:

- Catalog ingestion and onboarding of new titles.
- Internal back-office CRUD flows.
- Scheduled sync jobs that iterate the full catalog (`ListAllAsync`).
- Metadata correction workflows with optimistic concurrency tokens.

### `BookContent` (`IBookContentClient`)

Module accessor for book content workflows (for example upload, replacement, or content-related actions).

Available operations:

- `GetAsync(...)`: retrieve content metadata for a book.
- `UploadOrReplaceAsync(...)`: upload or replace book content and return the updated content metadata.
- `StartResumableUploadAsync(...)`: create a resumable upload session.
- `UploadChunkAsync(...)`: upload one chunk to an existing session.
- `GetUploadSessionAsync(...)`: query resumable upload progress/state.
- `CompleteResumableUploadAsync(...)`: finalize a resumable upload and materialize content metadata.

Typical use cases:

- Uploading source content for books.
- Replacing or versioning stored content artifacts.
- Resuming interrupted large uploads by session and byte range.

### `BookPublishing` (`IBookPublishingClient`)

Module accessor for publish-oriented workflows.

Available operations:

- `PublishAsync(...)`: publish a book and return current publishing state.
- `UnpublishAsync(...)`: reverse publication and return current publishing state.
- `ScheduleAsync(...)`: schedule publication at a specific date/time.
- `GetStatusAsync(...)`: fetch the current publishing status for a book.

Typical use cases:

- Triggering publication after metadata/content validation is complete.
- Scheduling time-based release windows.
- Operational checks for current publication state and failures.

### `BookDistribution` (`IBookDistributionClient`)

Module accessor for downstream distribution workflows.

Available operations:

- `StartAsync(...)`: start distribution to one or more channels.
- `GetStatusAsync(...)`: fetch the current state of a distribution operation.
- `RetryAsync(...)`: retry a previous distribution operation.
- `ListAsync(...)`: list tracked distribution operations for a book.

Typical use cases:

- Pushing published books to partner channels, mobile catalog feeds, and CDN-oriented pipelines.
- Tracking long-running delivery progress and failures by operation id.
- Retrying failed distribution workflows without re-running the publishing lifecycle.

### `BookAccess` (`IBookAccessClient`)

Module accessor for access-control and entitlement-related workflows.

Available operations:

- `GrantAsync(...)`: grant entitlement for a principal to a specific book.
- `RevokeAsync(...)`: revoke entitlement for a principal and book.
- `CheckAsync(...)`: check effective access for a principal and book.
- `ListAsync(...)`: list access grants for a specific book or globally.
- `ListAllAsync(...)`: stream access grants across all result pages.

Typical use cases:

- Access policy assignment.
- Reader or tenant entitlement operations.
- Support diagnostics for entitlement-denied scenarios.

### `BookAnalytics` (`IBookAnalyticsClient`)

Module accessor for analytics and reporting workflows.

Current status:

- Property is available on the root client.
- `GetSummaryAsync(GetBookAnalyticsRequest, CancellationToken)` is available for aggregate analytics retrieval.
- Current request-to-query mapping sends `bookId`, `from`, and `to` to `/book-analytics/summary`.
- `Granularity` and `IncludeUniqueReaders` exist on `GetBookAnalyticsRequest` as future-facing fields and are currently not sent until the backend contract expands.

Typical use cases once expanded:

- Consumption/performance reporting.
- Title-level engagement insights.

### `BookAuditLogs` (`IBookAuditLogsClient`)

Module accessor for audit trail workflows.

Available operations:

- `ListAsync(...)`: list paged audit log entries with optional filtering criteria.
- `ListAllAsync(...)`: stream audit log entries across all result pages.

Typical use cases:

- Compliance-oriented activity history.
- Operational troubleshooting and traceability.

### `Webhooks` (`IWebhooksClient`)

Module accessor for webhook-oriented integration workflows.

Current status:

- Property is available on the root client.
- `RegisterAsync(RegisterWebhookRequest, idempotencyKey, CancellationToken)` registers an HTTPS callback endpoint.
- `UpdateAsync(UpdateWebhookRequest, CancellationToken)` updates endpoint settings and subscribed events.
- `DeleteAsync(webhookId, CancellationToken)` removes a webhook registration.
- `ListAsync(ListWebhooksRequest, CancellationToken)` lists webhook registrations with optional filters.
- `ListAllAsync(ListWebhooksRequest, CancellationToken)` streams webhook registrations across all result pages.

Typical use cases:

- Registering callback endpoints.
- Receiving event notifications for asynchronous platform events.
- Keeping external systems synchronized with book lifecycle, publishing, and distribution changes.

Example:

```csharp
var webhook = await client.Webhooks.RegisterAsync(
    new RegisterWebhookRequest
    {
        EndpointUrl = "https://hooks.example.com/publishing-platform",
        Events = ["book.published", "book.distribution.completed"],
        IsActive = true,
        SigningKeyId = "signing-key-01",
    },
    idempotencyKey: "webhook-registration-001");

var page = await client.Webhooks.ListAsync(new ListWebhooksRequest
{
    Event = "book.published",
    IsActive = true,
});

await foreach (var activeWebhook in client.Webhooks.ListAllAsync(new ListWebhooksRequest
{
    IsActive = true,
}))
{
    Console.WriteLine(activeWebhook.Id);
}
```

## Which module should I use?

- Use `Books` today for production book CRUD/listing flows.
- Use `BookPublishing` when you need lifecycle state transitions (`publish`, `unpublish`, `schedule`).
- Use `BookDistribution` when you need downstream propagation and operational delivery tracking.
- Use `Webhooks` when external systems need asynchronous platform event notifications.

## Publishing vs distribution architecture

`BookPublishing` and `BookDistribution` are intentionally separate concerns.

- `BookPublishing` controls domain lifecycle state (`Draft -> Published`, schedule, unpublish).
- `BookDistribution` controls operational delivery workflows (channel propagation, retries, status polling, operation history).

Correlation model:

- Shared identity: both modules operate on the same `bookId`.
- Independent state machines: publishing status and distribution status are tracked separately.
- Long-running traceability: distribution returns an `operationId` used for polling, retries, and diagnostics.
- Business precondition: successful publication is a backend precondition for distribution, not a client-side coupling.

This separation improves observability, retry safety, and failure isolation for partner/channel sync workflows.

Status values returned by publishing and distribution responses are exposed as strings instead of enums. Backend status sets can grow independently of SDK releases, and strings preserve new or channel-specific values without deserialization failures. Consumers should compare known status values case-insensitively and handle unknown values as valid future API responses.

## Architecture tradeoffs

The SDK deliberately chooses depth over breadth. The current surface goes deep on book lifecycle workflows: catalog metadata, content, publishing, distribution, access, analytics, audit logs, and webhooks. Broader publishing domains should be added only when they support that lifecycle instead of turning the SDK into a shallow generic wrapper.

All modules use a central internal transport. This trades away per-client HTTP pipeline customization so correlation IDs, HTTPS enforcement, resilience, diagnostics, authentication, and error normalization stay consistent. Module clients remain responsible for orchestration, validation, endpoint shape, and response mapping.

Public models are strict, platform-owned SDK contracts. Provider-specific payloads, including Google Books-derived wire shapes, stay internal. Some backend-grown value sets, such as publishing and distribution status strings, intentionally remain strings so new server values do not break older consumers.

Retry defaults are conservative. Retries are opt-in, idempotent methods are the default safe retry target, and non-idempotent `POST` or `PATCH` retries require explicit configuration plus an `Idempotency-Key` when replay is possible. Mutating examples use idempotency keys because duplicate publishing, distribution, or webhook operations are worse than a visible transient failure.

Diagnostics are privacy-preserving by default. Logging, tracing, and metrics are opt-in, and request bodies, response bodies, API keys, authorization headers, idempotency keys, download URLs, uploaded content, and user/customer identifiers are excluded unless a future explicit option states otherwise.

Errors are normalized at the transport boundary before exception mapping. The default mapper returns structured SDK exceptions with status, error code, request ID, correlation ID, operation name, method, path, and message. Consumers can replace the mapper while keeping the same normalized error context.

### Side-by-side API usage

```csharp
var publishingStatus = await client.BookPublishing.PublishAsync(
    bookId,
    new PublishBookRequest { Notes = "Ready for release" },
    idempotencyKey: "pub-001");

if (!string.Equals(publishingStatus.Status, "published", StringComparison.OrdinalIgnoreCase))
{
    return;
}

var distributionStart = await client.BookDistribution.StartAsync(
    bookId,
    new StartBookDistributionRequest
    {
        Channels = ["mobile", "partner-store"],
    },
    idempotencyKey: "dist-001");

var distributionStatus = await client.BookDistribution.GetStatusAsync(
    bookId,
    distributionStart.OperationId);

if (string.Equals(distributionStatus.Status, "failed", StringComparison.OrdinalIgnoreCase))
{
    // Inspect FailureReason or retry according to your workflow.
}
```

## Books lifecycle usage

```csharp
using PublishingPlatform.SDK.Models;

var created = await client.Books.CreateAsync(new CreateBookRequest
{
    Title = "Domain-Driven Design",
    Author = "Eric Evans",
    Tags = new[] { "architecture", "ddd" },
    IdempotencyKey = "create-book-001"
});

var loaded = await client.Books.GetByIdAsync(created.Id);

var page = await client.Books.ListAsync(new ListBooksRequest
{
    Author = "Eric Evans",
    SortBy = "title",
    Descending = false,
    Page = 0,
    PageSize = 20
});

var updated = await client.Books.UpdateMetadataAsync(
    created.Id,
    new UpdateBookMetadataRequest
    {
        Title = "Domain-Driven Design (Updated)",
        Author = created.Author,
        Tags = created.Tags,
        ConcurrencyToken = created.ConcurrencyToken
    });

await client.Books.DeleteAsync(updated.Id);
```

For advanced pagination ergonomics, use `ListAllAsync(...)` on `Books`, `BookAccess`, `BookAuditLogs`, and `Webhooks` to stream all results.

## Opt-in resilience configuration

Resilience is disabled by default. Enable only the strategies you need.
By default, retries are applied only to idempotent methods (`GET`, `PUT`, `DELETE`, `HEAD`, `OPTIONS`, `TRACE`).
Retries for non-idempotent methods (`POST`, `PATCH`) are explicit and opt-in.

```csharp
var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
    Resilience = new PublishingPlatformResilienceOptions
    {
        Enabled = true,
        Retry = new RetryResilienceOptions
        {
            Enabled = true,
            MaxRetryAttempts = 5,
            BaseDelay = TimeSpan.FromMilliseconds(250),
            UseJitter = true
        },
        AttemptTimeout = new TimeoutResilienceOptions
        {
            Enabled = true,
            Timeout = TimeSpan.FromSeconds(5)
        }
    }
}).Build();
```

## Retry behavior defaults

| HTTP Method Category | Default Retry Behavior | Transient Statuses |
| --- | --- | --- |
| Idempotent (`GET`, `PUT`, `DELETE`, `HEAD`, `OPTIONS`, `TRACE`) | Retry enabled when retry strategy is enabled | `429`, `502`, `503`, `504` |
| Non-idempotent (`POST`, `PATCH`) | Not retried by default | N/A |

You can opt in to retries for non-idempotent methods:

```csharp
var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
    Resilience = new PublishingPlatformResilienceOptions
    {
        Enabled = true,
        Retry = new RetryResilienceOptions
        {
            Enabled = true,
            MaxRetryAttempts = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            RetryNonIdempotentMethods = true,
            UseJitter = true
        }
    }
}).Build();
```

When non-idempotent retries are enabled, the SDK retries only safe transient statuses (`429`, `503`) and only when an `Idempotency-Key` is present with replayable request content.

## ASP.NET Core registration (Options pattern)

Option 1: bind and validate via `IOptions<PublishingPlatformClientOptions>`.

```csharp
using PublishingPlatform.SDK.Extensions;
using PublishingPlatform.SDK.Options;

builder.Services.Configure<PublishingPlatformClientOptions>(builder.Configuration.GetSection("PublishingPlatform"));
builder.Services.AddPublishingPlatformClient();
```

Option 2: keep using direct configure callback.

```csharp
using PublishingPlatform.SDK.Extensions;

builder.Services.AddPublishingPlatformClient(options =>
{
    options.BaseUrl = "https://api.books.example";
    options.ApiKey = builder.Configuration["PublishingPlatform:ApiKey"] ?? string.Empty;
});
```

### Optional: custom error mapper

Use a custom mapper to throw domain-specific exceptions with normalized request context.

```csharp
using PublishingPlatform.SDK.Abstractions;

public sealed class PublishingErrorMapper : IPublishingPlatformErrorMapper
{
    public Exception Map(PublishingPlatformErrorContext context)
    {
        if (context.StatusCode == 404 && context.RelativePath.StartsWith("/books/", StringComparison.Ordinal))
        {
        return new InvalidOperationException(
            $"Book was not found. Code: {context.ErrorCode}. Request: {context.RequestId}. Path: {context.RelativePath}");
        }

    return new Exception(
        $"API call failed ({context.StatusCode}, {context.ErrorCode}) for {context.Method} {context.RelativePath}: {context.Message}");
    }
}

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
    ErrorMapper = new PublishingErrorMapper()
}).Build();
```

## Local Skill Structure

This repository includes local Codex skills under `.codex/skills`.

These skills follow a consistent structure (front matter, scoped usage, workflow, validation, and guardrails) so they are predictable and reusable across SDK work.

The skill design and guidance patterns are based on concepts from the official .NET skills project: [dotnet/skills](https://github.com/dotnet/skills/).
