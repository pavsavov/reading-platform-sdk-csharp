namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents publishing state for a specific book.
/// </summary>
public sealed class BookPublishingStatus
{
    /// <summary>
    /// Gets or sets the book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publishing status value.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scheduled publish date-time, if present.
    /// </summary>
    public DateTimeOffset? ScheduledAt { get; set; }

    /// <summary>
    /// Gets or sets the completed publish date-time, if present.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>
    /// Gets or sets the optional failure reason.
    /// </summary>
    public string? FailureReason { get; set; }
}
