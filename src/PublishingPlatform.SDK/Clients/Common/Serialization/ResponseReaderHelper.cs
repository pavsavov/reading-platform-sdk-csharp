using System.Net.Http.Json;
using PublishingPlatform.SDK.Exceptions;

namespace PublishingPlatform.SDK.Clients.Common.Serialization;

/// <summary>
/// Provides common response-deserialization helpers for module response readers.
/// </summary>
internal static class ResponseReaderHelper
{
    /// <summary>
    /// Reads and validates a required JSON payload from an HTTP response.
    /// </summary>
    /// <typeparam name="TPayload">The expected payload type.</typeparam>
    /// <param name="response">The response containing JSON content.</param>
    /// <param name="emptyPayloadMessage">The validation message when payload is missing.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="BookValidationException">Thrown when payload is missing.</exception>
    internal static async Task<TPayload> ReadRequiredAsync<TPayload>(
        HttpResponseMessage response,
        string emptyPayloadMessage,
        CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<TPayload>(cancellationToken).ConfigureAwait(false);
        if (payload is null)
        {
            throw new BookValidationException(emptyPayloadMessage);
        }

        return payload;
    }
}
