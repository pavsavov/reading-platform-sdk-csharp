using Microsoft.Extensions.DependencyInjection;
using PublishingPlatform.SDK.Exceptions;
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
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseAddress))
        {
            throw new PublishingPlatformConfigurationException("BaseUrl must be a valid absolute URL.");
        }

        if (baseAddress.Scheme != Uri.UriSchemeHttps)
        {
            throw new PublishingPlatformConfigurationException("BaseUrl must use HTTPS.");
        }

        var services = new ServiceCollection();
        services.AddHttpClient(ClientName, client =>
        {
            client.BaseAddress = baseAddress;
            client.Timeout = options.Timeout;
        });

        return services.BuildServiceProvider();
    }
}
