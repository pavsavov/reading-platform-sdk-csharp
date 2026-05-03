namespace PublishingPlatform.SDK.Models.Common;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public int TotalCount { get; set; }

    public string? ContinuationToken { get; set; }
}
