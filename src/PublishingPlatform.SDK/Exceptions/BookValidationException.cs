namespace PublishingPlatform.SDK.Exceptions;

/// <summary>
/// Represents validation failures for book requests.
/// </summary>
public sealed class BookValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookValidationException"/> class.
    /// </summary>
    /// <param name="message">The validation message.</param>
    public BookValidationException(string message)
        : base(message)
    {
    }
}
