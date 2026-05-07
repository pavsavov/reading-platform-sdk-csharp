//r "nuget: PublishingPlatform.SDK"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = Environment.GetEnvironmentVariable("PUBLISHING_PLATFORM_API_KEY") ?? string.Empty,
    Diagnostics = new DiagnosticsOptions
    {
        EnableTracing = true,
        EnableMetrics = true,
        EnableLogging = false
    },
    Resilience = new PublishingPlatformResilienceOptions
    {
        Enabled = true,
        Retry = new RetryResilienceOptions
        {
            Enabled = true,
            MaxRetryAttempts = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            RetryNonIdempotentMethods = false,
            UseJitter = true
        }
    }
}).Build();

var book = await client.Books.GetByIdAsync("book-123");
Console.WriteLine($"Loaded book: {book.Title}");

var publishStatus = await client.BookPublishing.PublishAsync(
    book.Id,
    new PublishBookRequest { Notes = "Release approved by editorial workflow." },
    idempotencyKey: $"publish-{book.Id}-2026-05");

Console.WriteLine($"Publishing status: {publishStatus.Status}");

var distribution = await client.BookDistribution.StartAsync(
    book.Id,
    new StartBookDistributionRequest
    {
        Channels = ["mobile", "partner-store"],
    },
    idempotencyKey: $"distribution-{book.Id}-2026-05");

Console.WriteLine($"Distribution operation: {distribution.OperationId}");
