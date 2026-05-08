using PublishingPlatform.SDK.Clients.Common.Requests;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients.BookDistribution.Requests;

/// <summary>
/// Produces header sets for book distribution requests.
/// </summary>
internal sealed class DefaultBookDistributionRequestHeadersFactory : IBookDistributionRequestHeadersFactory
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey)
    {
        return RequestHeadersFactoryHelper.CreateIdempotencyHeaders(idempotencyKey);
    }
}
