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

    [Fact]
    public async Task CreateAsync_ThrowsBookValidationException_WhenTitleMissing()
    {
        var sut = new BooksClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.CreateAsync(new CreateBookRequest
        {
            Title = " ",
            Author = "Author",
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Title is required");
    }

    [Fact]
    public async Task CreateAsync_ThrowsBookValidationException_WhenAuthorMissing()
    {
        var sut = new BooksClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.CreateAsync(new CreateBookRequest
        {
            Title = "Title",
            Author = "",
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Author is required");
    }

    [Theory]
    [InlineData(-1, 10, "Page must be greater than or equal to zero")]
    [InlineData(0, 0, "PageSize must be between 1 and 200")]
    [InlineData(0, 201, "PageSize must be between 1 and 200")]
    public async Task ListAsync_ThrowsBookValidationException_ForInvalidPaging(int page, int pageSize, string expectedMessage)
    {
        var sut = new BooksClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.ListAsync(new ListBooksRequest
        {
            SortBy = "title",
            Page = page,
            PageSize = pageSize,
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain(expectedMessage);
    }

    [Fact]
    public async Task ListAsync_ThrowsBookValidationException_ForUnsupportedSortField()
    {
        var sut = new BooksClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.ListAsync(new ListBooksRequest
        {
            SortBy = "rank",
            Page = 0,
            PageSize = 20,
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("SortBy must be one of");
    }

    [Fact]
    public async Task PatchMetadataAsync_ThrowsBookValidationException_WhenPatchIsEmpty()
    {
        var sut = new BooksClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.PatchMetadataAsync("book-1", new UpdateBookPatchRequest());

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("At least one patch field must be provided");
    }

    [Fact]
    public async Task UpdateMetadataAsync_ThrowsBookValidationException_WhenFullPayloadIsIncomplete()
    {
        var sut = new BooksClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.UpdateMetadataAsync("book-1", new UpdateBookMetadataRequest
        {
            Title = "Title",
            Author = "",
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Author is required for full metadata update");
    }

    [Fact]
    public async Task DeleteAsync_CallsTransportWithExpectedOperation()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Delete,
            "/books/book-2",
            null,
            null,
            "Books.Delete",
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent)));
        var sut = new BooksClient(transport);

        await sut.DeleteAsync("book-2");

        await transport.Received(1).SendAsync(
            HttpMethod.Delete,
            "/books/book-2",
            null,
            null,
            "Books.Delete",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DefaultErrorMapper_Maps409ToBookConflict()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = JsonContent.Create(new { Message = "conflict" }),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BooksClient(transport);

        var act = async () => await sut.UpdateMetadataAsync("book-409", new UpdateBookMetadataRequest
        {
            Title = "t",
            Author = "a",
        });

        _ = await act.Should().ThrowAsync<BookConflictException>();
    }

    [Fact]
    public async Task DefaultErrorMapper_Maps429ToBookRateLimited()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage((HttpStatusCode)429)
        {
            Content = JsonContent.Create(new { Message = "too many requests" }),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BooksClient(transport);

        var act = async () => await sut.GetByIdAsync("book-429");

        _ = await act.Should().ThrowAsync<BookRateLimitedException>();
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsBookValidationException_WhenResponsePayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BooksClient(transport);

        var act = async () => await sut.GetByIdAsync("book-null");

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book payload was empty");
    }

    [Fact]
    public async Task ListAsync_ThrowsBookValidationException_WhenPagedPayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BooksClient(transport);

        var act = async () => await sut.ListAsync(new ListBooksRequest
        {
            SortBy = "title",
            Page = 0,
            PageSize = 10,
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Paged book payload was empty");
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
