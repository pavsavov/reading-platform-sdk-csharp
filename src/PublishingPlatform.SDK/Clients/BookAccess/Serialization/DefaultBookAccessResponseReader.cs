using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.BookAccess.Serialization;

/// <summary>
/// Reads and validates book access response payloads.
/// </summary>
internal sealed class DefaultBookAccessResponseReader : IBookAccessResponseReader
{
    /// <inheritdoc />
    public async Task<BookAccessGrantResult> ReadGrantResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var payload = await response.Content.ReadFromJsonAsync<BookAccessGrantResult>(ct).ConfigureAwait(false);
        if (payload is null)
        {
            throw new BookValidationException("Book access grant payload was empty.");
        }

        return payload;
    }

    /// <inheritdoc />
    public async Task<BookAccessRevokeResult> ReadRevokeResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var payload = await response.Content.ReadFromJsonAsync<BookAccessRevokeResult>(ct).ConfigureAwait(false);
        if (payload is null)
        {
            throw new BookValidationException("Book access revoke payload was empty.");
        }

        return payload;
    }

    /// <inheritdoc />
    public async Task<BookAccessStatus> ReadStatusAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var payload = await response.Content.ReadFromJsonAsync<BookAccessStatus>(ct).ConfigureAwait(false);
        if (payload is null)
        {
            throw new BookValidationException("Book access status payload was empty.");
        }

        return payload;
    }

    /// <inheritdoc />
    public async Task<PagedResult<BookAccessGrant>> ReadPagedGrantResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var payload = await response.Content.ReadFromJsonAsync<PagedResult<BookAccessGrant>>(ct).ConfigureAwait(false);
        if (payload is null)
        {
            throw new BookValidationException("Paged book access payload was empty.");
        }

        return payload;
    }
}
