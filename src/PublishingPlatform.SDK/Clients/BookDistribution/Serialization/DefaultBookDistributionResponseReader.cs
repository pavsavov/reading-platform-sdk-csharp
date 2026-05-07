using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;
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
        var operation = await response.Content.ReadFromJsonAsync<BookDistributionOperation>(ct).ConfigureAwait(false);
        if (operation is null)
        {
            throw new BookValidationException("Book distribution operation payload was empty.");
        }

        return operation;
    }

    /// <inheritdoc />
    public async Task<BookDistributionListResult> ReadListAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var list = await response.Content.ReadFromJsonAsync<BookDistributionListResult>(ct).ConfigureAwait(false);
        if (list is null)
        {
            throw new BookValidationException("Book distribution list payload was empty.");
        }

        return list;
    }
}
