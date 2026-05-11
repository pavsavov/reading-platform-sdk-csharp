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

    /// <summary>
    /// Creates a builder from a minimal base URL and API key configuration.
    /// </summary>
    /// <param name="baseUrl">The Publishing Platform API base URL.</param>
    /// <param name="apiKey">The API key used by the SDK transport.</param>
    /// <returns>A configured <see cref="PublishingPlatformClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="baseUrl"/> or <paramref name="apiKey"/> is empty.
    /// </exception>
    public static PublishingPlatformClientBuilder Create(string baseUrl, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL must not be empty.", nameof(baseUrl));
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key must not be empty.", nameof(apiKey));
        }

        return Create(new PublishingPlatformClientOptions
        {
            BaseUrl = baseUrl,
            ApiKey = apiKey,
        });
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
