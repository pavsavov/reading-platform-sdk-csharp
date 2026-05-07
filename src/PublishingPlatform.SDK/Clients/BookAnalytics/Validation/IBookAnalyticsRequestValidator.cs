using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAnalytics.Validation;

/// <summary>
/// Defines validation rules for book analytics operations.
/// </summary>
internal interface IBookAnalyticsRequestValidator
{
    /// <summary>
    /// Validates analytics summary request payload.
    /// </summary>
    /// <param name="request">The summary request.</param>
    void ValidateGetSummary(GetBookAnalyticsRequest request);
}
