using System.Globalization;
using System.Text;
using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.Webhooks.Serialization;

/// <summary>
/// Builds deterministic URL-encoded webhook list paths.
/// </summary>
internal sealed class DefaultWebhookQueryStringBuilder : IWebhookQueryStringBuilder
{
    /// <inheritdoc />
    public string BuildListPath(ListWebhooksRequest request)
    {
        var builder = new StringBuilder("/webhooks");
        QueryStringBuilderHelper.AppendRequired(
            builder,
            "pageSize",
            request.PageSize.ToString(CultureInfo.InvariantCulture),
            isFirstParameter: true);

        _ = QueryStringBuilderHelper.AppendOptional(
            builder,
            "continuationToken",
            request.ContinuationToken,
            isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(
            builder,
            "event",
            request.Event,
            isFirstParameter: false);

        if (request.IsActive.HasValue)
        {
            QueryStringBuilderHelper.AppendRequired(
                builder,
                "isActive",
                request.IsActive.Value ? "true" : "false",
                isFirstParameter: false);
        }

        return builder.ToString();
    }
}
