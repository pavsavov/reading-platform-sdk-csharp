using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Exceptions;

namespace PublishingPlatform.SDK.Infrastructure.Errors;

/// <summary>
/// Represents a generic normalized API failure.
/// </summary>
public sealed class ApiException : PublishingPlatformApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="message">The normalized failure message.</param>
    public ApiException(int statusCode, string message)
        : base(statusCode, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    /// <param name="context">The normalized error context.</param>
    public ApiException(PublishingPlatformErrorContext context)
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
