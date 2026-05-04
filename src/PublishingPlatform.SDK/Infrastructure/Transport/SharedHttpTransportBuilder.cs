using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport.Requests;

namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal sealed class SharedHttpTransportBuilder
{
    private HttpClient? _httpClient;
    private IPublishingPlatformResiliencePipeline? _pipeline;
    private ICorrelationIdProvider? _correlationProvider;
    private IPublishingPlatformErrorMapper? _errorMapper;
    private ITransportRequestFactory _requestFactory = new DefaultTransportRequestFactory();
    private ITransportResponseErrorReader _responseErrorReader = new DefaultTransportResponseErrorReader();
    private ITransportErrorContextFactory _errorContextFactory = new DefaultTransportErrorContextFactory();

    private SharedHttpTransportBuilder()
    {
    }

    public static SharedHttpTransportBuilder Create()
    {
        return new SharedHttpTransportBuilder();
    }

    public SharedHttpTransportBuilder WithHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        return this;
    }

    public SharedHttpTransportBuilder WithResiliencePipeline(IPublishingPlatformResiliencePipeline pipeline)
    {
        _pipeline = pipeline;
        return this;
    }

    public SharedHttpTransportBuilder WithCorrelationProvider(ICorrelationIdProvider correlationProvider)
    {
        _correlationProvider = correlationProvider;
        return this;
    }

    public SharedHttpTransportBuilder WithErrorMapper(IPublishingPlatformErrorMapper errorMapper)
    {
        _errorMapper = errorMapper;
        return this;
    }

    public SharedHttpTransportBuilder WithRequestFactory(ITransportRequestFactory requestFactory)
    {
        _requestFactory = requestFactory;
        return this;
    }

    public SharedHttpTransportBuilder WithResponseErrorReader(ITransportResponseErrorReader responseErrorReader)
    {
        _responseErrorReader = responseErrorReader;
        return this;
    }

    public SharedHttpTransportBuilder WithErrorContextFactory(ITransportErrorContextFactory errorContextFactory)
    {
        _errorContextFactory = errorContextFactory;
        return this;
    }

    public SharedHttpTransport Build()
    {
        return new SharedHttpTransport(
            _httpClient ?? throw new InvalidOperationException("HttpClient is required."),
            _pipeline ?? throw new InvalidOperationException("Resilience pipeline is required."),
            _correlationProvider ?? throw new InvalidOperationException("Correlation provider is required."),
            _errorMapper ?? throw new InvalidOperationException("Error mapper is required."),
            _requestFactory,
            _responseErrorReader,
            _errorContextFactory);
    }
}
