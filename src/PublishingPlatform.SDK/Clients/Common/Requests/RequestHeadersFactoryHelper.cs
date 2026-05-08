using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients.Common.Requests;

/// <summary>
/// Creates common HTTP request header dictionaries for SDK client operations.
/// </summary>
internal static class RequestHeadersFactoryHelper
{
    /// <summary>
    /// Creates idempotency headers when an idempotency key is provided.
    /// </summary>
    /// <param name="idempotencyKey">The optional idempotency key value.</param>
    /// <returns>A case-insensitive header dictionary when the key is present; otherwise <see langword="null"/>.</returns>
    internal static IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return null;
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [TransportHeaderNames.IdempotencyKey] = idempotencyKey,
        };
    }
}
