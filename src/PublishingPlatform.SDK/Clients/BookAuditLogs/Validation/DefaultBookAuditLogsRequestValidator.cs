using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAuditLogs.Validation;

/// <summary>
/// Enforces validation rules for audit-log list requests.
/// </summary>
internal sealed class DefaultBookAuditLogsRequestValidator : IBookAuditLogsRequestValidator
{
    /// <inheritdoc />
    public void ValidateList(ListBookAuditLogsRequest request)
    {
        if (request.Page < 0)
        {
            throw new BookValidationException("Page must be greater than or equal to 0.");
        }

        if (request.PageSize < 1 || request.PageSize > 500)
        {
            throw new BookValidationException("PageSize must be between 1 and 500.");
        }
    }
}
