using System.Globalization;
using System.Text;
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
        var builder = new StringBuilder("/book-audit-logs");
        QueryStringBuilderHelper.AppendRequired(builder, "page", request.Page.ToString(CultureInfo.InvariantCulture), isFirstParameter: true);
        QueryStringBuilderHelper.AppendRequired(builder, "pageSize", request.PageSize.ToString(CultureInfo.InvariantCulture), isFirstParameter: false);

        _ = QueryStringBuilderHelper.AppendOptional(builder, "continuationToken", request.ContinuationToken, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, "bookId", request.BookId, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, "action", request.Action, isFirstParameter: false);

        return builder.ToString();
    }
}
