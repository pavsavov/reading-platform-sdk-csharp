using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;
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
        var payload = await response.Content.ReadFromJsonAsync<Webhook>(cancellationToken).ConfigureAwait(false);
        if (payload is null)
        {
            throw new BookValidationException("Webhook payload was empty.");
        }

        return payload;
    }

    /// <inheritdoc />
    public async Task<PagedResult<Webhook>> ReadPagedWebhooksAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<PagedResult<Webhook>>(cancellationToken).ConfigureAwait(false);
        if (payload is null)
        {
            throw new BookValidationException("Paged webhooks payload was empty.");
        }

        return payload;
    }
}
