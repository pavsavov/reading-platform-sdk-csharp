using Bogus;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Extensions;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class PublishingPlatformRequestOptionsExtensionsTests
{
    [Fact]
    public async Task PublishAsync_ForwardsIdempotencyKey_FromRequestOptions()
    {
        Randomizer.Seed = new Random(111);
        var faker = new Faker();
        var bookId = $"book-{faker.Random.Int(10, 99)}";
        var idempotencyKey = faker.Random.Guid().ToString("N");

        var client = Substitute.For<IBookPublishingClient>();
        var expected = new BookPublishingStatus { BookId = bookId, Status = "published" };
        client.PublishAsync(
                bookId,
                Arg.Any<PublishBookRequest>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await client.PublishAsync(
            bookId,
            new PublishBookRequest { Notes = "ready" },
            new PublishingPlatformRequestOptions { IdempotencyKey = idempotencyKey });

        result.Should().BeSameAs(expected);
        await client.Received(1).PublishAsync(
            bookId,
            Arg.Is<PublishBookRequest>(request => request.Notes == "ready"),
            idempotencyKey,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_ForwardsIdempotencyKey_FromRequestOptions()
    {
        var idempotencyKey = new Faker().Random.Guid().ToString("N");
        var client = Substitute.For<IBookDistributionClient>();
        client.StartAsync(
                "book-1",
                Arg.Any<StartBookDistributionRequest>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BookDistributionOperation { BookId = "book-1", OperationId = "op-1", Status = "pending" }));

        await client.StartAsync(
            "book-1",
            new StartBookDistributionRequest { Channels = ["mobile"] },
            new PublishingPlatformRequestOptions { IdempotencyKey = idempotencyKey });

        await client.Received(1).StartAsync(
            "book-1",
            Arg.Is<StartBookDistributionRequest>(request =>
                request.Channels.Count == 1
                && request.Channels[0] == "mobile"),
            idempotencyKey,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_ForwardsIdempotencyKey_FromRequestOptions()
    {
        var idempotencyKey = new Faker().Random.Guid().ToString("N");
        var client = Substitute.For<IWebhooksClient>();
        client.RegisterAsync(
                Arg.Any<RegisterWebhookRequest>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new Webhook { Id = "whk-1", EndpointUrl = "https://hooks.example.com", Events = ["book.published"] }));

        await client.RegisterAsync(
            new RegisterWebhookRequest
            {
                EndpointUrl = "https://hooks.example.com",
                Events = ["book.published"],
                IsActive = true,
            },
            new PublishingPlatformRequestOptions { IdempotencyKey = idempotencyKey });

        await client.Received(1).RegisterAsync(
            Arg.Is<RegisterWebhookRequest>(request => request.EndpointUrl == "https://hooks.example.com"),
            idempotencyKey,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnpublishAsync_WithNullRequestOptions_ForwardsNullIdempotencyKey()
    {
        var client = Substitute.For<IBookPublishingClient>();
        client.UnpublishAsync(
                "book-1",
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BookPublishingStatus { BookId = "book-1", Status = "unpublished" }));

        await client.UnpublishAsync("book-1", requestOptions: null, ct: CancellationToken.None);

        await client.Received(1).UnpublishAsync(
            "book-1",
            Arg.Is<string?>(value => value == null),
            CancellationToken.None);
    }

    [Fact]
    public async Task RetryAsync_WithRequestOptionsWithoutIdempotencyKey_ForwardsNullIdempotencyKey()
    {
        var client = Substitute.For<IBookDistributionClient>();
        client.RetryAsync(
                "book-1",
                "op-1",
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BookDistributionOperation { BookId = "book-1", OperationId = "op-1", Status = "pending" }));

        await client.RetryAsync(
            "book-1",
            "op-1",
            new PublishingPlatformRequestOptions { CorrelationId = "corr-1" },
            CancellationToken.None);

        await client.Received(1).RetryAsync(
            "book-1",
            "op-1",
            Arg.Is<string?>(value => value == null),
            CancellationToken.None);
    }

    [Fact]
    public async Task PublishAsync_ForwardsNullIdempotency_WhenOnlyCorrelationIdProvided()
    {
        var client = Substitute.For<IBookPublishingClient>();
        client.PublishAsync(
                "book-1",
                Arg.Any<PublishBookRequest>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BookPublishingStatus { BookId = "book-1", Status = "published" }));

        await client.PublishAsync(
            "book-1",
            new PublishBookRequest { Notes = "ready" },
            new PublishingPlatformRequestOptions { CorrelationId = "corr-explicit" });

        await client.Received(1).PublishAsync(
            "book-1",
            Arg.Any<PublishBookRequest>(),
            Arg.Is<string?>(value => value == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_ForwardsNullIdempotency_WhenCorrelationIdIsWhitespace()
    {
        var client = Substitute.For<IBookPublishingClient>();
        client.PublishAsync(
                "book-1",
                Arg.Any<PublishBookRequest>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BookPublishingStatus { BookId = "book-1", Status = "published" }));

        await client.PublishAsync(
            "book-1",
            new PublishBookRequest { Notes = "ready" },
            new PublishingPlatformRequestOptions { CorrelationId = "  " });

        await client.Received(1).PublishAsync(
            "book-1",
            Arg.Any<PublishBookRequest>(),
            Arg.Is<string?>(value => value == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WithNullClient_ThrowsArgumentNullException()
    {
        IBookPublishingClient? client = null;

        var act = async () => await client!.PublishAsync(
            "book-1",
            new PublishBookRequest(),
            new PublishingPlatformRequestOptions { IdempotencyKey = "id-1" });

        var exception = await act.Should().ThrowAsync<ArgumentNullException>();
        exception.Which.ParamName.Should().Be("client");
    }
}
