using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;
using MockPublishingPlatform.Api.Models;
using MockPublishingPlatform.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(FixtureLoader.Load(Path.Combine(builder.Environment.ContentRootPath, "Fixtures")));

var app = builder.Build();
var storedAtTimestamp = DateTimeOffset.Parse("2026-05-08T12:00:00Z");
var publishedAtTimestamp = DateTimeOffset.Parse("2026-05-08T12:01:00Z");
var distributionStartTimestamp = DateTimeOffset.Parse("2026-05-08T12:02:00Z");
var webhookCreatedTimestamp = DateTimeOffset.Parse("2026-05-08T12:03:00Z");
var webhookUpdatedTimestamp = DateTimeOffset.Parse("2026-05-08T12:04:00Z");

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var existing)
        && !string.IsNullOrWhiteSpace(existing)
        ? existing.ToString()
        : $"mock-{Guid.NewGuid():N}";

    context.Response.Headers["X-Correlation-Id"] = correlationId;
    await next().ConfigureAwait(false);
});

app.MapPost("/books", (CreateBookRequest request, HttpRequest httpRequest, MockApiState state) =>
{
    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Author))
    {
        return Results.BadRequest(ApiErrorResponse.FromFixture(state.ApiErrors["validation"], httpRequest.HttpContext.TraceIdentifier));
    }

    var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
    if (IdempotencyHelper.TryReplay<Book>(state, $"POST:/books:{idempotencyKey}", out var replayed))
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
    if (!string.IsNullOrWhiteSpace(idempotencyKey))
    {
        state.IdempotencyResponses[$"POST:/books:{idempotencyKey}"] = created;
    }

    return Results.Ok(created);
});

app.MapGet("/books/{bookId}", (string bookId, HttpRequest httpRequest, MockApiState state) =>
{
    var found = state.Books.FirstOrDefault(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase));
    if (found is null)
    {
        return CreateNotFound(httpRequest, state);
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

app.MapPut("/books/{bookId}/content", async (string bookId, HttpRequest httpRequest, MockApiState state) =>
{
    var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
    if (IdempotencyHelper.TryReplay<BookContent>(state, $"PUT:/books/{bookId}/content:{idempotencyKey}", out var replayed))
    {
        return Results.Ok(replayed);
    }

    if (!state.Books.Any(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase)))
    {
        return CreateNotFound(httpRequest, state);
    }

    if (!httpRequest.HasFormContentType)
    {
        return Results.BadRequest(ApiErrorResponse.FromFixture(state.ApiErrors["validation"], httpRequest.HttpContext.TraceIdentifier));
    }

    var form = await httpRequest.ReadFormAsync().ConfigureAwait(false);
    var file = form.Files["file"];
    if (file is null || file.Length == 0)
    {
        return Results.BadRequest(ApiErrorResponse.FromFixture(state.ApiErrors["validation"], httpRequest.HttpContext.TraceIdentifier));
    }

    var format = form["format"].ToString();
    if (string.IsNullOrWhiteSpace(format))
    {
        format = Path.GetExtension(file.FileName).Trim('.').ToLowerInvariant();
    }

    var now = DateTimeOffset.Parse("2026-05-08T12:00:00Z");
    var content = new BookContent
    {
        BookId = bookId,
        Format = string.IsNullOrWhiteSpace(format) ? "unknown" : format,
        ContentUrl = new Uri($"https://mock.publisher-platform.dev/content/{Uri.EscapeDataString(bookId)}"),
        LocalPath = $"/mock-storage/{bookId}/{file.FileName}",
        StoredAt = storedAtTimestamp,
    };

    state.BookContents[bookId] = content;
    if (!string.IsNullOrWhiteSpace(idempotencyKey))
    {
        state.IdempotencyResponses[$"PUT:/books/{bookId}/content:{idempotencyKey}"] = content;
    }

    return Results.Ok(content);
});

app.MapGet("/books/{bookId}/content", (string bookId, HttpRequest httpRequest, MockApiState state) =>
{
    if (!state.Books.Any(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase)))
    {
        return CreateNotFound(httpRequest, state);
    }

    if (state.BookContents.TryGetValue(bookId, out var content))
    {
        return Results.Ok(content);
    }

    return CreateNotFound(httpRequest, state);
});

app.MapPost("/books/{bookId}/publishing/publish", (string bookId, PublishBookRequest _, HttpRequest httpRequest, MockApiState state) =>
{
    var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
    if (IdempotencyHelper.TryReplay<BookPublishingStatus>(state, $"POST:/books/{bookId}/publishing/publish:{idempotencyKey}", out var replayed))
    {
        return Results.Ok(replayed);
    }

    if (!state.Books.Any(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase)))
    {
        return CreateNotFound(httpRequest, state);
    }

    var published = new BookPublishingStatus
    {
        BookId = bookId,
        Status = "published",
        PublishedAt = publishedAtTimestamp,
    };

    state.PublishingStatuses[bookId] = published;
    if (!string.IsNullOrWhiteSpace(idempotencyKey))
    {
        state.IdempotencyResponses[$"POST:/books/{bookId}/publishing/publish:{idempotencyKey}"] = published;
    }

    return Results.Ok(published);
});

app.MapGet("/books/{bookId}/publishing/status", (string bookId, HttpRequest httpRequest, MockApiState state) =>
{
    if (state.PublishingStatuses.TryGetValue(bookId, out var status))
    {
        return Results.Ok(status);
    }

    return CreateNotFound(httpRequest, state);
});

