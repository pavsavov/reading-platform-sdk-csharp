using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookDistribution.Serialization;

/// <summary>
/// Parses distribution responses and enforces non-empty payloads.
/// </summary>
internal sealed class DefaultBookDistributionResponseReader : IBookDistributionResponseReader
{
    /// <inheritdoc />
    public async Task<BookDistributionOperation> ReadOperationAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<BookDistributionOperation>(response, "Book distribution operation payload was empty.", ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookDistributionListResult> ReadListAsync(HttpResponseMessage response, CancellationToken ct)
    {
        return await ResponseReaderHelper
            .ReadRequiredAsync<BookDistributionListResult>(response, "Book distribution list payload was empty.", ct)
            .ConfigureAwait(false);
    }
}
