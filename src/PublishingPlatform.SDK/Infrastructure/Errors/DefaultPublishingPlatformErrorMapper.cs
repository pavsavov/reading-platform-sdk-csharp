using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Exceptions;

namespace PublishingPlatform.SDK.Infrastructure.Errors;

internal sealed class DefaultPublishingPlatformErrorMapper : IPublishingPlatformErrorMapper
{
    public Exception Map(PublishingPlatformErrorContext context)
    {
        if (context.RelativePath.StartsWith("/books", StringComparison.OrdinalIgnoreCase))
        {
            if (context.StatusCode == 404)
            {
                return new BookNotFoundException(ExtractBookId(context.RelativePath), context.Message);
            }

            if (context.StatusCode is 409 or 412)
            {
                return new BookConflictException(ExtractBookId(context.RelativePath), context.Message);
            }

            if (context.StatusCode == 429)
            {
                return new BookRateLimitedException(context.Message);
            }
        }

        return ErrorMapper.Map(context.StatusCode, context.Message);
    }

    private static string ExtractBookId(string relativePath)
    {
        var path = relativePath.Trim('/');
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return string.Empty;
        }

        return segments[1];
    }
}
