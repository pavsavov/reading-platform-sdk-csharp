using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.Books.Serialization;

internal sealed class DefaultBookResponseReader : IBookResponseReader
{
    public async Task<Book> ReadBookAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var book = await response.Content.ReadFromJsonAsync<Book>(ct).ConfigureAwait(false);
        if (book is null)
        {
            throw new BookValidationException("Book payload was empty.");
        }

        return book;
    }

    public async Task<PagedResult<Book>> ReadPagedResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var paged = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(ct).ConfigureAwait(false);
        if (paged is null)
        {
            throw new BookValidationException("Paged book payload was empty.");
        }

        return paged;
    }
}
