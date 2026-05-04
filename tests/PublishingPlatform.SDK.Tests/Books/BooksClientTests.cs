using Bogus;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace PublishingPlatform.SDK.Tests.Books;

public sealed class BooksClientTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void BuildClient_ReturnsClientWithAllBookCentricModules()
    {
        var options = new PublishingPlatform.SDK.Options.PublishingPlatformClientOptions
        {
            BaseUrl = "https://example.test",
            ApiKey = "test-key",
        };

        var client = PublishingPlatformClientBuilder.Create(options).Build();

        client.Books.Should().NotBeNull();
        client.BookContent.Should().NotBeNull();
        client.BookPublishing.Should().NotBeNull();
        client.BookDistribution.Should().NotBeNull();
        client.BookAccess.Should().NotBeNull();
        client.BookAnalytics.Should().NotBeNull();
        client.BookAuditLogs.Should().NotBeNull();
        client.BookAssets.Should().NotBeNull();
        client.Webhooks.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_SendsPost_WithIdempotencyHeaderAndCancellation()
    {
        var request = new CreateBookRequest
        {
            Title = "T",
            Author = "A",
            IdempotencyKey = "idem-1",
        };
        var responseBook = new Book { Id = "b1", Title = "T", Author = "A" };
        var token = new CancellationTokenSource().Token;
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Post,
            "/books",
            Arg.Any<HttpContent>(),
            Arg.Any<IReadOnlyDictionary<string, string>>(),
            "Books.Create",
            token).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(responseBook),
            }));

        var sut = new BooksClient(transport);
        var result = await sut.CreateAsync(request, token);

        result.Id.Should().Be("b1");
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(h => h["Idempotency-Key"] == "idem-1"),
            "Books.Create",
            token);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetByIdAsync_ThrowsBookValidationException_ForInvalidId(string id)
    {
        var sut = new BooksClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.GetByIdAsync(id);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book id is required");
    }

    [Fact]
    public async Task ListAsync_SerializesQueryDeterministically()
    {
        var request = new ListBooksRequest
        {
            Title = "Title",
            Author = "Author",
            SortBy = "title",
            Descending = true,
            Page = 1,
            PageSize = 20,
            ContinuationToken = "c-1",
            Tags = new[] { "zeta", "alpha" },
        };
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            Arg.Any<HttpMethod>(),
            Arg.Any<string>(),
            null,
            null,
            "Books.List",
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new PagedResult<Book> { Items = Array.Empty<Book>() }),
            }));
        var sut = new BooksClient(transport);

        _ = await sut.ListAsync(request);

        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/books?page=1&pageSize=20&sortBy=title&descending=true&title=Title&author=Author&continuationToken=c-1&tag=alpha&tag=zeta",
            null,
            null,
            "Books.List",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PatchMetadataAsync_SendsPatchWithIfMatchHeader()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Patch,
            "/books/book-1",
            Arg.Any<HttpContent>(),
            Arg.Any<IReadOnlyDictionary<string, string>>(),
            "Books.PatchMetadata",
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new Book { Id = "book-1", Title = "T", Author = "A" }),
            }));
        var sut = new BooksClient(transport);

        _ = await sut.PatchMetadataAsync("book-1", new UpdateBookPatchRequest { Title = "X", ConcurrencyToken = "\"etag\"" });

        await transport.Received(1).SendAsync(
            HttpMethod.Patch,
            "/books/book-1",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(h => h["If-Match"] == "\"etag\""),
            "Books.PatchMetadata",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAllAsync_YieldsItemsAcrossPages()
    {
        using var handler = new SequenceHandler(new[]
        {
            JsonContent.Create(new PagedResult<Book>
            {
                Items = new[] { new Book { Id = "1", Title = "A", Author = "A" } },
                ContinuationToken = "next",
            }),
            JsonContent.Create(new PagedResult<Book>
            {
                Items = new[] { new Book { Id = "2", Title = "B", Author = "B" } },
            }),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BooksClient(transport);

        var items = new List<Book>();
        await foreach (var item in sut.ListAllAsync(new ListBooksRequest { SortBy = "title", PageSize = 1 }))
        {
            items.Add(item);
        }

        items.Select(x => x.Id).Should().Equal("1", "2");
    }

    [Fact]
    public async Task DefaultErrorMapper_Maps404ToBookNotFound()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = JsonContent.Create(new { Message = "missing" }),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BooksClient(transport);

        var act = async () => await sut.GetByIdAsync("book-404");

        _ = await act.Should().ThrowAsync<BookNotFoundException>();
    }

    private sealed class SequenceHandler : HttpMessageHandler
    {
        private readonly Queue<HttpContent> _responses;

        public SequenceHandler(IEnumerable<HttpContent> responses)
        {
            _responses = new Queue<HttpContent>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = _responses.Dequeue();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
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
            return Faker.Random.Guid().ToString("N");
        }
    }
}
