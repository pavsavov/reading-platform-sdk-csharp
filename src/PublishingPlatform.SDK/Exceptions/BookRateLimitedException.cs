using PublishingPlatform.SDK.Abstractions;

namespace PublishingPlatform.SDK.Exceptions;

/// <summary>
/// Represents rate limiting failures for book operations.
/// </summary>
public sealed class BookRateLimitedException : PublishingPlatformApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookRateLimitedException"/> class.
    /// </summary>
    /// <param name="message">The failure message.</param>
    public BookRateLimitedException(string message)
        : base(429, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRateLimitedException"/> class.
    /// </summary>
    /// <param name="context">The normalized error context.</param>
    public BookRateLimitedException(PublishingPlatformErrorContext context)
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
    }
}
