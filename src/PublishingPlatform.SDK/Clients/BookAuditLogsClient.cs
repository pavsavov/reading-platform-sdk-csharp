using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.Common.Pagination;
using PublishingPlatform.SDK.Clients.BookAuditLogs.Serialization;
using PublishingPlatform.SDK.Clients.BookAuditLogs.Validation;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Orchestrates book audit-log list operations through the shared transport.
/// </summary>
public sealed class BookAuditLogsClient : IBookAuditLogsClient
{
    private const string ListOperationName = "BookAuditLogs.List";
    private readonly ISharedHttpTransport _transport;
    private readonly IBookAuditLogsRequestValidator _validator;
    private readonly IBookAuditLogsQueryStringBuilder _queryStringBuilder;
    private readonly IBookAuditLogsResponseReader _responseReader;

    internal BookAuditLogsClient(ISharedHttpTransport transport)
        : this(
            transport,
            new DefaultBookAuditLogsRequestValidator(),
            new DefaultBookAuditLogsQueryStringBuilder(),
            new DefaultBookAuditLogsResponseReader())
    {
    }

    internal BookAuditLogsClient(
        ISharedHttpTransport transport,
        IBookAuditLogsRequestValidator validator,
        IBookAuditLogsQueryStringBuilder queryStringBuilder,
        IBookAuditLogsResponseReader responseReader)
    {
        _transport = transport;
        _validator = validator;
        _queryStringBuilder = queryStringBuilder;
        _responseReader = responseReader;
    }

    /// <inheritdoc />
    public async Task<PagedResult<AuditLog>> ListAsync(
        ListBookAuditLogsRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateList(request);

        var relativePath = _queryStringBuilder.BuildListPath(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            relativePath,
            null,
            null,
            ListOperationName,
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadListAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AuditLog> ListAllAsync(
        ListBookAuditLogsRequest request,
        CancellationToken cancellationToken = default)
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
            cancellationToken);
    }

    private static ListBookAuditLogsRequest CloneListRequest(ListBookAuditLogsRequest request)
    {
        return new ListBookAuditLogsRequest
        {
            BookId = request.BookId,
            ActorId = request.ActorId,
            Action = request.Action,
            From = request.From,
            To = request.To,
            CorrelationId = request.CorrelationId,
            Page = request.Page,
            PageSize = request.PageSize,
            ContinuationToken = request.ContinuationToken,
        };
    }
}
