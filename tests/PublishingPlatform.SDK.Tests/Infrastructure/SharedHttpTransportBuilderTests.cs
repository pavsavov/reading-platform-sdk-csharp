using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class SharedHttpTransportBuilderTests
{
    [Fact]
    public void Build_Throws_WhenHttpClientMissing()
    {
        var act = () => SharedHttpTransportBuilder.Create().Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*HttpClient*");
    }

    [Fact]
    public void Build_CreatesTransport_WhenRequiredDependenciesProvided()
    {
        using var httpClient = new HttpClient();
        var pipeline = new NoOpPublishingPlatformResiliencePipeline();
        var correlation = Substitute.For<ICorrelationIdProvider>();
        correlation.Create().Returns("corr");
        var mapper = Substitute.For<IPublishingPlatformErrorMapper>();

        var transport = SharedHttpTransportBuilder.Create()
            .WithHttpClient(httpClient)
            .WithResiliencePipeline(pipeline)
            .WithCorrelationProvider(correlation)
            .WithErrorMapper(mapper)
            .Build();

        transport.Should().NotBeNull();
    }
}
