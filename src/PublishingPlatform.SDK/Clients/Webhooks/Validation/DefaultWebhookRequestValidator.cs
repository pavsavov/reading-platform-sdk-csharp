using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.Webhooks.Validation;

/// <summary>
/// Enforces input validation rules for webhook management operations.
/// </summary>
internal sealed class DefaultWebhookRequestValidator : IWebhookRequestValidator
{
    private const int MinimumPageSize = 1;
    private const int MaximumPageSize = 500;

    /// <inheritdoc />
    public void ValidateRegister(RegisterWebhookRequest request)
    {
        ValidateEndpointUrl(request.EndpointUrl);
        ValidateEvents(request.Events);
    }

    /// <inheritdoc />
    public void ValidateUpdate(UpdateWebhookRequest request)
    {
        ValidateWebhookId(request.WebhookId);
        ValidateEndpointUrl(request.EndpointUrl);
        ValidateEvents(request.Events);
    }

    /// <inheritdoc />
    public void ValidateWebhookId(string webhookId)
    {
        if (string.IsNullOrWhiteSpace(webhookId))
        {
            throw new BookValidationException("Webhook id is required.");
        }
    }

    /// <inheritdoc />
    public void ValidateList(ListWebhooksRequest request)
    {
        if (request.PageSize is < MinimumPageSize or > MaximumPageSize)
        {
            throw new BookValidationException("PageSize must be between 1 and 500.");
        }
    }

    private static void ValidateEndpointUrl(string endpointUrl)
    {
        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            throw new BookValidationException("Webhook endpoint URL is required.");
        }

        if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new BookValidationException("Webhook endpoint URL must be an absolute HTTPS URL.");
        }
    }

    private static void ValidateEvents(IReadOnlyList<string> events)
    {
        if (events.Count == 0)
        {
            throw new BookValidationException("At least one webhook event is required.");
        }

        foreach (var webhookEvent in events)
        {
            if (string.IsNullOrWhiteSpace(webhookEvent))
            {
                throw new BookValidationException("Webhook events cannot contain empty values.");
            }
        }
    }
}
