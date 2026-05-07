using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents criteria for listing webhook registrations.
/// </summary>
public sealed class ListWebhooksRequest : PaginationRequest
{
    /// <summary>
    /// Gets or sets an optional subscribed event filter.
    /// </summary>
    public string? Event { get; set; }

    /// <summary>
    /// Gets or sets an optional active-state filter.
    /// </summary>
    public bool? IsActive { get; set; }
}
