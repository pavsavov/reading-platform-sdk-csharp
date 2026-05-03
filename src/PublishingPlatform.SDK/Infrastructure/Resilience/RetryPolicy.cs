namespace PublishingPlatform.SDK.Infrastructure.Resilience;

public sealed class RetryPolicy
{
    public int MaxRetries { get; set; } = 2;

    public TimeSpan Delay { get; set; } = TimeSpan.FromMilliseconds(200);
}
