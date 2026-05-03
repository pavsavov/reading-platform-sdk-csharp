namespace PublishingPlatform.SDK.Models.Common;

public sealed class PaginationRequest
{
    public int PageSize { get; set; } = 50;

    public string? ContinuationToken { get; set; }
}
