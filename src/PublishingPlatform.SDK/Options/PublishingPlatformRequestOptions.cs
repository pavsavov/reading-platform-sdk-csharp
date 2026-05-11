namespace PublishingPlatform.SDK.Options;

/// <summary>
/// Defines request-scoped SDK options for additive overloads and extension-based convenience APIs.
/// </summary>
public sealed class PublishingPlatformRequestOptions
{
    /// <summary>
    /// Gets or sets an optional idempotency key for retry-safe mutating operations.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Gets or sets an optional correlation identifier for future request-scoped propagation.
    /// </summary>
    public string? CorrelationId { get; set; }
}
