using MockPublishingPlatform.Api.Services;
using PublishingPlatform.SDK.Models;

namespace MockPublishingPlatform.Api.Endpoints;

/// <summary>
/// Maps mock API endpoints for book distribution operations.
/// </summary>
internal static class BookDistributionEndpointsModule
{
    internal static void Map(WebApplication app)
    {
        app.MapPost("/books/{bookId}/distribution/start", (string bookId, StartBookDistributionRequest request, HttpRequest httpRequest, MockApiState state) =>
        {
            var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
            var pathKey = $"/books/{bookId}/distribution/start";
            if (IdempotencyHelper.TryReplay<BookDistributionOperation>(state, HttpMethod.Post.Method, pathKey, idempotencyKey, out var replayed))
            {
                return Results.Ok(replayed);
            }

            if (!state.Books.Any(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase)))
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            if (request.Channels.Count == 0)
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            var operation = new BookDistributionOperation
            {
                OperationId = $"dist-{state.DistributionOperations.Count + 1:D4}",
                BookId = bookId,
                Status = MockApiConstants.InProgressStatus,
                StartedAt = MockApiConstants.DistributionStartTimestamp,
            };

            state.DistributionOperations[operation.OperationId] = operation;
            IdempotencyHelper.StoreReplay(state, HttpMethod.Post.Method, pathKey, idempotencyKey, operation);

            return Results.Ok(operation);
        });

        app.MapGet("/books/{bookId}/distribution/{operationId}/status", (string bookId, string operationId, HttpRequest httpRequest, MockApiState state) =>
        {
            if (state.DistributionOperations.TryGetValue(operationId, out var operation)
                && string.Equals(operation.BookId, bookId, StringComparison.OrdinalIgnoreCase))
            {
                return Results.Ok(operation);
            }

            return MockApiResultFactory.CreateNotFound(httpRequest, state);
        });
    }
}
