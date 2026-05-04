namespace PublishingPlatform.SDK.Options;

public sealed class TimeoutResilienceOptions
{
    public bool Enabled { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
