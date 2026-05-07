using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.BookContent.Requests;
using PublishingPlatform.SDK.Clients.BookContent.Serialization;
using PublishingPlatform.SDK.Clients.BookContent.Validation;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;
using BookContentModel = PublishingPlatform.SDK.Models.BookContent;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Implements book content operations using the shared SDK transport.
/// </summary>
public sealed class BookContentClient : IBookContentClient
{
    private const string GetOperationName = "BookContent.Get";
    private const string UploadOrReplaceOperationName = "BookContent.UploadOrReplace";

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
            $"/books/{Uri.EscapeDataString(bookId)}/content",
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
            $"/books/{Uri.EscapeDataString(bookId)}/content",
            content,
            headers,
            UploadOrReplaceOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadBookContentAsync(response, ct).ConfigureAwait(false);
    }
}
