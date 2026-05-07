# Auth

The SDK authenticates outbound requests with the API key supplied through `PublishingPlatformClientOptions`.

Use environment variables or a secret manager in real applications. Do not commit real API keys in source code, example scripts, or CI configuration.

```csharp
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = Environment.GetEnvironmentVariable("PUBLISHING_PLATFORM_API_KEY") ?? string.Empty,
}).Build();

var page = await client.Books.ListAsync(new() { Page = 0, PageSize = 10 });
```

Authentication behavior:

- The SDK rejects empty API keys during client creation.
- The SDK rejects non-HTTPS base URLs before any HTTP request is sent.
- The configured API key is applied internally to outbound SDK requests.
