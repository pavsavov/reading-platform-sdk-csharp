using System.Net.Http.Json;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.BookPublishing.Requests;
using PublishingPlatform.SDK.Clients.BookPublishing.Serialization;
using PublishingPlatform.SDK.Clients.BookPublishing.Validation;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Implements book publishing lifecycle operations using the shared SDK transport.
/// </summary>
public sealed class BookPublishingClient : IBookPublishingClient
{
    private const string PublishOperationName = "BookPublishing.Publish";
    private const string UnpublishOperationName = "BookPublishing.Unpublish";
    private const string ScheduleOperationName = "BookPublishing.Schedule";
    private const string GetStatusOperationName = "BookPublishing.GetStatus";

    private readonly ISharedHttpTransport _transport;
    private readonly IBookPublishingRequestValidator _validator;
    private readonly IBookPublishingRequestHeadersFactory _requestHeadersFactory;
    private readonly IBookPublishingResponseReader _responseReader;

    internal BookPublishingClient(ISharedHttpTransport transport)
        : this(
            transport,
            new DefaultBookPublishingRequestValidator(),
            new DefaultBookPublishingRequestHeadersFactory(),
            new DefaultBookPublishingResponseReader())
    {
    }

    internal BookPublishingClient(
        ISharedHttpTransport transport,
        IBookPublishingRequestValidator validator,
        IBookPublishingRequestHeadersFactory requestHeadersFactory,
        IBookPublishingResponseReader responseReader)
    {
        _transport = transport;
        _validator = validator;
        _requestHeadersFactory = requestHeadersFactory;
        _responseReader = responseReader;
    }

    /// <inheritdoc />
    public async Task<BookPublishingStatus> PublishAsync(string bookId, PublishBookRequest request, string? idempotencyKey = null, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        Guards.NotNull(request, nameof(request));
        _validator.ValidatePublish(request);

        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(idempotencyKey);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            $"/books/{Uri.EscapeDataString(bookId)}/publishing/publish",
            content,
            headers,
            PublishOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadStatusAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookPublishingStatus> UnpublishAsync(string bookId, string? idempotencyKey = null, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);

        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(idempotencyKey);
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            $"/books/{Uri.EscapeDataString(bookId)}/publishing/unpublish",
            null,
            headers,
            UnpublishOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadStatusAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookPublishingStatus> ScheduleAsync(string bookId, ScheduleBookPublishingRequest request, string? idempotencyKey = null, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        Guards.NotNull(request, nameof(request));
        _validator.ValidateSchedule(request);

        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(idempotencyKey);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            $"/books/{Uri.EscapeDataString(bookId)}/publishing/schedule",
            content,
            headers,
            ScheduleOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadStatusAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookPublishingStatus> GetStatusAsync(string bookId, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);

        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            $"/books/{Uri.EscapeDataString(bookId)}/publishing/status",
            null,
            null,
            GetStatusOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadStatusAsync(response, ct).ConfigureAwait(false);
    }
}
