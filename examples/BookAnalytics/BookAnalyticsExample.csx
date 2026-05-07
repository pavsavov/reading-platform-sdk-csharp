//r "nuget: PublishingPlatform.SDK"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
}).Build();

var from = DateTimeOffset.UtcNow.AddDays(-7);
var to = DateTimeOffset.UtcNow;

var summary = await client.BookAnalytics.GetSummaryAsync(new GetBookAnalyticsRequest
{
    BookId = "book-123",
    From = from,
    To = to,
    Granularity = "day",
    IncludeUniqueReaders = true,
});

Console.WriteLine($"Summary window: {from:O} -> {to:O}");
Console.WriteLine($"Total views: {summary.TotalViews}");
Console.WriteLine($"Total downloads: {summary.TotalDownloads}");
Console.WriteLine($"Active readers: {summary.ActiveReaders}");
