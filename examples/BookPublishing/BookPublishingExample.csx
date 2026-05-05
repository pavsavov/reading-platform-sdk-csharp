//r "nuget: PublishingPlatform.SDK"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
}).Build();

var publishStatus = await client.BookPublishing.PublishAsync(
    "book-123",
    new PublishBookRequest
    {
        Notes = "Publishing after final editorial review.",
    },
    idempotencyKey: "publish-book-123-1");

Console.WriteLine($"Publish status: {publishStatus.Status}");

var scheduleStatus = await client.BookPublishing.ScheduleAsync(
    "book-123",
    new ScheduleBookPublishingRequest
    {
        ScheduledAt = DateTimeOffset.UtcNow.AddDays(2),
    },
    idempotencyKey: "schedule-book-123-1");

Console.WriteLine($"Scheduled at: {scheduleStatus.ScheduledAt:O}");

var currentStatus = await client.BookPublishing.GetStatusAsync("book-123");
Console.WriteLine($"Current publishing state: {currentStatus.Status}");

var unpublished = await client.BookPublishing.UnpublishAsync(
    "book-123",
    idempotencyKey: "unpublish-book-123-1");

Console.WriteLine($"After unpublish: {unpublished.Status}");
