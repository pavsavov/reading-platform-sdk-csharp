using System.Text;
using PublishingPlatform.SDK.Clients.BookAccess;
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
        var builder = new StringBuilder(BookAccessEndpoints.Check(request.BookId));
        QueryStringBuilderHelper.AppendRequired(builder, QueryParameterNames.PrincipalId, request.PrincipalId, isFirstParameter: true);
        QueryStringBuilderHelper.AppendRequired(builder, QueryParameterNames.PrincipalType, request.PrincipalType, isFirstParameter: false);
        return builder.ToString();
    }

    /// <inheritdoc />
    public string BuildListPath(ListBookAccessRequest request)
    {
        var path = string.IsNullOrWhiteSpace(request.BookId)
            ? BookAccessEndpoints.Collection
            : BookAccessEndpoints.ForBook(request.BookId);

        var builder = new StringBuilder(path);
        QueryStringBuilderHelper.AppendRequired(builder, QueryParameterNames.PageSize, request.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture), isFirstParameter: true);

        _ = QueryStringBuilderHelper.AppendOptional(builder, QueryParameterNames.ContinuationToken, request.ContinuationToken, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, QueryParameterNames.PrincipalId, request.PrincipalId, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, QueryParameterNames.PrincipalType, request.PrincipalType, isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(builder, QueryParameterNames.AccessLevel, request.AccessLevel, isFirstParameter: false);

        return builder.ToString();
    }
}
