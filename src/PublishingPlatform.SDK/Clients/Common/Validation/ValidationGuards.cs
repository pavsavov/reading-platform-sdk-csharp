using PublishingPlatform.SDK.Exceptions;

namespace PublishingPlatform.SDK.Clients.Common.Validation;

/// <summary>
/// Provides shared validation guard methods for SDK request validators.
/// </summary>
internal static class ValidationGuards
{
    /// <summary>
    /// Validates that a required book identifier is provided.
    /// </summary>
    /// <param name="bookId">The candidate book identifier.</param>
    /// <exception cref="BookValidationException">Thrown when <paramref name="bookId"/> is null, empty, or whitespace.</exception>
    internal static void ValidateBookId(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId))
        {
            throw new BookValidationException(ValidationMessages.BookIdRequired);
        }
    }
}
