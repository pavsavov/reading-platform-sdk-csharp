namespace PublishingPlatform.SDK.Models.Common;

public class PaginationRequest
{
    public int PageSize { get; set; } = 50;

    public string? ContinuationToken { get; set; }
}
