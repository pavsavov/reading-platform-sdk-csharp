using System.Text;
using Bogus;
using PublishingPlatform.SDK.Clients.BookAnalytics.Serialization;
using PublishingPlatform.SDK.Clients.BookAnalytics.Validation;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.BookAnalytics;

public sealed class BookAnalyticsCollaboratorsTests
{
    [Fact]
    public void Validator_Throws_WhenBookIdMissing()
    {
        var validator = new DefaultBookAnalyticsRequestValidator();
        var request = new GetBookAnalyticsRequest
        {
            BookId = " ",
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow,
        };

        Action act = () => validator.ValidateGetSummary(request);

        act.Should().Throw<BookValidationException>()
            .WithMessage("*Book id is required*");
    }

    [Fact]
    public void Validator_Throws_WhenToMissing()
    {
        var validator = new DefaultBookAnalyticsRequestValidator();
        var request = new GetBookAnalyticsRequest
        {
            BookId = "book-1",
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = null,
        };

        Action act = () => validator.ValidateGetSummary(request);

        act.Should().Throw<BookValidationException>()
            .WithMessage("*To is required*");
    }

    [Fact]
    public void Validator_Throws_WhenRangeIsInvalid()
    {
        var validator = new DefaultBookAnalyticsRequestValidator();
        var request = new GetBookAnalyticsRequest
        {
            BookId = "book-1",
            From = DateTimeOffset.UtcNow,
            To = DateTimeOffset.UtcNow.AddDays(-1),
        };

        Action act = () => validator.ValidateGetSummary(request);

        act.Should().Throw<BookValidationException>()
            .WithMessage("*From must be less than or equal to To*");
    }

    [Fact]
    public void QueryBuilder_BuildsDeterministicPath_AndOmitsFutureFacingFields()
    {
        Randomizer.Seed = new Random(54);
        var faker = new Faker();
        var builder = new DefaultBookAnalyticsQueryStringBuilder();
        var from = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 5, 7, 23, 59, 59, TimeSpan.Zero);
        var request = new GetBookAnalyticsRequest
        {
            BookId = $"book {faker.Random.Number(10, 99)}",
            From = from,
            To = to,
            Granularity = "month",
            IncludeUniqueReaders = false,
        };

        var path = builder.BuildSummaryPath(request);

        path.Should().StartWith("/book-analytics/summary?");
        path.Should().Contain($"bookId={Uri.EscapeDataString(request.BookId)}");
        path.Should().Contain($"from={Uri.EscapeDataString(from.ToString("O", System.Globalization.CultureInfo.InvariantCulture))}");
        path.Should().Contain($"to={Uri.EscapeDataString(to.ToString("O", System.Globalization.CultureInfo.InvariantCulture))}");
        path.Should().NotContain("granularity=");
        path.Should().NotContain("includeUniqueReaders=");
    }

    [Fact]
    public async Task ResponseReader_Throws_WhenPayloadIsNull()
    {
        var reader = new DefaultBookAnalyticsResponseReader();
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        };

        Func<Task> act = async () => await reader.ReadSummaryAsync(response, CancellationToken.None);

        _ = await act.Should().ThrowAsync<BookValidationException>()
            .WithMessage("*payload was empty*");
    }
}
