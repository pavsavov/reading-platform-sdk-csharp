using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Clients.Common.Validation;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookDistribution.Validation;

/// <summary>
/// Enforces input validation rules for book distribution operations.
/// </summary>
internal sealed class DefaultBookDistributionRequestValidator : IBookDistributionRequestValidator
{
    /// <inheritdoc />
    public void ValidateBookId(string bookId)
    {
        ValidationGuards.ValidateBookId(bookId);
    }

    /// <inheritdoc />
    public void ValidateOperationId(string operationId)
    {
        if (string.IsNullOrWhiteSpace(operationId))
        {
            throw new BookValidationException("Distribution operation id is required.");
        }
    }

    /// <inheritdoc />
    public void ValidateStartRequest(StartBookDistributionRequest request)
    {
        if (request.Channels.Count == 0)
        {
            throw new BookValidationException("At least one distribution channel is required.");
        }

        if (request.Channels.Any(string.IsNullOrWhiteSpace))
        {
            throw new BookValidationException("Distribution channels cannot contain empty values.");
        }
    }
}
