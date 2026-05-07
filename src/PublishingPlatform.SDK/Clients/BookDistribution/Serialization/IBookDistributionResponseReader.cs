using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookDistribution.Serialization;

/// <summary>
/// Reads and validates distribution response payloads.
/// </summary>
internal interface IBookDistributionResponseReader
{
    /// <summary>
    /// Reads a distribution operation payload from an HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed <see cref="BookDistributionOperation"/>.</returns>
    Task<BookDistributionOperation> ReadOperationAsync(HttpResponseMessage response, CancellationToken ct);

    /// <summary>
    /// Reads a distribution operation list payload from an HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed <see cref="BookDistributionListResult"/>.</returns>
    Task<BookDistributionListResult> ReadListAsync(HttpResponseMessage response, CancellationToken ct);
}
