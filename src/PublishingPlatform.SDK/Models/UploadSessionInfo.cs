namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents resumable upload session progress and state.
/// </summary>
public sealed class UploadSessionInfo
{
    /// <summary>
    /// Gets or sets the unique upload session identifier.
    /// </summary>
    public string UploadSessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the associated book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets session status value.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets uploaded bytes count.
    /// </summary>
    public long UploadedBytes { get; set; }

    /// <summary>
    /// Gets or sets total file bytes.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Gets or sets the session expiration timestamp.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
