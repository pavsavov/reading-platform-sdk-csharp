using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Abstractions;

/// <summary>
/// Provides book lifecycle operations for creating, retrieving, listing, updating, and deleting books.
/// </summary>
public interface IBooksClient
{
    /// <summary>
    /// Creates a new book entry.
    /// </summary>
    /// <param name="request">The create request payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The created <see cref="Book"/>.</returns>
    Task<Book> CreateAsync(CreateBookRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets a book by its unique identifier.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The matching <see cref="Book"/>.</returns>
    Task<Book> GetByIdAsync(string bookId, CancellationToken ct = default);

    /// <summary>
    /// Lists books using filtering, sorting, and pagination criteria.
    /// </summary>
    /// <param name="request">The list request criteria.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A paged result of books.</returns>
    Task<PagedResult<Book>> ListAsync(ListBooksRequest request, CancellationToken ct = default);

    /// <summary>
    /// Streams books across all pages for the provided list request.
    /// </summary>
    /// <param name="request">The list request criteria.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An async sequence of books.</returns>
    IAsyncEnumerable<Book> ListAllAsync(ListBooksRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates mutable metadata of a book.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The metadata update payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The updated <see cref="Book"/>.</returns>
    Task<Book> UpdateMetadataAsync(string bookId, UpdateBookMetadataRequest request, CancellationToken ct = default);

    /// <summary>
    /// Partially updates mutable metadata of a book.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The patch payload.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The updated <see cref="Book"/>.</returns>
    Task<Book> PatchMetadataAsync(string bookId, UpdateBookPatchRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a book by its unique identifier.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    Task DeleteAsync(string bookId, CancellationToken ct = default);
}
