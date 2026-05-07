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
            Author = payload.VolumeInfo?.Authors is { Count: > 0 } authors
                ? authors[0]
                : string.Empty,
            Tags = payload.VolumeInfo?.Categories is { Count: > 0 } categories
                ? categories
                : Array.Empty<string>(),
        };
    }

    /// <summary>
    /// Maps a Google Books volumes response into platform <see cref="Book"/> models.
    /// </summary>
    /// <param name="payload">The source payload.</param>
    /// <returns>Mapped books.</returns>
    public static IReadOnlyList<Book> ToBooks(this GoogleBooksVolumesResponsePayload payload)
    {
        if (payload.Items is null || payload.Items.Count == 0)
        {
            return Array.Empty<Book>();
        }

        var books = new Book[payload.Items.Count];
        for (var i = 0; i < payload.Items.Count; i++)
        {
            books[i] = payload.Items[i].ToBook();
        }

        return books;
    }
}
