using System.Net;
using System.Net.Http.Json;
using System.Text;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.BookContent;

public sealed class BookContentClientTests
{
    [Fact]
    public async Task GetAsync_SendsGetToExpectedPath_WithCancellation()
    {
        var token = new CancellationTokenSource().Token;
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Get,
            "/books/book-1/content",
            null,
            null,
            "BookContent.Get",
            token).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new Models.BookContent { BookId = "book-1", Format = "pdf" }),
            }));

        var sut = new BookContentClient(transport);

        var result = await sut.GetAsync("book-1", token);

        result.BookId.Should().Be("book-1");
        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/books/book-1/content",
            null,
            null,
            "BookContent.Get",
            token);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetAsync_ThrowsBookValidationException_ForInvalidBookId(string bookId)
    {
        var sut = new BookContentClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.GetAsync(bookId);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book id is required");
    }

    [Fact]
    public async Task UploadOrReplaceAsync_SendsPutMultipart_WithIdempotencyHeader()
    {
        string? multipartPayload = null;
        string? multipartContentType = null;
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Put,
                "/books/book-2/content",
                Arg.Any<HttpContent>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                "BookContent.UploadOrReplace",
                Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                var requestContent = call.ArgAt<HttpContent>(2);
                multipartContentType = requestContent.Headers.ContentType?.MediaType;
                multipartPayload = await requestContent.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = JsonContent.Create(new Models.BookContent { BookId = "book-2", Format = "epub" }),
                };
            });

        var sut = new BookContentClient(transport);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("sample-content"));

        _ = await sut.UploadOrReplaceAsync("book-2", new UploadBookContentRequest
        {
            File = stream,
            FileName = "book.epub",
            ContentType = "application/epub+zip",
            Format = "epub",
            IdempotencyKey = "idem-2",
        });

        await transport.Received(1).SendAsync(
            HttpMethod.Put,
            "/books/book-2/content",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(h => h["Idempotency-Key"] == "idem-2"),
            "BookContent.UploadOrReplace",
            Arg.Any<CancellationToken>());

        multipartContentType.Should().Be("multipart/form-data");
        multipartPayload.Should().NotBeNull();
        multipartPayload!.Should().Contain("name=file");
        multipartPayload.Should().Contain("filename=book.epub");
        multipartPayload.Should().Contain("name=format");
        multipartPayload.Should().Contain("epub");
        multipartPayload.Should().Contain("name=idempotencyKey");
        multipartPayload.Should().Contain("idem-2");
    }

    [Theory]
    [InlineData("mobi")]
    [InlineData("txt")]
    public async Task UploadOrReplaceAsync_ThrowsBookValidationException_ForUnsupportedFormat(string format)
    {
        var sut = new BookContentClient(Substitute.For<ISharedHttpTransport>());
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("sample-content"));

        var act = async () => await sut.UploadOrReplaceAsync("book-2", new UploadBookContentRequest
        {
            File = stream,
            FileName = "book.bin",
            Format = format,
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Format must be one of");
    }

    [Fact]
    public async Task UploadOrReplaceAsync_ThrowsBookValidationException_ForEmptyStream()
    {
        var sut = new BookContentClient(Substitute.For<ISharedHttpTransport>());
        await using var stream = new MemoryStream(Array.Empty<byte>());

        var act = async () => await sut.UploadOrReplaceAsync("book-2", new UploadBookContentRequest
        {
            File = stream,
            FileName = "book.pdf",
            Format = "pdf",
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("must not be empty");
    }

    [Fact]
    public async Task GetAsync_ThrowsBookValidationException_WhenResponsePayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookContentClient(transport);

        var act = async () => await sut.GetAsync("book-null");

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book content payload was empty");
    }

    [Fact]
    public async Task StartResumableUploadAsync_SendsPostWithIdempotencyHeader()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Post,
                "/books/book-3/content/uploads",
                Arg.Any<HttpContent>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                "BookContent.StartResumableUpload",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new UploadSessionInfo
                {
                    UploadSessionId = "upl-1",
                    BookId = "book-3",
                    Status = "pending",
                    UploadedBytes = 0,
                    TotalBytes = 10,
                }),
            }));

        var sut = new BookContentClient(transport);
        var result = await sut.StartResumableUploadAsync("book-3", new StartResumableUploadRequest
        {
            FileName = "demo.epub",
            Format = "epub",
            TotalBytes = 10,
            IdempotencyKey = "start-1",
        });

        result.UploadSessionId.Should().Be("upl-1");
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books/book-3/content/uploads",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(headers => headers["Idempotency-Key"] == "start-1"),
            "BookContent.StartResumableUpload",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadChunkAsync_SendsPutWithContentRangeHeader()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Put,
                "/books/book-3/content/uploads/upl-1/chunks",
                Arg.Any<HttpContent>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                "BookContent.UploadChunk",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new UploadChunkResult
                {
                    UploadSessionId = "upl-1",
                    AcceptedRangeStart = 0,
                    AcceptedRangeEnd = 4,
                    UploadedBytes = 5,
                    IsComplete = false,
                }),
            }));

        var sut = new BookContentClient(transport);
        await using var chunk = new MemoryStream(Encoding.UTF8.GetBytes("chunk"));
        var result = await sut.UploadChunkAsync("book-3", "upl-1", new UploadChunkRequest
        {
            Chunk = chunk,
            ChunkStart = 0,
            ChunkEnd = 4,
            TotalBytes = 10,
            ChunkChecksum = "sha256:abc",
        });

        result.UploadedBytes.Should().Be(5);
        await transport.Received(1).SendAsync(
            HttpMethod.Put,
            "/books/book-3/content/uploads/upl-1/chunks",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(headers =>
                headers["Content-Range"] == "bytes 0-4/10"
                && headers["X-Chunk-Checksum"] == "sha256:abc"),
            "BookContent.UploadChunk",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUploadSessionAsync_SendsGetToExpectedPath()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Get,
                "/books/book-3/content/uploads/upl-1",
                null,
                null,
                "BookContent.GetUploadSession",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new UploadSessionInfo
                {
                    UploadSessionId = "upl-1",
                    BookId = "book-3",
                    Status = "in_progress",
                    UploadedBytes = 5,
                    TotalBytes = 10,
                }),
            }));

        var sut = new BookContentClient(transport);
        var result = await sut.GetUploadSessionAsync("book-3", "upl-1");
        result.Status.Should().Be("in_progress");
    }

    [Fact]
    public async Task CompleteResumableUploadAsync_SendsPostWithIdempotencyHeader()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
                HttpMethod.Post,
                "/books/book-3/content/uploads/upl-1/complete",
                Arg.Any<HttpContent>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                "BookContent.CompleteResumableUpload",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new Models.BookContent
                {
                    BookId = "book-3",
                    Format = "epub",
                }),
            }));

        var sut = new BookContentClient(transport);
        var result = await sut.CompleteResumableUploadAsync("book-3", "upl-1", new CompleteResumableUploadRequest
        {
            IdempotencyKey = "complete-1",
        });

        result.BookId.Should().Be("book-3");
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books/book-3/content/uploads/upl-1/complete",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(headers => headers["Idempotency-Key"] == "complete-1"),
            "BookContent.CompleteResumableUpload",
            Arg.Any<CancellationToken>());
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
