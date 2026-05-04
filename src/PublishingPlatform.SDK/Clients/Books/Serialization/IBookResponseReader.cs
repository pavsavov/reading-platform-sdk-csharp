using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.Books.Serialization;

internal interface IBookResponseReader
{
    Task<Book> ReadBookAsync(HttpResponseMessage response, CancellationToken ct);

    Task<PagedResult<Book>> ReadPagedResultAsync(HttpResponseMessage response, CancellationToken ct);
}
