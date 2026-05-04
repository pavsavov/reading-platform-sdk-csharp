using System.Text.Json.Serialization;

namespace PublishingPlatform.SDK.Infrastructure.GoogleBooks;

/// <summary>
/// Represents the minimal Google Books volume payload used for internal enrichment.
/// </summary>
internal sealed class GoogleBooksVolumePayload
{
    /// <summary>
    /// Gets or sets the volume identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets nested volume metadata.
    /// </summary>
    [JsonPropertyName("volumeInfo")]
    public GoogleBooksVolumeInfoPayload? VolumeInfo { get; set; }
}
