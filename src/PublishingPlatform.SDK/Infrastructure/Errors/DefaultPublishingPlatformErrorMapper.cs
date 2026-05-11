using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients.BookAccess;
using PublishingPlatform.SDK.Clients.BookAnalytics;
using PublishingPlatform.SDK.Clients.BookAuditLogs;
using PublishingPlatform.SDK.Clients.Books;
using PublishingPlatform.SDK.Exceptions;
using System.Linq;

namespace PublishingPlatform.SDK.Infrastructure.Errors;

/// <summary>
/// Maps normalized Publishing Platform API failures to SDK exception types.
/// </summary>
internal sealed class DefaultPublishingPlatformErrorMapper : IPublishingPlatformErrorMapper
{
    private static readonly string[] BookCentricOperationPrefixes =
    [
        "Books",
        "BookContent",
        "BookPublishing",
        "BookDistribution",
        "BookAccess",
        "BookAnalytics",
        "BookAuditLogs",
    ];

    public Exception Map(PublishingPlatformErrorContext context)
    {
        if (IsBookCentric(context))
        {
            var bookId = ExtractBookId(context.RelativePath);
            return context.StatusCode switch
            {
                404 => new BookNotFoundException(bookId, context),
                409 or 412 => new BookConflictException(bookId, context),
                429 => new BookRateLimitedException(context),
                _ => new ApiException(context),
            };
        }

        return new ApiException(context);
    }

    private static bool IsBookCentric(PublishingPlatformErrorContext context)
    {
        if (StartsWithOperationPrefix(context.OperationName, "Webhooks"))
        {
            return false;
        }

        if (BookCentricOperationPrefixes.Any(prefix => StartsWithOperationPrefix(context.OperationName, prefix)))
        {
            return true;
        }

        if (StartsWithBookCentricPath(context.RelativePath))
        {
            return true;
        }

        return false;
    }

    private static bool StartsWithBookCentricPath(string relativePath)
    {
        var path = GetPathWithoutQuery(relativePath);
        return path.StartsWith(BooksEndpoints.Collection, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(BookAccessEndpoints.Collection, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(BookAnalyticsEndpoints.Collection, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(BookAuditLogsEndpoints.Collection, StringComparison.OrdinalIgnoreCase);
    }

    private static bool StartsWithOperationPrefix(string? operationName, string prefix)
    {
        return operationName is not null
            && (operationName.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                || (operationName.Length > prefix.Length
                    && operationName[prefix.Length] == '.'
                    && operationName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));
    }

    private static string ExtractBookId(string relativePath)
    {
        var path = GetPathWithoutQuery(relativePath).Trim('/');
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2 && segments[0].Equals("books", StringComparison.OrdinalIgnoreCase))
        {
            return Uri.UnescapeDataString(segments[1]);
        }

        return ExtractQueryValue(relativePath, "bookId") ?? string.Empty;
    }

    private static string GetPathWithoutQuery(string relativePath)
    {
        var queryStart = relativePath.IndexOf('?', StringComparison.Ordinal);
        return queryStart < 0 ? relativePath : relativePath[..queryStart];
    }

    private static string? ExtractQueryValue(string relativePath, string key)
    {
        var queryStart = relativePath.IndexOf('?', StringComparison.Ordinal);
        if (queryStart < 0 || queryStart == relativePath.Length - 1)
        {
            return null;
        }

        var query = relativePath[(queryStart + 1)..];
        var value = query.Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(pair =>
            {
                var separatorIndex = pair.IndexOf('=', StringComparison.Ordinal);
                var candidateKey = separatorIndex < 0 ? pair : pair[..separatorIndex];
                return new
                {
                    CandidateKey = Uri.UnescapeDataString(candidateKey),
                    Value = separatorIndex < 0 ? string.Empty : pair[(separatorIndex + 1)..],
                };
            })
            .Where(candidate => candidate.CandidateKey.Equals(key, StringComparison.OrdinalIgnoreCase))
            .Select(candidate => Uri.UnescapeDataString(candidate.Value))
            .FirstOrDefault();
        if (value is not null)
        {
            return value;
        }

        return null;
    }
}
