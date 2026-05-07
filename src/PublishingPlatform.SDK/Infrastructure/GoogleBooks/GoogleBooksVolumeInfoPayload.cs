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
    /// Gets or sets the subtitle.
    /// </summary>
    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets author names.
    /// </summary>
    [JsonPropertyName("authors")]
    public IReadOnlyList<string>? Authors { get; set; }

    /// <summary>
    /// Gets or sets category names.
    /// </summary>
    [JsonPropertyName("categories")]
    public IReadOnlyList<string>? Categories { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the published date.
    /// </summary>
    [JsonPropertyName("publishedDate")]
    public string? PublishedDate { get; set; }

    /// <summary>
    /// Gets or sets page count.
    /// </summary>
    [JsonPropertyName("pageCount")]
    public int? PageCount { get; set; }

    /// <summary>
    /// Gets or sets language code.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets image links.
    /// </summary>
    [JsonPropertyName("imageLinks")]
    public GoogleBooksImageLinksPayload? ImageLinks { get; set; }

    /// <summary>
    /// Gets or sets industry identifiers.
    /// </summary>
    [JsonPropertyName("industryIdentifiers")]
    public IReadOnlyList<GoogleBooksIndustryIdentifierPayload>? IndustryIdentifiers { get; set; }
}
