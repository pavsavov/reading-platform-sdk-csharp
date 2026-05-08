using System.Globalization;
using System.Text;
using PublishingPlatform.SDK.Clients.BookAuditLogs;
using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAuditLogs.Serialization;

/// <summary>
/// Builds deterministic audit-log query strings with optional filters.
/// </summary>
internal sealed class DefaultBookAuditLogsQueryStringBuilder : IBookAuditLogsQueryStringBuilder
{
    /// <inheritdoc />
    public string BuildListPath(ListBookAuditLogsRequest request)
    {
        var builder = new StringBuilder(BookAuditLogsEndpoints.Collection);
        QueryStringBuilderHelper.AppendRequired(builder, QueryParameterNames.Page, request.Page.ToString(CultureInfo.InvariantCulture), isFirstParameter: true);
        QueryStringBuilderHelper.AppendRequired(builder, QueryParameterNames.PageSize, request.PageSize.ToString(CultureInfo.InvariantCulture), isFirstParameter: false);

        _ = QueryStringBuilderHelper.AppendOptional(builder, QueryParameterNames.ContinuationToken, request.ContinuationToken, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, QueryParameterNames.BookId, request.BookId, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, QueryParameterNames.Action, request.Action, isFirstParameter: false);

        return builder.ToString();
    }
}
