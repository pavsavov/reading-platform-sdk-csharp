using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAnalytics.Serialization;

/// <summary>
/// Reads and validates analytics summary response payloads.
/// </summary>
internal sealed class DefaultBookAnalyticsResponseReader : IBookAnalyticsResponseReader
{
    /// <inheritdoc />
    public async Task<AnalyticsSummary> ReadSummaryAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<AnalyticsSummary>(response, "Book analytics summary payload was empty.", ct)
            .ConfigureAwait(false);
    }
}
