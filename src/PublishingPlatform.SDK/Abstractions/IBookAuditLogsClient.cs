using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Abstractions;

/// <summary>
/// Provides audit-log listing operations for books.
/// </summary>
public interface IBookAuditLogsClient
{
    /// <summary>
    /// Lists paged audit log entries for the requested filtering criteria.
    /// </summary>
    /// <param name="request">The audit-log query criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged audit log result.</returns>
    Task<PagedResult<AuditLog>> ListAsync(
        ListBookAuditLogsRequest request,
        CancellationToken cancellationToken = default);
}
