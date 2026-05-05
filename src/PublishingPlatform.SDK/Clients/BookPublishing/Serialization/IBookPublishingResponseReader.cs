using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookPublishing.Serialization;

/// <summary>
/// Reads and validates publishing response payloads.
/// </summary>
internal interface IBookPublishingResponseReader
{
    /// <summary>
    /// Reads a publishing status payload from an HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed <see cref="BookPublishingStatus"/>.</returns>
    Task<BookPublishingStatus> ReadStatusAsync(HttpResponseMessage response, CancellationToken ct);
}
