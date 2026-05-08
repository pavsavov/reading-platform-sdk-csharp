using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.Books.Serialization;

internal sealed class DefaultBookResponseReader : IBookResponseReader
{
    public async Task<Book> ReadBookAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<Book>(response, "Book payload was empty.", ct)
            .ConfigureAwait(false);
    }

    public async Task<PagedResult<Book>> ReadPagedResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<PagedResult<Book>>(response, "Paged book payload was empty.", ct)
            .ConfigureAwait(false);
    }
}
