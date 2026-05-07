using System.Globalization;
using System.Text;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAnalytics.Serialization;

/// <summary>
/// Builds deterministic query paths for analytics summary requests.
/// </summary>
internal sealed class DefaultBookAnalyticsQueryStringBuilder : IBookAnalyticsQueryStringBuilder
{
    /// <inheritdoc />
    public string BuildSummaryPath(GetBookAnalyticsRequest request)
    {
        var builder = new StringBuilder("/book-analytics/summary?");
        AppendQuery(builder, "bookId", request.BookId);
        builder.Append('&');
        AppendQuery(builder, "from", request.From!.Value.ToString("O", CultureInfo.InvariantCulture));
        builder.Append('&');
        AppendQuery(builder, "to", request.To!.Value.ToString("O", CultureInfo.InvariantCulture));

        return builder.ToString();
    }

    private static void AppendQuery(StringBuilder builder, string key, string value)
    {
        builder.Append(Uri.EscapeDataString(key));
        builder.Append('=');
        builder.Append(Uri.EscapeDataString(value));
    }
}
