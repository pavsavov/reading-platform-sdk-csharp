using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAnalytics.Serialization;

/// <summary>
/// Builds querystring-based relative paths for analytics operations.
/// </summary>
internal interface IBookAnalyticsQueryStringBuilder
{
    /// <summary>
    /// Builds a relative path for analytics summary retrieval.
    /// </summary>
    /// <param name="request">The analytics request.</param>
    /// <returns>The transport relative path.</returns>
    string BuildSummaryPath(GetBookAnalyticsRequest request);
}
