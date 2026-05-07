using System.Diagnostics;
using System.Net.Http.Json;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.BookAccess.Serialization;
using PublishingPlatform.SDK.Clients.BookAccess.Validation;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Orchestrates book access and entitlement operations through the shared transport.
/// </summary>
public sealed class BookAccessClient : IBookAccessClient
{
    private readonly ISharedHttpTransport _transport;
    private readonly IBookAccessRequestValidator _validator;
    private readonly IBookAccessQueryStringBuilder _queryStringBuilder;
    private readonly IBookAccessResponseReader _responseReader;

    internal BookAccessClient(ISharedHttpTransport transport)
        : this(
            transport,
            new DefaultBookAccessRequestValidator(),
            new DefaultBookAccessQueryStringBuilder(),
            new DefaultBookAccessResponseReader())
    {
    }

    internal BookAccessClient(
        ISharedHttpTransport transport,
        IBookAccessRequestValidator validator,
        IBookAccessQueryStringBuilder queryStringBuilder,
        IBookAccessResponseReader responseReader)
    {
        _transport = transport;
        _validator = validator;
        _queryStringBuilder = queryStringBuilder;
        _responseReader = responseReader;
    }

    /// <inheritdoc />
    public async Task<BookAccessGrantResult> GrantAsync(
        BookAccessGrantRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateGrant(request);

        using var activity = StartActivity("BookAccess.Grant");
        var content = JsonContent.Create(request);
        var relativePath = $"/books/{Uri.EscapeDataString(request.BookId)}/access";
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            relativePath,
            content,
            null,
            "BookAccess.Grant",
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadGrantResultAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookAccessRevokeResult> RevokeAsync(
        BookAccessRevokeRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateRevoke(request);

        using var activity = StartActivity("BookAccess.Revoke");
        var content = JsonContent.Create(request);
        var relativePath = $"/books/{Uri.EscapeDataString(request.BookId)}/access/revoke";
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            relativePath,
            content,
            null,
            "BookAccess.Revoke",
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadRevokeResultAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BookAccessStatus> CheckAsync(
        BookAccessCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateCheck(request);

        using var activity = StartActivity("BookAccess.Check");
        var relativePath = _queryStringBuilder.BuildCheckPath(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            relativePath,
            null,
            null,
            "BookAccess.Check",
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadStatusAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedResult<BookAccessGrant>> ListAsync(
        ListBookAccessRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateList(request);

        using var activity = StartActivity("BookAccess.List");
        var relativePath = _queryStringBuilder.BuildListPath(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            relativePath,
            null,
            null,
            "BookAccess.List",
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadPagedGrantResultAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static Activity? StartActivity(string operationName)
    {
        var activity = ActivitySourceProvider.ActivitySource.StartActivity(operationName, ActivityKind.Client);
        activity?.SetTag("sdk.operation", operationName);
        return activity;
    }
}
