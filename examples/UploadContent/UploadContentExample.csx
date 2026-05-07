//r "nuget: PublishingPlatform.SDK"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
}).Build();

await using var file = File.OpenRead("book.epub");

var content = await client.BookContent.UploadOrReplaceAsync(
    "book-123",
    new UploadBookContentRequest
    {
        File = file,
        FileName = "book.epub",
        ContentType = "application/epub+zip",
        Format = "epub",
        IdempotencyKey = "content-upload-book-123-1",
    });

Console.WriteLine($"Uploaded content format: {content.Format}");
