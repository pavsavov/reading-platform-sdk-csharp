namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents filtering, sorting, and pagination criteria for listing books.
/// </summary>
public sealed class ListBooksRequest
{
    /// <summary>
    /// Gets or sets a title filter.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets an author filter.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets tag filters.
    /// </summary>
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the field used for sorting.
    /// </summary>
    public string SortBy { get; set; } = "title";

    /// <summary>
    /// Gets or sets a value indicating whether sorting is descending.
    /// </summary>
    public bool Descending { get; set; }

    /// <summary>
    /// Gets or sets zero-based page index.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets requested page size.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets continuation token when continuing paged reads.
    /// </summary>
    public string? ContinuationToken { get; set; }
}
