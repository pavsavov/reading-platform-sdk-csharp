namespace PublishingPlatform.SDK.Clients.BookContent.Validation;

using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;

internal sealed class DefaultBookContentRequestValidator : IBookContentRequestValidator
{
    private static readonly HashSet<string> AllowedFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf",
        "epub",
    };

    public void ValidateBookId(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId))
        {
            throw new BookValidationException("Book id is required.");
        }
    }

    public void ValidateUploadRequest(UploadBookContentRequest request)
    {
        if (request.File == Stream.Null)
        {
            throw new BookValidationException("File stream is required.");
        }

        if (request.File is null || !request.File.CanRead)
        {
            throw new BookValidationException("File stream must be readable.");
        }

        if (request.File.CanSeek && request.File.Length <= 0)
        {
            throw new BookValidationException("File stream must not be empty.");
        }

        if (!string.IsNullOrWhiteSpace(request.Format) && !AllowedFormats.Contains(request.Format))
        {
            throw new BookValidationException($"Format must be one of: {string.Join(", ", AllowedFormats)}.");
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new BookValidationException("File name is required.");
        }
    }
}
