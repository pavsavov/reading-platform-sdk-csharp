namespace PublishingPlatform.SDK.Clients.BookContent.Serialization;

using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Models;
using BookContentModel = PublishingPlatform.SDK.Models.BookContent;

internal sealed class DefaultBookContentResponseReader : IBookContentResponseReader
{
    public async Task<BookContentModel> ReadBookContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<BookContentModel>(response, "Book content payload was empty.", cancellationToken)
            .ConfigureAwait(false);
    }
}
