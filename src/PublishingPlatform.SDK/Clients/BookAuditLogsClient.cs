using System.Diagnostics;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.BookAuditLogs.Serialization;
using PublishingPlatform.SDK.Clients.BookAuditLogs.Validation;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
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

        using var activity = StartActivity(ListOperationName);
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

    private static Activity? StartActivity(string operationName)
    {
        var activity = ActivitySourceProvider.ActivitySource.StartActivity(operationName, ActivityKind.Client);
        activity?.SetTag("sdk.operation", operationName);
        return activity;
    }
}
