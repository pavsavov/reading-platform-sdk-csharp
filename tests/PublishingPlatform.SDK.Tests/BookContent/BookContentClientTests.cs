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
