namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents the outcome of revoking access from a book principal.
/// </summary>
public sealed class BookAccessRevokeResult
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
    /// Gets or sets a value indicating whether an active grant was revoked.
    /// </summary>
    public bool Revoked { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when revocation occurred.
    /// </summary>
    public DateTimeOffset RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets optional revoke reason.
    /// </summary>
    public string? Reason { get; set; }
}
