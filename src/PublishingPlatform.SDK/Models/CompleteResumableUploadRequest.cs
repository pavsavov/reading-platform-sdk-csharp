namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents completion metadata for a resumable upload session.
/// </summary>
public sealed class CompleteResumableUploadRequest
{
    /// <summary>
    /// Gets or sets optional final checksum.
    /// </summary>
    public string? FinalChecksum { get; set; }

    /// <summary>
    /// Gets or sets optional idempotency key for safe retries.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}
