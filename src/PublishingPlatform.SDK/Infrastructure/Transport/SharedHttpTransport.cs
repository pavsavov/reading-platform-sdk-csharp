using System.Net.Http.Headers;
using System.Net.Http.Json;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Errors;

namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal sealed class SharedHttpTransport : ISharedHttpTransport
{
    internal const string CorrelationHeaderName = "X-Correlation-Id";

    private readonly HttpClient _httpClient;
    private readonly IPublishingPlatformResiliencePipeline _resiliencePipeline;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public SharedHttpTransport(
        HttpClient httpClient,
        IPublishingPlatformResiliencePipeline resiliencePipeline,
        ICorrelationIdProvider correlationIdProvider)
    {
        _httpClient = httpClient;
        _resiliencePipeline = resiliencePipeline;
        _correlationIdProvider = correlationIdProvider;
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePath,
        HttpContent? content,
        CancellationToken cancellationToken = default)
    {
        return await _resiliencePipeline.ExecuteAsync(
            async ct =>
            {
                using var request = BuildRequest(method, relativePath, content);
                var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var message = await ReadErrorMessageAsync(response, ct).ConfigureAwait(false);
                    throw ErrorMapper.Map((int)response.StatusCode, message);
                }

                return response;
            },
            cancellationToken).ConfigureAwait(false);
    }

    internal HttpRequestMessage BuildRequest(HttpMethod method, string relativePath, HttpContent? content)
    {
        var request = new HttpRequestMessage(method, relativePath)
        {
            Content = content,
        };

        if (!request.Headers.Contains(CorrelationHeaderName))
        {
            request.Headers.Add(CorrelationHeaderName, _correlationIdProvider.Create());
        }

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<TransportErrorResponse>(cancellationToken).ConfigureAwait(false);
        if (payload is not null && !string.IsNullOrWhiteSpace(payload.Message))
        {
            return payload.Message;
        }

        return $"HTTP {(int)response.StatusCode} returned by Publishing Platform API.";
    }
}
