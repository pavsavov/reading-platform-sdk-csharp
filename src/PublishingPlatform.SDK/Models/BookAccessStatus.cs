namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents effective access state for a principal and book.
/// </summary>
public sealed class BookAccessStatus
{
    /// <summary>
    /// Gets or sets the related book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the principal identifier.
    /// </summary>
    public string PrincipalId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the principal type.
    /// </summary>
    public string PrincipalType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether access is currently allowed.
    /// </summary>
    public bool HasAccess { get; set; }

    /// <summary>
    /// Gets or sets the effective access level when access is present.
    /// </summary>
    public string? AccessLevel { get; set; }

    /// <summary>
    /// Gets or sets why access is allowed or denied.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets optional expiration of current effective access.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
