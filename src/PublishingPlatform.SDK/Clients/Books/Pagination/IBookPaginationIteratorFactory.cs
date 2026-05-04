using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.Books.Pagination;

internal interface IBookPaginationIteratorFactory
{
    IAsyncEnumerable<Book> IterateAsync(
        ListBooksRequest request,
        Func<ListBooksRequest, CancellationToken, Task<PagedResult<Book>>> pageLoader,
        CancellationToken ct);
}
