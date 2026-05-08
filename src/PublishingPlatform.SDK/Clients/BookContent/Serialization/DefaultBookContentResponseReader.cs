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

    public async Task<UploadSessionInfo> ReadUploadSessionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<UploadSessionInfo>(response, "Upload session payload was empty.", cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UploadChunkResult> ReadUploadChunkResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<UploadChunkResult>(response, "Upload chunk payload was empty.", cancellationToken)
            .ConfigureAwait(false);
    }
}
