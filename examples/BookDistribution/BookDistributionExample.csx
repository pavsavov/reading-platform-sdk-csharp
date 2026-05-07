//r "nuget: PublishingPlatform.SDK"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
}).Build();

var start = await client.BookDistribution.StartAsync(
    "book-123",
    new StartBookDistributionRequest
    {
        Channels = ["mobile", "partner-store"],
    },
    idempotencyKey: "distribution-book-123-1");

Console.WriteLine($"Distribution operation id: {start.OperationId}, status: {start.Status}");

var status = await client.BookDistribution.GetStatusAsync("book-123", start.OperationId);
Console.WriteLine($"Distribution status: {status.Status}");

var retry = await client.BookDistribution.RetryAsync(
    "book-123",
    start.OperationId,
    idempotencyKey: "distribution-retry-book-123-1");

Console.WriteLine($"Retry status: {retry.Status}");

var history = await client.BookDistribution.ListAsync("book-123");
Console.WriteLine($"Tracked distribution operations: {history.Operations.Count}");
