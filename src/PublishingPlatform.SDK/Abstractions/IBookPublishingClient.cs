using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Abstractions;

/// <summary>
/// Provides operations for publishing lifecycle state transitions of a book.
/// </summary>
public interface IBookPublishingClient
{
    /// <summary>
    /// Publishes a book and returns its current publishing status.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The publish request payload.</param>
    /// <param name="idempotencyKey">An optional idempotency key for safe retries.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The resulting <see cref="BookPublishingStatus"/>.</returns>
    Task<BookPublishingStatus> PublishAsync(string bookId, PublishBookRequest request, string? idempotencyKey = null, CancellationToken ct = default);

    /// <summary>
    /// Unpublishes a book and returns its current publishing status.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="idempotencyKey">An optional idempotency key for safe retries.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The resulting <see cref="BookPublishingStatus"/>.</returns>
    Task<BookPublishingStatus> UnpublishAsync(string bookId, string? idempotencyKey = null, CancellationToken ct = default);

    /// <summary>
    /// Schedules a book to be published at a specified time and returns its current publishing status.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The schedule request payload.</param>
    /// <param name="idempotencyKey">An optional idempotency key for safe retries.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The resulting <see cref="BookPublishingStatus"/>.</returns>
    Task<BookPublishingStatus> ScheduleAsync(string bookId, ScheduleBookPublishingRequest request, string? idempotencyKey = null, CancellationToken ct = default);

    /// <summary>
    /// Gets the current publishing status for a book.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The current <see cref="BookPublishingStatus"/>.</returns>
    Task<BookPublishingStatus> GetStatusAsync(string bookId, CancellationToken ct = default);
}
