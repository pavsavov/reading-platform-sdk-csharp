#r "../../src/PublishingPlatform.SDK/bin/Debug/net10.0/PublishingPlatform.SDK.dll"
#r "nuget: Microsoft.Extensions.DependencyInjection, 9.0.0"

using Microsoft.Extensions.DependencyInjection;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Extensions;

var services = new ServiceCollection();

services.AddPublishingPlatformClient(options =>
{
    options.BaseUrl = "https://api.books.example";
    options.ApiKey = "demo-key";
    options.Timeout = TimeSpan.FromSeconds(30);
});

using var provider = services.BuildServiceProvider();
var platformClient = provider.GetRequiredService<IPublishingPlatformClient>();

var books = platformClient.Books;
var content = platformClient.BookContent;
var publishing = platformClient.BookPublishing;
var webhooks = platformClient.Webhooks;

Console.WriteLine($"Client initialized through DI: {platformClient.GetType().Name}");
Console.WriteLine($"Modules: {nameof(books)}, {nameof(content)}, {nameof(publishing)}, {nameof(webhooks)}");
