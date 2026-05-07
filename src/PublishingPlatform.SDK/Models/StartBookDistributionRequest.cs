namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request to start distribution for a book.
/// </summary>
public sealed class StartBookDistributionRequest
{
    /// <summary>
    /// Gets or sets destination channel identifiers.
    /// </summary>
    public IReadOnlyList<string> Channels { get; set; } = Array.Empty<string>();
}
