using System.Net;
using System.Net.Http.Json;
using System.Text;
using Bogus;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.BookDistribution;

public sealed class BookDistributionClientTests
{
    [Fact]
    public async Task StartAsync_SendsPostToExpectedPath_WithIdempotencyHeader()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Post,
                "/books/book-1/distribution/start",
                Arg.Any<HttpContent>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                "BookDistribution.Start",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookDistributionOperation { BookId = "book-1", OperationId = "op-1", Status = "pending" }),
            }));

        var sut = new BookDistributionClient(transport);

        var result = await sut.StartAsync("book-1", CreateStartRequest(), "idem-start");

        result.OperationId.Should().Be("op-1");
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books/book-1/distribution/start",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(h => h["Idempotency-Key"] == "idem-start"),
            "BookDistribution.Start",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStatusAsync_SendsGetToExpectedPath()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Get,
                "/books/book-2/distribution/op-9/status",
                null,
                null,
                "BookDistribution.GetStatus",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookDistributionOperation { BookId = "book-2", OperationId = "op-9", Status = "completed" }),
            }));

        var sut = new BookDistributionClient(transport);

        var result = await sut.GetStatusAsync("book-2", "op-9");

        result.Status.Should().Be("completed");
        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/books/book-2/distribution/op-9/status",
            null,
            null,
            "BookDistribution.GetStatus",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryAsync_SendsPostToExpectedPath_WithIdempotencyHeader()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Post,
                "/books/book-3/distribution/op-7/retry",
                null,
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                "BookDistribution.Retry",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookDistributionOperation { BookId = "book-3", OperationId = "op-7", Status = "in_progress" }),
            }));

        var sut = new BookDistributionClient(transport);

        var result = await sut.RetryAsync("book-3", "op-7", "idem-retry");

        result.Status.Should().Be("in_progress");
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books/book-3/distribution/op-7/retry",
            null,
            Arg.Is<IReadOnlyDictionary<string, string>>(h => h["Idempotency-Key"] == "idem-retry"),
            "BookDistribution.Retry",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAsync_SendsGetToExpectedPath()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Get,
                "/books/book-4/distribution",
                null,
                null,
                "BookDistribution.List",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookDistributionListResult
                {
                    Operations =
                    [
                        new BookDistributionOperation { BookId = "book-4", OperationId = "op-1", Status = "pending" },
                    ],
                }),
            }));

        var sut = new BookDistributionClient(transport);

        var result = await sut.ListAsync("book-4");

        result.Operations.Should().HaveCount(1);
        result.Operations[0].Status.Should().Be("pending");
        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/books/book-4/distribution",
            null,
            null,
            "BookDistribution.List",
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task StartAsync_ThrowsBookValidationException_ForInvalidBookId(string bookId)
    {
        var sut = new BookDistributionClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.StartAsync(bookId, CreateStartRequest());

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book id is required");
    }

    [Fact]
    public async Task StartAsync_ThrowsArgumentNullException_ForNullRequest()
    {
        var sut = new BookDistributionClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.StartAsync("book-1", null!);

        _ = await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StartAsync_ThrowsBookValidationException_ForMissingChannels()
    {
        var sut = new BookDistributionClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.StartAsync("book-1", new StartBookDistributionRequest());

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("At least one distribution channel is required");
    }

    [Fact]
    public async Task StartAsync_ThrowsBookValidationException_ForWhitespaceChannel()
    {
        var sut = new BookDistributionClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.StartAsync("book-1", new StartBookDistributionRequest { Channels = [CreateStartRequest().Channels[0], " "] });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Distribution channels cannot contain empty values");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetStatusAsync_ThrowsBookValidationException_ForInvalidOperationId(string operationId)
    {
        var sut = new BookDistributionClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.GetStatusAsync("book-1", operationId);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Distribution operation id is required");
    }

    [Fact]
    public async Task GetStatusAsync_PreservesStringStatusFromJsonResponse()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"operationId\":\"op-string\",\"bookId\":\"book-string\",\"status\":\"queued_for_partner_review\"}",
                Encoding.UTF8,
                "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookDistributionClient(transport);

        var result = await sut.GetStatusAsync("book-string", "op-string");

        result.Status.Should().Be("queued_for_partner_review");
    }

    [Fact]
    public async Task GetStatusAsync_ThrowsBookValidationException_WhenResponsePayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookDistributionClient(transport);

        var act = async () => await sut.GetStatusAsync("book-null", "op-null");

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book distribution operation payload was empty");
    }

    [Fact]
    public async Task ListAsync_ThrowsBookValidationException_WhenResponsePayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookDistributionClient(transport);

        var act = async () => await sut.ListAsync("book-null");

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book distribution list payload was empty");
    }

    [Fact]
    public async Task DefaultErrorMapper_Maps404ToBookNotFound_ForStart()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = JsonContent.Create(new { Message = "missing" }),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookDistributionClient(transport);

        var act = async () => await sut.StartAsync("book-404", CreateStartRequest());

        _ = await act.Should().ThrowAsync<BookNotFoundException>();
    }

    [Fact]
    public async Task DefaultErrorMapper_Maps409ToBookConflict_ForGetStatus()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = JsonContent.Create(new { Message = "conflict" }),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookDistributionClient(transport);

        var act = async () => await sut.GetStatusAsync("book-409", "op-1");

        _ = await act.Should().ThrowAsync<BookConflictException>();
    }

    [Fact]
    public async Task DefaultErrorMapper_Maps429ToBookRateLimited_ForRetry()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage((HttpStatusCode)429)
        {
            Content = JsonContent.Create(new { Message = "too many requests" }),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookDistributionClient(transport);

        var act = async () => await sut.RetryAsync("book-429", "op-1");

        _ = await act.Should().ThrowAsync<BookRateLimitedException>();
    }

    [Fact]
    public async Task ValidationFailure_DoesNotCallTransport()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        var sut = new BookDistributionClient(transport);

        var act = async () => await sut.RetryAsync(" ", "op-1");

        _ = await act.Should().ThrowAsync<BookValidationException>();
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    private sealed class SingleResponseHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public SingleResponseHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }

    private sealed class FixedCorrelationIdProvider : ICorrelationIdProvider
    {
        public string Create()
        {
            return Guid.NewGuid().ToString("N");
        }
    }

    private static StartBookDistributionRequest CreateStartRequest()
    {
        Randomizer.Seed = new Random(51);
        var faker = new Faker();
        var channel = faker.PickRandom("mobile", "partner-store", "cdn");
        return new StartBookDistributionRequest
        {
            Channels = [channel],
        };
    }
}
