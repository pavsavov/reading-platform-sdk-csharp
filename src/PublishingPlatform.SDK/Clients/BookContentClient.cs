using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.BookContent;
using PublishingPlatform.SDK.Clients.BookContent.Requests;
using PublishingPlatform.SDK.Clients.BookContent.Serialization;
using PublishingPlatform.SDK.Clients.BookContent.Validation;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;
using BookContentModel = PublishingPlatform.SDK.Models.BookContent;
using System.Net.Http.Json;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Implements book content operations using the shared SDK transport.
/// </summary>
public sealed class BookContentClient : IBookContentClient
{
    private const string GetOperationName = "BookContent.Get";
    private const string UploadOrReplaceOperationName = "BookContent.UploadOrReplace";
    private const string StartResumableUploadOperationName = "BookContent.StartResumableUpload";
    private const string UploadChunkOperationName = "BookContent.UploadChunk";
    private const string GetUploadSessionOperationName = "BookContent.GetUploadSession";
    private const string CompleteResumableUploadOperationName = "BookContent.CompleteResumableUpload";

    private readonly ISharedHttpTransport _transport;
    private readonly IBookContentRequestValidator _validator;
    private readonly IBookContentRequestHeadersFactory _requestHeadersFactory;
    private readonly IBookContentMultipartFormFactory _multipartFormFactory;
    private readonly IBookContentResponseReader _responseReader;

    internal BookContentClient(ISharedHttpTransport transport)
        : this(
            transport,
            new DefaultBookContentRequestValidator(),
            new DefaultBookContentRequestHeadersFactory(),
            new DefaultBookContentMultipartFormFactory(),
            new DefaultBookContentResponseReader())
    {
    }

    internal BookContentClient(
        ISharedHttpTransport transport,
        IBookContentRequestValidator validator,
        IBookContentRequestHeadersFactory requestHeadersFactory,
        IBookContentMultipartFormFactory multipartFormFactory,
        IBookContentResponseReader responseReader)
    {
        _transport = transport;
        _validator = validator;
        _requestHeadersFactory = requestHeadersFactory;
        _multipartFormFactory = multipartFormFactory;
        _responseReader = responseReader;
    }

    /// <inheritdoc />
    public async Task<BookContentModel> GetAsync(string bookId, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);

        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            BookContentEndpoints.ByBookId(bookId),
            null,
            null,
            GetOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadBookContentAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookContentModel> UploadOrReplaceAsync(string bookId, UploadBookContentRequest request, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        Guards.NotNull(request, nameof(request));
        _validator.ValidateUploadRequest(request);

        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(request.IdempotencyKey);
        using var content = _multipartFormFactory.Create(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Put,
            BookContentEndpoints.ByBookId(bookId),
            content,
            headers,
            UploadOrReplaceOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadBookContentAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<UploadSessionInfo> StartResumableUploadAsync(string bookId, StartResumableUploadRequest request, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        Guards.NotNull(request, nameof(request));
        _validator.ValidateStartResumableUploadRequest(request);

        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(request.IdempotencyKey);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            $"/books/{Uri.EscapeDataString(bookId)}/content/uploads",
            content,
            headers,
            StartResumableUploadOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadUploadSessionAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<UploadChunkResult> UploadChunkAsync(string bookId, string uploadSessionId, UploadChunkRequest request, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        _validator.ValidateUploadSessionId(uploadSessionId);
        Guards.NotNull(request, nameof(request));
        _validator.ValidateUploadChunkRequest(request);

        using var chunkContent = new StreamContent(request.Chunk);
        var headers = _requestHeadersFactory.CreateChunkHeaders(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Put,
            $"/books/{Uri.EscapeDataString(bookId)}/content/uploads/{Uri.EscapeDataString(uploadSessionId)}/chunks",
            chunkContent,
            headers,
            UploadChunkOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadUploadChunkResultAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<UploadSessionInfo> GetUploadSessionAsync(string bookId, string uploadSessionId, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        _validator.ValidateUploadSessionId(uploadSessionId);

        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            $"/books/{Uri.EscapeDataString(bookId)}/content/uploads/{Uri.EscapeDataString(uploadSessionId)}",
            null,
            null,
            GetUploadSessionOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadUploadSessionAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookContentModel> CompleteResumableUploadAsync(string bookId, string uploadSessionId, CompleteResumableUploadRequest request, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        _validator.ValidateUploadSessionId(uploadSessionId);
        Guards.NotNull(request, nameof(request));

        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(request.IdempotencyKey);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            $"/books/{Uri.EscapeDataString(bookId)}/content/uploads/{Uri.EscapeDataString(uploadSessionId)}/complete",
            content,
            headers,
            CompleteResumableUploadOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadBookContentAsync(response, ct).ConfigureAwait(false);
    }
}
