using System.Globalization;
using System.Text;
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
        var builder = new StringBuilder("/book-audit-logs?");
        AppendQuery(builder, "page", request.Page.ToString(CultureInfo.InvariantCulture));
        builder.Append('&');
        AppendQuery(builder, "pageSize", request.PageSize.ToString(CultureInfo.InvariantCulture));

        AppendOptionalQuery(builder, "continuationToken", request.ContinuationToken);
        AppendOptionalQuery(builder, "bookId", request.BookId);
        AppendOptionalQuery(builder, "action", request.Action);

        return builder.ToString();
    }

    private static void AppendOptionalQuery(StringBuilder builder, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.Append('&');
        AppendQuery(builder, key, value);
    }

    private static void AppendQuery(StringBuilder builder, string key, string value)
    {
        builder.Append(Uri.EscapeDataString(key));
        builder.Append('=');
        builder.Append(Uri.EscapeDataString(value));
    }
}
