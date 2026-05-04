#r "../../src/PublishingPlatform.SDK/bin/Debug/net10.0/PublishingPlatform.SDK.dll"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Options;

var clientWithIdempotentRetriesOnly = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "demo-key",
    Resilience = new PublishingPlatformResilienceOptions
    {
        Enabled = true,
        Retry = new RetryResilienceOptions
        {
            Enabled = true,
            MaxRetryAttempts = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            RetryNonIdempotentMethods = false
        }
    }
}).Build();

var clientWithPostPatchRetries = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "demo-key",
    Resilience = new PublishingPlatformResilienceOptions
    {
        Enabled = true,
        Retry = new RetryResilienceOptions
        {
            Enabled = true,
            MaxRetryAttempts = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            RetryNonIdempotentMethods = true
        }
    }
}).Build();

Console.WriteLine(clientWithIdempotentRetriesOnly.GetType().Name);
Console.WriteLine(clientWithPostPatchRetries.GetType().Name);
