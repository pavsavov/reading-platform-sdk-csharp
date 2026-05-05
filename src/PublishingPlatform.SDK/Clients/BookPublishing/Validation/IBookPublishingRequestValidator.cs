using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookPublishing.Validation;

/// <summary>
/// Validates book publishing request inputs before transport execution.
/// </summary>
internal interface IBookPublishingRequestValidator
{
    /// <summary>
    /// Validates the book identifier.
    /// </summary>
    /// <param name="bookId">The book identifier value.</param>
    void ValidateBookId(string bookId);

    /// <summary>
    /// Validates a publish request payload.
    /// </summary>
    /// <param name="request">The publish request payload.</param>
    void ValidatePublish(PublishBookRequest request);

    /// <summary>
    /// Validates a schedule publishing request payload.
    /// </summary>
    /// <param name="request">The schedule request payload.</param>
    void ValidateSchedule(ScheduleBookPublishingRequest request);
}
