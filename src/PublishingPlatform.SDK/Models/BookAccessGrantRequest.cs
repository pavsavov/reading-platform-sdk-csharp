namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request to grant access to a book principal.
/// </summary>
public sealed class BookAccessGrantRequest
{
    /// <summary>
    /// Gets or sets the book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the principal identifier receiving access.
    /// </summary>
    public string PrincipalId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the principal type receiving access (for example user, group, organization).
    /// </summary>
    public string PrincipalType { get; set; } = "user";

    /// <summary>
    /// Gets or sets the access level to grant.
    /// </summary>
    public string AccessLevel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional expiration date for the grant.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets an optional idempotency key for retry-safe grant requests.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}
