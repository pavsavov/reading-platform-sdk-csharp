using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;
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
        var payload = await response.Content.ReadFromJsonAsync<PagedResult<AuditLog>>(ct).ConfigureAwait(false);
        if (payload is null)
        {
            throw new BookValidationException("Paged book audit logs payload was empty.");
        }

        return payload;
    }
}
