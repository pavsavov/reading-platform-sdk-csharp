using System.Net;
using System.Net.Http.Json;
using System.Text;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.BookPublishing;

public sealed class BookPublishingClientTests
{
    [Fact]
    public async Task PublishAsync_SendsPostToExpectedPath_WithIdempotencyHeader()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Post,
                "/books/book-1/publishing/publish",
                Arg.Any<HttpContent>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                "BookPublishing.Publish",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookPublishingStatus { BookId = "book-1", Status = "published" }),
            }));

        var sut = new BookPublishingClient(transport);

        var result = await sut.PublishAsync("book-1", new PublishBookRequest { Notes = "Ship it." }, "idem-publish");

        result.Status.Should().Be("published");
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books/book-1/publishing/publish",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(h => h["Idempotency-Key"] == "idem-publish"),
            "BookPublishing.Publish",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnpublishAsync_SendsPostToExpectedPath()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Post,
                "/books/book-2/publishing/unpublish",
                null,
                null,
                "BookPublishing.Unpublish",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookPublishingStatus { BookId = "book-2", Status = "unpublished" }),
            }));

        var sut = new BookPublishingClient(transport);

        var result = await sut.UnpublishAsync("book-2");

        result.Status.Should().Be("unpublished");
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books/book-2/publishing/unpublish",
            null,
            null,
            "BookPublishing.Unpublish",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScheduleAsync_SendsPostToExpectedPath_WithPayload()
    {
        var scheduledAt = DateTimeOffset.Parse("2026-06-01T10:00:00Z");
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Post,
                "/books/book-3/publishing/schedule",
                Arg.Any<HttpContent>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                "BookPublishing.Schedule",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookPublishingStatus { BookId = "book-3", Status = "scheduled", ScheduledAt = scheduledAt }),
            }));

        var sut = new BookPublishingClient(transport);

        var result = await sut.ScheduleAsync("book-3", new ScheduleBookPublishingRequest { ScheduledAt = scheduledAt }, "idem-schedule");

        result.Status.Should().Be("scheduled");
        result.ScheduledAt.Should().Be(scheduledAt);
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books/book-3/publishing/schedule",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(h => h["Idempotency-Key"] == "idem-schedule"),
            "BookPublishing.Schedule",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStatusAsync_SendsGetToExpectedPath()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Get,
                "/books/book-4/publishing/status",
                null,
                null,
                "BookPublishing.GetStatus",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookPublishingStatus { BookId = "book-4", Status = "published" }),
            }));

        var sut = new BookPublishingClient(transport);

        var result = await sut.GetStatusAsync("book-4");

        result.BookId.Should().Be("book-4");
        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/books/book-4/publishing/status",
            null,
            null,
            "BookPublishing.GetStatus",
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task PublishAsync_ThrowsBookValidationException_ForInvalidBookId(string bookId)
    {
        var sut = new BookPublishingClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.PublishAsync(bookId, new PublishBookRequest());

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book id is required");
    }

    [Fact]
    public async Task ScheduleAsync_ThrowsBookValidationException_ForMissingScheduledAt()
    {
        var sut = new BookPublishingClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.ScheduleAsync("book-5", new ScheduleBookPublishingRequest());

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("ScheduledAt is required");
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
        var sut = new BookPublishingClient(transport);

        var act = async () => await sut.GetStatusAsync("book-null");

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book publishing status payload was empty");
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
}
