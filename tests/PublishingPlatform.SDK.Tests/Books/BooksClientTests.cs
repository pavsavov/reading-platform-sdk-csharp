using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Tests.Books;

public sealed class BooksClientTests
{
    [Fact]
    public void BuildClient_ReturnsClientWithAllBookCentricModules()
    {
        var options = new PublishingPlatformClientOptions
        {
            BaseUrl = "https://example.test",
            ApiKey = "test-key",
        };

        var client = PublishingPlatformClientBuilder.Create(options).Build();

        Assert.NotNull(client.Books);
        Assert.NotNull(client.BookContent);
        Assert.NotNull(client.BookPublishing);
        Assert.NotNull(client.BookDistribution);
        Assert.NotNull(client.BookAccess);
        Assert.NotNull(client.BookAnalytics);
        Assert.NotNull(client.BookAuditLogs);
        Assert.NotNull(client.BookAssets);
        Assert.NotNull(client.Webhooks);
    }
}
