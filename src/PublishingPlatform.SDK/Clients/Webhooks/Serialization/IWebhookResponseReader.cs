using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.Webhooks.Serialization;

/// <summary>
/// Reads and validates webhook response payloads.
/// </summary>
internal interface IWebhookResponseReader
{
    /// <summary>
    /// Reads a webhook response payload.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The webhook payload.</returns>
    Task<Webhook> ReadWebhookAsync(HttpResponseMessage response, CancellationToken cancellationToken);

    /// <summary>
    /// Reads a paged webhook response payload.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged webhook payload.</returns>
    Task<PagedResult<Webhook>> ReadPagedWebhooksAsync(HttpResponseMessage response, CancellationToken cancellationToken);
}
