using System.Text.Json.Serialization;

namespace PublishingPlatform.SDK.Infrastructure.GoogleBooks;

/// <summary>
/// Represents minimal volume info metadata from Google Books.
/// </summary>
internal sealed class GoogleBooksVolumeInfoPayload
{
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets author names.
    /// </summary>
    [JsonPropertyName("authors")]
    public IReadOnlyList<string>? Authors { get; set; }
}
