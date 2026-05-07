using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients.Webhooks.Requests;

/// <summary>
/// Produces header sets for webhook management requests.
/// </summary>
internal sealed class DefaultWebhookRequestHeadersFactory : IWebhookRequestHeadersFactory
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey)
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
