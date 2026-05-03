namespace PublishingPlatform.SDK.Models;

public sealed class BookContent
{
    public string BookId { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;

    public Uri? ContentUrl { get; set; }
}
