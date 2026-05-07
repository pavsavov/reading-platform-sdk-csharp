namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

/// <summary>
/// Configures SDK diagnostics behavior for logging, tracing, metrics, and correlation.
/// </summary>
public sealed class DiagnosticsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether SDK request lifecycle logging is enabled.
    /// </summary>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether SDK Activity-based tracing is enabled.
    /// </summary>
    public bool EnableTracing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether SDK metrics are recorded.
    /// </summary>
    public bool EnableMetrics { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether request bodies are logged.
    /// </summary>
    public bool LogRequestBody { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether response bodies are logged.
    /// </summary>
    public bool LogResponseBody { get; set; }

    /// <summary>
    /// Gets or sets the HTTP header name used to propagate correlation IDs.
    /// </summary>
    public string CorrelationHeaderName { get; set; } = "X-Correlation-Id";

    /// <summary>
    /// Gets or sets a value indicating whether the SDK generates correlation IDs for outgoing requests.
    /// </summary>
    public bool GenerateCorrelationIds { get; set; } = true;
}
