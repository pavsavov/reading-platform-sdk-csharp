namespace PublishingPlatform.SDK.Options;

/// <summary>
/// Configures retry strategy behavior for SDK transport resilience.
/// </summary>
public sealed class RetryResilienceOptions
{
    /// <summary>
    /// Gets or sets whether retry strategy is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets maximum retry attempts after the initial request.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets base delay used by retry backoff.
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets whether non-idempotent methods can be retried when retry-safety requirements are satisfied.
    /// </summary>
    public bool RetryNonIdempotentMethods { get; set; }

    /// <summary>
    /// Gets or sets whether randomized jitter is applied to retry delays.
    /// </summary>
    public bool UseJitter { get; set; } = true;
}
