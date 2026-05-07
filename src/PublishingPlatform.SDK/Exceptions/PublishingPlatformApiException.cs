namespace PublishingPlatform.SDK.Exceptions;

/// <summary>
/// Represents a normalized API failure returned by the Publishing Platform API.
/// </summary>
public class PublishingPlatformApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishingPlatformApiException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="message">The normalized failure message.</param>
    /// <param name="errorCode">The normalized API error code.</param>
    /// <param name="requestId">The API request identifier, when available.</param>
    /// <param name="correlationId">The correlation identifier associated with the request.</param>
    /// <param name="operationName">The SDK operation name associated with the request.</param>
    /// <param name="method">The HTTP method used by the request.</param>
    /// <param name="relativePath">The relative request path.</param>
    public PublishingPlatformApiException(
        int statusCode,
        string message,
        string? errorCode = null,
        string? requestId = null,
        string? correlationId = null,
        string? operationName = null,
        HttpMethod? method = null,
        string? relativePath = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? $"http_{statusCode}" : errorCode;
        RequestId = requestId;
        CorrelationId = correlationId;
        OperationName = operationName;
        Method = method;
        RelativePath = relativePath;
    }

    /// <summary>
    /// Gets the HTTP status code returned by the API.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the normalized API error code.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the API request identifier, when available.
    /// </summary>
    public string? RequestId { get; }

    /// <summary>
    /// Gets the correlation identifier associated with the request.
    /// </summary>
    public string? CorrelationId { get; }

    /// <summary>
    /// Gets the SDK operation name associated with the request.
    /// </summary>
    public string? OperationName { get; }

    /// <summary>
    /// Gets the HTTP method used by the request.
    /// </summary>
    public HttpMethod? Method { get; }

    /// <summary>
    /// Gets the relative request path.
    /// </summary>
    public string? RelativePath { get; }
}
