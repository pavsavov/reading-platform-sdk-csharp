using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAccess.Validation;

/// <summary>
/// Defines validation rules for book access operations.
/// </summary>
internal interface IBookAccessRequestValidator
{
    /// <summary>
    /// Validates grant request payload.
    /// </summary>
    /// <param name="request">The grant request.</param>
    void ValidateGrant(BookAccessGrantRequest request);

    /// <summary>
    /// Validates revoke request payload.
    /// </summary>
    /// <param name="request">The revoke request.</param>
    void ValidateRevoke(BookAccessRevokeRequest request);

    /// <summary>
    /// Validates check request payload.
    /// </summary>
    /// <param name="request">The check request.</param>
    void ValidateCheck(BookAccessCheckRequest request);

    /// <summary>
    /// Validates list request payload.
    /// </summary>
    /// <param name="request">The list request.</param>
    void ValidateList(ListBookAccessRequest request);
}
