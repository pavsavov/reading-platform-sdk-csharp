namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

/// <summary>
/// Represents the effective diagnostics behavior for a single SDK module request.
/// </summary>
internal sealed class ResolvedDiagnosticsOptions
{
    public ResolvedDiagnosticsOptions(
        bool enableLogging,
        bool enableTracing,
        bool enableMetrics,
        bool logRequestBody,
        bool logResponseBody,
        string correlationHeaderName,
        bool generateCorrelationIds)
    {
        EnableLogging = enableLogging;
        EnableTracing = enableTracing;
        EnableMetrics = enableMetrics;
        LogRequestBody = logRequestBody;
        LogResponseBody = logResponseBody;
        CorrelationHeaderName = correlationHeaderName;
        GenerateCorrelationIds = generateCorrelationIds;
    }

    public bool EnableLogging { get; }

    public bool EnableTracing { get; }

    public bool EnableMetrics { get; }

    public bool LogRequestBody { get; }

    public bool LogResponseBody { get; }

    public string CorrelationHeaderName { get; }

    public bool GenerateCorrelationIds { get; }
}
