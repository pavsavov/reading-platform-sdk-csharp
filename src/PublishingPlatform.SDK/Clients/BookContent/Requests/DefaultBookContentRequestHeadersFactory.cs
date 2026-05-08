using PublishingPlatform.SDK.Clients.Common.Requests;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookContent.Requests;

internal sealed class DefaultBookContentRequestHeadersFactory : IBookContentRequestHeadersFactory
{
    private const string ContentRangeHeaderName = "Content-Range";
    private const string ChunkChecksumHeaderName = "X-Chunk-Checksum";

    public IReadOnlyDictionary<string, string>? CreateIdempotencyHeaders(string? idempotencyKey)
    {
        return RequestHeadersFactoryHelper.CreateIdempotencyHeaders(idempotencyKey);
    }

    public IReadOnlyDictionary<string, string> CreateChunkHeaders(UploadChunkRequest request)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ContentRangeHeaderName] = $"bytes {request.ChunkStart}-{request.ChunkEnd}/{request.TotalBytes}",
        };

        if (!string.IsNullOrWhiteSpace(request.ChunkChecksum))
        {
            headers[ChunkChecksumHeaderName] = request.ChunkChecksum;
        }

        return headers;
    }
}
