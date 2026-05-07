namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a single distribution operation snapshot for a book.
/// </summary>
public sealed class BookDistributionOperation
{
    /// <summary>
    /// Gets or sets the unique operation identifier.
    /// </summary>
    public string OperationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the distribution status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation start timestamp.
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the operation completion timestamp.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the optional failure reason.
    /// </summary>
    public string? FailureReason { get; set; }
}
