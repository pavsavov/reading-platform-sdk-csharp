using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients.BookAuditLogs.Serialization;

/// <summary>
/// Parses audit-log responses and enforces non-empty payloads.
/// </summary>
internal sealed class DefaultBookAuditLogsResponseReader : IBookAuditLogsResponseReader
{
    /// <inheritdoc />
    public async Task<PagedResult<AuditLog>> ReadListAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<PagedResult<AuditLog>>(response, "Paged book audit logs payload was empty.", ct)
            .ConfigureAwait(false);
    }
}
