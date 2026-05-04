using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PublishingPlatform.SDK.Abstractions;

namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal sealed class SharedHttpTransport : ISharedHttpTransport
{
    internal const string CorrelationHeaderName = "X-Correlation-Id";

    private readonly HttpClient _httpClient;
    private readonly IPublishingPlatformResiliencePipeline _resiliencePipeline;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private readonly IPublishingPlatformErrorMapper _errorMapper;

    public SharedHttpTransport(
        HttpClient httpClient,
        IPublishingPlatformResiliencePipeline resiliencePipeline,
        ICorrelationIdProvider correlationIdProvider,
        IPublishingPlatformErrorMapper errorMapper)
    {
        _httpClient = httpClient;
        _resiliencePipeline = resiliencePipeline;
        _correlationIdProvider = correlationIdProvider;
        _errorMapper = errorMapper;
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
                    request.Headers.TryGetValues(CorrelationHeaderName, out var correlationValues);
                    var context = new PublishingPlatformErrorContext
                    {
                        Method = method,
                        RelativePath = relativePath,
                        StatusCode = (int)response.StatusCode,
                        Message = message,
                        CorrelationId = correlationValues?.FirstOrDefault(),
                    };

                    throw _errorMapper.Map(context);
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
        TransportErrorResponse? payload = null;
        try
        {
            payload = await response.Content.ReadFromJsonAsync<TransportErrorResponse>(cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            // Fallback to generic message when response body is not JSON.
        }

        if (payload is not null && !string.IsNullOrWhiteSpace(payload.Message))
        {
            return payload.Message;
        }

        return $"HTTP {(int)response.StatusCode} returned by Publishing Platform API.";
    }
}
