using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Abstractions;

/// <summary>
/// Provides aggregate analytics operations for books.
/// </summary>
public interface IBookAnalyticsClient
{
    /// <summary>
    /// Gets aggregate analytics summary metrics for a book within a date range.
    /// </summary>
    /// <param name="request">The analytics summary request criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The aggregate analytics summary for the requested book and range.</returns>
    Task<AnalyticsSummary> GetSummaryAsync(
        GetBookAnalyticsRequest request,
        CancellationToken cancellationToken = default);
}
