using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Errors;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class ErrorMappingTests
{
    [Fact]
    public void Map_ReturnsBookNotFoundException_WithNormalizedMetadata()
    {
        var mapper = new DefaultPublishingPlatformErrorMapper();
        var context = CreateContext(404, "/books/book-27", "Books.GetById");

        var exception = mapper.Map(context);

        var typed = exception.Should().BeOfType<BookNotFoundException>().Subject;
        typed.BookId.Should().Be("book-27");
        AssertApiMetadata(typed, context);
    }

    [Theory]
    [InlineData(409)]
    [InlineData(412)]
    public void Map_ReturnsBookConflictException_ForConflictStatuses(int statusCode)
    {
        var mapper = new DefaultPublishingPlatformErrorMapper();
        var context = CreateContext(statusCode, "/books/book-27/publishing/publish", "BookPublishing.Publish");

        var exception = mapper.Map(context);

        var typed = exception.Should().BeOfType<BookConflictException>().Subject;
        typed.BookId.Should().Be("book-27");
        AssertApiMetadata(typed, context);
    }

    [Fact]
    public void Map_ReturnsBookRateLimitedException_ForBookCentricRateLimit()
    {
        var mapper = new DefaultPublishingPlatformErrorMapper();
        var context = CreateContext(429, "/book-analytics/summary?bookId=book-27", "BookAnalytics.GetSummary");

        var exception = mapper.Map(context);

        var typed = exception.Should().BeOfType<BookRateLimitedException>().Subject;
        AssertApiMetadata(typed, context);
    }

    [Fact]
    public void Map_ExtractsBookId_FromQueryStringForBookCentricModules()
    {
        var mapper = new DefaultPublishingPlatformErrorMapper();
        var context = CreateContext(404, "/book-audit-logs?page=1&bookId=book%2042", "BookAuditLogs.List");

        var exception = mapper.Map(context);

        exception.Should().BeOfType<BookNotFoundException>()
            .Subject.BookId.Should().Be("book 42");
    }

    [Fact]
    public void Map_ReturnsGenericApiException_ForBookCentricUnknownStatus()
    {
        var mapper = new DefaultPublishingPlatformErrorMapper();
        var context = CreateContext(500, "/books/book-27", "Books.GetById");

        var exception = mapper.Map(context);

        var typed = exception.Should().BeOfType<ApiException>().Subject;
        AssertApiMetadata(typed, context);
    }

    [Fact]
    public void Map_ReturnsGenericApiException_ForWebhooksEvenWhenPathContainsBooks()
    {
        var mapper = new DefaultPublishingPlatformErrorMapper();
        var context = CreateContext(404, "/webhooks/books", "Webhooks.List");

        var exception = mapper.Map(context);

        exception.Should().BeOfType<ApiException>();
    }

    private static PublishingPlatformErrorContext CreateContext(
        int statusCode,
        string relativePath,
        string operationName)
    {
        return new PublishingPlatformErrorContext
        {
            Method = HttpMethod.Get,
            RelativePath = relativePath,
            StatusCode = statusCode,
            Message = "normalized message",
            ErrorCode = "book.failure",
            RequestId = "req-27",
            CorrelationId = "corr-27",
            OperationName = operationName,
        };
    }

    private static void AssertApiMetadata(PublishingPlatformApiException exception, PublishingPlatformErrorContext context)
    {
        exception.StatusCode.Should().Be(context.StatusCode);
        exception.Message.Should().Be(context.Message);
        exception.ErrorCode.Should().Be(context.ErrorCode);
        exception.RequestId.Should().Be(context.RequestId);
        exception.CorrelationId.Should().Be(context.CorrelationId);
        exception.OperationName.Should().Be(context.OperationName);
        exception.Method.Should().Be(context.Method);
        exception.RelativePath.Should().Be(context.RelativePath);
    }
}
