using System.Text.Json.Serialization;

namespace PublishingPlatform.SDK.Infrastructure.GoogleBooks;

/// <summary>
/// Represents the Google Books volumes API response payload.
/// </summary>
internal sealed class GoogleBooksVolumesResponsePayload
{
    /// <summary>
    /// Gets or sets the response kind.
    /// </summary>
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    /// <summary>
    /// Gets or sets the total number of matches.
    /// </summary>
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the returned volume items.
    /// </summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<GoogleBooksVolumePayload>? Items { get; set; }
}
