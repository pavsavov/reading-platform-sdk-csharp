namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents accepted range details for an uploaded chunk.
/// </summary>
public sealed class UploadChunkResult
{
    /// <summary>
    /// Gets or sets the upload session identifier.
    /// </summary>
    public string UploadSessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets accepted chunk start offset.
    /// </summary>
    public long AcceptedRangeStart { get; set; }

    /// <summary>
    /// Gets or sets accepted chunk end offset.
    /// </summary>
    public long AcceptedRangeEnd { get; set; }

    /// <summary>
    /// Gets or sets cumulative uploaded bytes.
    /// </summary>
    public long UploadedBytes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether all bytes were uploaded.
    /// </summary>
    public bool IsComplete { get; set; }
}
