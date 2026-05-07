using System.Diagnostics.Metrics;

namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

/// <summary>
/// Provides SDK metrics instruments for OpenTelemetry-compatible collection.
/// </summary>
internal static class DiagnosticsMeterProvider
{
    internal static readonly Meter Meter = new("PublishingPlatform.SDK");
    internal static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>("publishing_platform.sdk.requests");
    internal static readonly Counter<long> FailureCounter = Meter.CreateCounter<long>("publishing_platform.sdk.request.failures");
    internal static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>("publishing_platform.sdk.request.duration", "ms");
    internal static readonly Counter<long> RetryCounter = Meter.CreateCounter<long>("publishing_platform.sdk.request.retries");
}
