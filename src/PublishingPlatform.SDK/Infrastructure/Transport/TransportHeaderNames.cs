namespace PublishingPlatform.SDK.Infrastructure.Transport;

/// <summary>
/// Defines canonical transport-level HTTP header names used by SDK internals.
/// </summary>
internal static class TransportHeaderNames
{
    /// <summary>
    /// Correlation header used to propagate request tracing context.
    /// </summary>
    internal const string CorrelationId = "X-Correlation-Id";
}
