namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents input data required to create a book.
/// </summary>
public sealed class CreateBookRequest
{
    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional tags for categorization.
    /// </summary>
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets an optional idempotency key used to safely retry create requests.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}
