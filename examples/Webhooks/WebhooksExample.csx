//r "nuget: PublishingPlatform.SDK"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
}).Build();

var webhook = await client.Webhooks.RegisterAsync(
    new RegisterWebhookRequest
    {
        EndpointUrl = "https://hooks.example.com/publishing-platform",
        Events = ["book.published", "book.distribution.completed"],
        IsActive = true,
        SigningKeyId = "signing-key-01",
    },
    idempotencyKey: "webhook-registration-001");

Console.WriteLine($"Registered webhook: {webhook.Id}");

var updated = await client.Webhooks.UpdateAsync(new UpdateWebhookRequest
{
    WebhookId = webhook.Id,
    EndpointUrl = webhook.EndpointUrl,
    Events = ["book.published", "book.distribution.failed"],
    IsActive = true,
});

Console.WriteLine($"Updated webhook events: {string.Join(", ", updated.Events)}");

var page = await client.Webhooks.ListAsync(new ListWebhooksRequest
{
    Event = "book.published",
    IsActive = true,
});

Console.WriteLine($"Active publishing webhooks: {page.Items.Count}");

await client.Webhooks.DeleteAsync(webhook.Id);
