using System.Net;
using System.Net.Http.Json;
using System.Text;
using Bogus;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.BookAnalytics;

public sealed class BookAnalyticsClientTests
{
    [Fact]
    public async Task GetSummaryAsync_SendsGetWithExpectedRouteAndCancellation()
    {
        Randomizer.Seed = new Random(53);
        var faker = new Faker();
        var token = new CancellationTokenSource().Token;
        var from = new DateTimeOffset(2026, 5, 1, 10, 30, 0, TimeSpan.FromHours(2));
        var to = new DateTimeOffset(2026, 5, 6, 9, 15, 0, TimeSpan.FromHours(2));
        var bookId = $"book {faker.Random.Number(10, 99)}+{faker.Random.Number(100, 999)}";
        var request = new GetBookAnalyticsRequest
        {
            BookId = bookId,
            From = from,
            To = to,
            Granularity = "week",
            IncludeUniqueReaders = false,
        };
        var expectedPath = BuildExpectedPath(bookId, from, to);
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Get,
            expectedPath,
            null,
            null,
            "BookAnalytics.GetSummary",
            token).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new AnalyticsSummary
                {
                    TotalViews = 120,
                    TotalDownloads = 35,
                    ActiveReaders = 18,
                }),
            }));
        BookAnalyticsClient sut = new BookAnalyticsClient(transport);

        var result = await sut.GetSummaryAsync(request, token);

        result.TotalViews.Should().Be(120);
        result.TotalDownloads.Should().Be(35);
        result.ActiveReaders.Should().Be(18);
        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            expectedPath,
            null,
            null,
            "BookAnalytics.GetSummary",
            token);
    }

    [Fact]
    public async Task GetSummaryAsync_ThrowsArgumentNullException_ForNullRequest()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        BookAnalyticsClient sut = new BookAnalyticsClient(transport);

        var act = async () => await sut.GetSummaryAsync(null!);

        _ = await act.Should().ThrowAsync<ArgumentNullException>();
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Fact]
    public async Task GetSummaryAsync_ThrowsBookValidationException_ForMissingRequiredFields()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        BookAnalyticsClient sut = new BookAnalyticsClient(transport);
        var request = new GetBookAnalyticsRequest
        {
            BookId = "book-1",
            From = null,
            To = DateTimeOffset.UtcNow,
        };

        var act = async () => await sut.GetSummaryAsync(request);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("From is required");
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Fact]
    public async Task GetSummaryAsync_ThrowsBookValidationException_ForMissingBookId()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        BookAnalyticsClient sut = new BookAnalyticsClient(transport);
        var request = new GetBookAnalyticsRequest
        {
            BookId = " ",
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow,
        };

        var act = async () => await sut.GetSummaryAsync(request);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book id is required");
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Fact]
    public async Task GetSummaryAsync_ThrowsBookValidationException_ForMissingTo()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        BookAnalyticsClient sut = new BookAnalyticsClient(transport);
        var request = new GetBookAnalyticsRequest
        {
            BookId = "book-1",
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = null,
        };

        var act = async () => await sut.GetSummaryAsync(request);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("To is required");
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Fact]
    public async Task GetSummaryAsync_ThrowsBookValidationException_ForInvalidRange()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        BookAnalyticsClient sut = new BookAnalyticsClient(transport);
        var request = new GetBookAnalyticsRequest
        {
            BookId = "book-1",
            From = DateTimeOffset.UtcNow,
            To = DateTimeOffset.UtcNow.AddDays(-2),
        };

        var act = async () => await sut.GetSummaryAsync(request);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("From must be less than or equal to To");
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Fact]
    public async Task GetSummaryAsync_ThrowsBookValidationException_WhenPayloadIsNull()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Get,
            Arg.Any<string>(),
            null,
            null,
            "BookAnalytics.GetSummary",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json"),
            }));
        BookAnalyticsClient sut = new BookAnalyticsClient(transport);
        var request = new GetBookAnalyticsRequest
        {
            BookId = "book-null",
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow,
        };

        var act = async () => await sut.GetSummaryAsync(request);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book analytics summary payload was empty");
    }

    private static string BuildExpectedPath(string bookId, DateTimeOffset from, DateTimeOffset to)
    {
        return $"/book-analytics/summary?bookId={Uri.EscapeDataString(bookId)}&from={Uri.EscapeDataString(from.ToString("O", System.Globalization.CultureInfo.InvariantCulture))}&to={Uri.EscapeDataString(to.ToString("O", System.Globalization.CultureInfo.InvariantCulture))}";
    }
}
