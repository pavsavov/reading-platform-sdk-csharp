using PublishingPlatform.SDK.Clients.Common.Requests;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients.BookPublishing.Requests;

/// <summary>
/// Produces header sets for book publishing requests.
/// </summary>
internal sealed class DefaultBookPublishingRequestHeadersFactory : IBookPublishingRequestHeadersFactory
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey)
    {
        return RequestHeadersFactoryHelper.CreateIdempotencyHeaders(idempotencyKey);
    }
}
