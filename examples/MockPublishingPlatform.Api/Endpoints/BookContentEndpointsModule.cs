using System.Globalization;
using MockPublishingPlatform.Api.Models;
using MockPublishingPlatform.Api.Services;
using PublishingPlatform.SDK.Models;

namespace MockPublishingPlatform.Api.Endpoints;

/// <summary>
/// Maps mock API endpoints for book content upload and retrieval operations.
/// </summary>
internal static class BookContentEndpointsModule
{
    private const string ContentRangePrefix = "bytes ";

    internal static void Map(WebApplication app)
    {
        app.MapPut("/books/{bookId}/content", async (string bookId, HttpRequest httpRequest, MockApiState state) =>
        {
            var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
            var pathKey = $"/books/{bookId}/content";
            if (IdempotencyHelper.TryReplay<BookContent>(state, HttpMethod.Put.Method, pathKey, idempotencyKey, out var replayed))
            {
                return Results.Ok(replayed);
            }

            if (!state.Books.Any(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase)))
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            if (!httpRequest.HasFormContentType)
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            var form = await httpRequest.ReadFormAsync().ConfigureAwait(false);
            var file = form.Files["file"];
            if (file is null || file.Length == 0)
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            var format = form["format"].ToString();
            if (string.IsNullOrWhiteSpace(format))
            {
                format = Path.GetExtension(file.FileName).Trim('.').ToLowerInvariant();
            }

            var content = new BookContent
            {
                BookId = bookId,
                Format = string.IsNullOrWhiteSpace(format) ? MockApiConstants.UnknownFormat : format,
                ContentUrl = new Uri($"https://mock.publisher-platform.dev/content/{Uri.EscapeDataString(bookId)}"),
                LocalPath = $"/mock-storage/{bookId}/{file.FileName}",
                StoredAt = MockApiConstants.StoredAtTimestamp,
            };

            state.BookContents[bookId] = content;
            IdempotencyHelper.StoreReplay(state, HttpMethod.Put.Method, pathKey, idempotencyKey, content);

            return Results.Ok(content);
        });

