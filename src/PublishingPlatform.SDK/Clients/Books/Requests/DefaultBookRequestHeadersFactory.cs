using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients.Books.Requests;

internal sealed class DefaultBookRequestHeadersFactory : IBookRequestHeadersFactory
{
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

    public IReadOnlyDictionary<string, string>? CreateConcurrencyHeaders(string? concurrencyToken)
    {
        if (string.IsNullOrWhiteSpace(concurrencyToken))
        {
            return null;
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["If-Match"] = concurrencyToken,
        };
    }
}
