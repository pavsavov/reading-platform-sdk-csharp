# Diagnostics

The SDK diagnostics layer provides opt-in logging, tracing, metrics, and correlation IDs through shared transport infrastructure. Module clients pass operation metadata to the shared transport; diagnostics behavior is resolved centrally from global and per-module options.

## Safe Defaults

Diagnostics are disabled by default except correlation ID generation. The SDK never logs API keys, bearer tokens, auth headers, idempotency keys, request bodies, response bodies, or uploaded content unless a future consumer explicitly enables body logging and accepts that risk.

```csharp
var options = new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "from-secure-configuration",
    Diagnostics = new DiagnosticsOptions
    {
        EnableLogging = true,
        EnableTracing = true,
        EnableMetrics = true
    }
};
```

## Per-Module Overrides

Global diagnostics settings apply to all modules. A module override changes only the values explicitly set on that module.

```csharp
options.Diagnostics = new DiagnosticsOptions
{
    EnableLogging = true,
    EnableTracing = false
};

options.BookContentDiagnostics = new ModuleDiagnosticsOptions
{
    EnableTracing = true
};

options.BookAnalyticsDiagnostics = new ModuleDiagnosticsOptions
{
    EnableLogging = false
};
```

Supported module override properties are `BooksDiagnostics`, `BookContentDiagnostics`, `BookPublishingDiagnostics`, `BookDistributionDiagnostics`, `BookAccessDiagnostics`, `BookAnalyticsDiagnostics`, `BookAuditLogsDiagnostics`, and `WebhooksDiagnostics`.

## Correlation

Outgoing SDK requests use `X-Correlation-Id` by default. The SDK preserves a caller-provided correlation header and otherwise generates one when `GenerateCorrelationIds` is enabled.

The resolved correlation ID is added to:

- HTTP request headers
- activity tags as `correlation.id`
- structured log fields when logging is enabled
- SDK error context when an HTTP failure is mapped to an SDK exception

Use `DiagnosticsOptions.CorrelationHeaderName` to align with an existing platform convention.

## OpenTelemetry Compatibility

Tracing uses `System.Diagnostics.ActivitySource` named `PublishingPlatform.SDK`.

Metrics use `System.Diagnostics.Metrics.Meter` named `PublishingPlatform.SDK` and record request count, failure count, request duration, and retry count. Consumers can collect these signals with OpenTelemetry or any listener that supports the built-in .NET diagnostics APIs.
