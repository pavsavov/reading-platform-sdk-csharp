namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request to check whether a principal can access a book.
/// </summary>
public sealed class BookAccessCheckRequest
{
    /// <summary>
    /// Gets or sets the book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the principal identifier.
    /// </summary>
    public string PrincipalId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the principal type (for example user, group, organization).
    /// </summary>
    public string PrincipalType { get; set; } = "user";
}
