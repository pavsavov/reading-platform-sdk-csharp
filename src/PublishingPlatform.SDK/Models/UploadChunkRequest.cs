namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents one resumable upload chunk payload.
/// </summary>
public sealed class UploadChunkRequest
{
    /// <summary>
    /// Gets or sets the chunk stream.
    /// </summary>
    public Stream Chunk { get; set; } = Stream.Null;

    /// <summary>
    /// Gets or sets the inclusive chunk start offset.
    /// </summary>
    public long ChunkStart { get; set; }

    /// <summary>
    /// Gets or sets the inclusive chunk end offset.
    /// </summary>
    public long ChunkEnd { get; set; }

    /// <summary>
    /// Gets or sets the full file size in bytes.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Gets or sets the optional chunk checksum.
    /// </summary>
    public string? ChunkChecksum { get; set; }
}
