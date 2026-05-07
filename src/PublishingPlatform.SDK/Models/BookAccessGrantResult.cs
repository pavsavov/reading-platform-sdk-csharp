namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents the outcome of granting access to a book principal.
/// </summary>
public sealed class BookAccessGrantResult
{
    /// <summary>
    /// Gets or sets the created or updated grant identifier.
    /// </summary>
    public string GrantId { get; set; } = string.Empty;

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
    /// Gets or sets the granted access level.
    /// </summary>
    public string AccessLevel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the grant was newly created.
    /// </summary>
    public bool Created { get; set; }

    /// <summary>
    /// Gets or sets optional grant expiration.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
