using PublishingPlatform.SDK.Abstractions;

namespace PublishingPlatform.SDK.Exceptions;

/// <summary>
/// Represents a not found failure for a book resource.
/// </summary>
public sealed class BookNotFoundException : PublishingPlatformApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookNotFoundException"/> class.
    /// </summary>
    /// <param name="bookId">The missing book identifier.</param>
    /// <param name="message">The failure message.</param>
    public BookNotFoundException(string bookId, string message)
        : base(404, message)
    {
        BookId = bookId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BookNotFoundException"/> class.
    /// </summary>
    /// <param name="bookId">The missing book identifier.</param>
    /// <param name="context">The normalized error context.</param>
    public BookNotFoundException(string bookId, PublishingPlatformErrorContext context)
        : base(
            context.StatusCode,
            context.Message,
            context.ErrorCode,
            context.RequestId,
            context.CorrelationId,
            context.OperationName,
            context.Method,
            context.RelativePath)
    {
        BookId = bookId;
    }

    /// <summary>
    /// Gets the missing book identifier.
    /// </summary>
    public string BookId { get; }
}
