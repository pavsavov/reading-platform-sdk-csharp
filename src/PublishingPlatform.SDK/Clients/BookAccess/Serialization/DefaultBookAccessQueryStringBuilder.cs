using System.Text;
using PublishingPlatform.SDK.Clients.Common.Serialization;
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
        QueryStringBuilderHelper.AppendRequired(builder, "pageSize", request.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture), isFirstParameter: true);

        _ = QueryStringBuilderHelper.AppendOptional(builder, "continuationToken", request.ContinuationToken, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, "principalId", request.PrincipalId, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, "principalType", request.PrincipalType, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, "accessLevel", request.AccessLevel, isFirstParameter: false);

        return builder.ToString();
    }
}
