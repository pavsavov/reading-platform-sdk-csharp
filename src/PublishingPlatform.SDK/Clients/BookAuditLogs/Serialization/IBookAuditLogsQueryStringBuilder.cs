using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAuditLogs.Serialization;

/// <summary>
/// Builds deterministic audit-log list query paths.
/// </summary>
internal interface IBookAuditLogsQueryStringBuilder
{
    /// <summary>
    /// Builds a relative path for audit-log list requests.
    /// </summary>
    /// <param name="request">The request containing route and query criteria.</param>
    /// <returns>The relative request path including query string.</returns>
    string BuildListPath(ListBookAuditLogsRequest request);
}
