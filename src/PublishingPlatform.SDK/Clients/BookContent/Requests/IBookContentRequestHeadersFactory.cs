namespace PublishingPlatform.SDK.Clients.BookContent.Requests;

using PublishingPlatform.SDK.Models;

internal interface IBookContentRequestHeadersFactory
{
    IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey);

    IReadOnlyDictionary<string, string> CreateChunkHeaders(UploadChunkRequest request);
}
