using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Clients.Common.Validation;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAnalytics.Validation;

/// <summary>
/// Enforces input validation rules for analytics summary requests.
/// </summary>
internal sealed class DefaultBookAnalyticsRequestValidator : IBookAnalyticsRequestValidator
{
    /// <inheritdoc />
    public void ValidateGetSummary(GetBookAnalyticsRequest request)
    {
        ValidationGuards.ValidateBookId(request.BookId);

        if (!request.From.HasValue)
        {
            throw new BookValidationException("From is required.");
        }

        if (!request.To.HasValue)
        {
            throw new BookValidationException("To is required.");
        }

        if (request.From.Value > request.To.Value)
        {
            throw new BookValidationException("From must be less than or equal to To.");
        }
    }
}
