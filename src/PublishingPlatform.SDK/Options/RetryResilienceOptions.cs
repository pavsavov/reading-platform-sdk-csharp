namespace PublishingPlatform.SDK.Options;

public sealed class RetryResilienceOptions
{
    public bool Enabled { get; set; }

    public int MaxRetryAttempts { get; set; } = 3;

    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);
}
