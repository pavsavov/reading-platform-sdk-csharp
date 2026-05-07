namespace PublishingPlatform.SDK.Abstractions;

/// <summary>
/// Describes a normalized API failure before it is mapped to an SDK exception.
/// </summary>
public sealed class PublishingPlatformErrorContext
{
    /// <summary>
    /// Gets the HTTP method used by the failed request.
    /// </summary>
    public required HttpMethod Method { get; init; }

    /// <summary>
    /// Gets the relative request path used by the failed request.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets the HTTP status code returned by the API.
    /// </summary>
    public required int StatusCode { get; init; }

    /// <summary>
    /// Gets the normalized failure message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the normalized API error code.
    /// </summary>
    public required string ErrorCode { get; init; }

    /// <summary>
    /// Gets the API request identifier, when one is returned by the API.
    /// </summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// Gets the request correlation identifier.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the SDK operation name associated with the failed request.
    /// </summary>
    public string? OperationName { get; init; }
}
