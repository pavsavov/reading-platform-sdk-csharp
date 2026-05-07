using System.Text.Json.Serialization;

namespace PublishingPlatform.SDK.Infrastructure.GoogleBooks;

/// <summary>
/// Represents image link metadata from Google Books.
/// </summary>
internal sealed class GoogleBooksImageLinksPayload
{
    /// <summary>
    /// Gets or sets the small thumbnail URL.
    /// </summary>
    [JsonPropertyName("smallThumbnail")]
    public string? SmallThumbnail { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail URL.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}
