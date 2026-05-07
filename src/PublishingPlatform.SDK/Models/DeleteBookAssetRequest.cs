namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request to delete an existing book asset.
/// </summary>
public sealed class DeleteBookAssetRequest
{
    /// <summary>
    /// Gets or sets the book identifier.
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the asset identifier.
    /// </summary>
    public string AssetId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional reason for deletion.
    /// </summary>
    public string? Reason { get; set; }
}
