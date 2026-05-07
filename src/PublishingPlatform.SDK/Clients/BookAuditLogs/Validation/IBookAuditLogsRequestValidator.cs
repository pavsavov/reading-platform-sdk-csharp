using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAuditLogs.Validation;

/// <summary>
/// Validates audit-log list requests before transport execution.
/// </summary>
internal interface IBookAuditLogsRequestValidator
{
    /// <summary>
    /// Validates an audit-log list request.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    void ValidateList(ListBookAuditLogsRequest request);
}
