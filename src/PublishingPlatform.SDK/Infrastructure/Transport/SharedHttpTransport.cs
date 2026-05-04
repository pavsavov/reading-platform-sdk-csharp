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
        IReadOnlyDictionary<string, string>? headers = null,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        var correlationId = _correlationIdProvider.Create();
        var response = await _resiliencePipeline.ExecuteAsync(
            new PublishingPlatformResilienceContext
            {
                Method = method,
            },
            async ct =>
            {
                using var request = BuildRequest(method, relativePath, content, correlationId, headers);
                return await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var message = await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
        var context = new PublishingPlatformErrorContext
        {
            Method = method,
            RelativePath = relativePath,
            StatusCode = (int)response.StatusCode,
            Message = message,
            CorrelationId = correlationId,
            OperationName = operationName,
        };

        throw _errorMapper.Map(context);
    }

    internal static HttpRequestMessage BuildRequest(
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

        if (!request.Headers.Contains(CorrelationHeaderName))
        {
            request.Headers.Add(CorrelationHeaderName, correlationId);
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
