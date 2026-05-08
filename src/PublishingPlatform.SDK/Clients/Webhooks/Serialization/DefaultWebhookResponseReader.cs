using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.Webhooks.Serialization;

/// <summary>
/// Reads and validates webhook response payloads.
/// </summary>
internal sealed class DefaultWebhookResponseReader : IWebhookResponseReader
{
    /// <inheritdoc />
    public async Task<Webhook> ReadWebhookAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<Webhook>(response, "Webhook payload was empty.", cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedResult<Webhook>> ReadPagedWebhooksAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<PagedResult<Webhook>>(response, "Paged webhooks payload was empty.", cancellationToken)
            .ConfigureAwait(false);
    }
}
