#r "../../src/PublishingPlatform.SDK/bin/Debug/net10.0/PublishingPlatform.SDK.dll"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Options;

var platformClient = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "demo-key",
    Timeout = TimeSpan.FromSeconds(30),
}).Build();

var books = platformClient.Books;
var content = platformClient.BookContent;
var publishing = platformClient.BookPublishing;
var webhooks = platformClient.Webhooks;

Console.WriteLine($"Client initialized: {platformClient.GetType().Name}");
Console.WriteLine($"Modules: {nameof(books)}, {nameof(content)}, {nameof(publishing)}, {nameof(webhooks)}");
