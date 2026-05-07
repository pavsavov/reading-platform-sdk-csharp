using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookDistribution.Validation;

/// <summary>
/// Validates book distribution request inputs before transport execution.
/// </summary>
internal interface IBookDistributionRequestValidator
{
    /// <summary>
    /// Validates the book identifier.
    /// </summary>
    /// <param name="bookId">The book identifier value.</param>
    void ValidateBookId(string bookId);

    /// <summary>
    /// Validates the operation identifier.
    /// </summary>
    /// <param name="operationId">The operation identifier value.</param>
    void ValidateOperationId(string operationId);

    /// <summary>
    /// Validates a distribution start request payload.
    /// </summary>
    /// <param name="request">The distribution start request payload.</param>
    void ValidateStartRequest(StartBookDistributionRequest request);
}
