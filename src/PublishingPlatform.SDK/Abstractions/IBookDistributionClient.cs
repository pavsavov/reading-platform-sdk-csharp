using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Abstractions;

/// <summary>
/// Provides operational workflows for distributing published books to delivery channels.
/// </summary>
public interface IBookDistributionClient
{
    /// <summary>
    /// Starts a book distribution operation for one or more channels.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The distribution start payload.</param>
    /// <param name="idempotencyKey">An optional idempotency key for safe retries.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The created distribution operation snapshot.</returns>
    Task<BookDistributionOperation> StartAsync(string bookId, StartBookDistributionRequest request, string? idempotencyKey = null, CancellationToken ct = default);

    /// <summary>
    /// Gets the status of a specific distribution operation.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="operationId">The unique distribution operation identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The distribution operation snapshot.</returns>
    Task<BookDistributionOperation> GetStatusAsync(string bookId, string operationId, CancellationToken ct = default);

    /// <summary>
    /// Retries a previously failed or incomplete distribution operation.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="operationId">The unique distribution operation identifier.</param>
    /// <param name="idempotencyKey">An optional idempotency key for safe retries.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The distribution operation snapshot after retry.</returns>
    Task<BookDistributionOperation> RetryAsync(string bookId, string operationId, string? idempotencyKey = null, CancellationToken ct = default);

    /// <summary>
    /// Lists tracked distribution operations for a specific book.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list wrapper containing distribution operations.</returns>
    Task<BookDistributionListResult> ListAsync(string bookId, CancellationToken ct = default);
}
