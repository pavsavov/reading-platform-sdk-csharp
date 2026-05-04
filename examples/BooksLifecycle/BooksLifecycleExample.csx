//r "nuget: PublishingPlatform.SDK"

using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Options;

var client = PublishingPlatformClientBuilder.Create(new PublishingPlatformClientOptions
{
    BaseUrl = "https://api.books.example",
    ApiKey = "your-api-key",
}).Build();

var created = await client.Books.CreateAsync(new CreateBookRequest
{
    Title = "The Pragmatic Programmer",
    Author = "Andy Hunt",
    Tags = new[] { "software", "craft" },
    IdempotencyKey = "books-create-1",
});

Console.WriteLine($"Created: {created.Id} - {created.Title}");

var listed = await client.Books.ListAsync(new ListBooksRequest
{
    SortBy = "title",
    Page = 0,
    PageSize = 10,
});

Console.WriteLine($"Page returned {listed.Items.Count} books.");

await client.Books.PatchMetadataAsync(created.Id, new UpdateBookPatchRequest
{
    Title = "The Pragmatic Programmer (20th Anniversary Edition)",
    ConcurrencyToken = created.ConcurrencyToken,
});

await client.Books.DeleteAsync(created.Id);
Console.WriteLine("Deleted created sample book.");
