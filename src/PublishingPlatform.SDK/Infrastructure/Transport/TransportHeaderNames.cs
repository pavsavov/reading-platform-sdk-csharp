namespace PublishingPlatform.SDK.Infrastructure.Transport;

/// <summary>
/// Defines canonical transport-level HTTP header names used by SDK internals.
/// </summary>
internal static class TransportHeaderNames
{
    /// <summary>
    /// API key header used to authenticate SDK requests with the backend API.
    /// </summary>
    internal const string ApiKey = "X-API-Key";

    /// <summary>
    /// Correlation header used to propagate request tracing context.
    /// </summary>
    internal const string CorrelationId = "X-Correlation-Id";

    /// <summary>
    /// Idempotency header used for retry-safe mutating operations.
    /// </summary>
    internal const string IdempotencyKey = "Idempotency-Key";

    /// <summary>
    /// Concurrency header used for optimistic updates.
    /// </summary>
    internal const string IfMatch = "If-Match";
}
