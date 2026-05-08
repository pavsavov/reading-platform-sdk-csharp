using System.Text;
using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Clients.Books;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.Books.Serialization;

internal sealed class DefaultBookQueryStringBuilder : IBookQueryStringBuilder
{
    public string BuildListPath(ListBooksRequest request)
    {
        var builder = new StringBuilder(BooksEndpoints.Collection);

        QueryStringBuilderHelper.AppendRequired(
            builder,
            QueryParameterNames.Page,
            request.Page.ToString(System.Globalization.CultureInfo.InvariantCulture),
            isFirstParameter: true);
        QueryStringBuilderHelper.AppendRequired(
            builder,
            QueryParameterNames.PageSize,
            request.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            isFirstParameter: false);
        QueryStringBuilderHelper.AppendRequired(
            builder,
            QueryParameterNames.SortBy,
            request.SortBy,
            isFirstParameter: false);
        QueryStringBuilderHelper.AppendRequired(
            builder,
            QueryParameterNames.Descending,
            request.Descending ? QueryParameterValues.BooleanTrue : QueryParameterValues.BooleanFalse,
            isFirstParameter: false);

        _ = QueryStringBuilderHelper.AppendOptional(
            builder,
            QueryParameterNames.Title,
            request.Title,
            isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(
            builder,
            QueryParameterNames.Author,
            request.Author,
            isFirstParameter: false);
        _ = QueryStringBuilderHelper.AppendOptional(
            builder,
            QueryParameterNames.ContinuationToken,
            request.ContinuationToken,
            isFirstParameter: false);

        foreach (var tag in request.Tags.OrderBy(x => x, StringComparer.Ordinal))
        {
            QueryStringBuilderHelper.AppendRequired(
                builder,
                QueryParameterNames.Tag,
                tag,
                isFirstParameter: false);
        }

        return builder.ToString();
    }
}
