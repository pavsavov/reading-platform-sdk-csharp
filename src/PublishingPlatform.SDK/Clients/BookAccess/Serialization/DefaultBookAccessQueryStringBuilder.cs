using System.Text;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAccess.Serialization;

/// <summary>
/// Builds deterministic book access query paths.
/// </summary>
internal sealed class DefaultBookAccessQueryStringBuilder : IBookAccessQueryStringBuilder
{
    /// <inheritdoc />
    public string BuildCheckPath(BookAccessCheckRequest request)
    {
        var builder = new StringBuilder($"/books/{Uri.EscapeDataString(request.BookId)}/access/check?");
        builder.Append("principalId=");
        builder.Append(Uri.EscapeDataString(request.PrincipalId));
        builder.Append("&principalType=");
        builder.Append(Uri.EscapeDataString(request.PrincipalType));
        return builder.ToString();
    }

    /// <inheritdoc />
    public string BuildListPath(ListBookAccessRequest request)
    {
        var path = string.IsNullOrWhiteSpace(request.BookId)
            ? "/book-access"
            : $"/books/{Uri.EscapeDataString(request.BookId)}/access";

        var builder = new StringBuilder(path);
        builder.Append('?');
        AppendQuery(builder, "pageSize", request.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture));

        AppendOptionalQuery(builder, "continuationToken", request.ContinuationToken);
        AppendOptionalQuery(builder, "principalId", request.PrincipalId);
        AppendOptionalQuery(builder, "principalType", request.PrincipalType);
        AppendOptionalQuery(builder, "accessLevel", request.AccessLevel);

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
