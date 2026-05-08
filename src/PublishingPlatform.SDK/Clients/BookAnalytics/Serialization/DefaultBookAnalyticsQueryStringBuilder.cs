using System.Globalization;
using System.Text;
using PublishingPlatform.SDK.Clients.BookAnalytics;
using PublishingPlatform.SDK.Clients.Common.Serialization;
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
        var builder = new StringBuilder(BookAnalyticsEndpoints.Summary);
        QueryStringBuilderHelper.AppendRequired(builder, QueryParameterNames.BookId, request.BookId, isFirstParameter: true);
        QueryStringBuilderHelper.AppendRequired(
            builder,
            QueryParameterNames.From,
            request.From!.Value.ToString("O", CultureInfo.InvariantCulture),
            isFirstParameter: false);
        QueryStringBuilderHelper.AppendRequired(
            builder,
            QueryParameterNames.To,
            request.To!.Value.ToString("O", CultureInfo.InvariantCulture),
            isFirstParameter: false);

        return builder.ToString();
    }
}
