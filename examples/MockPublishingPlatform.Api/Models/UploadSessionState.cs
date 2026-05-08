namespace MockPublishingPlatform.Api.Models;

/// <summary>
/// Represents in-memory resumable upload session state for the mock API.
/// </summary>
public sealed class UploadSessionState
{
    /// <summary>
    /// Gets or sets upload session identifier.
    /// </summary>
    public string UploadSessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets associated book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional content type.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets optional format.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets uploaded bytes count.
    /// </summary>
    public long UploadedBytes { get; set; }

    /// <summary>
    /// Gets or sets total bytes.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Gets or sets session status.
    /// </summary>
    public string Status { get; set; } = "pending";
}
