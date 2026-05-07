using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.Transport.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PublishingPlatform.SDK.Options;

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
    private IDiagnosticsOptionsResolver _diagnosticsOptionsResolver =
        new DefaultDiagnosticsOptionsResolver(new PublishingPlatformClientOptions());
    private ILogger<SharedHttpTransport> _logger = NullLogger<SharedHttpTransport>.Instance;

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

    public SharedHttpTransportBuilder WithDiagnosticsOptionsResolver(IDiagnosticsOptionsResolver diagnosticsOptionsResolver)
    {
        _diagnosticsOptionsResolver = diagnosticsOptionsResolver;
        return this;
    }

    public SharedHttpTransportBuilder WithLogger(ILogger<SharedHttpTransport> logger)
    {
        _logger = logger;
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
            _errorContextFactory,
            _diagnosticsOptionsResolver,
            _logger);
    }
}
