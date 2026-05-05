using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport.Requests;

namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal sealed class SharedHttpTransport : ISharedHttpTransport
{
    internal const string CorrelationHeaderName = TransportHeaderNames.CorrelationId;

    private readonly HttpClient _httpClient;
    private readonly IPublishingPlatformResiliencePipeline _resiliencePipeline;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private readonly IPublishingPlatformErrorMapper _errorMapper;
    private readonly ITransportRequestFactory _requestFactory;
    private readonly ITransportResponseErrorReader _responseErrorReader;
    private readonly ITransportErrorContextFactory _errorContextFactory;

    public SharedHttpTransport(
        HttpClient httpClient,
        IPublishingPlatformResiliencePipeline resiliencePipeline,
        ICorrelationIdProvider correlationIdProvider,
        IPublishingPlatformErrorMapper errorMapper)
        : this(httpClient, resiliencePipeline, correlationIdProvider, errorMapper, TransportDependencies.CreateDefault())
    {
    }

    private SharedHttpTransport(
        HttpClient httpClient,
        IPublishingPlatformResiliencePipeline resiliencePipeline,
        ICorrelationIdProvider correlationIdProvider,
        IPublishingPlatformErrorMapper errorMapper,
        TransportDependencies dependencies)
        : this(
            httpClient,
            resiliencePipeline,
            correlationIdProvider,
            errorMapper,
            dependencies.RequestFactory,
            dependencies.ResponseErrorReader,
            dependencies.ErrorContextFactory)
    {
    }

    internal SharedHttpTransport(
        HttpClient httpClient,
        IPublishingPlatformResiliencePipeline resiliencePipeline,
        ICorrelationIdProvider correlationIdProvider,
        IPublishingPlatformErrorMapper errorMapper,
        ITransportRequestFactory requestFactory,
        ITransportResponseErrorReader responseErrorReader,
        ITransportErrorContextFactory errorContextFactory)
    {
        _httpClient = httpClient;
        _resiliencePipeline = resiliencePipeline;
        _correlationIdProvider = correlationIdProvider;
        _errorMapper = errorMapper;
        _requestFactory = requestFactory;
        _responseErrorReader = responseErrorReader;
        _errorContextFactory = errorContextFactory;
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
                using var request = _requestFactory.Create(method, relativePath, content, correlationId, headers);
                return await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var message = await _responseErrorReader.ReadAsync(response, cancellationToken).ConfigureAwait(false);
        var context = _errorContextFactory.Create(
            method,
            relativePath,
            (int)response.StatusCode,
            message,
            correlationId,
            operationName);

        throw _errorMapper.Map(context);
    }
}
