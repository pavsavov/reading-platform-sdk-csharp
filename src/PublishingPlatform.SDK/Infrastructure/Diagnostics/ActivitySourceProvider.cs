using System.Diagnostics;

namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

/// <summary>
/// Provides the SDK ActivitySource used by OpenTelemetry-compatible tracing.
/// </summary>
public static class ActivitySourceProvider
{
    /// <summary>
    /// Gets the SDK ActivitySource.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new("PublishingPlatform.SDK");
}
