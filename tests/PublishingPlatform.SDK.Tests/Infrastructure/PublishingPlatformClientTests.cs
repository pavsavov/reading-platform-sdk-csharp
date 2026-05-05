using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class PublishingPlatformClientTests
{
    [Fact]
    public void Constructor_AssignsModules_WhenAllDependenciesProvided()
    {
        var books = Substitute.For<IBooksClient>();
        var bookContent = Substitute.For<IBookContentClient>();
        var bookPublishing = Substitute.For<IBookPublishingClient>();
        var bookDistribution = Substitute.For<IBookDistributionClient>();
        var bookAccess = Substitute.For<IBookAccessClient>();
        var bookAnalytics = Substitute.For<IBookAnalyticsClient>();
        var bookAuditLogs = Substitute.For<IBookAuditLogsClient>();
        var bookAssets = Substitute.For<IBookAssetsClient>();
        var webhooks = Substitute.For<IWebhooksClient>();

        var client = new PublishingPlatformClient(
            books,
            bookContent,
            bookPublishing,
            bookDistribution,
            bookAccess,
            bookAnalytics,
            bookAuditLogs,
            bookAssets,
            webhooks);

        client.Books.Should().BeSameAs(books);
        client.BookContent.Should().BeSameAs(bookContent);
        client.BookPublishing.Should().BeSameAs(bookPublishing);
        client.BookDistribution.Should().BeSameAs(bookDistribution);
        client.BookAccess.Should().BeSameAs(bookAccess);
        client.BookAnalytics.Should().BeSameAs(bookAnalytics);
        client.BookAuditLogs.Should().BeSameAs(bookAuditLogs);
        client.BookAssets.Should().BeSameAs(bookAssets);
        client.Webhooks.Should().BeSameAs(webhooks);
    }

    [Fact]
    public void CreateFromTransport_ThrowsArgumentNullException_WhenTransportIsNull()
    {
        Action act = () => PublishingPlatformClient.CreateFromTransport(null!);

        var exception = act.Should().Throw<ArgumentNullException>().Which;
        exception.ParamName.Should().Be("transport");
    }

    [Fact]
    public void CreateFromTransport_ReturnsClientWithAllModules_WhenTransportProvided()
    {
        var transport = Substitute.For<ISharedHttpTransport>();

        var client = PublishingPlatformClient.CreateFromTransport(transport);

        client.Should().NotBeNull();
        client.Books.Should().NotBeNull();
        client.BookContent.Should().NotBeNull();
        client.BookPublishing.Should().NotBeNull();
        client.BookDistribution.Should().NotBeNull();
        client.BookAccess.Should().NotBeNull();
        client.BookAnalytics.Should().NotBeNull();
        client.BookAuditLogs.Should().NotBeNull();
        client.BookAssets.Should().NotBeNull();
        client.Webhooks.Should().NotBeNull();
    }
}
