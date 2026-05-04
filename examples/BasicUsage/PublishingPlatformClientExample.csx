#r "../../src/PublishingPlatform.SDK/bin/Debug/net10.0/PublishingPlatform.SDK.dll"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
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

Console.WriteLine(client.GetType().Name);
