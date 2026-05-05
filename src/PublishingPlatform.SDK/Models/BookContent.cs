namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents book content metadata.
/// </summary>
public sealed class BookContent
{
    /// <summary>
    /// Gets or sets the book identifier associated with the content.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content format.
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remote content URL when available.
    /// </summary>
    public Uri? ContentUrl { get; set; }

    /// <summary>
    /// Gets or sets the local storage path when available.
    /// </summary>
    public string? LocalPath { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when content was stored.
    /// </summary>
    public DateTimeOffset? StoredAt { get; set; }
}
