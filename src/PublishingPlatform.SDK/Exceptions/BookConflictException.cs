namespace PublishingPlatform.SDK.Exceptions;

/// <summary>
/// Represents a conflict failure for book mutations.
/// </summary>
public sealed class BookConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookConflictException"/> class.
    /// </summary>
    /// <param name="bookId">The book identifier associated with the conflict.</param>
    /// <param name="message">The failure message.</param>
    public BookConflictException(string bookId, string message)
        : base(message)
    {
        BookId = bookId;
    }

    /// <summary>
    /// Gets the book identifier associated with the conflict.
    /// </summary>
    public string BookId { get; }
}
