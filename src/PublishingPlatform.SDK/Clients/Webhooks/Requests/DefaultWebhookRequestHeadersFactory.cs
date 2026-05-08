using PublishingPlatform.SDK.Clients.Common.Requests;
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
        return RequestHeadersFactoryHelper.CreateIdempotencyHeaders(idempotencyKey);
    }
}
