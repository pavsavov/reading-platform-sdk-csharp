using System.Net.Http.Json;
using System.Text.Json;

namespace PublishingPlatform.SDK.Infrastructure.Transport.Errors;

internal sealed class DefaultTransportResponseErrorReader : ITransportResponseErrorReader
{
    public async Task<string> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        TransportErrorResponse? payload = null;
        try
        {
            payload = await response.Content.ReadFromJsonAsync<TransportErrorResponse>(cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            // Fallback message is returned below.
        }

        if (payload is not null && !string.IsNullOrWhiteSpace(payload.Message))
        {
            return payload.Message;
        }

        return $"HTTP {(int)response.StatusCode} returned by Publishing Platform API.";
    }
}