        app.MapGet("/books/{bookId}/content", (string bookId, HttpRequest httpRequest, MockApiState state) =>
        {
            if (!state.Books.Any(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase)))
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            if (state.BookContents.TryGetValue(bookId, out var content))
            {
                return Results.Ok(content);
            }

            return MockApiResultFactory.CreateNotFound(httpRequest, state);
        });

        app.MapPost("/books/{bookId}/content/uploads", (string bookId, StartResumableUploadRequest request, HttpRequest httpRequest, MockApiState state) =>
        {
            if (!state.Books.Any(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase)))
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            if (string.IsNullOrWhiteSpace(request.FileName) || request.TotalBytes <= 0)
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
            var pathKey = $"/books/{bookId}/content/uploads";
            if (IdempotencyHelper.TryReplay<UploadSessionInfo>(state, HttpMethod.Post.Method, pathKey, idempotencyKey, out var replayed))
            {
                return Results.Ok(replayed);
            }

            var sessionId = $"upl-{state.UploadSessions.Count + 1:D4}";
            state.UploadSessions[sessionId] = new UploadSessionState
            {
                UploadSessionId = sessionId,
                BookId = bookId,
                FileName = request.FileName.Trim(),
                ContentType = request.ContentType,
                Format = request.Format,
                UploadedBytes = 0,
                TotalBytes = request.TotalBytes,
                Status = MockApiConstants.PendingStatus,
            };

            var info = new UploadSessionInfo
            {
                UploadSessionId = sessionId,
                BookId = bookId,
                UploadedBytes = 0,
                TotalBytes = request.TotalBytes,
                Status = MockApiConstants.PendingStatus,
                ExpiresAt = MockApiConstants.UploadSessionExpiresAtTimestamp,
            };

            IdempotencyHelper.StoreReplay(state, HttpMethod.Post.Method, pathKey, idempotencyKey, info);
            return Results.Ok(info);
        });

        app.MapPut("/books/{bookId}/content/uploads/{uploadSessionId}/chunks", (string bookId, string uploadSessionId, HttpRequest httpRequest, MockApiState state) =>
        {
            if (!state.UploadSessions.TryGetValue(uploadSessionId, out var session)
                || !string.Equals(session.BookId, bookId, StringComparison.OrdinalIgnoreCase))
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            if (!httpRequest.Headers.TryGetValue(MockApiConstants.ContentRangeHeaderName, out var rangeHeaderValues))
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            var rangeHeader = rangeHeaderValues.ToString();
            if (!TryParseContentRange(rangeHeader, out var rangeStart, out var rangeEnd, out var rangeTotal))
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            if (rangeStart != session.UploadedBytes || rangeTotal != session.TotalBytes || rangeEnd < rangeStart)
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            if (!httpRequest.ContentLength.HasValue || httpRequest.ContentLength.Value <= 0)
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            var expectedLength = (rangeEnd - rangeStart) + 1;
            var actualLength = httpRequest.ContentLength.Value;
            if (actualLength != expectedLength)
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            session.UploadedBytes += actualLength;
            session.Status = session.UploadedBytes == session.TotalBytes
                ? MockApiConstants.UploadedStatus
                : MockApiConstants.InProgressStatus;

            var chunkResult = new UploadChunkResult
            {
                UploadSessionId = uploadSessionId,
                AcceptedRangeStart = rangeStart,
                AcceptedRangeEnd = rangeEnd,
                UploadedBytes = session.UploadedBytes,
                IsComplete = session.UploadedBytes == session.TotalBytes,
            };

            return Results.Ok(chunkResult);
        });

        app.MapGet("/books/{bookId}/content/uploads/{uploadSessionId}", (string bookId, string uploadSessionId, HttpRequest httpRequest, MockApiState state) =>
        {
            if (!state.UploadSessions.TryGetValue(uploadSessionId, out var session)
                || !string.Equals(session.BookId, bookId, StringComparison.OrdinalIgnoreCase))
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            var info = new UploadSessionInfo
            {
                UploadSessionId = session.UploadSessionId,
                BookId = session.BookId,
                UploadedBytes = session.UploadedBytes,
                TotalBytes = session.TotalBytes,
                Status = session.Status,
                ExpiresAt = MockApiConstants.UploadSessionExpiresAtTimestamp,
            };

            return Results.Ok(info);
        });

        app.MapPost("/books/{bookId}/content/uploads/{uploadSessionId}/complete", (string bookId, string uploadSessionId, CompleteResumableUploadRequest _, HttpRequest httpRequest, MockApiState state) =>
        {
            if (!state.UploadSessions.TryGetValue(uploadSessionId, out var session)
                || !string.Equals(session.BookId, bookId, StringComparison.OrdinalIgnoreCase))
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
            var pathKey = $"/books/{bookId}/content/uploads/{uploadSessionId}/complete";
            if (IdempotencyHelper.TryReplay<BookContent>(state, HttpMethod.Post.Method, pathKey, idempotencyKey, out var replayed))
            {
                return Results.Ok(replayed);
            }

            if (session.UploadedBytes != session.TotalBytes)
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            session.Status = MockApiConstants.CompletedStatus;
            var format = string.IsNullOrWhiteSpace(session.Format)
                ? Path.GetExtension(session.FileName).Trim('.').ToLowerInvariant()
                : session.Format;

            var content = new BookContent
            {
                BookId = bookId,
                Format = string.IsNullOrWhiteSpace(format) ? MockApiConstants.UnknownFormat : format,
                ContentUrl = new Uri($"https://mock.publisher-platform.dev/content/{Uri.EscapeDataString(bookId)}"),
                LocalPath = $"/mock-storage/{bookId}/{session.FileName}",
                StoredAt = MockApiConstants.StoredAtTimestamp,
            };

            state.BookContents[bookId] = content;
            IdempotencyHelper.StoreReplay(state, HttpMethod.Post.Method, pathKey, idempotencyKey, content);

            return Results.Ok(content);
        });
    }

    private static bool TryParseContentRange(string value, out long rangeStart, out long rangeEnd, out long totalBytes)
    {
        rangeStart = 0;
        rangeEnd = 0;
        totalBytes = 0;

        if (!value.StartsWith(ContentRangePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var pieces = value[ContentRangePrefix.Length..].Split('/', StringSplitOptions.TrimEntries);
        if (pieces.Length != 2)
        {
            return false;
        }

        var range = pieces[0].Split('-', StringSplitOptions.TrimEntries);
        if (range.Length != 2)
        {
            return false;
        }

        return long.TryParse(range[0], NumberStyles.None, CultureInfo.InvariantCulture, out rangeStart)
            && long.TryParse(range[1], NumberStyles.None, CultureInfo.InvariantCulture, out rangeEnd)
            && long.TryParse(pieces[1], NumberStyles.None, CultureInfo.InvariantCulture, out totalBytes);
    }
}
