using System.Text.Json.Serialization;

namespace PublishingPlatform.SDK.Infrastructure.GoogleBooks;

/// <summary>
/// Represents industry identifier metadata from Google Books.
/// </summary>
internal sealed class GoogleBooksIndustryIdentifierPayload
{
    /// <summary>
    /// Gets or sets the identifier type.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the identifier value.
    /// </summary>
    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }
}
