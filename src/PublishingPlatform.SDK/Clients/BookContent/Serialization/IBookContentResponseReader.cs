namespace PublishingPlatform.SDK.Clients.BookContent.Serialization;

using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;
using BookContentModel = PublishingPlatform.SDK.Models.BookContent;

internal interface IBookContentResponseReader
{
    Task<BookContentModel> ReadBookContentAsync(HttpResponseMessage response, CancellationToken cancellationToken);

    Task<UploadSessionInfo> ReadUploadSessionAsync(HttpResponseMessage response, CancellationToken cancellationToken);

    Task<UploadChunkResult> ReadUploadChunkResultAsync(HttpResponseMessage response, CancellationToken cancellationToken);
}
