namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents criteria for listing webhook registrations.
/// </summary>
public sealed class ListWebhooksRequest
{
    /// <summary>
    /// Gets or sets the maximum number of webhooks to return.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets the continuation token for the next result page.
    /// </summary>
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Gets or sets an optional subscribed event filter.
    /// </summary>
    public string? Event { get; set; }

    /// <summary>
    /// Gets or sets an optional active-state filter.
    /// </summary>
    public bool? IsActive { get; set; }
}
