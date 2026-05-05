namespace PublishingPlatform.SDK.Models;

/// <summary>
/// Represents a request to schedule book publishing.
/// </summary>
public sealed class ScheduleBookPublishingRequest
{
    /// <summary>
    /// Gets or sets the date-time when publishing should occur.
    /// </summary>
    public DateTimeOffset ScheduledAt { get; set; }
}
