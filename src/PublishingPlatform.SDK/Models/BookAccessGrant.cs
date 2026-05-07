namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents an active or historical grant entry for book access.
/// </summary>
public sealed class BookAccessGrant
{
    /// <summary>
    /// Gets or sets the grant identifier.
    /// </summary>
    public string GrantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the book identifier.
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
    /// Gets or sets the granted access level.
    /// </summary>
    public string AccessLevel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional reason for the grant.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the grant was created.
    /// </summary>
    public DateTimeOffset GrantedAt { get; set; }

    /// <summary>
    /// Gets or sets the actor who granted access.
    /// </summary>
    public string GrantedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional grant expiration.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
