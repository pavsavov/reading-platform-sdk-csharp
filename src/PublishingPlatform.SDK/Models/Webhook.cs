namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a webhook registration that receives asynchronous platform event notifications.
/// </summary>
public sealed class Webhook
{
    /// <summary>
    /// Gets or sets the webhook identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTPS callback endpoint URL.
    /// </summary>
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscribed event names.
    /// </summary>
    public IReadOnlyList<string> Events { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether this webhook is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the optional signing key identifier managed outside the SDK.
    /// </summary>
    public string? SigningKeyId { get; set; }

    /// <summary>
    /// Gets or sets the webhook creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the webhook update timestamp.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
