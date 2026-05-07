using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Http.Handlers;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Extensions;

/// <summary>
/// Provides dependency injection registration helpers for the Publishing Platform SDK.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers SDK services and resolves configuration from <see cref="IOptions{TOptions}"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddPublishingPlatformClient(
        this IServiceCollection services)
    {
        services.AddOptions<PublishingPlatformClientOptions>()
            .Validate(PublishingPlatformClientOptionsValidator.Validate)
            .ValidateOnStart();

        RegisterPublishingPlatformClient(services);

        return services;
    }

    /// <summary>
    /// Registers SDK services using direct POCO configuration for non-hosted or manual setup scenarios.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">The configuration callback that populates <see cref="PublishingPlatformClientOptions"/>.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddPublishingPlatformClient(
        this IServiceCollection services,
        Action<PublishingPlatformClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<PublishingPlatformClientOptions>()
            .Configure(configure)
            .Validate(PublishingPlatformClientOptionsValidator.Validate)
            .ValidateOnStart();

        RegisterPublishingPlatformClient(services);

        return services;
    }

    private static void RegisterPublishingPlatformClient(IServiceCollection services)
    {
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<PublishingPlatformClientOptions>>().Value);
        services.AddHttpClient(SdkHttpClientResolver.ClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PublishingPlatformClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = options.Timeout;
        })
        .AddHttpMessageHandler(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PublishingPlatformClientOptions>>().Value;
            return new ApiKeyAuthHandler(options.ApiKey);
        });

        services.AddSingleton<IPublishingPlatformResiliencePipeline>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PublishingPlatformClientOptions>>().Value;
            return ResiliencePipelineFactory.Create(options.Resilience);
        });
        services.AddSingleton<IPublishingPlatformErrorMapper>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PublishingPlatformClientOptions>>().Value;
            return options.ErrorMapper ?? new DefaultPublishingPlatformErrorMapper();
        });
        services.AddSingleton<ICorrelationIdProvider, GuidCorrelationIdProvider>();
        services.AddSingleton<IDiagnosticsOptionsResolver>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PublishingPlatformClientOptions>>().Value;
            return new DefaultDiagnosticsOptionsResolver(options);
        });
        services.AddSingleton<HttpPipelinePolicy>();
        services.AddSingleton<ISharedHttpTransport>(sp =>
        {
            var httpClient = SdkHttpClientResolver.Resolve(sp);
            var pipeline = sp.GetRequiredService<IPublishingPlatformResiliencePipeline>();
            var correlationProvider = sp.GetRequiredService<ICorrelationIdProvider>();
            var errorMapper = sp.GetRequiredService<IPublishingPlatformErrorMapper>();
            var diagnosticsResolver = sp.GetRequiredService<IDiagnosticsOptionsResolver>();
            var logger = sp.GetService<ILogger<SharedHttpTransport>>();
            return SharedHttpTransportBuilder.Create()
                .WithHttpClient(httpClient)
                .WithResiliencePipeline(pipeline)
                .WithCorrelationProvider(correlationProvider)
                .WithErrorMapper(errorMapper)
                .WithDiagnosticsOptionsResolver(diagnosticsResolver)
                .WithLogger(logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<SharedHttpTransport>.Instance)
                .Build();
        });

        services.AddSingleton<IBooksClient>(sp => new BooksClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookContentClient>(sp => new BookContentClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookPublishingClient>(sp => new BookPublishingClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookDistributionClient>(sp => new BookDistributionClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookAccessClient>(sp => new BookAccessClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookAnalyticsClient>(sp => new BookAnalyticsClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookAuditLogsClient>(sp => new BookAuditLogsClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IWebhooksClient>(sp => new WebhooksClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IPublishingPlatformClient, PublishingPlatformClient>();
    }
}
