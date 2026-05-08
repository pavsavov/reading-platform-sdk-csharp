using MockPublishingPlatform.Api.Services;
using PublishingPlatform.SDK.Models;

namespace MockPublishingPlatform.Api.Endpoints;

/// <summary>
/// Maps mock API endpoints for book publishing lifecycle operations.
/// </summary>
internal static class BookPublishingEndpointsModule
{
    internal static void Map(WebApplication app)
    {
        app.MapPost("/books/{bookId}/publishing/publish", (string bookId, PublishBookRequest _, HttpRequest httpRequest, MockApiState state) =>
        {
            var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
            var pathKey = $"/books/{bookId}/publishing/publish";
            if (IdempotencyHelper.TryReplay<BookPublishingStatus>(state, HttpMethod.Post.Method, pathKey, idempotencyKey, out var replayed))
            {
                return Results.Ok(replayed);
            }

            if (!state.Books.Any(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase)))
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            var published = new BookPublishingStatus
            {
                BookId = bookId,
                Status = MockApiConstants.PublishedStatus,
                PublishedAt = MockApiConstants.PublishedAtTimestamp,
            };

            state.PublishingStatuses[bookId] = published;
            IdempotencyHelper.StoreReplay(state, HttpMethod.Post.Method, pathKey, idempotencyKey, published);

            return Results.Ok(published);
        });

        app.MapGet("/books/{bookId}/publishing/status", (string bookId, HttpRequest httpRequest, MockApiState state) =>
        {
            if (state.PublishingStatuses.TryGetValue(bookId, out var status))
            {
                return Results.Ok(status);
            }

            return MockApiResultFactory.CreateNotFound(httpRequest, state);
        });
    }
}
