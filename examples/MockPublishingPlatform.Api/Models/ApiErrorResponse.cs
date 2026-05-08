namespace MockPublishingPlatform.Api.Models;

/// <summary>
/// Represents an API error payload compatible with SDK error expectations.
/// </summary>
public sealed class ApiErrorResponse
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional platform error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Creates a deterministic response object from fixture data.
    /// </summary>
    /// <param name="fixture">The error fixture template.</param>
    /// <param name="requestId">The current request identifier.</param>
    /// <returns>A stable API error payload.</returns>
    public static ApiErrorResponse FromFixture(ApiErrorFixture fixture, string requestId)
    {
        return new ApiErrorResponse
        {
            Message = fixture.Message,
            ErrorCode = fixture.ErrorCode,
            RequestId = requestId,
        };
    }
}
