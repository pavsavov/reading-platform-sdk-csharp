using Microsoft.Extensions.DependencyInjection;
using PublishingPlatform.SDK.Infrastructure.Http.Handlers;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal static class SdkHttpClientResolver
{
    internal const string ClientName = "PublishingPlatform.SDK";

    public static HttpClient Resolve(IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        return factory.CreateClient(ClientName);
    }

    public static ServiceProvider BuildBootstrapServiceProvider(PublishingPlatformClientOptions options)
    {
        return BuildBootstrapServiceProvider(options, primaryHandler: null);
    }

    internal static ServiceProvider BuildBootstrapServiceProvider(
        PublishingPlatformClientOptions options,
        HttpMessageHandler? primaryHandler)
    {
        PublishingPlatformClientOptionsValidator.Validate(options);
        var baseAddress = new Uri(options.BaseUrl, UriKind.Absolute);

        var services = new ServiceCollection();
        var httpClientBuilder = services.AddHttpClient(ClientName, client =>
        {
            client.BaseAddress = baseAddress;
            client.Timeout = options.Timeout;
        })
        .AddHttpMessageHandler(() => new ApiKeyAuthHandler(options.ApiKey));

        if (primaryHandler is not null)
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => primaryHandler);
        }

        return services.BuildServiceProvider();
    }
}
