namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents filtering and paging criteria for book access queries.
/// </summary>
public sealed class ListBookAccessRequest
{
    /// <summary>
    /// Gets or sets an optional book identifier. When omitted, listing is global.
    /// </summary>
    public string? BookId { get; set; }

    /// <summary>
    /// Gets or sets an optional principal identifier filter.
    /// </summary>
    public string? PrincipalId { get; set; }

    /// <summary>
    /// Gets or sets an optional principal type filter.
    /// </summary>
    public string? PrincipalType { get; set; }

    /// <summary>
    /// Gets or sets an optional access level filter.
    /// </summary>
    public string? AccessLevel { get; set; }

    /// <summary>
    /// Gets or sets the page size for the current read.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets continuation token for paged iteration.
    /// </summary>
    public string? ContinuationToken { get; set; }
}
