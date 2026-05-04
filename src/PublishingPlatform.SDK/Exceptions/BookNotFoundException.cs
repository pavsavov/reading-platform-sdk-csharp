namespace PublishingPlatform.SDK.Exceptions;

/// <summary>
/// Represents a not found failure for a book resource.
/// </summary>
public sealed class BookNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookNotFoundException"/> class.
    /// </summary>
    /// <param name="bookId">The missing book identifier.</param>
    /// <param name="message">The failure message.</param>
    public BookNotFoundException(string bookId, string message)
        : base(message)
    {
        BookId = bookId;
    }

    /// <summary>
    /// Gets the missing book identifier.
    /// </summary>
    public string BookId { get; }
}
