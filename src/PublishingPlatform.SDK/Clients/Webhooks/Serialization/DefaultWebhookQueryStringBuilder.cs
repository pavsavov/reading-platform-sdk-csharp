using System.Globalization;
using System.Text;
using PublishingPlatform.SDK.Clients.Webhooks;
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
        var builder = new StringBuilder(WebhooksEndpoints.Collection);
        QueryStringBuilderHelper.AppendRequired(
            builder,
            QueryParameterNames.PageSize,
            request.PageSize.ToString(CultureInfo.InvariantCulture),
            isFirstParameter: true);

        _ = QueryStringBuilderHelper.AppendOptional(
            builder,
            QueryParameterNames.ContinuationToken,
            request.ContinuationToken,
            isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(
            builder,
            QueryParameterNames.Event,
            request.Event,
            isFirstParameter: false);

        if (request.IsActive.HasValue)
        {
            QueryStringBuilderHelper.AppendRequired(
                builder,
                QueryParameterNames.IsActive,
                request.IsActive.Value ? QueryParameterValues.BooleanTrue : QueryParameterValues.BooleanFalse,
                isFirstParameter: false);
        }

        return builder.ToString();
    }
}
