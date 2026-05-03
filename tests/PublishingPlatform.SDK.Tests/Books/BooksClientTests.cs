using PublishingPlatform.SDK.Clients;

namespace PublishingPlatform.SDK.Tests.Books;

public sealed class BooksClientTests
{
    [Fact]
    public void CanCreateBooksClient()
    {
        var client = new BooksClient();

        Assert.NotNull(client);
    }
}
