namespace MockPublishingPlatform.Api.Models;

/// <summary>
/// Represents a deterministic error fixture shape for mock API responses.
/// </summary>
public sealed class ApiErrorFixture
{
    /// <summary>
    /// Gets or sets the HTTP status code used for this error fixture.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the stable message returned to SDK callers.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional platform error code.
    /// </summary>
    public string? ErrorCode { get; set; }
}
