using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookPublishing.Validation;

/// <summary>
/// Enforces input validation rules for book publishing operations.
/// </summary>
internal sealed class DefaultBookPublishingRequestValidator : IBookPublishingRequestValidator
{
    /// <inheritdoc />
    public void ValidateBookId(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId))
        {
            throw new BookValidationException("Book id is required.");
        }
    }

    /// <inheritdoc />
    public void ValidatePublish(PublishBookRequest request)
    {
        if (request.Notes is not null && string.IsNullOrWhiteSpace(request.Notes))
        {
            throw new BookValidationException("Notes cannot be empty when provided.");
        }
    }

    /// <inheritdoc />
    public void ValidateSchedule(ScheduleBookPublishingRequest request)
    {
        if (request.ScheduledAt == default)
        {
            throw new BookValidationException("ScheduledAt is required.");
        }
    }
}
