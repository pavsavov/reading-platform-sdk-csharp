using System.Net.Http.Json;
using System.Text.Json;

namespace PublishingPlatform.SDK.Infrastructure.Transport.Errors;

internal sealed class DefaultTransportResponseErrorReader : ITransportResponseErrorReader
{
    public async Task<NormalizedTransportError> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
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

        var statusCode = (int)response.StatusCode;
        var message = payload is not null && !string.IsNullOrWhiteSpace(payload.Message)
            ? payload.Message
            : $"HTTP {statusCode} returned by Publishing Platform API.";
        var errorCode = payload is not null && !string.IsNullOrWhiteSpace(payload.ErrorCode)
            ? payload.ErrorCode
            : $"http_{statusCode}";

        return new NormalizedTransportError
        {
            Message = message,
            ErrorCode = errorCode,
            CorrelationId = payload?.CorrelationId,
            RequestId = payload?.RequestId,
        };
    }
}
