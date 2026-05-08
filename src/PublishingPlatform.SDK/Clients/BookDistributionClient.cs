using System.Net.Http.Json;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.BookDistribution;
using PublishingPlatform.SDK.Clients.BookDistribution.Requests;
using PublishingPlatform.SDK.Clients.BookDistribution.Serialization;
using PublishingPlatform.SDK.Clients.BookDistribution.Validation;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Implements operational distribution workflows using the shared SDK transport.
/// </summary>
public sealed class BookDistributionClient : IBookDistributionClient
{
    private const string StartOperationName = "BookDistribution.Start";
    private const string GetStatusOperationName = "BookDistribution.GetStatus";
    private const string RetryOperationName = "BookDistribution.Retry";
    private const string ListOperationName = "BookDistribution.List";

    private readonly ISharedHttpTransport _transport;
    private readonly IBookDistributionRequestValidator _validator;
    private readonly IBookDistributionRequestHeadersFactory _requestHeadersFactory;
    private readonly IBookDistributionResponseReader _responseReader;

    internal BookDistributionClient(ISharedHttpTransport transport)
        : this(
            transport,
            new DefaultBookDistributionRequestValidator(),
            new DefaultBookDistributionRequestHeadersFactory(),
            new DefaultBookDistributionResponseReader())
    {
    }

    internal BookDistributionClient(
        ISharedHttpTransport transport,
        IBookDistributionRequestValidator validator,
        IBookDistributionRequestHeadersFactory requestHeadersFactory,
        IBookDistributionResponseReader responseReader)
    {
        _transport = transport;
        _validator = validator;
        _requestHeadersFactory = requestHeadersFactory;
        _responseReader = responseReader;
    }

    /// <inheritdoc />
    public async Task<BookDistributionOperation> StartAsync(string bookId, StartBookDistributionRequest request, string? idempotencyKey = null, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        Guards.NotNull(request, nameof(request));
        _validator.ValidateStartRequest(request);

        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(idempotencyKey);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            BookDistributionEndpoints.Start(bookId),
            content,
            headers,
            StartOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadOperationAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookDistributionOperation> GetStatusAsync(string bookId, string operationId, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        _validator.ValidateOperationId(operationId);

        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            BookDistributionEndpoints.Status(bookId, operationId),
            null,
            null,
            GetStatusOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadOperationAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookDistributionOperation> RetryAsync(string bookId, string operationId, string? idempotencyKey = null, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);
        _validator.ValidateOperationId(operationId);

        var headers = _requestHeadersFactory.CreateIdempotencyHeaders(idempotencyKey);
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            BookDistributionEndpoints.Retry(bookId, operationId),
            null,
            headers,
            RetryOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadOperationAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookDistributionListResult> ListAsync(string bookId, CancellationToken ct = default)
    {
        _validator.ValidateBookId(bookId);

        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            BookDistributionEndpoints.List(bookId),
            null,
            null,
            ListOperationName,
            ct).ConfigureAwait(false);

        return await _responseReader.ReadListAsync(response, ct).ConfigureAwait(false);
    }
}
