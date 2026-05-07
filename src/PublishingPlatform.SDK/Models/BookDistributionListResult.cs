namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a collection of distribution operation snapshots for a book.
/// </summary>
public sealed class BookDistributionListResult
{
    /// <summary>
    /// Gets or sets the returned operations.
    /// </summary>
    public IReadOnlyList<BookDistributionOperation> Operations { get; set; } = Array.Empty<BookDistributionOperation>();
}
