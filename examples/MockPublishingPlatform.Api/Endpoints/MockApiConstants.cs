namespace MockPublishingPlatform.Api.Endpoints;

/// <summary>
/// Defines shared constants used by the mock API endpoint modules.
/// </summary>
internal static class MockApiConstants
{
    internal const string CorrelationHeaderName = "X-Correlation-Id";
    internal const string ContentRangeHeaderName = "Content-Range";
    internal const string PublishedStatus = "published";
    internal const string InProgressStatus = "in_progress";
    internal const string PendingStatus = "pending";
    internal const string UploadedStatus = "uploaded";
    internal const string CompletedStatus = "completed";
    internal const string UnknownFormat = "unknown";
    internal const string ValidationErrorKey = "validation";
    internal const string NotFoundErrorKey = "notFound";

    internal static readonly DateTimeOffset StoredAtTimestamp = DateTimeOffset.Parse("2026-05-08T12:00:00Z");
    internal static readonly DateTimeOffset PublishedAtTimestamp = DateTimeOffset.Parse("2026-05-08T12:01:00Z");
    internal static readonly DateTimeOffset DistributionStartTimestamp = DateTimeOffset.Parse("2026-05-08T12:02:00Z");
    internal static readonly DateTimeOffset WebhookCreatedTimestamp = DateTimeOffset.Parse("2026-05-08T12:03:00Z");
    internal static readonly DateTimeOffset WebhookUpdatedTimestamp = DateTimeOffset.Parse("2026-05-08T12:04:00Z");
    internal static readonly DateTimeOffset UploadSessionExpiresAtTimestamp = DateTimeOffset.Parse("2026-05-09T12:00:00Z");
}
