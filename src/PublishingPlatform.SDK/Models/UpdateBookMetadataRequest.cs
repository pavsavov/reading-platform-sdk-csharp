namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents input data required to update full mutable book metadata.
/// </summary>
public sealed class UpdateBookMetadataRequest
{
    /// <summary>
    /// Gets or sets the updated title value.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated author value.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets updated tags.
    /// </summary>
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the optimistic concurrency token (for example ETag or version).
    /// </summary>
    public string? ConcurrencyToken { get; set; }
}
