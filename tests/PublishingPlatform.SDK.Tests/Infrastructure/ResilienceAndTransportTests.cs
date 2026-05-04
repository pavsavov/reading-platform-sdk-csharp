using Microsoft.Extensions.DependencyInjection;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Extensions;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class ResilienceAndTransportTests
{
    [Fact]
    public void CreatePipeline_ReturnsNoOp_WhenResilienceNotConfigured()
    {
        var pipeline = ResiliencePipelineFactory.Create(null);

        Assert.IsType<NoOpPublishingPlatformResiliencePipeline>(pipeline);
    }

    [Fact]
    public void CreatePipeline_ReturnsNoOp_WhenResilienceDisabled()
    {
        var pipeline = ResiliencePipelineFactory.Create(new PublishingPlatformResilienceOptions { Enabled = false });

        Assert.IsType<NoOpPublishingPlatformResiliencePipeline>(pipeline);
    }

    [Fact]
    public void BuildBootstrapServiceProvider_ResolvesHttpClientFactoryClient()
    {
        var options = new PublishingPlatformClientOptions { BaseUrl = "https://example.test" };
        using var serviceProvider = SdkHttpClientResolver.BuildBootstrapServiceProvider(options);

        var client = SdkHttpClientResolver.Resolve(serviceProvider);

        Assert.Equal(new Uri("https://example.test"), client.BaseAddress);
    }

    [Fact]
    public void AddPublishingPlatformClient_RegistersFactoryResolvedTransportDependencies()
    {
        var services = new ServiceCollection();
        services.AddPublishingPlatformClient(options =>
        {
            options.BaseUrl = "https://example.test";
            options.ApiKey = "key";
        });

        using var provider = services.BuildServiceProvider();

        var platformClient = provider.GetRequiredService<IPublishingPlatformClient>();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

        Assert.NotNull(platformClient);
        Assert.NotNull(httpClientFactory.CreateClient(SdkHttpClientResolver.ClientName));
    }
}
