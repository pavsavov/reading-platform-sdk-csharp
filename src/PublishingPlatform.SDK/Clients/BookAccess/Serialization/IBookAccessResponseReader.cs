using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.BookAccess.Serialization;

/// <summary>
/// Reads book access response payloads.
/// </summary>
internal interface IBookAccessResponseReader
{
    /// <summary>
    /// Reads grant result payload.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The grant result.</returns>
    Task<BookAccessGrantResult> ReadGrantResultAsync(HttpResponseMessage response, CancellationToken ct);

    /// <summary>
    /// Reads revoke result payload.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The revoke result.</returns>
    Task<BookAccessRevokeResult> ReadRevokeResultAsync(HttpResponseMessage response, CancellationToken ct);

    /// <summary>
    /// Reads access status payload.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The access status.</returns>
    Task<BookAccessStatus> ReadStatusAsync(HttpResponseMessage response, CancellationToken ct);

    /// <summary>
    /// Reads paged list payload of grants.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The paged list of grants.</returns>
    Task<PagedResult<BookAccessGrant>> ReadPagedGrantResultAsync(HttpResponseMessage response, CancellationToken ct);
}