app.MapPost("/books/{bookId}/distribution/start", (string bookId, StartBookDistributionRequest request, HttpRequest httpRequest, MockApiState state) =>
{
    var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
    if (IdempotencyHelper.TryReplay<BookDistributionOperation>(state, $"POST:/books/{bookId}/distribution/start:{idempotencyKey}", out var replayed))
    {
        return Results.Ok(replayed);
    }

    if (!state.Books.Any(book => string.Equals(book.Id, bookId, StringComparison.OrdinalIgnoreCase)))
    {
        return CreateNotFound(httpRequest, state);
    }

    if (request.Channels.Count == 0)
    {
        return Results.BadRequest(ApiErrorResponse.FromFixture(state.ApiErrors["validation"], httpRequest.HttpContext.TraceIdentifier));
    }

    var operation = new BookDistributionOperation
    {
        OperationId = $"dist-{state.DistributionOperations.Count + 1:D4}",
        BookId = bookId,
        Status = "in_progress",
        StartedAt = distributionStartTimestamp,
    };

    state.DistributionOperations[operation.OperationId] = operation;
    if (!string.IsNullOrWhiteSpace(idempotencyKey))
    {
        state.IdempotencyResponses[$"POST:/books/{bookId}/distribution/start:{idempotencyKey}"] = operation;
    }

    return Results.Ok(operation);
});

app.MapGet("/books/{bookId}/distribution/{operationId}/status", (string bookId, string operationId, HttpRequest httpRequest, MockApiState state) =>
{
    if (state.DistributionOperations.TryGetValue(operationId, out var operation)
        && string.Equals(operation.BookId, bookId, StringComparison.OrdinalIgnoreCase))
    {
        return Results.Ok(operation);
    }

    return CreateNotFound(httpRequest, state);
});

app.MapPost("/webhooks", (RegisterWebhookRequest request, HttpRequest httpRequest, MockApiState state) =>
{
    var idempotencyKey = IdempotencyHelper.Resolve(httpRequest);
    if (IdempotencyHelper.TryReplay<Webhook>(state, $"POST:/webhooks:{idempotencyKey}", out var replayed))
    {
        return Results.Ok(replayed);
    }

    if (string.IsNullOrWhiteSpace(request.EndpointUrl) || request.Events.Count == 0)
    {
        return Results.BadRequest(ApiErrorResponse.FromFixture(state.ApiErrors["validation"], httpRequest.HttpContext.TraceIdentifier));
    }

    var webhook = new Webhook
    {
        Id = $"wh-{state.Webhooks.Count + 1:D4}",
        EndpointUrl = request.EndpointUrl.Trim(),
        Events = request.Events.Select(value => value.Trim()).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
        IsActive = request.IsActive,
        SigningKeyId = request.SigningKeyId,
        CreatedAt = webhookCreatedTimestamp,
    };

    state.Webhooks.Add(webhook);
    if (!string.IsNullOrWhiteSpace(idempotencyKey))
    {
        state.IdempotencyResponses[$"POST:/webhooks:{idempotencyKey}"] = webhook;
    }

    return Results.Ok(webhook);
});

app.MapGet("/webhooks", (HttpRequest httpRequest, MockApiState state) =>
{
    var pageSize = QueryParser.ParsePositiveInt(httpRequest.Query["pageSize"], defaultValue: 50, min: 1, max: 500);
    var eventFilter = httpRequest.Query["event"].ToString();
    var isActiveFilter = QueryParser.ParseNullableBoolean(httpRequest.Query["isActive"]);
    var continuationToken = httpRequest.Query["continuationToken"].ToString();
    var start = QueryParser.ParseContinuationOffset(continuationToken);

    IEnumerable<Webhook> filtered = state.Webhooks;
    if (!string.IsNullOrWhiteSpace(eventFilter))
    {
        filtered = filtered.Where(webhook => webhook.Events.Contains(eventFilter, StringComparer.OrdinalIgnoreCase));
    }

    if (isActiveFilter.HasValue)
    {
        filtered = filtered.Where(webhook => webhook.IsActive == isActiveFilter.Value);
    }

    var total = filtered.Count();
    var items = filtered.Skip(start).Take(pageSize).ToArray();
    var next = start + items.Length < total ? $"offset:{start + items.Length}" : null;
    var result = new PagedResult<Webhook>
    {
        Items = items,
        TotalCount = total,
        ContinuationToken = next,
    };

    return Results.Ok(result);
});

app.MapPatch("/webhooks/{webhookId}", (string webhookId, UpdateWebhookRequest request, HttpRequest httpRequest, MockApiState state) =>
{
    var existing = state.Webhooks.FirstOrDefault(webhook => string.Equals(webhook.Id, webhookId, StringComparison.OrdinalIgnoreCase));
    if (existing is null)
    {
        return CreateNotFound(httpRequest, state);
    }

    existing.EndpointUrl = request.EndpointUrl.Trim();
    existing.Events = request.Events.Select(value => value.Trim()).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    existing.IsActive = request.IsActive;
    existing.UpdatedAt = webhookUpdatedTimestamp;
    return Results.Ok(existing);
});

app.MapDelete("/webhooks/{webhookId}", (string webhookId, HttpRequest httpRequest, MockApiState state) =>
{
    var removed = state.Webhooks.RemoveAll(webhook => string.Equals(webhook.Id, webhookId, StringComparison.OrdinalIgnoreCase));
    if (removed == 0)
    {
        return CreateNotFound(httpRequest, state);
    }

    return Results.NoContent();
});

app.Run();

static IResult CreateNotFound(HttpRequest request, MockApiState state)
{
    return Results.NotFound(ApiErrorResponse.FromFixture(state.ApiErrors["notFound"], request.HttpContext.TraceIdentifier));
}
