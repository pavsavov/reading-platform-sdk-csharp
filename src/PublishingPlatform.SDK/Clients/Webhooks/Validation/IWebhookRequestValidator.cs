using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.Webhooks.Validation;

/// <summary>
/// Validates webhook management requests before transport execution.
/// </summary>
internal interface IWebhookRequestValidator
{
    /// <summary>
    /// Validates a webhook registration request.
    /// </summary>
    /// <param name="request">The registration request.</param>
    void ValidateRegister(RegisterWebhookRequest request);

    /// <summary>
    /// Validates a webhook update request.
    /// </summary>
    /// <param name="request">The update request.</param>
    void ValidateUpdate(UpdateWebhookRequest request);

    /// <summary>
    /// Validates a webhook identifier.
    /// </summary>
    /// <param name="webhookId">The webhook identifier.</param>
    void ValidateWebhookId(string webhookId);

    /// <summary>
    /// Validates a webhook list request.
    /// </summary>
    /// <param name="request">The list request.</param>
    void ValidateList(ListWebhooksRequest request);
}
