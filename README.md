# reading-platform-sdk-csharp

A reference C# SDK for a reading platform, designed to simplify integration for external publishers and partners.

## Configure the client

`PublishingPlatformClientOptions` is the central configuration object.

```csharp
var options = new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
    Timeout = TimeSpan.FromSeconds(30),

    // Optional: resilience is disabled by default.
    Resilience = new PublishingPlatformResilienceOptions
    {
        Enabled = true,
        Retry = new RetryResilienceOptions
        {
            Enabled = true,
            MaxRetryAttempts = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200)
        },
        CircuitBreaker = new CircuitBreakerResilienceOptions
        {
            Enabled = true,
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(30)
        }
    }
};
```

## Choose a configuration style

- Use the Options pattern in hosted apps (ASP.NET Core, worker services) when configuration comes from `appsettings`, environment variables, or secrets providers and you want startup-time validation.
- Use direct POCO/builder configuration in non-hosted apps, scripts, tests, or quick integrations where you construct options in code.

## Create a client (no host, direct POCO)

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

## ASP.NET Core registration (Options pattern)

Option 1: bind and validate via `IOptions<PublishingPlatformClientOptions>`.

```csharp
using PublishingPlatform.SDK.Extensions;
using PublishingPlatform.SDK.Options;

builder.Services.Configure<PublishingPlatformClientOptions>(builder.Configuration.GetSection("PublishingPlatform"));
builder.Services.AddPublishingPlatformClient();
```

Option 2: keep using direct configure callback.

```csharp
using PublishingPlatform.SDK.Extensions;

builder.Services.AddPublishingPlatformClient(options =>
{
    options.BaseUrl = "https://api.books.example";
    options.ApiKey = builder.Configuration["PublishingPlatform:ApiKey"] ?? string.Empty;
});
```

### Optional: custom error mapper

Use a custom mapper to throw domain-specific exceptions with request context.

```csharp
using PublishingPlatform.SDK.Abstractions;

public sealed class PublishingErrorMapper : IPublishingPlatformErrorMapper
{
    public Exception Map(PublishingPlatformErrorContext context)
    {
        if (context.StatusCode == 404 && context.RelativePath.StartsWith("/books/", StringComparison.Ordinal))
        {
            return new InvalidOperationException($"Book was not found. Path: {context.RelativePath}");
        }

        return new Exception($"API call failed ({context.StatusCode}) for {context.Method} {context.RelativePath}: {context.Message}");
    }
}

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
    ErrorMapper = new PublishingErrorMapper()
}).Build();
```

## Local Skill Structure

This repository includes local Codex skills under `.codex/skills`.

These skills follow a consistent structure (front matter, scoped usage, workflow, validation, and guardrails) so they are predictable and reusable across SDK work.

The skill design and guidance patterns are based on concepts from the official .NET skills project: [dotnet/skills](https://github.com/dotnet/skills/).
