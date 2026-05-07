using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients.BookContent.Requests;

internal sealed class DefaultBookContentRequestHeadersFactory : IBookContentRequestHeadersFactory
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
}
