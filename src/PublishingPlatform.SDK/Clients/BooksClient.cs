using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Implements book lifecycle operations using the shared SDK transport.
/// </summary>
public sealed class BooksClient : IBooksClient
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "title",
        "author",
        "createdAt",
        "updatedAt",
    };

    private readonly ISharedHttpTransport _transport;

    internal BooksClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }

    /// <inheritdoc />
    public async Task<Book> CreateAsync(CreateBookRequest request, CancellationToken ct = default)
    {
        Guards.NotNull(request, nameof(request));
        ValidateCreateRequest(request);

        using var activity = StartActivity("Books.Create");
        var headers = BuildIdempotencyHeaders(request.IdempotencyKey);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(HttpMethod.Post, "/books", content, headers, "Books.Create", ct).ConfigureAwait(false);

        return await ReadBookAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Book> GetByIdAsync(string bookId, CancellationToken ct = default)
    {
        ValidateBookId(bookId);
        using var activity = StartActivity("Books.GetById");
        using var response = await _transport.SendAsync(HttpMethod.Get, $"/books/{Uri.EscapeDataString(bookId)}", null, null, "Books.GetById", ct).ConfigureAwait(false);
        return await ReadBookAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedResult<Book>> ListAsync(ListBooksRequest request, CancellationToken ct = default)
    {
        Guards.NotNull(request, nameof(request));
        ValidateListRequest(request);

        using var activity = StartActivity("Books.List");
        var relativePath = BuildListPath(request);
        using var response = await _transport.SendAsync(HttpMethod.Get, relativePath, null, null, "Books.List", ct).ConfigureAwait(false);
        return await ReadPagedResultAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<Book> ListAllAsync(ListBooksRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        Guards.NotNull(request, nameof(request));
        ValidateListRequest(request);

        var iterationRequest = CloneRequest(request);
        while (true)
        {
            var page = await ListAsync(iterationRequest, ct).ConfigureAwait(false);
            foreach (var item in page.Items)
            {
                yield return item;
            }

            if (string.IsNullOrWhiteSpace(page.ContinuationToken))
            {
                yield break;
            }

            iterationRequest.ContinuationToken = page.ContinuationToken;
            iterationRequest.Page++;
        }
    }

    /// <inheritdoc />
    public async Task<Book> UpdateMetadataAsync(string bookId, UpdateBookMetadataRequest request, CancellationToken ct = default)
    {
        ValidateBookId(bookId);
        Guards.NotNull(request, nameof(request));
        ValidateUpdateRequest(request);

        using var activity = StartActivity("Books.UpdateMetadata");
        var headers = BuildConcurrencyHeaders(request.ConcurrencyToken);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(HttpMethod.Put, $"/books/{Uri.EscapeDataString(bookId)}", content, headers, "Books.UpdateMetadata", ct).ConfigureAwait(false);
        return await ReadBookAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Book> PatchMetadataAsync(string bookId, UpdateBookPatchRequest request, CancellationToken ct = default)
    {
        ValidateBookId(bookId);
        Guards.NotNull(request, nameof(request));
        ValidatePatchRequest(request);

        using var activity = StartActivity("Books.PatchMetadata");
        var headers = BuildConcurrencyHeaders(request.ConcurrencyToken);
        var content = JsonContent.Create(request);
        using var response = await _transport.SendAsync(HttpMethod.Patch, $"/books/{Uri.EscapeDataString(bookId)}", content, headers, "Books.PatchMetadata", ct).ConfigureAwait(false);
        return await ReadBookAsync(response, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string bookId, CancellationToken ct = default)
    {
        ValidateBookId(bookId);
        using var activity = StartActivity("Books.Delete");
        using var _ = await _transport.SendAsync(HttpMethod.Delete, $"/books/{Uri.EscapeDataString(bookId)}", null, null, "Books.Delete", ct).ConfigureAwait(false);
    }

    private static void ValidateBookId(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId))
        {
            throw new BookValidationException("Book id is required.");
        }
    }

    private static void ValidateCreateRequest(CreateBookRequest request)
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

    private static void ValidateUpdateRequest(UpdateBookMetadataRequest request)
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

    private static void ValidatePatchRequest(UpdateBookPatchRequest request)
    {
        if (request.Title is null && request.Author is null && request.Tags is null)
        {
            throw new BookValidationException("At least one patch field must be provided.");
        }
    }

    private static void ValidateListRequest(ListBooksRequest request)
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

    private static string BuildListPath(ListBooksRequest request)
    {
        var query = new List<KeyValuePair<string, string>>
        {
            new("page", request.Page.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new("pageSize", request.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new("sortBy", request.SortBy),
            new("descending", request.Descending ? "true" : "false"),
        };

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            query.Add(new("title", request.Title));
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            query.Add(new("author", request.Author));
        }

        if (!string.IsNullOrWhiteSpace(request.ContinuationToken))
        {
            query.Add(new("continuationToken", request.ContinuationToken));
        }

        foreach (var tag in request.Tags.OrderBy(x => x, StringComparer.Ordinal))
        {
            query.Add(new("tag", tag));
        }

        var builder = new StringBuilder("/books?");
        for (var i = 0; i < query.Count; i++)
        {
            if (i > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(query[i].Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(query[i].Value));
        }

        return builder.ToString();
    }

    private static Dictionary<string, string>? BuildIdempotencyHeaders(string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return null;
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Idempotency-Key"] = idempotencyKey,
        };
    }

    private static Dictionary<string, string>? BuildConcurrencyHeaders(string? concurrencyToken)
    {
        if (string.IsNullOrWhiteSpace(concurrencyToken))
        {
            return null;
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["If-Match"] = concurrencyToken,
        };
    }

    private static async Task<Book> ReadBookAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var book = await response.Content.ReadFromJsonAsync<Book>(ct).ConfigureAwait(false);
        if (book is null)
        {
            throw new BookValidationException("Book payload was empty.");
        }

        return book;
    }

    private static async Task<PagedResult<Book>> ReadPagedResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var paged = await response.Content.ReadFromJsonAsync<PagedResult<Book>>(ct).ConfigureAwait(false);
        if (paged is null)
        {
            throw new BookValidationException("Paged book payload was empty.");
        }

        return paged;
    }

    private static ListBooksRequest CloneRequest(ListBooksRequest request)
    {
        return new ListBooksRequest
        {
            Title = request.Title,
            Author = request.Author,
            Tags = request.Tags.ToArray(),
            SortBy = request.SortBy,
            Descending = request.Descending,
            Page = request.Page,
            PageSize = request.PageSize,
            ContinuationToken = request.ContinuationToken,
        };
    }

    private static Activity? StartActivity(string operationName)
    {
        var activity = ActivitySourceProvider.ActivitySource.StartActivity(operationName, ActivityKind.Client);
        activity?.SetTag("sdk.operation", operationName);
        return activity;
    }
}
