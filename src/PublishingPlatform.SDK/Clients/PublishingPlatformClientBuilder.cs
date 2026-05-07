using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Clients;

public sealed class PublishingPlatformClientBuilder
{
    private readonly PublishingPlatformClientOptions _options;
    private IPublishingPlatformResiliencePipeline? _customPipeline;
    private IPublishingPlatformErrorMapper? _customErrorMapper;
    private HttpMessageHandler? _primaryHttpMessageHandler;

    private PublishingPlatformClientBuilder(PublishingPlatformClientOptions options)
    {
        _options = options;
    }

    public static PublishingPlatformClientBuilder Create(PublishingPlatformClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new PublishingPlatformClientBuilder(options);
    }

    public PublishingPlatformClientBuilder WithResiliencePipeline(IPublishingPlatformResiliencePipeline pipeline)
    {
        _customPipeline = pipeline;
        return this;
    }

    public PublishingPlatformClientBuilder WithErrorMapper(IPublishingPlatformErrorMapper errorMapper)
    {
        ArgumentNullException.ThrowIfNull(errorMapper);
        _customErrorMapper = errorMapper;
        return this;
    }

    internal PublishingPlatformClientBuilder WithPrimaryHttpMessageHandler(HttpMessageHandler primaryHttpMessageHandler)
    {
        ArgumentNullException.ThrowIfNull(primaryHttpMessageHandler);
        _primaryHttpMessageHandler = primaryHttpMessageHandler;
        return this;
    }

    public IPublishingPlatformClient Build()
    {
        var serviceProvider = SdkHttpClientResolver.BuildBootstrapServiceProvider(_options, _primaryHttpMessageHandler);
        var httpClient = SdkHttpClientResolver.Resolve(serviceProvider);
        var pipeline = _customPipeline ?? ResiliencePipelineFactory.Create(_options.Resilience);
        var errorMapper = _customErrorMapper ?? _options.ErrorMapper ?? new DefaultPublishingPlatformErrorMapper();
        var transport = SharedHttpTransportBuilder.Create()
            .WithHttpClient(httpClient)
            .WithResiliencePipeline(pipeline)
            .WithCorrelationProvider(new GuidCorrelationIdProvider())
            .WithErrorMapper(errorMapper)
            .WithDiagnosticsOptionsResolver(new DefaultDiagnosticsOptionsResolver(_options))
            .Build();

        return PublishingPlatformClient.CreateFromTransport(transport);
    }
}
