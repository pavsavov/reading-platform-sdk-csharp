namespace PublishingPlatform.SDK.Clients.BookContent.Validation;

using PublishingPlatform.SDK.Clients.Common.Validation;
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
        ValidationGuards.ValidateBookId(bookId);
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

    public void ValidateUploadSessionId(string uploadSessionId)
    {
        if (string.IsNullOrWhiteSpace(uploadSessionId))
        {
            throw new BookValidationException("Upload session id is required.");
        }
    }

    public void ValidateStartResumableUploadRequest(StartResumableUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new BookValidationException("File name is required.");
        }

        if (request.TotalBytes <= 0)
        {
            throw new BookValidationException("Total bytes must be greater than zero.");
        }

        if (!string.IsNullOrWhiteSpace(request.Format) && !AllowedFormats.Contains(request.Format))
        {
            throw new BookValidationException($"Format must be one of: {string.Join(", ", AllowedFormats)}.");
        }
    }

    public void ValidateUploadChunkRequest(UploadChunkRequest request)
    {
        if (request.Chunk == Stream.Null)
        {
            throw new BookValidationException("Chunk stream is required.");
        }

        if (request.Chunk is null || !request.Chunk.CanRead)
        {
            throw new BookValidationException("Chunk stream must be readable.");
        }

        if (request.TotalBytes <= 0)
        {
            throw new BookValidationException("Total bytes must be greater than zero.");
        }

        if (request.ChunkStart < 0 || request.ChunkEnd < 0)
        {
            throw new BookValidationException("Chunk range offsets must be non-negative.");
        }

        if (request.ChunkStart > request.ChunkEnd)
        {
            throw new BookValidationException("Chunk start must be less than or equal to chunk end.");
        }

        if (request.ChunkEnd >= request.TotalBytes)
        {
            throw new BookValidationException("Chunk end must be less than total bytes.");
        }
    }
}
