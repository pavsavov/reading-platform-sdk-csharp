using System.Net.Http.Headers;

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
        var request = new HttpRequestMessage(method, relativePath)
        {
            Content = content,
        };

        if (!request.Headers.Contains(SharedHttpTransport.CorrelationHeaderName))
        {
            request.Headers.Add(SharedHttpTransport.CorrelationHeaderName, correlationId);
        }

        if (headers is not null)
        {
            foreach (var header in headers.Where(x => !request.Headers.TryAddWithoutValidation(x.Key, x.Value)))
            {
                request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }
}
