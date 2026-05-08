using PublishingPlatform.SDK.Clients.Common.Requests;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients.BookContent.Requests;

internal sealed class DefaultBookContentRequestHeadersFactory : IBookContentRequestHeadersFactory
{
    public IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey)
    {
        return RequestHeadersFactoryHelper.CreateIdempotencyHeaders(idempotencyKey);
    }
}
