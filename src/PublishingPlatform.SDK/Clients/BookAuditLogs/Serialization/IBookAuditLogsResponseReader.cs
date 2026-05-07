using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.BookAuditLogs.Serialization;

/// <summary>
/// Reads and validates audit-log list responses.
/// </summary>
internal interface IBookAuditLogsResponseReader
{
    /// <summary>
    /// Reads an audit-log list response payload.
    /// </summary>
    /// <param name="response">The HTTP response containing payload data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed paged audit-log result.</returns>
    Task<PagedResult<AuditLog>> ReadListAsync(HttpResponseMessage response, CancellationToken ct);
}
