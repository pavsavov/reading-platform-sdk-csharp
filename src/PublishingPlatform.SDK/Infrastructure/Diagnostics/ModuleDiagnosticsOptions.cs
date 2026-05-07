namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

/// <summary>
/// Overrides global diagnostics behavior for one SDK module when a value is explicitly set.
/// </summary>
public sealed class ModuleDiagnosticsOptions
{
    /// <summary>
    /// Gets or sets an override for SDK request lifecycle logging.
    /// </summary>
    public bool? EnableLogging { get; set; }

    /// <summary>
    /// Gets or sets an override for SDK Activity-based tracing.
    /// </summary>
    public bool? EnableTracing { get; set; }

    /// <summary>
    /// Gets or sets an override for SDK metrics recording.
    /// </summary>
    public bool? EnableMetrics { get; set; }

    /// <summary>
    /// Gets or sets an override for request body logging.
    /// </summary>
    public bool? LogRequestBody { get; set; }

    /// <summary>
    /// Gets or sets an override for response body logging.
    /// </summary>
    public bool? LogResponseBody { get; set; }

    /// <summary>
    /// Gets or sets an override for the correlation header name.
    /// </summary>
    public string? CorrelationHeaderName { get; set; }

    /// <summary>
    /// Gets or sets an override for SDK-generated correlation IDs.
    /// </summary>
    public bool? GenerateCorrelationIds { get; set; }
}
