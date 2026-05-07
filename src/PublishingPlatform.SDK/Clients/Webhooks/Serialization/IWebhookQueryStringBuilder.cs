using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.Webhooks.Serialization;

/// <summary>
/// Builds deterministic webhook query paths.
/// </summary>
internal interface IWebhookQueryStringBuilder
{
    /// <summary>
    /// Builds the webhook list path for a request.
    /// </summary>
    /// <param name="request">The list request.</param>
    /// <returns>The relative list path with query parameters.</returns>
    string BuildListPath(ListWebhooksRequest request);
}
