using System.Net.Http.Json;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.Common.Pagination;
using PublishingPlatform.SDK.Clients.BookAccess.Serialization;
using PublishingPlatform.SDK.Clients.BookAccess.Validation;
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
    private const string GrantOperationName = "BookAccess.Grant";
    private const string RevokeOperationName = "BookAccess.Revoke";
    private const string CheckOperationName = "BookAccess.Check";
    private const string ListOperationName = "BookAccess.List";

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

        var content = JsonContent.Create(request);
        var relativePath = $"/books/{Uri.EscapeDataString(request.BookId)}/access";
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            relativePath,
            content,
            null,
            GrantOperationName,
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

        var content = JsonContent.Create(request);
        var relativePath = $"/books/{Uri.EscapeDataString(request.BookId)}/access/revoke";
        using var response = await _transport.SendAsync(
            HttpMethod.Post,
            relativePath,
            content,
            null,
            RevokeOperationName,
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

        var relativePath = _queryStringBuilder.BuildCheckPath(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            relativePath,
            null,
            null,
            CheckOperationName,
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

        var relativePath = _queryStringBuilder.BuildListPath(request);
        using var response = await _transport.SendAsync(
            HttpMethod.Get,
            relativePath,
            null,
            null,
            ListOperationName,
            cancellationToken).ConfigureAwait(false);

        return await _responseReader.ReadPagedGrantResultAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<BookAccessGrant> ListAllAsync(
        ListBookAccessRequest request,
        CancellationToken cancellationToken = default)
    {
        Guards.NotNull(request, nameof(request));
        _validator.ValidateList(request);

        return PagedAsyncIterator.IterateAsync(
            request,
            CloneListRequest,
            static (iterationRequest, continuationToken) => iterationRequest.ContinuationToken = continuationToken,
            ListAsync,
            cancellationToken);
    }

    private static ListBookAccessRequest CloneListRequest(ListBookAccessRequest request)
    {
        return new ListBookAccessRequest
        {
            BookId = request.BookId,
            PrincipalId = request.PrincipalId,
            PrincipalType = request.PrincipalType,
            AccessLevel = request.AccessLevel,
            PageSize = request.PageSize,
            ContinuationToken = request.ContinuationToken,
        };
    }
}
