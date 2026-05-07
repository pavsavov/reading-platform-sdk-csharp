namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request to register a webhook endpoint.
/// </summary>
public sealed class RegisterWebhookRequest
{
    /// <summary>
    /// Gets or sets the callback endpoint URL.
    /// </summary>
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets subscribed event names.
    /// </summary>
    public IReadOnlyList<string> Events { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether this webhook is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional signing key identifier managed outside the SDK.
    /// </summary>
    public string? SigningKeyId { get; set; }
}
