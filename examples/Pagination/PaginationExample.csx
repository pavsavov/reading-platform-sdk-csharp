using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.example.com",
    ApiKey = Environment.GetEnvironmentVariable("PUBLISHING_PLATFORM_API_KEY") ?? string.Empty,
}).Build();

var booksPage = await client.Books.ListAsync(new ListBooksRequest
{
    SortBy = "title",
    PageSize = 20,
});
Console.WriteLine($"Books page count: {booksPage.Items.Count}");

await foreach (var book in client.Books.ListAllAsync(new ListBooksRequest
{
    SortBy = "title",
    PageSize = 20,
}))
{
    Console.WriteLine($"Book: {book.Id}");
}

await foreach (var grant in client.BookAccess.ListAllAsync(new ListBookAccessRequest
{
    BookId = "book-1",
    PageSize = 50,
}))
{
    Console.WriteLine($"Access grant: {grant.GrantId}");
}

await foreach (var auditLog in client.BookAuditLogs.ListAllAsync(new ListBookAuditLogsRequest
{
    BookId = "book-1",
    Page = 0,
    PageSize = 100,
}))
{
    Console.WriteLine($"Audit log: {auditLog.Id}");
}

await foreach (var webhook in client.Webhooks.ListAllAsync(new ListWebhooksRequest
{
    IsActive = true,
    PageSize = 50,
}))
{
    Console.WriteLine($"Webhook: {webhook.Id}");
}
