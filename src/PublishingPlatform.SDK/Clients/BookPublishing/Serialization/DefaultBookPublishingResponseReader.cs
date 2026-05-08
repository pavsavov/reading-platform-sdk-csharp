using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookPublishing.Serialization;

/// <summary>
/// Parses book publishing status responses and enforces non-empty payloads.
/// </summary>
internal sealed class DefaultBookPublishingResponseReader : IBookPublishingResponseReader
{
    /// <inheritdoc />
    public async Task<BookPublishingStatus> ReadStatusAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<BookPublishingStatus>(response, "Book publishing status payload was empty.", ct)
            .ConfigureAwait(false);
    }
}
