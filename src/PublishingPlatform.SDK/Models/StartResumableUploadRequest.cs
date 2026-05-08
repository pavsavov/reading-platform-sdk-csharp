namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents input required to create a resumable upload session.
/// </summary>
public sealed class StartResumableUploadRequest
{
    /// <summary>
    /// Gets or sets the file name associated with the upload.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional media content type (for example application/pdf).
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the optional format (for example pdf or epub).
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets total file size in bytes.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Gets or sets optional idempotency key for safe retries.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}
