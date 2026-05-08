using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Clients.Common.Validation;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.BookAccess.Validation;

/// <summary>
/// Enforces validation rules for book access requests.
/// </summary>
internal sealed class DefaultBookAccessRequestValidator : IBookAccessRequestValidator
{
    private static readonly HashSet<string> AllowedPrincipalTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "user",
        "group",
        "organization",
    };

    private static readonly HashSet<string> AllowedAccessLevels = new(StringComparer.OrdinalIgnoreCase)
    {
        "read",
        "write",
        "owner",
    };

    /// <inheritdoc />
    public void ValidateGrant(BookAccessGrantRequest request)
    {
        ValidateBookId(request.BookId);
        ValidatePrincipalId(request.PrincipalId);
        ValidateAccessLevel(request.AccessLevel);
        ValidatePrincipalType(request.PrincipalType);

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            throw new BookValidationException("ExpiresAt must be in the future when provided.");
        }
    }

    /// <inheritdoc />
    public void ValidateRevoke(BookAccessRevokeRequest request)
    {
        ValidateBookId(request.BookId);
        ValidatePrincipalId(request.PrincipalId);
        ValidatePrincipalType(request.PrincipalType);
    }

    /// <inheritdoc />
    public void ValidateCheck(BookAccessCheckRequest request)
    {
        ValidateBookId(request.BookId);
        ValidatePrincipalId(request.PrincipalId);
        ValidatePrincipalType(request.PrincipalType);
    }

    /// <inheritdoc />
    public void ValidateList(ListBookAccessRequest request)
    {
        if (request.PageSize < 1 || request.PageSize > 200)
        {
            throw new BookValidationException("PageSize must be between 1 and 200.");
        }

        if (!string.IsNullOrWhiteSpace(request.PrincipalType))
        {
            ValidatePrincipalType(request.PrincipalType);
        }

        if (!string.IsNullOrWhiteSpace(request.AccessLevel))
        {
            ValidateAccessLevel(request.AccessLevel);
        }
    }

    private static void ValidateBookId(string bookId)
    {
        ValidationGuards.ValidateBookId(bookId);
    }

    private static void ValidatePrincipalId(string principalId)
    {
        if (string.IsNullOrWhiteSpace(principalId))
        {
            throw new BookValidationException("PrincipalId is required.");
        }
    }

    private static void ValidatePrincipalType(string principalType)
    {
        if (string.IsNullOrWhiteSpace(principalType))
        {
            throw new BookValidationException("PrincipalType is required.");
        }

        if (!AllowedPrincipalTypes.Contains(principalType))
        {
            throw new BookValidationException("PrincipalType must be one of: user, group, organization.");
        }
    }

    private static void ValidateAccessLevel(string accessLevel)
    {
        if (string.IsNullOrWhiteSpace(accessLevel))
        {
            throw new BookValidationException("AccessLevel is required.");
        }

        if (!AllowedAccessLevels.Contains(accessLevel))
        {
            throw new BookValidationException("AccessLevel must be one of: read, write, owner.");
        }
    }
}
