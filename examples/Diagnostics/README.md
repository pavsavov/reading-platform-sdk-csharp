# Diagnostics

Diagnostics are opt-in and safe by default.

```csharp
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Options;

var options = new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = Environment.GetEnvironmentVariable("PUBLISHING_PLATFORM_API_KEY") ?? string.Empty,
    Diagnostics = new DiagnosticsOptions
    {
        EnableLogging = true,
        EnableTracing = true,
        EnableMetrics = true
    },
    BookContentDiagnostics = new ModuleDiagnosticsOptions
    {
        EnableTracing = true
    },
    BookAnalyticsDiagnostics = new ModuleDiagnosticsOptions
    {
        EnableLogging = false
    }
};

var client = PublishingPlatformClientBuilder.Create(options).Build();
var book = await client.Books.GetByIdAsync("book-123");
```

The SDK adds a correlation ID header to outgoing requests and attaches the same value to Activity tags and logs when diagnostics are enabled. Request and response bodies, API keys, bearer tokens, idempotency keys, and uploaded file content are not logged by default.
