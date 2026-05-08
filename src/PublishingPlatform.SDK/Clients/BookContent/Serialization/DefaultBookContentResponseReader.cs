namespace PublishingPlatform.SDK.Clients.BookContent.Serialization;

using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;
using BookContentModel = PublishingPlatform.SDK.Models.BookContent;

internal sealed class DefaultBookContentResponseReader : IBookContentResponseReader
{
    public async Task<BookContentModel> ReadBookContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadFromJsonAsync<BookContentModel>(cancellationToken).ConfigureAwait(false);
        if (content is null)
        {
            throw new BookValidationException("Book content payload was empty.");
        }

        return content;
    }

    public async Task<UploadSessionInfo> ReadUploadSessionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var session = await response.Content.ReadFromJsonAsync<UploadSessionInfo>(cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            throw new BookValidationException("Upload session payload was empty.");
        }

        return session;
    }

    public async Task<UploadChunkResult> ReadUploadChunkResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var result = await response.Content.ReadFromJsonAsync<UploadChunkResult>(cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            throw new BookValidationException("Upload chunk payload was empty.");
        }

        return result;
    }
}
