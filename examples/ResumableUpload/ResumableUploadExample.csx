#r "nuget: Microsoft.Extensions.Http, 9.0.1"
#r "nuget: Microsoft.Extensions.Http.Resilience, 9.1.0"
#r "nuget: Polly.Core, 8.4.2"
#r "../../src/PublishingPlatform.SDK/bin/Debug/net10.0/PublishingPlatform.SDK.dll"

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

var created = await client.Books.CreateAsync(new CreateBookRequest
{
    Title = "Resumable Upload Demo",
    Author = "SDK Candidate",
    Tags = new[] { "demo", "resumable" },
    IdempotencyKey = "resumable-book-create-1",
});

var payload = System.Text.Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz0123456789");
var session = await client.BookContent.StartResumableUploadAsync(created.Id, new StartResumableUploadRequest
{
    FileName = "resumable-demo.epub",
    ContentType = "application/epub+zip",
    Format = "epub",
    TotalBytes = payload.Length,
    IdempotencyKey = "resumable-start-1",
});

var firstChunk = payload.AsMemory(0, 18).ToArray();
await using (var firstStream = new MemoryStream(firstChunk))
{
    _ = await client.BookContent.UploadChunkAsync(created.Id, session.UploadSessionId, new UploadChunkRequest
    {
        Chunk = firstStream,
        ChunkStart = 0,
        ChunkEnd = firstChunk.Length - 1,
        TotalBytes = payload.Length,
    });
}

session = await client.BookContent.GetUploadSessionAsync(created.Id, session.UploadSessionId);
var secondStart = session.UploadedBytes;
var secondChunk = payload.AsMemory((int)secondStart, payload.Length - (int)secondStart).ToArray();
await using (var secondStream = new MemoryStream(secondChunk))
{
    _ = await client.BookContent.UploadChunkAsync(created.Id, session.UploadSessionId, new UploadChunkRequest
    {
        Chunk = secondStream,
        ChunkStart = secondStart,
        ChunkEnd = secondStart + secondChunk.Length - 1,
        TotalBytes = payload.Length,
    });
}

var completed = await client.BookContent.CompleteResumableUploadAsync(
    created.Id,
    session.UploadSessionId,
    new CompleteResumableUploadRequest
    {
        IdempotencyKey = "resumable-complete-1",
    });

Console.WriteLine($"Resumable upload completed for book {completed.BookId} with format {completed.Format}.");
