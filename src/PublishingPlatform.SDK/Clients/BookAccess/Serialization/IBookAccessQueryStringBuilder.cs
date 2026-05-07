using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAccess.Serialization;

/// <summary>
/// Builds querystring-based relative paths for book access operations.
/// </summary>
internal interface IBookAccessQueryStringBuilder
{
    /// <summary>
    /// Builds a relative path for an access check operation.
    /// </summary>
    /// <param name="request">The check request.</param>
    /// <returns>The transport relative path.</returns>
    string BuildCheckPath(BookAccessCheckRequest request);

    /// <summary>
    /// Builds a relative path for access list operation.
    /// </summary>
    /// <param name="request">The list request.</param>
    /// <returns>The transport relative path.</returns>
    string BuildListPath(ListBookAccessRequest request);
}
