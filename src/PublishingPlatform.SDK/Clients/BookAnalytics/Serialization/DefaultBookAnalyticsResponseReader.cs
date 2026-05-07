using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;
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
        var payload = await response.Content.ReadFromJsonAsync<AnalyticsSummary>(ct).ConfigureAwait(false);
        if (payload is null)
        {
            throw new BookValidationException("Book analytics summary payload was empty.");
        }

        return payload;
    }
}
