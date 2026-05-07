using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Extensions;
using PublishingPlatform.SDK.Infrastructure.Auth;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Http.Handlers;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class HandlersAndDiCoverageTests
{
    [Fact]
    public async Task AuthHandler_SetsBearerToken_AndCallsInnerHandler()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider.GetTokenAsync(Arg.Any<CancellationToken>()).Returns("secret-token");

        using var innerHandler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        var handler = new AuthHandler(tokenProvider)
        {
            InnerHandler = innerHandler,
        };
        using var client = new HttpClient(handler);

        _ = await client.GetAsync("https://api.example.test/books");

        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();
        innerHandler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        innerHandler.LastRequest.Headers.Authorization.Parameter.Should().Be("secret-token");
        await tokenProvider.Received(1).GetTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CorrelationHandler_AddsHeader_WhenMissing()
    {
        using var innerHandler = new RecordingHandler((request, _) =>
        {
            request.Headers.Contains(CorrelationHandler.CorrelationHeader).Should().BeTrue();
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        });
        var handler = new CorrelationHandler
        {
            InnerHandler = innerHandler,
        };
        using var client = new HttpClient(handler);

        _ = await client.GetAsync("https://api.example.test/books");

        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.TryGetValues(CorrelationHandler.CorrelationHeader, out var values).Should().BeTrue();
        values.Should().ContainSingle();
    }

    [Fact]
    public async Task CorrelationHandler_PreservesHeader_WhenAlreadyPresent()
    {
        var expected = "existing-correlation-id";
        using var innerHandler = new RecordingHandler((request, _) =>
        {
            request.Headers.TryGetValues(CorrelationHandler.CorrelationHeader, out var values).Should().BeTrue();
            values.Should().ContainSingle().Which.Should().Be(expected);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        });
        var handler = new CorrelationHandler
        {
            InnerHandler = innerHandler,
        };
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.test/books");
        request.Headers.Add(CorrelationHandler.CorrelationHeader, expected);

        _ = await client.SendAsync(request);

        innerHandler.LastRequest.Should().NotBeNull();
    }

    [Fact]
    public async Task DiagnosticsHandler_CallsInnerHandler_AndReturnsResponse()
    {
        using var innerHandler = new RecordingHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.Accepted)));
        var handler = new DiagnosticsHandler
        {
            InnerHandler = innerHandler,
        };
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://api.example.test/books");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);
        innerHandler.LastRequest.Should().NotBeNull();
    }

    [Fact]
    public void AddPublishingPlatformClient_ThrowsForNullInputs()
    {
        var services = new ServiceCollection();

        Action nullConfigure = () => ServiceCollectionExtensions.AddPublishingPlatformClient(services, null!);
        Action nullServices = () => ServiceCollectionExtensions.AddPublishingPlatformClient(null!, _ => { });
        Action nullServicesForOptions = () => ServiceCollectionExtensions.AddPublishingPlatformClient(null!);

        nullConfigure.Should().Throw<ArgumentNullException>();
        nullServices.Should().Throw<ArgumentNullException>();
        nullServicesForOptions.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddPublishingPlatformClient_ResolvesFromOptionsPatternConfiguration()
    {
        var services = new ServiceCollection();
        services.Configure<PublishingPlatformClientOptions>(options =>
        {
            options.BaseUrl = "https://api.example.test";
            options.ApiKey = "key";
            options.Timeout = TimeSpan.FromSeconds(18);
        });
        services.AddPublishingPlatformClient();

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<PublishingPlatformClientOptions>>().Value;
        var clientOptions = provider.GetRequiredService<PublishingPlatformClientOptions>();
        var client = provider.GetRequiredService<IPublishingPlatformClient>();

        options.BaseUrl.Should().Be("https://api.example.test");
        options.Timeout.Should().Be(TimeSpan.FromSeconds(18));
        clientOptions.BaseUrl.Should().Be("https://api.example.test");
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddPublishingPlatformClient_ResolvesSharedTransportAndPipelines()
    {
        var customMapper = Substitute.For<IPublishingPlatformErrorMapper>();
        var services = new ServiceCollection();
        services.AddPublishingPlatformClient(options =>
        {
            options.BaseUrl = "https://api.example.test";
            options.ApiKey = "key";
            options.Timeout = TimeSpan.FromSeconds(12);
            options.ErrorMapper = customMapper;
            options.Resilience = new PublishingPlatformResilienceOptions
            {
                Enabled = true,
                Retry = new RetryResilienceOptions
                {
                    Enabled = true,
                    MaxRetryAttempts = 1,
                    BaseDelay = TimeSpan.FromMilliseconds(5),
                },
                CircuitBreaker = new CircuitBreakerResilienceOptions
                {
                    Enabled = true,
                    FailureRatio = 0.5,
                    MinimumThroughput = 2,
                    BreakDuration = TimeSpan.FromMilliseconds(600),
                },
                AttemptTimeout = new TimeoutResilienceOptions
                {
                    Enabled = true,
                    Timeout = TimeSpan.FromSeconds(2),
                },
                TotalTimeout = new TimeoutResilienceOptions
                {
                    Enabled = true,
                    Timeout = TimeSpan.FromSeconds(4),
                },
            };
        });

        using var provider = services.BuildServiceProvider();

        var transport = provider.GetRequiredService<ISharedHttpTransport>();
        var resilience = provider.GetRequiredService<IPublishingPlatformResiliencePipeline>();
        var mapper = provider.GetRequiredService<IPublishingPlatformErrorMapper>();
        var client = provider.GetRequiredService<IPublishingPlatformClient>();

        transport.Should().NotBeNull();
        resilience.Should().BeOfType<DefaultPublishingPlatformResiliencePipeline>();
        mapper.Should().BeSameAs(customMapper);
        client.Should().NotBeNull();
        client.Books.Should().NotBeNull();
        client.BookContent.Should().NotBeNull();
        client.BookPublishing.Should().NotBeNull();
        client.BookDistribution.Should().NotBeNull();
        client.BookAccess.Should().NotBeNull();
        client.BookAnalytics.Should().NotBeNull();
        client.BookAuditLogs.Should().NotBeNull();
        client.Webhooks.Should().NotBeNull();
    }

    [Fact]
    public void ResiliencePipelineFactory_CreatesDefaultPipeline_WhenAllStrategiesEnabled()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(10),
            },
            CircuitBreaker = new CircuitBreakerResilienceOptions
            {
                Enabled = true,
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                BreakDuration = TimeSpan.FromMilliseconds(600),
            },
            AttemptTimeout = new TimeoutResilienceOptions
            {
                Enabled = true,
                Timeout = TimeSpan.FromMilliseconds(100),
            },
            TotalTimeout = new TimeoutResilienceOptions
            {
                Enabled = true,
                Timeout = TimeSpan.FromMilliseconds(200),
            },
        };

        var pipeline = ResiliencePipelineFactory.Create(options);

        pipeline.Should().BeOfType<DefaultPublishingPlatformResiliencePipeline>();
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responseFactory;

        public RecordingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return await _responseFactory(request, cancellationToken);
        }
    }
}
