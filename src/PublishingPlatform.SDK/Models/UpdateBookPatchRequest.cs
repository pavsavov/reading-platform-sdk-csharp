namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents partial metadata updates for a book.
/// </summary>
public sealed class UpdateBookPatchRequest
{
    /// <summary>
    /// Gets or sets an updated title when provided.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets an updated author when provided.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets updated tags when provided.
    /// </summary>
    public IReadOnlyList<string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the optimistic concurrency token (for example ETag or version).
    /// </summary>
    public string? ConcurrencyToken { get; set; }
}
