using Microsoft.Extensions.DependencyInjection;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPublishingPlatformClient(
        this IServiceCollection services,
        Action<PublishingPlatformClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new PublishingPlatformClientOptions();
        configure(options);

        services.AddSingleton(options);

        services.AddHttpClient(SdkHttpClientResolver.ClientName, client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = options.Timeout;
        });

        services.AddSingleton(_ =>
            ResiliencePipelineFactory.Create(options.Resilience));
        services.AddSingleton<ICorrelationIdProvider, GuidCorrelationIdProvider>();
        services.AddSingleton<ISharedHttpTransport>(sp =>
        {
            var httpClient = SdkHttpClientResolver.Resolve(sp);
            var pipeline = sp.GetRequiredService<IPublishingPlatformResiliencePipeline>();
            var correlationProvider = sp.GetRequiredService<ICorrelationIdProvider>();
            return new SharedHttpTransport(httpClient, pipeline, correlationProvider);
        });

        services.AddSingleton<IBooksClient>(sp => new BooksClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookContentClient>(sp => new BookContentClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookPublishingClient>(sp => new BookPublishingClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookDistributionClient>(sp => new BookDistributionClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookAccessClient>(sp => new BookAccessClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookAnalyticsClient>(sp => new BookAnalyticsClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookAuditLogsClient>(sp => new BookAuditLogsClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IBookAssetsClient>(sp => new BookAssetsClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IWebhooksClient>(sp => new WebhooksClient(sp.GetRequiredService<ISharedHttpTransport>()));
        services.AddSingleton<IPublishingPlatformClient, PublishingPlatformClient>();

        return services;
    }
}
