using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;
using MockPublishingPlatform.Api.Services;

namespace MockPublishingPlatform.Api.Endpoints;

/// <summary>
/// Maps mock API endpoints for book CRUD and listing operations.
/// </summary>
internal static class BooksEndpointsModule
{
    internal static void Map(WebApplication app)
    {
        app.MapPost("/books", (CreateBookRequest request, HttpRequest httpRequest, MockApiState state) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Author))
            {
                return MockApiResultFactory.CreateValidationError(httpRequest, state);
            }

            var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
            if (IdempotencyHelper.TryReplay<Book>(state, HttpMethod.Post.Method, "/books", idempotencyKey, out var replayed))
            {
                return Results.Ok(replayed);
            }

            var nextIndex = state.Books.Count + 1;
            var created = new Book
            {
                Id = $"book-{nextIndex:D4}",
                Title = request.Title.Trim(),
                Author = request.Author.Trim(),
                Tags = request.Tags.Select(tag => tag.Trim()).Where(tag => !string.IsNullOrWhiteSpace(tag)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                ConcurrencyToken = $"v{nextIndex}",
            };

            state.Books.Add(created);
            IdempotencyHelper.StoreReplay(state, HttpMethod.Post.Method, "/books", idempotencyKey, created);

            return Results.Ok(created);
        });

        app.MapGet("/books/{bookId}", (string bookId, HttpRequest httpRequest, MockApiState state) =>
        {
            var found = state.Books.FirstOrDefault(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase));
            if (found is null)
            {
                return MockApiResultFactory.CreateNotFound(httpRequest, state);
            }

            return Results.Ok(found);
        });

        app.MapGet("/books", (HttpRequest httpRequest, MockApiState state) =>
        {
            var pageSize = QueryParser.ParsePositiveInt(httpRequest.Query["pageSize"], defaultValue: 50, min: 1, max: 200);
            var page = QueryParser.ParseNonNegativeInt(httpRequest.Query["page"], defaultValue: 0);
            var title = httpRequest.Query["title"].ToString();
            var author = httpRequest.Query["author"].ToString();
            var sortBy = httpRequest.Query["sortBy"].ToString();
            var descending = QueryParser.ParseBoolean(httpRequest.Query["descending"], defaultValue: false);
            var tags = httpRequest.Query["tag"].Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();

            IEnumerable<Book> filtered = state.Books;
            if (!string.IsNullOrWhiteSpace(title))
            {
                filtered = filtered.Where(book => book.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                filtered = filtered.Where(book => book.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
            }

            if (tags.Length > 0)
            {
                filtered = filtered.Where(book => tags.All(tag => book.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
            }

            var ordered = sortBy.ToLowerInvariant() switch
            {
                "author" => descending ? filtered.OrderByDescending(book => book.Author, StringComparer.OrdinalIgnoreCase) : filtered.OrderBy(book => book.Author, StringComparer.OrdinalIgnoreCase),
                _ => descending ? filtered.OrderByDescending(book => book.Title, StringComparer.OrdinalIgnoreCase) : filtered.OrderBy(book => book.Title, StringComparer.OrdinalIgnoreCase),
            };

            var total = ordered.Count();
            var items = ordered.Skip(page * pageSize).Take(pageSize).ToArray();
            var nextPageExists = (page + 1) * pageSize < total;

            var result = new PagedResult<Book>
            {
                Items = items,
                TotalCount = total,
                ContinuationToken = nextPageExists ? $"page:{page + 1}" : null,
            };

            return Results.Ok(result);
        });
    }
}
