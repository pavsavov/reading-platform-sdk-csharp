namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request to revoke access from a book principal.
/// </summary>
public sealed class BookAccessRevokeRequest
{
    /// <summary>
    /// Gets or sets the book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the principal identifier losing access.
    /// </summary>
    public string PrincipalId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional reason for auditability.
    /// </summary>
    public string? Reason { get; set; }
}
