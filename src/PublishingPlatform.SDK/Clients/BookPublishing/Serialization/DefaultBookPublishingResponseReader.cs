using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;
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
        var status = await response.Content.ReadFromJsonAsync<BookPublishingStatus>(ct).ConfigureAwait(false);
        if (status is null)
        {
            throw new BookValidationException("Book publishing status payload was empty.");
        }

        return status;
    }
}
