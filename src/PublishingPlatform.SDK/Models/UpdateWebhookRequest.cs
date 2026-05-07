namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request to update webhook endpoint settings.
/// </summary>
public sealed class UpdateWebhookRequest
{
    /// <summary>
    /// Gets or sets the webhook identifier.
    /// </summary>
    public string WebhookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback endpoint URL.
    /// </summary>
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets subscribed event names.
    /// </summary>
    public IReadOnlyList<string> Events { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether this webhook remains active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
