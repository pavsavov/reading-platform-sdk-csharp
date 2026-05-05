using System.Runtime.CompilerServices;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.Books.Pagination;

internal sealed class DefaultBookPaginationIteratorFactory : IBookPaginationIteratorFactory
{
    public async IAsyncEnumerable<Book> IterateAsync(
        ListBooksRequest request,
        Func<ListBooksRequest, CancellationToken, Task<PagedResult<Book>>> pageLoader,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var iterationRequest = CloneRequest(request);
        while (true)
        {
            var page = await pageLoader(iterationRequest, ct).ConfigureAwait(false);
            foreach (var item in page.Items)
            {
                yield return item;
            }

            if (string.IsNullOrWhiteSpace(page.ContinuationToken))
            {
                yield break;
            }

            iterationRequest.ContinuationToken = page.ContinuationToken;
            iterationRequest.Page++;
        }
    }

    private static ListBooksRequest CloneRequest(ListBooksRequest request)
    {
        return new ListBooksRequest
        {
            Title = request.Title,
            Author = request.Author,
            Tags = request.Tags.ToArray(),
            SortBy = request.SortBy,
            Descending = request.Descending,
            Page = request.Page,
            PageSize = request.PageSize,
            ContinuationToken = request.ContinuationToken,
        };
    }
}
