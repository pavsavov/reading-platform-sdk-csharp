namespace PublishingPlatform.SDK.Abstractions;

using PublishingPlatform.SDK.Models;

/// <summary>
/// Provides operations for retrieving and managing book content files.
/// </summary>
public interface IBookContentClient
{
    /// <summary>
    /// Gets content metadata for a book.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The book content metadata.</returns>
    Task<BookContent> GetAsync(string bookId, CancellationToken ct = default);

    /// <summary>
    /// Uploads or replaces book content for a book.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The upload request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The updated book content metadata.</returns>
    Task<BookContent> UploadOrReplaceAsync(string bookId, UploadBookContentRequest request, CancellationToken ct = default);
}
