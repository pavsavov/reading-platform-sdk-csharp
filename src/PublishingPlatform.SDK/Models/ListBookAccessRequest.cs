using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents filtering and paging criteria for book access queries.
/// </summary>
public sealed class ListBookAccessRequest : PaginationRequest
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

}
