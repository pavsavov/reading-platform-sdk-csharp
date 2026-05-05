namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a publish request payload for a book.
/// </summary>
public sealed class PublishBookRequest
{
    /// <summary>
    /// Gets or sets optional publishing notes.
    /// </summary>
    public string? Notes { get; set; }
}
