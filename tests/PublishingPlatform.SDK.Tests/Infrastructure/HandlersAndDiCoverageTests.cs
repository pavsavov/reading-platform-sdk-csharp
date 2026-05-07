using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Extensions;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Http.Handlers;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;
using System.Net.Http.Json;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class HandlersAndDiCoverageTests
{
    [Fact]
    public async Task ApiKeyAuthHandler_SetsApiKeyHeader_AndCallsInnerHandler()
    {
        const string expectedApiKey = "secret-api-key";

        using var innerHandler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        var handler = new ApiKeyAuthHandler(expectedApiKey)
        {
            InnerHandler = innerHandler,
        };
        using var client = new HttpClient(handler);

        _ = await client.GetAsync("https://api.example.test/books");

        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.TryGetValues(TransportHeaderNames.ApiKey, out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be(expectedApiKey);
    }

    [Fact]
    public async Task AddPublishingPlatformClient_AppliesApiKeyHeaderThroughNamedHttpClientPipeline()
    {
        const string expectedApiKey = "di-api-key";
        var innerHandler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        var services = new ServiceCollection();
        services.AddPublishingPlatformClient(options =>
        {
            options.BaseUrl = "https://api.example.test";
            options.ApiKey = expectedApiKey;
        });
        services.Configure<HttpClientFactoryOptions>(SdkHttpClientResolver.ClientName, options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(builder => builder.PrimaryHandler = innerHandler);
        });

        using var provider = services.BuildServiceProvider();
        var transport = provider.GetRequiredService<ISharedHttpTransport>();

        using var response = await transport.SendAsync(HttpMethod.Get, "/books", null, null, "Books.List");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.TryGetValues(TransportHeaderNames.ApiKey, out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be(expectedApiKey);
    }

    [Fact]
    public async Task PublishingPlatformClientBuilder_AppliesApiKeyHeaderThroughBootstrapHttpClientPipeline()
    {
        const string expectedApiKey = "builder-api-key";
        using var innerHandler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { Items = Array.Empty<Book>(), TotalCount = 0, ContinuationToken = (string?)null }),
        }));
        var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
        {
            BaseUrl = "https://api.example.test",
            ApiKey = expectedApiKey,
        })
        .WithPrimaryHttpMessageHandler(innerHandler)
        .Build();

        _ = await client.Books.ListAsync(new ListBooksRequest());

        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.TryGetValues(TransportHeaderNames.ApiKey, out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be(expectedApiKey);
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
