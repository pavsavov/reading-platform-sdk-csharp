using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAnalytics.Serialization;

/// <summary>
/// Reads analytics response payloads.
/// </summary>
internal interface IBookAnalyticsResponseReader
{
    /// <summary>
    /// Reads analytics summary payload.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The analytics summary.</returns>
    Task<AnalyticsSummary> ReadSummaryAsync(HttpResponseMessage response, CancellationToken ct);
}
