using MockPublishingPlatform.Api.Services;
using PublishingPlatform.SDK.Models;

namespace MockPublishingPlatform.Api.Endpoints;

/// <summary>
/// Maps mock API endpoints for book content upload and retrieval operations.
/// </summary>
internal static class BookContentEndpointsModule
{
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
    }
}
