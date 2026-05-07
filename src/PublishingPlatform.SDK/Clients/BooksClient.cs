using System.Diagnostics;
using System.Net.Http.Json;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.Common.Pagination;
using PublishingPlatform.SDK.Clients.Books.Requests;
using PublishingPlatform.SDK.Clients.Books.Serialization;
using PublishingPlatform.SDK.Clients.Books.Validation;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Implements book lifecycle operations using the shared SDK transport.
/// </summary>
public sealed class BooksClient : IBooksClient
{
    private readonly ISharedHttpTransport _transport;
    private readonly IBookRequestValidator _validator;
    private readonly IBookQueryStringBuilder _queryStringBuilder;
    private readonly IBookResponseReader _responseReader;
    private readonly IBookRequestHeadersFactory _requestHeadersFactory;

    internal BooksClient(ISharedHttpTransport transport)
        : this(
            transport,
            new DefaultBookRequestValidator(),
            new DefaultBookQueryStringBuilder(),
            new DefaultBookResponseReader(),
            new DefaultBookRequestHeadersFactory())
    {
    }

    internal BooksClient(
        ISharedHttpTransport transport,
        IBookRequestValidator validator,
        IBookQueryStringBuilder queryStringBuilder,
        IBookResponseReader responseReader,
        IBookRequestHeadersFactory requestHeadersFactory)
    {
        _transport = transport;
        _validator = validator;
        _queryStringBuilder = queryStringBuilder;
        _responseReader = responseReader;
        _requestHeadersFactory = requestHeadersFactory;
    }

    /// <inheritdoc />
    public async Task<Book> CreateAsync(CreateBookRequest request, CancellationToken ct = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateCreate(request);

        using var activity = StartActivity("Books.Create");
        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(request.IdempotencyKey);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(HttpMethod.Post, "/books", content, headers, "Books.Create", ct).ConfigureAwait(false);

        return await _responseReader.ReadBookAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Book> GetByIdAsync(string bookId, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        using var activity = StartActivity("Books.GetById");
        using var response = await _transport.SendAsync(HttpMethod.Get, $"/books/{Uri.EscapeDataString(bookId)}", null, null, "Books.GetById", ct).ConfigureAwait(false);
        return await _responseReader.ReadBookAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedResult<Book>> ListAsync(ListBooksRequest request, CancellationToken ct = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateList(request);

        using var activity = StartActivity("Books.List");
        var relativePath = _queryStringBuilder.BuildListPath(request);
        using var response = await _transport.SendAsync(HttpMethod.Get, relativePath, null, null, "Books.List", ct).ConfigureAwait(false);
        return await _responseReader.ReadPagedResultAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<Book> ListAllAsync(ListBooksRequest request, CancellationToken ct = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateList(request);

        return PagedAsyncIterator.IterateAsync(
            request,
            CloneListRequest,
            static (iterationRequest, continuationToken) =>
            {
                iterationRequest.ContinuationToken = continuationToken;
                iterationRequest.Page++;
            },
            ListAsync,
            ct);
    }

    /// <inheritdoc />
    public async Task<Book> UpdateMetadataAsync(string bookId, UpdateBookMetadataRequest request, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        Guards.NotNull(request, nameof(request));
        _validator.ValidateUpdate(request);

        using var activity = StartActivity("Books.UpdateMetadata");
        var headers = _requestHeadersFactory.CreateConcurrencyHeaders(request.ConcurrencyToken);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(HttpMethod.Put, $"/books/{Uri.EscapeDataString(bookId)}", content, headers, "Books.UpdateMetadata", ct).ConfigureAwait(false);
        return await _responseReader.ReadBookAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Book> PatchMetadataAsync(string bookId, UpdateBookPatchRequest request, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        Guards.NotNull(request, nameof(request));
        _validator.ValidatePatch(request);

        using var activity = StartActivity("Books.PatchMetadata");
        var headers = _requestHeadersFactory.CreateConcurrencyHeaders(request.ConcurrencyToken);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(HttpMethod.Patch, $"/books/{Uri.EscapeDataString(bookId)}", content, headers, "Books.PatchMetadata", ct).ConfigureAwait(false);
        return await _responseReader.ReadBookAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string bookId, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        using var activity = StartActivity("Books.Delete");
        using var _ = await _transport.SendAsync(HttpMethod.Delete, $"/books/{Uri.EscapeDataString(bookId)}", null, null, "Books.Delete", ct).ConfigureAwait(false);
    }

    private static Activity? StartActivity(string operationName)
    {
        var activity = ActivitySourceProvider.ActivitySource.StartActivity(operationName, ActivityKind.Client);
        activity?.SetTag("sdk.operation", operationName);
        return activity;
    }

    private static ListBooksRequest CloneListRequest(ListBooksRequest request)
    {
        return new ListBooksRequest
        {
            Title = request.Title,
            Author = request.Author,
            Tags = request.Tags.ToArray(),
            SortBy = request.SortBy,
            Descending = request.Descending,
            Page = request.Page,
            PageSize = request.PageSize,
            ContinuationToken = request.ContinuationToken,
        };
    }
}
