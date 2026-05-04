# reading-platform-sdk-csharp

A reference C# SDK for a reading platform, designed to simplify integration for external publishers and partners.

## Create a client (no host)

```csharp
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key"
}).Build();
```

## Opt-in resilience configuration

Resilience is disabled by default. Enable only the strategies you need.

```csharp
var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
    Resilience = new PublishingPlatformResilienceOptions
    {
        Enabled = true,
        Retry = new RetryResilienceOptions
        {
            Enabled = true,
            MaxRetryAttempts = 5,
            BaseDelay = TimeSpan.FromMilliseconds(250)
        },
        AttemptTimeout = new TimeoutResilienceOptions
        {
            Enabled = true,
            Timeout = TimeSpan.FromSeconds(5)
        }
    }
}).Build();
```

## ASP.NET Core registration

```csharp
using PublishingPlatform.SDK.Extensions;

builder.Services.AddPublishingPlatformClient(options =>
{
    options.BaseUrl = "https://api.books.example";
    options.ApiKey = builder.Configuration["PublishingPlatform:ApiKey"] ?? string.Empty;
});
```

## Local Skill Structure

This repository includes local Codex skills under `.codex/skills`.

These skills follow a consistent structure (front matter, scoped usage, workflow, validation, and guardrails) so they are predictable and reusable across SDK work.

The skill design and guidance patterns are based on concepts from the official .NET skills project: [dotnet/skills](https://github.com/dotnet/skills/).
