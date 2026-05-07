using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Abstractions;

/// <summary>
/// Provides entitlement and access-control operations for books.
/// </summary>
public interface IBookAccessClient
{
    /// <summary>
    /// Grants access to a principal for a specific book.
    /// </summary>
    /// <param name="request">The grant request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resulting access grant outcome.</returns>
    Task<BookAccessGrantResult> GrantAsync(
        BookAccessGrantRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes access from a principal for a specific book.
    /// </summary>
    /// <param name="request">The revoke request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resulting revoke outcome.</returns>
    Task<BookAccessRevokeResult> RevokeAsync(
        BookAccessRevokeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks effective access for a principal and book combination.
    /// </summary>
    /// <param name="request">The check request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The effective access status.</returns>
    Task<BookAccessStatus> CheckAsync(
        BookAccessCheckRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists book access grants with optional global scope.
    /// </summary>
    /// <param name="request">The list request criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result of access grants.</returns>
    Task<PagedResult<BookAccessGrant>> ListAsync(
        ListBookAccessRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams access grants across all pages for the provided list request.
    /// </summary>
    /// <param name="request">The list request criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of access grants.</returns>
    IAsyncEnumerable<BookAccessGrant> ListAllAsync(
        ListBookAccessRequest request,
        CancellationToken cancellationToken = default);
}
