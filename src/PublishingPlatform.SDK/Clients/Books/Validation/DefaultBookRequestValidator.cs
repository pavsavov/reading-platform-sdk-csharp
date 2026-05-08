using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Clients.Common.Validation;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.Books.Validation;

internal sealed class DefaultBookRequestValidator : IBookRequestValidator
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "title",
        "author",
        "createdAt",
        "updatedAt",
    };

    public void ValidateBookId(string bookId)
    {
        ValidationGuards.ValidateBookId(bookId);
    }

    public void ValidateCreate(CreateBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BookValidationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Author))
        {
            throw new BookValidationException("Author is required.");
        }
    }

    public void ValidateList(ListBooksRequest request)
    {
        if (request.Page < 0)
        {
            throw new BookValidationException("Page must be greater than or equal to zero.");
        }

        if (request.PageSize is < 1 or > 200)
        {
            throw new BookValidationException("PageSize must be between 1 and 200.");
        }

        if (!AllowedSortFields.Contains(request.SortBy))
        {
            throw new BookValidationException($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");
        }
    }

    public void ValidateUpdate(UpdateBookMetadataRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new BookValidationException("Title is required for full metadata update.");
        }

        if (string.IsNullOrWhiteSpace(request.Author))
        {
            throw new BookValidationException("Author is required for full metadata update.");
        }
    }

    public void ValidatePatch(UpdateBookPatchRequest request)
    {
        if (request.Title is null && request.Author is null && request.Tags is null)
        {
            throw new BookValidationException("At least one patch field must be provided.");
        }
    }
}
