using PublishingPlatform.SDK.Clients.Common.Serialization;
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
        return await ResponseReaderHelper
            .ReadRequiredAsync<BookAccessGrantResult>(response, "Book access grant payload was empty.", ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookAccessRevokeResult> ReadRevokeResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<BookAccessRevokeResult>(response, "Book access revoke payload was empty.", ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookAccessStatus> ReadStatusAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<BookAccessStatus>(response, "Book access status payload was empty.", ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedResult<BookAccessGrant>> ReadPagedGrantResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<PagedResult<BookAccessGrant>>(response, "Paged book access payload was empty.", ct)
            .ConfigureAwait(false);
    }
}
