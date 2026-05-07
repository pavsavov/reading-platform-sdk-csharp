namespace PublishingPlatform.SDK.Abstractions;

/// <summary>
/// Describes request characteristics used by resilience policies.
/// </summary>
public sealed class PublishingPlatformResilienceContext
{
    /// <summary>
    /// Gets the HTTP method for the logical operation.
    /// </summary>
    public required HttpMethod Method { get; init; }

    /// <summary>
    /// Gets whether the request includes an idempotency key header.
    /// </summary>
    public bool HasIdempotencyKey { get; init; }

    /// <summary>
    /// Gets whether request content can be replayed safely across retries.
    /// </summary>
    public bool CanReplayContent { get; init; } = true;
}
