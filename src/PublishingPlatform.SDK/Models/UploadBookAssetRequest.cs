namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request to upload or replace a book asset.
/// </summary>
public sealed class UploadBookAssetRequest
{
    /// <summary>
    /// Gets or sets the book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the logical asset type (for example cover/preview).
    /// </summary>
    public string AssetType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content type of the uploaded asset.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional idempotency key for retry-safe uploads.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}
