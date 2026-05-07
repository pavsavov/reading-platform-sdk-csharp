namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request for book analytics aggregation data.
/// </summary>
public sealed class GetBookAnalyticsRequest
{
    /// <summary>
    /// Gets or sets the book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the inclusive range start.
    /// </summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>
    /// Gets or sets the inclusive range end.
    /// </summary>
    public DateTimeOffset? To { get; set; }

    /// <summary>
    /// Gets or sets the aggregation granularity (for example, day/week/month).
    /// Current analytics summary endpoint does not consume this value yet; this field is future-facing.
    /// </summary>
    public string Granularity { get; set; } = "day";

    /// <summary>
    /// Gets or sets a value indicating whether unique reader count is requested.
    /// Current analytics summary endpoint does not consume this value yet; this field is future-facing.
    /// </summary>
    public bool IncludeUniqueReaders { get; set; } = true;
}
