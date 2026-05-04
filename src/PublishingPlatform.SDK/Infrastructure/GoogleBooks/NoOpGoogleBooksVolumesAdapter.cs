using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Infrastructure.GoogleBooks;

/// <summary>
/// Default adapter implementation that performs no enrichment.
/// </summary>
internal sealed class NoOpGoogleBooksVolumesAdapter : IGoogleBooksVolumesAdapter
{
    /// <inheritdoc />
    public Task<Book> EnrichAsync(Book book, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(book);
    }
}
