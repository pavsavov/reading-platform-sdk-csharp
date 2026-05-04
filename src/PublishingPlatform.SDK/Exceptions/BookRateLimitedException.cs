namespace PublishingPlatform.SDK.Exceptions;

/// <summary>
/// Represents rate limiting failures for book operations.
/// </summary>
public sealed class BookRateLimitedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookRateLimitedException"/> class.
    /// </summary>
    /// <param name="message">The failure message.</param>
    public BookRateLimitedException(string message)
        : base(message)
    {
    }
}
