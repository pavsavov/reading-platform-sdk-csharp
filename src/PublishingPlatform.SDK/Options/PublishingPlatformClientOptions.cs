using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;

namespace PublishingPlatform.SDK.Options;

/// <summary>
/// Configures the Publishing Platform SDK client.
/// </summary>
public sealed class PublishingPlatformClientOptions
{
    /// <summary>
    /// Gets or sets the Publishing Platform API base URL.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.publishing-platform.local";

    /// <summary>
    /// Gets or sets the API key used by the SDK.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP timeout used by the SDK client.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets resilience configuration for retry, timeout, and circuit breaker behavior.
    /// </summary>
    public PublishingPlatformResilienceOptions? Resilience { get; set; }

    /// <summary>
    /// Gets or sets a custom error mapper for SDK HTTP failures.
    /// </summary>
    public IPublishingPlatformErrorMapper? ErrorMapper { get; set; }

    /// <summary>
    /// Gets or sets global diagnostics configuration inherited by all SDK modules.
    /// </summary>
    public DiagnosticsOptions? Diagnostics { get; set; }

    /// <summary>
    /// Gets or sets diagnostics overrides for the books module.
    /// </summary>
    public ModuleDiagnosticsOptions? BooksDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets diagnostics overrides for the book content module.
    /// </summary>
    public ModuleDiagnosticsOptions? BookContentDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets diagnostics overrides for the book publishing module.
    /// </summary>
    public ModuleDiagnosticsOptions? BookPublishingDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets diagnostics overrides for the book distribution module.
    /// </summary>
    public ModuleDiagnosticsOptions? BookDistributionDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets diagnostics overrides for the book access module.
    /// </summary>
    public ModuleDiagnosticsOptions? BookAccessDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets diagnostics overrides for the book analytics module.
    /// </summary>
    public ModuleDiagnosticsOptions? BookAnalyticsDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets diagnostics overrides for the book audit logs module.
    /// </summary>
    public ModuleDiagnosticsOptions? BookAuditLogsDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets diagnostics overrides for the webhooks module.
    /// </summary>
    public ModuleDiagnosticsOptions? WebhooksDiagnostics { get; set; }
}
