using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Clients;

public sealed class PublishingPlatformClientBuilder
{
    private readonly PublishingPlatformClientOptions _options;
    private IPublishingPlatformResiliencePipeline? _customPipeline;

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

    public IPublishingPlatformClient Build()
    {
        var serviceProvider = SdkHttpClientResolver.BuildBootstrapServiceProvider(_options);
        var httpClient = SdkHttpClientResolver.Resolve(serviceProvider);
        var pipeline = _customPipeline ?? ResiliencePipelineFactory.Create(_options.Resilience);
        var transport = new SharedHttpTransport(httpClient, pipeline, new GuidCorrelationIdProvider());

        return PublishingPlatformClient.CreateFromTransport(transport);
    }
}
