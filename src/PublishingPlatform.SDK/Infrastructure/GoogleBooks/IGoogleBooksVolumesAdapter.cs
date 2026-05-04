using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Infrastructure.GoogleBooks;

/// <summary>
/// Represents an internal adapter seam for future Google Books volume enrichment.
/// </summary>
internal interface IGoogleBooksVolumesAdapter
{
    /// <summary>
    /// Attempts to enrich a platform book from Google Books data.
    /// </summary>
    /// <param name="book">The platform book to enrich.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The enriched book when available; otherwise the original book.</returns>
    Task<Book> EnrichAsync(Book book, CancellationToken cancellationToken = default);
}
