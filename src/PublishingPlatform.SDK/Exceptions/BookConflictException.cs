using PublishingPlatform.SDK.Abstractions;

namespace PublishingPlatform.SDK.Exceptions;

/// <summary>
/// Represents a conflict failure for book mutations.
/// </summary>
public sealed class BookConflictException : PublishingPlatformApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookConflictException"/> class.
    /// </summary>
    /// <param name="bookId">The book identifier associated with the conflict.</param>
    /// <param name="message">The failure message.</param>
    public BookConflictException(string bookId, string message)
        : base(409, message)
    {
        BookId = bookId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BookConflictException"/> class.
    /// </summary>
    /// <param name="bookId">The book identifier associated with the conflict.</param>
    /// <param name="context">The normalized error context.</param>
    public BookConflictException(string bookId, PublishingPlatformErrorContext context)
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
    /// Gets the book identifier associated with the conflict.
    /// </summary>
    public string BookId { get; }
}
