namespace PublishingPlatform.SDK.Options;

public sealed class CircuitBreakerResilienceOptions
{
    public bool Enabled { get; set; }

    public double FailureRatio { get; set; } = 0.5;

    public int MinimumThroughput { get; set; } = 10;

    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);
}
