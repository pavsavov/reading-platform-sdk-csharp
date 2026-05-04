using Bogus;
using Microsoft.Extensions.DependencyInjection;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Extensions;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Options;
using System.Net.Http.Json;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class ResilienceAndTransportTests
{
    private static readonly Faker Faker = new();

    static ResilienceAndTransportTests()
    {
        Randomizer.Seed = new Random(1337);
    }

    [Fact]
    public async Task SendAsync_AddsAcceptAndCorrelationHeaders_AndUsesRelativePath()
    {
        var correlationId = Faker.Random.Hexadecimal(32, prefix: string.Empty);
        var correlationProvider = new FixedCorrelationIdProvider(correlationId);

        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test"),
        };

        var pipeline = BuildPassThroughPipeline();
        var transport = new SharedHttpTransport(httpClient, pipeline, correlationProvider, new DefaultPublishingPlatformErrorMapper());

        await transport.SendAsync(HttpMethod.Get, "/books", null);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Headers.Accept.ToString().Should().Contain("application/json");
        handler.LastRequest.Headers.TryGetValues(SharedHttpTransport.CorrelationHeaderName, out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be(correlationId);
        handler.LastRequest.RequestUri!.ToString().Should().Be("https://api.example.test/books");
        correlationProvider.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_ThrowsApiException_WithMessageFromErrorPayload()
    {
        var expectedMessage = Faker.Lorem.Sentence();
        using var handler = new RecordingHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                Content = JsonContent.Create(new { Message = expectedMessage }),
            }));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };

        var transport = new SharedHttpTransport(httpClient, BuildPassThroughPipeline(), new FixedCorrelationIdProvider(Faker.Random.Guid().ToString("N")), new DefaultPublishingPlatformErrorMapper());

        Func<Task> act = async () => await transport.SendAsync(HttpMethod.Post, "/books", JsonContent.Create(new { Title = "A" }));

        var exception = await act.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(400);
        exception.Which.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task SendAsync_ThrowsApiException_WithFallbackMessage_WhenErrorPayloadMissing()
    {
        using var handler = new RecordingHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("plain-text-error"),
            }));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };

        var transport = new SharedHttpTransport(httpClient, BuildPassThroughPipeline(), new FixedCorrelationIdProvider(Faker.Random.Guid().ToString("N")), new DefaultPublishingPlatformErrorMapper());

        Func<Task> act = async () => await transport.SendAsync(HttpMethod.Get, "/books", null);

        var exception = await act.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(500);
        exception.Which.Message.Should().Be("HTTP 500 returned by Publishing Platform API.");
    }

    [Fact]
    public async Task SendAsync_ForwardsCancellationToken_ToResiliencePipeline()
    {
        var tokenSource = new CancellationTokenSource();
        var capturedToken = CancellationToken.None;

        var pipeline = Substitute.For<IPublishingPlatformResiliencePipeline>();
        pipeline.ExecuteAsync(Arg.Any<Func<CancellationToken, Task<HttpResponseMessage>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedToken = callInfo.Arg<CancellationToken>();
                var operation = callInfo.Arg<Func<CancellationToken, Task<HttpResponseMessage>>>();
                return operation(capturedToken);
            });

        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = new SharedHttpTransport(httpClient, pipeline, new FixedCorrelationIdProvider(Faker.Random.Guid().ToString("N")), new DefaultPublishingPlatformErrorMapper());

        await transport.SendAsync(HttpMethod.Get, "/books", null, tokenSource.Token);

        capturedToken.Should().Be(tokenSource.Token);
    }

    [Fact]
    public async Task SendAsync_UsesCustomErrorMapperWithContext()
    {
        var mapper = Substitute.For<IPublishingPlatformErrorMapper>();
        var correlationId = Faker.Random.Guid().ToString("N");
        var expectedException = new InvalidOperationException("custom-mapped-error");
        mapper.Map(Arg.Any<PublishingPlatformErrorContext>()).Returns(expectedException);

        using var handler = new RecordingHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
            {
                Content = JsonContent.Create(new { Message = "book not found" }),
            }));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = new SharedHttpTransport(
            httpClient,
            BuildPassThroughPipeline(),
            new FixedCorrelationIdProvider(correlationId),
            mapper);

        Func<Task> act = async () => await transport.SendAsync(HttpMethod.Get, "/books/42", null);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Should().BeSameAs(expectedException);
        mapper.Received(1).Map(Arg.Is<PublishingPlatformErrorContext>(context =>
            context.StatusCode == 404
            && context.Message == "book not found"
            && context.RelativePath == "/books/42"
            && context.Method == HttpMethod.Get
            && context.CorrelationId == correlationId));
    }

    [Fact]
    public void BuildBootstrapServiceProvider_ThrowsForInvalidBaseUrl()
    {
        var options = new PublishingPlatformClientOptions
        {
            BaseUrl = "not-an-absolute-url",
        };

        Action act = () => SdkHttpClientResolver.BuildBootstrapServiceProvider(options);

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*BaseUrl must be a valid absolute URL.*");
    }

    [Fact]
    public void BuildBootstrapServiceProvider_ConfiguresBaseAddressAndTimeout()
    {
        var expectedTimeout = TimeSpan.FromSeconds(19);
        var options = new PublishingPlatformClientOptions
        {
            BaseUrl = "https://api.example.test",
            Timeout = expectedTimeout,
        };

        using var provider = SdkHttpClientResolver.BuildBootstrapServiceProvider(options);
        var client = SdkHttpClientResolver.Resolve(provider);

        client.BaseAddress.Should().Be(new Uri("https://api.example.test"));
        client.Timeout.Should().Be(expectedTimeout);
    }

    [Fact]
    public void Resolve_UsesNamedClientFromFactory()
    {
        using var expectedClient = new HttpClient();
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(SdkHttpClientResolver.ClientName).Returns(expectedClient);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(factory)
            .BuildServiceProvider();

        var resolvedClient = SdkHttpClientResolver.Resolve(serviceProvider);

        resolvedClient.Should().BeSameAs(expectedClient);
        factory.Received(1).CreateClient(SdkHttpClientResolver.ClientName);
    }

    [Fact]
    public void AddPublishingPlatformClient_UsesConfiguredErrorMapper()
    {
        var customMapper = Substitute.For<IPublishingPlatformErrorMapper>();
        var services = new ServiceCollection();
        services.AddPublishingPlatformClient(options =>
        {
            options.BaseUrl = "https://api.example.test";
            options.ApiKey = "key";
            options.ErrorMapper = customMapper;
        });

        using var provider = services.BuildServiceProvider();
        var resolvedMapper = provider.GetRequiredService<IPublishingPlatformErrorMapper>();

        resolvedMapper.Should().BeSameAs(customMapper);
    }

    [Fact]
    public void CreatePipeline_ReturnsNoOp_WhenResilienceNotConfigured()
    {
        var pipeline = ResiliencePipelineFactory.Create(null);

        pipeline.Should().BeOfType<NoOpPublishingPlatformResiliencePipeline>();
    }

    [Fact]
    public void CreatePipeline_ReturnsNoOp_WhenResilienceDisabled()
    {
        var pipeline = ResiliencePipelineFactory.Create(new PublishingPlatformResilienceOptions { Enabled = false });

        pipeline.Should().BeOfType<NoOpPublishingPlatformResiliencePipeline>();
    }

    [Fact]
    public void CreatePipeline_ReturnsDefaultPipeline_WhenResilienceEnabled()
    {
        var pipeline = ResiliencePipelineFactory.Create(new PublishingPlatformResilienceOptions
        {
            Enabled = true,
        });

        pipeline.Should().BeOfType<DefaultPublishingPlatformResiliencePipeline>();
    }

    [Fact]
    public void CreatePipeline_DoesNotValidateDisabledSubStrategies()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            CircuitBreaker = new CircuitBreakerResilienceOptions
            {
                Enabled = false,
                FailureRatio = 99,
                MinimumThroughput = -1,
                BreakDuration = TimeSpan.Zero,
            },
        };

        Action act = () => ResiliencePipelineFactory.Create(options);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void CreatePipeline_Throws_WhenRetryAttemptsInvalid(int invalidAttempts)
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                MaxRetryAttempts = invalidAttempts,
            },
        };

        Action act = () => ResiliencePipelineFactory.Create(options);

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*MaxRetryAttempts*");
    }

    [Fact]
    public void CreatePipeline_Throws_WhenRetryBaseDelayInvalid()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                BaseDelay = TimeSpan.Zero,
            },
        };

        Action act = () => ResiliencePipelineFactory.Create(options);

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*BaseDelay*");
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-0.1d)]
    [InlineData(1.1d)]
    public void CreatePipeline_Throws_WhenCircuitBreakerRatioInvalid(double invalidRatio)
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            CircuitBreaker = new CircuitBreakerResilienceOptions
            {
                Enabled = true,
                FailureRatio = invalidRatio,
            },
        };

        Action act = () => ResiliencePipelineFactory.Create(options);

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*FailureRatio*");
    }

    [Fact]
    public void CreatePipeline_Throws_WhenCircuitBreakerThroughputInvalid()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            CircuitBreaker = new CircuitBreakerResilienceOptions
            {
                Enabled = true,
                MinimumThroughput = 0,
            },
        };

        Action act = () => ResiliencePipelineFactory.Create(options);

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*MinimumThroughput*");
    }

    [Fact]
    public void CreatePipeline_Throws_WhenCircuitBreakerDurationInvalid()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            CircuitBreaker = new CircuitBreakerResilienceOptions
            {
                Enabled = true,
                BreakDuration = TimeSpan.Zero,
            },
        };

        Action act = () => ResiliencePipelineFactory.Create(options);

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*BreakDuration*");
    }

    [Theory]
    [InlineData(true, false, "AttemptTimeout")]
    [InlineData(false, true, "TotalTimeout")]
    public void CreatePipeline_Throws_WhenEnabledTimeoutInvalid(bool attemptEnabled, bool totalEnabled, string expectedName)
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            AttemptTimeout = new TimeoutResilienceOptions
            {
                Enabled = attemptEnabled,
                Timeout = TimeSpan.Zero,
            },
            TotalTimeout = new TimeoutResilienceOptions
            {
                Enabled = totalEnabled,
                Timeout = TimeSpan.Zero,
            },
        };

        Action act = () => ResiliencePipelineFactory.Create(options);

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage($"*{expectedName}*");
    }

    private static IPublishingPlatformResiliencePipeline BuildPassThroughPipeline()
    {
        var pipeline = Substitute.For<IPublishingPlatformResiliencePipeline>();
        pipeline.ExecuteAsync(Arg.Any<Func<CancellationToken, Task<HttpResponseMessage>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var operation = callInfo.Arg<Func<CancellationToken, Task<HttpResponseMessage>>>();
                var token = callInfo.Arg<CancellationToken>();
                return operation(token);
            });

        return pipeline;
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

    private sealed class FixedCorrelationIdProvider : ICorrelationIdProvider
    {
        private readonly string _value;

        public FixedCorrelationIdProvider(string value)
        {
            _value = value;
        }

        public int CallCount { get; private set; }

        public string Create()
        {
            CallCount++;
            return _value;
        }
    }
}

