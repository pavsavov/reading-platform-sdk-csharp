using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Infrastructure.GoogleBooks;

/// <summary>
/// Provides explicit static mapping helpers from Google payloads to SDK models.
/// </summary>
internal static class GoogleBooksMappingExtensions
{
    /// <summary>
    /// Maps a Google Books volume payload into a platform <see cref="Book"/> model.
    /// </summary>
    /// <param name="payload">The source payload.</param>
    /// <returns>A mapped <see cref="Book"/> instance.</returns>
    public static Book ToBook(this GoogleBooksVolumePayload payload)
    {
        return new Book
        {
            Id = payload.Id,
            Title = payload.VolumeInfo?.Title ?? string.Empty,
            Author = payload.VolumeInfo?.Authors?.FirstOrDefault() ?? string.Empty,
        };
    }
}
