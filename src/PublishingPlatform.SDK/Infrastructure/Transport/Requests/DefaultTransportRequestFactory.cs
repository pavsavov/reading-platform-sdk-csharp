using System.Net.Http.Headers;
using System.Linq;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Infrastructure.Transport.Requests;

internal sealed class DefaultTransportRequestFactory : ITransportRequestFactory
{
    public HttpRequestMessage Create(
        HttpMethod method,
        string relativePath,
        HttpContent? content,
        string correlationId,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        return Create(method, relativePath, content, TransportHeaderNames.CorrelationId, correlationId, headers);
    }

    public HttpRequestMessage Create(
        HttpMethod method,
        string relativePath,
        HttpContent? content,
        string correlationHeaderName,
        string? correlationId,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(method, relativePath)
        {
            Content = content,
        };

        if (headers is not null)
        {
            foreach (var header in headers.Where(static header => !string.IsNullOrWhiteSpace(header.Key)))
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(correlationId) && !request.Headers.Contains(correlationHeaderName))
        {
            request.Headers.Add(correlationHeaderName, correlationId);
        }

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }
}
