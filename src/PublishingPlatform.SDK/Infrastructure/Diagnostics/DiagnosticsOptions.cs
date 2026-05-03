namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

public sealed class DiagnosticsOptions
{
    public bool EnableTracing { get; set; } = true;

    public bool EnableMetrics { get; set; } = true;
}
