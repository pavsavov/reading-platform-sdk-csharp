namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents filtering and paging criteria for book audit log queries.
/// </summary>
public sealed class ListBookAuditLogsRequest
{
    /// <summary>
    /// Gets or sets the book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional actor identifier filter.
    /// </summary>
    public string? ActorId { get; set; }

    /// <summary>
    /// Gets or sets an optional action filter.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets an optional inclusive range start.
    /// </summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>
    /// Gets or sets an optional inclusive range end.
    /// </summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>
    /// Gets or sets an optional correlation identifier filter.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets zero-based page index.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets requested page size.
    /// </summary>
    public int PageSize { get; set; } = 50;
}
