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

    /// <summary>
    /// Starts a resumable upload session for a book.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="request">The upload session request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The created upload session snapshot.</returns>
    Task<UploadSessionInfo> StartResumableUploadAsync(string bookId, StartResumableUploadRequest request, CancellationToken ct = default);

    /// <summary>
    /// Uploads one chunk for an existing upload session.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="uploadSessionId">The upload session identifier.</param>
    /// <param name="request">The chunk upload request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The accepted chunk result.</returns>
    Task<UploadChunkResult> UploadChunkAsync(string bookId, string uploadSessionId, UploadChunkRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets the current resumable upload session state.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="uploadSessionId">The upload session identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The current upload session snapshot.</returns>
    Task<UploadSessionInfo> GetUploadSessionAsync(string bookId, string uploadSessionId, CancellationToken ct = default);

    /// <summary>
    /// Completes a resumable upload session and materializes content metadata.
    /// </summary>
    /// <param name="bookId">The unique book identifier.</param>
    /// <param name="uploadSessionId">The upload session identifier.</param>
    /// <param name="request">The completion request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The completed book content metadata.</returns>
    Task<BookContent> CompleteResumableUploadAsync(string bookId, string uploadSessionId, CompleteResumableUploadRequest request, CancellationToken ct = default);
}
