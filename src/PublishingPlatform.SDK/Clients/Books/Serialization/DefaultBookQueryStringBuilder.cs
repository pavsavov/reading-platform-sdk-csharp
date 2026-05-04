using System.Text;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.Books.Serialization;

internal sealed class DefaultBookQueryStringBuilder : IBookQueryStringBuilder
{
    public string BuildListPath(ListBooksRequest request)
    {
        var query = new List<KeyValuePair<string, string>>
        {
            new("page", request.Page.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new("pageSize", request.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new("sortBy", request.SortBy),
            new("descending", request.Descending ? "true" : "false"),
        };

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            query.Add(new("title", request.Title));
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            query.Add(new("author", request.Author));
        }

        if (!string.IsNullOrWhiteSpace(request.ContinuationToken))
        {
            query.Add(new("continuationToken", request.ContinuationToken));
        }

        foreach (var tag in request.Tags.OrderBy(x => x, StringComparer.Ordinal))
        {
            query.Add(new("tag", tag));
        }

        var builder = new StringBuilder("/books?");
        for (var i = 0; i < query.Count; i++)
        {
            if (i > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(query[i].Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(query[i].Value));
        }

        return builder.ToString();
    }
}
