//r "nuget: PublishingPlatform.SDK"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

var baseUrl = Environment.GetEnvironmentVariable("MOCK_API_BASE_URL") ?? "https://localhost:7025";
var apiKey = Environment.GetEnvironmentVariable("MOCK_API_KEY") ?? "demo-nonsecret-key";

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = baseUrl,
    ApiKey = apiKey,
}).Build();

Console.WriteLine($"Using mock API: {baseUrl}");

var created = await client.Books.CreateAsync(new CreateBookRequest
{
    Title = "Interview Demo Book",
    Author = "SDK Candidate",
    Tags = new[] { "demo", "sdk" },
    IdempotencyKey = "books-create-demo-1",
});
Console.WriteLine($"Created book: {created.Id}");

using var contentStream = new MemoryStream("sample content for demo"u8.ToArray());
var content = await client.BookContent.UploadOrReplaceAsync(
    created.Id,
    new UploadBookContentRequest
    {
        File = contentStream,
        FileName = "interview-demo.epub",
        ContentType = "application/epub+zip",
        Format = "epub",
        IdempotencyKey = "content-upload-demo-1",
    });
Console.WriteLine($"Uploaded content format: {content.Format}");

var publish = await client.BookPublishing.PublishAsync(
    created.Id,
    new PublishBookRequest
    {
        Notes = "Publish for interview showcase.",
    },
    idempotencyKey: "publish-demo-1");
Console.WriteLine($"Publishing status: {publish.Status}");

var distribution = await client.BookDistribution.StartAsync(
    created.Id,
    new StartBookDistributionRequest
    {
        Channels = new[] { "mobile", "partner-store" },
    },
    idempotencyKey: "distribution-start-demo-1");
Console.WriteLine($"Distribution operation: {distribution.OperationId} ({distribution.Status})");

var distributionStatus = await client.BookDistribution.GetStatusAsync(created.Id, distribution.OperationId);
Console.WriteLine($"Distribution status: {distributionStatus.Status}");

var registered = await client.Webhooks.RegisterAsync(
    new RegisterWebhookRequest
    {
        EndpointUrl = "https://hooks.example.com/interview-sdk",
        Events = new[] { "book.published", "book.distribution.completed" },
        IsActive = true,
        SigningKeyId = "signing-key-demo",
    },
    idempotencyKey: "webhook-register-demo-1");
Console.WriteLine($"Registered webhook: {registered.Id}");

var page = await client.Webhooks.ListAsync(new ListWebhooksRequest
{
    PageSize = 20,
});
Console.WriteLine($"Webhook count from API: {page.TotalCount}");

var updated = await client.Webhooks.UpdateAsync(new UpdateWebhookRequest
{
    WebhookId = registered.Id,
    EndpointUrl = "https://hooks.example.com/interview-sdk-updated",
    Events = new[] { "book.published" },
    IsActive = true,
});
Console.WriteLine($"Updated webhook endpoint: {updated.EndpointUrl}");

await client.Webhooks.DeleteAsync(updated.Id);
Console.WriteLine("Deleted webhook.");

Console.WriteLine("Mock API end-to-end SDK showcase completed.");
