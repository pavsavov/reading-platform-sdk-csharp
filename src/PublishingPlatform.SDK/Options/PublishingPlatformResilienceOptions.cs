namespace PublishingPlatform.SDK.Options;

public sealed class PublishingPlatformResilienceOptions
{
    public bool Enabled { get; set; }

    public RetryResilienceOptions Retry { get; set; } = new();

    public CircuitBreakerResilienceOptions CircuitBreaker { get; set; } = new();

    public TimeoutResilienceOptions AttemptTimeout { get; set; } = new();

    public TimeoutResilienceOptions TotalTimeout { get; set; } = new();
}
