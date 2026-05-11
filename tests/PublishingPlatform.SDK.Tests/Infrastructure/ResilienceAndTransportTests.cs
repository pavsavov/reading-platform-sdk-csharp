using Bogus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Extensions;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Infrastructure.Transport.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport.Requests;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
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

        await transport.SendAsync(HttpMethod.Get, "/books", null, null, null);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Headers.Accept.ToString().Should().Contain("application/json");
        handler.LastRequest.Headers.TryGetValues(SharedHttpTransport.CorrelationHeaderName, out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be(correlationId);
        handler.LastRequest.RequestUri!.ToString().Should().Be("https://api.example.test/books");
        correlationProvider.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task SendAsync_PreservesProvidedCorrelationHeader_AndDoesNotGenerateNewId()
    {
        const string expectedCorrelationId = "caller-correlation";
        var correlationProvider = new FixedCorrelationIdProvider("generated-correlation");
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            correlationProvider,
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions(),
            });
        var headers = new Dictionary<string, string>
        {
            [SharedHttpTransport.CorrelationHeaderName] = expectedCorrelationId,
        };

        await transport.SendAsync(HttpMethod.Get, "/books", null, headers, "Books.GetById", CancellationToken.None, "Books");

        handler.LastRequest!.Headers.TryGetValues(SharedHttpTransport.CorrelationHeaderName, out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be(expectedCorrelationId);
        correlationProvider.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task SendAsync_UsesRequestScopedCorrelationOverride_WhenHeaderMissing()
    {
        var correlationProvider = new FixedCorrelationIdProvider("generated-correlation");
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            correlationProvider,
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions(),
            });

        using (RequestScopedCorrelationContext.Push("request-scope-correlation"))
        {
            await transport.SendAsync(HttpMethod.Get, "/books", null, null, "Books.GetById", CancellationToken.None, "Books");
        }

        handler.LastRequest!.Headers.TryGetValues(SharedHttpTransport.CorrelationHeaderName, out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be("request-scope-correlation");
        correlationProvider.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task SendAsync_PrefersExplicitHeader_OverRequestScopedCorrelationOverride()
    {
        var correlationProvider = new FixedCorrelationIdProvider("generated-correlation");
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            correlationProvider,
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions(),
            });
        var headers = new Dictionary<string, string>
        {
            [SharedHttpTransport.CorrelationHeaderName] = "caller-correlation",
        };

        using (RequestScopedCorrelationContext.Push("request-scope-correlation"))
        {
            await transport.SendAsync(HttpMethod.Get, "/books", null, headers, "Books.GetById", CancellationToken.None, "Books");
        }

        handler.LastRequest!.Headers.TryGetValues(SharedHttpTransport.CorrelationHeaderName, out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be("caller-correlation");
        correlationProvider.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task SendAsync_UsesCustomCorrelationHeaderName()
    {
        const string headerName = "X-Trace-Id";
        const string correlationId = "trace-123";
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            new FixedCorrelationIdProvider(correlationId),
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions
                {
                    CorrelationHeaderName = headerName,
                },
            });

        await transport.SendAsync(HttpMethod.Get, "/books", null, null, "Books.GetById", CancellationToken.None, "Books");

        handler.LastRequest!.Headers.TryGetValues(headerName, out var values).Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be(correlationId);
        handler.LastRequest.Headers.Contains(SharedHttpTransport.CorrelationHeaderName).Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_EmitsActivityWithCorrelation_WhenTracingEnabled()
    {
        const string correlationId = "corr-trace";
        var startedActivities = new ConcurrentQueue<Activity>();
        using var listener = CreatePublishingSdkActivityListener(startedActivities);
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            new FixedCorrelationIdProvider(correlationId),
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions
                {
                    EnableTracing = true,
                },
            });

        await transport.SendAsync(HttpMethod.Get, "/books/42?include=secret", null, null, "Books.GetById", CancellationToken.None, "Books");

        var activity = startedActivities.Should().ContainSingle().Subject;
        activity.OperationName.Should().Be("Books.GetById");
        activity.Tags.Should().Contain(tag => tag.Key == "sdk.module" && tag.Value == "Books");
        activity.Tags.Should().Contain(tag => tag.Key == "sdk.operation" && tag.Value == "Books.GetById");
        activity.Tags.Should().Contain(tag => tag.Key == "url.path" && tag.Value == "/books/42");
        activity.Tags.Should().Contain(tag => tag.Key == "correlation.id" && tag.Value == correlationId);
    }

    [Fact]
    public async Task SendAsync_DoesNotEmitActivity_WhenTracingDisabled()
    {
        var startedActivities = new ConcurrentQueue<Activity>();
        using var listener = CreatePublishingSdkActivityListener(startedActivities);
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            new FixedCorrelationIdProvider("corr"),
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions
                {
                    EnableTracing = false,
                },
            });

        await transport.SendAsync(HttpMethod.Get, "/books", null, null, "Books.List", CancellationToken.None, "Books");

        startedActivities.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAsync_EmitsStructuredLogsWithoutSensitiveValues_WhenLoggingEnabled()
    {
        var logger = new RecordingLogger<SharedHttpTransport>();
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            new FixedCorrelationIdProvider("safe-correlation"),
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions
                {
                    EnableLogging = true,
                    LogRequestBody = false,
                    LogResponseBody = false,
                },
            },
            logger);
        using var content = JsonContent.Create(new { ApiKey = "secret-api-key", Token = "secret-token" });
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer secret-token",
            [TransportHeaderNames.ApiKey] = "secret-api-key-header",
            ["Idempotency-Key"] = "secret-idempotency",
        };

        await transport.SendAsync(HttpMethod.Post, "/books", content, headers, "Books.Create", CancellationToken.None, "Books");

        logger.Entries.Should().HaveCount(2);
        var joinedLogs = string.Join(Environment.NewLine, logger.Entries.Select(entry => entry.Message));
        joinedLogs.Should().Contain("Books");
        joinedLogs.Should().Contain("Books.Create");
        joinedLogs.Should().Contain("safe-correlation");
        joinedLogs.Should().NotContain("secret-api-key");
        joinedLogs.Should().NotContain("secret-token");
        joinedLogs.Should().NotContain("secret-api-key-header");
        joinedLogs.Should().NotContain("secret-idempotency");
        joinedLogs.Should().NotContain("Authorization");
        joinedLogs.Should().NotContain(TransportHeaderNames.ApiKey);
        joinedLogs.Should().NotContain("Idempotency-Key");
    }

    [Fact]
    public async Task SendAsync_DoesNotEmitLogs_WhenLoggingDisabled()
    {
        var logger = new RecordingLogger<SharedHttpTransport>();
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            new FixedCorrelationIdProvider("corr"),
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions
                {
                    EnableLogging = false,
                },
            },
            logger);

        await transport.SendAsync(HttpMethod.Get, "/books", null, null, "Books.List", CancellationToken.None, "Books");

        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAsync_RecordsMetrics_WhenMetricsEnabled()
    {
        var measurements = new ConcurrentBag<string>();
        using var listener = CreatePublishingSdkMeterListener(measurements);
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            new FixedCorrelationIdProvider("corr"),
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions
                {
                    EnableMetrics = true,
                },
            });

        await transport.SendAsync(HttpMethod.Get, "/books", null, null, "Books.List", CancellationToken.None, "Books");

        measurements.Should().Contain("publishing_platform.sdk.requests");
        measurements.Should().Contain("publishing_platform.sdk.request.duration");
    }

    [Fact]
    public async Task SendAsync_DoesNotRecordMetrics_WhenMetricsDisabled()
    {
        var measurements = new ConcurrentBag<string>();
        using var listener = CreatePublishingSdkMeterListener(measurements);
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            new FixedCorrelationIdProvider("corr"),
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions
                {
                    EnableMetrics = false,
                },
            });

        await transport.SendAsync(HttpMethod.Get, "/books", null, null, "Books.List", CancellationToken.None, "Books");

        measurements.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAsync_UsesModuleDiagnosticsOverride()
    {
        var logger = new RecordingLogger<SharedHttpTransport>();
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = CreateDiagnosticsTransport(
            httpClient,
            new FixedCorrelationIdProvider("corr"),
            new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions
                {
                    EnableLogging = true,
                },
                BookAnalyticsDiagnostics = new ModuleDiagnosticsOptions
                {
                    EnableLogging = false,
                },
            },
            logger);

        await transport.SendAsync(HttpMethod.Get, "/analytics", null, null, "BookAnalytics.GetSummary", CancellationToken.None, "BookAnalytics");

        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAsync_LogsAndMapsFailureDiagnostics()
    {
        var logger = new RecordingLogger<SharedHttpTransport>();
        var mapper = Substitute.For<IPublishingPlatformErrorMapper>();
        mapper.Map(Arg.Any<PublishingPlatformErrorContext>())
            .Returns(new ApiException(503, "mapped failure"));
        using var handler = new RecordingHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = JsonContent.Create(new { Message = "service unavailable" }),
            }));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = new SharedHttpTransport(
            httpClient,
            BuildPassThroughPipeline(),
            new FixedCorrelationIdProvider("corr-failure"),
            mapper,
            new DefaultTransportRequestFactory(),
            new DefaultTransportResponseErrorReader(),
            new DefaultTransportErrorContextFactory(),
            new DefaultDiagnosticsOptionsResolver(new PublishingPlatformClientOptions
            {
                Diagnostics = new DiagnosticsOptions
                {
                    EnableLogging = true,
                },
            }),
            logger);

        Func<Task> act = async () => await transport.SendAsync(HttpMethod.Get, "/books/42", null, null, "Books.GetById", CancellationToken.None, "Books");

        await act.Should().ThrowAsync<ApiException>();
        logger.Entries.Should().Contain(entry =>
            entry.Message.Contains("503", StringComparison.Ordinal)
            && entry.Message.Contains("Books.GetById", StringComparison.Ordinal)
            && entry.Message.Contains("corr-failure", StringComparison.Ordinal));
        mapper.Received(1).Map(Arg.Is<PublishingPlatformErrorContext>(context =>
            context.StatusCode == 503
            && context.OperationName == "Books.GetById"
            && context.CorrelationId == "corr-failure"));
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

        Func<Task> act = async () => await transport.SendAsync(HttpMethod.Post, "/books", JsonContent.Create(new { Title = "A" }), null, null);

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

        Func<Task> act = async () => await transport.SendAsync(HttpMethod.Get, "/books", null, null, null);

        var exception = await act.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(500);
        exception.Which.Message.Should().Be("HTTP 500 returned by Publishing Platform API.");
    }

    [Fact]
    public async Task SendAsync_ForwardsCancellationToken_ToResiliencePipeline()
    {
        var tokenSource = new CancellationTokenSource();
        var capturedToken = CancellationToken.None;
        PublishingPlatformResilienceContext? capturedContext = null;

        var pipeline = Substitute.For<IPublishingPlatformResiliencePipeline>();
        pipeline.ExecuteAsync(Arg.Any<PublishingPlatformResilienceContext>(), Arg.Any<Func<CancellationToken, Task<HttpResponseMessage>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedContext = callInfo.Arg<PublishingPlatformResilienceContext>();
                capturedToken = callInfo.Arg<CancellationToken>();
                var operation = callInfo.Arg<Func<CancellationToken, Task<HttpResponseMessage>>>();
                return operation(capturedToken);
            });

        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = new SharedHttpTransport(httpClient, pipeline, new FixedCorrelationIdProvider(Faker.Random.Guid().ToString("N")), new DefaultPublishingPlatformErrorMapper());

        await transport.SendAsync(HttpMethod.Get, "/books", null, null, null, tokenSource.Token);

        capturedToken.Should().Be(tokenSource.Token);
        capturedContext.Should().NotBeNull();
        capturedContext!.Method.Should().Be(HttpMethod.Get);
        capturedContext.HasIdempotencyKey.Should().BeFalse();
        capturedContext.CanReplayContent.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_SetsRetrySafetyContext_ForIdempotencyHeaderAndNonReplayableContent()
    {
        PublishingPlatformResilienceContext? capturedContext = null;
        var pipeline = Substitute.For<IPublishingPlatformResiliencePipeline>();
        pipeline.ExecuteAsync(Arg.Any<PublishingPlatformResilienceContext>(), Arg.Any<Func<CancellationToken, Task<HttpResponseMessage>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedContext = callInfo.Arg<PublishingPlatformResilienceContext>();
                var operation = callInfo.Arg<Func<CancellationToken, Task<HttpResponseMessage>>>();
                return operation(callInfo.Arg<CancellationToken>());
            });

        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = new SharedHttpTransport(httpClient, pipeline, new FixedCorrelationIdProvider("corr"), new DefaultPublishingPlatformErrorMapper());
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [TransportHeaderNames.IdempotencyKey] = "idem-1",
        };
        await using var stream = new MemoryStream([1, 2, 3]);
        using var content = new StreamContent(stream);

        await transport.SendAsync(HttpMethod.Post, "/books", content, headers, "Books.Create", CancellationToken.None, "Books");

        capturedContext.Should().NotBeNull();
        capturedContext!.Method.Should().Be(HttpMethod.Post);
        capturedContext.HasIdempotencyKey.Should().BeTrue();
        capturedContext.CanReplayContent.Should().BeFalse();
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

        Func<Task> act = async () => await transport.SendAsync(HttpMethod.Get, "/books/42", null, null, null);

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
    public void BuildBootstrapServiceProvider_ThrowsForInsecureHttpBaseUrl()
    {
        var options = new PublishingPlatformClientOptions
        {
            BaseUrl = "http://api.example.test",
        };

        Action act = () => SdkHttpClientResolver.BuildBootstrapServiceProvider(options);

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*BaseUrl must use HTTPS.*");
    }

    [Fact]
    public void BuildBootstrapServiceProvider_ConfiguresBaseAddressAndTimeout()
    {
        var expectedTimeout = TimeSpan.FromSeconds(19);
        var options = new PublishingPlatformClientOptions
        {
            BaseUrl = "https://api.example.test",
            ApiKey = "key",
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
    public void AddPublishingPlatformClient_ThrowsForInvalidOptions_WhenUsingOptionsPattern()
    {
        var services = new ServiceCollection();
        services.Configure<PublishingPlatformClientOptions>(options =>
        {
            options.BaseUrl = "invalid-url";
        });
        services.AddPublishingPlatformClient();

        using var provider = services.BuildServiceProvider(validateScopes: true);

        Action act = () => _ = provider.GetRequiredService<IOptions<PublishingPlatformClientOptions>>().Value;

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*BaseUrl must be a valid absolute URL.*");
    }

    [Fact]
    public void AddPublishingPlatformClient_ThrowsForMissingApiKey_WhenUsingOptionsPattern()
    {
        var services = new ServiceCollection();
        services.Configure<PublishingPlatformClientOptions>(options =>
        {
            options.BaseUrl = "https://api.example.test";
            options.ApiKey = " ";
        });
        services.AddPublishingPlatformClient();

        using var provider = services.BuildServiceProvider(validateScopes: true);

        Action act = () => _ = provider.GetRequiredService<IOptions<PublishingPlatformClientOptions>>().Value;

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*ApiKey must not be empty.*");
    }

    [Fact]
    public void PublishingPlatformClientBuilder_ThrowsForMissingApiKey()
    {
        var options = new PublishingPlatformClientOptions
        {
            BaseUrl = "https://api.example.test",
            ApiKey = string.Empty,
        };

        Action act = () => PublishingPlatformClientBuilder.Create(options).Build();

        act.Should().Throw<PublishingPlatformConfigurationException>()
            .WithMessage("*ApiKey must not be empty.*");
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

    [Fact]
    public async Task CreatePipeline_RetriesTransientStatusCodes_ForIdempotentMethod()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var pipeline = ResiliencePipelineFactory.Create(options);
        var attempts = 0;

        var response = await pipeline.ExecuteAsync(
            new PublishingPlatformResilienceContext { Method = HttpMethod.Get },
            _ =>
            {
                attempts++;
                return Task.FromResult(new HttpResponseMessage(
                    attempts < 3 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK));
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task CreatePipeline_DoesNotRetryTransientStatusCodes_ForPostByDefault()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var pipeline = ResiliencePipelineFactory.Create(options);
        var attempts = 0;

        var response = await pipeline.ExecuteAsync(
            new PublishingPlatformResilienceContext { Method = HttpMethod.Post },
            _ =>
            {
                attempts++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreatePipeline_RetriesTransientStatusCodes_ForPostWhenEnabled()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                RetryNonIdempotentMethods = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var pipeline = ResiliencePipelineFactory.Create(options);
        var attempts = 0;

        var response = await pipeline.ExecuteAsync(
            new PublishingPlatformResilienceContext
            {
                Method = HttpMethod.Post,
                HasIdempotencyKey = true,
                CanReplayContent = true,
            },
            _ =>
            {
                attempts++;
                return Task.FromResult(new HttpResponseMessage(
                    attempts < 3 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK));
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task CreatePipeline_DoesNotRetryNonTransientStatusCodes_ForIdempotentMethod()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var pipeline = ResiliencePipelineFactory.Create(options);
        var attempts = 0;

        var response = await pipeline.ExecuteAsync(
            new PublishingPlatformResilienceContext { Method = HttpMethod.Get },
            _ =>
            {
                attempts++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreatePipeline_DoesNotRetryBadGateway_ForPostWhenEnabled()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                RetryNonIdempotentMethods = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var pipeline = ResiliencePipelineFactory.Create(options);
        var attempts = 0;

        var response = await pipeline.ExecuteAsync(
            new PublishingPlatformResilienceContext
            {
                Method = HttpMethod.Post,
                HasIdempotencyKey = true,
                CanReplayContent = true,
            },
            _ =>
            {
                attempts++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway));
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreatePipeline_DoesNotRetryTransientStatusCodes_ForPostWithoutIdempotencyKey()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                RetryNonIdempotentMethods = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var pipeline = ResiliencePipelineFactory.Create(options);
        var attempts = 0;

        var response = await pipeline.ExecuteAsync(
            new PublishingPlatformResilienceContext
            {
                Method = HttpMethod.Post,
                HasIdempotencyKey = false,
                CanReplayContent = true,
            },
            _ =>
            {
                attempts++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreatePipeline_DoesNotRetryTransientStatusCodes_ForPostWithNonReplayableContent()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                RetryNonIdempotentMethods = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var pipeline = ResiliencePipelineFactory.Create(options);
        var attempts = 0;

        var response = await pipeline.ExecuteAsync(
            new PublishingPlatformResilienceContext
            {
                Method = HttpMethod.Post,
                HasIdempotencyKey = true,
                CanReplayContent = false,
            },
            _ =>
            {
                attempts++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreatePipeline_AcceptsRetryConfiguration_WithJitterDisabled()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                UseJitter = false,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var pipeline = ResiliencePipelineFactory.Create(options);
        var attempts = 0;

        var response = await pipeline.ExecuteAsync(
            new PublishingPlatformResilienceContext { Method = HttpMethod.Get },
            _ =>
            {
                attempts++;
                return Task.FromResult(new HttpResponseMessage(
                    attempts < 3 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK));
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task SendAsync_RetriesBeforeErrorMapping_WhenPipelineRetries()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var attempts = 0;
        using var handler = new RecordingHandler((_, _) =>
        {
            attempts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = JsonContent.Create(new { Message = "busy" }),
            });
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var mapper = Substitute.For<IPublishingPlatformErrorMapper>();
        mapper.Map(Arg.Any<PublishingPlatformErrorContext>()).Returns(new InvalidOperationException("mapped"));
        var transport = new SharedHttpTransport(
            httpClient,
            ResiliencePipelineFactory.Create(options),
            new FixedCorrelationIdProvider(Faker.Random.Guid().ToString("N")),
            mapper);

        Func<Task> act = async () => await transport.SendAsync(HttpMethod.Get, "/books", null, null, null);

        _ = await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(3);
        mapper.Received(1).Map(Arg.Any<PublishingPlatformErrorContext>());
    }

    [Fact]
    public async Task SendAsync_UsesStableCorrelationIdAcrossRetries()
    {
        var options = new PublishingPlatformResilienceOptions
        {
            Enabled = true,
            Retry = new RetryResilienceOptions
            {
                Enabled = true,
                MaxRetryAttempts = 2,
                BaseDelay = TimeSpan.FromMilliseconds(1),
            },
        };

        var observedCorrelationIds = new List<string>();
        using var handler = new RecordingHandler((request, _) =>
        {
            request.Headers.TryGetValues(SharedHttpTransport.CorrelationHeaderName, out var values).Should().BeTrue();
            observedCorrelationIds.Add(values!.Single());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = JsonContent.Create(new { Message = "busy" }),
            });
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var transport = new SharedHttpTransport(
            httpClient,
            ResiliencePipelineFactory.Create(options),
            new FixedCorrelationIdProvider("fixed-correlation-id"),
            new DefaultPublishingPlatformErrorMapper());

        Func<Task> act = async () => await transport.SendAsync(HttpMethod.Get, "/books", null, null, null);

        _ = await act.Should().ThrowAsync<ApiException>();
        observedCorrelationIds.Should().HaveCount(3);
        observedCorrelationIds.Should().OnlyContain(id => id == "fixed-correlation-id");
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
        pipeline.ExecuteAsync(Arg.Any<PublishingPlatformResilienceContext>(), Arg.Any<Func<CancellationToken, Task<HttpResponseMessage>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var operation = callInfo.Arg<Func<CancellationToken, Task<HttpResponseMessage>>>();
                var token = callInfo.Arg<CancellationToken>();
                return operation(token);
            });

        return pipeline;
    }

    private static SharedHttpTransport CreateDiagnosticsTransport(
        HttpClient httpClient,
        ICorrelationIdProvider correlationIdProvider,
        PublishingPlatformClientOptions options,
        ILogger<SharedHttpTransport>? logger = null)
    {
        return new SharedHttpTransport(
            httpClient,
            BuildPassThroughPipeline(),
            correlationIdProvider,
            new DefaultPublishingPlatformErrorMapper(),
            new DefaultTransportRequestFactory(),
            new DefaultTransportResponseErrorReader(),
            new DefaultTransportErrorContextFactory(),
            new DefaultDiagnosticsOptionsResolver(options),
            logger ?? new RecordingLogger<SharedHttpTransport>());
    }

    private static ActivityListener CreatePublishingSdkActivityListener(ConcurrentQueue<Activity> startedActivities)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = activitySource => activitySource.Name == "PublishingPlatform.SDK",
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = startedActivities.Enqueue,
        };

        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private static MeterListener CreatePublishingSdkMeterListener(ConcurrentBag<string> measurements)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == "PublishingPlatform.SDK")
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            },
        };

        listener.SetMeasurementEventCallback<long>((instrument, _, _, _) => measurements.Add(instrument.Name));
        listener.SetMeasurementEventCallback<double>((instrument, _, _, _) => measurements.Add(instrument.Name));
        listener.Start();
        return listener;
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

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
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

