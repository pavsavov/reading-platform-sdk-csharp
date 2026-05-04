using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class PublishingPlatformClient : IPublishingPlatformClient
{
    public PublishingPlatformClient(
        IBooksClient books,
        IBookContentClient bookContent,
        IBookPublishingClient bookPublishing,
        IBookDistributionClient bookDistribution,
        IBookAccessClient bookAccess,
        IBookAnalyticsClient bookAnalytics,
        IBookAuditLogsClient bookAuditLogs,
        IBookAssetsClient bookAssets,
        IWebhooksClient webhooks)
    {
        Books = books;
        BookContent = bookContent;
        BookPublishing = bookPublishing;
        BookDistribution = bookDistribution;
        BookAccess = bookAccess;
        BookAnalytics = bookAnalytics;
        BookAuditLogs = bookAuditLogs;
        BookAssets = bookAssets;
        Webhooks = webhooks;
    }

    public IBooksClient Books { get; }

    public IBookContentClient BookContent { get; }

    public IBookPublishingClient BookPublishing { get; }

    public IBookDistributionClient BookDistribution { get; }

    public IBookAccessClient BookAccess { get; }

    public IBookAnalyticsClient BookAnalytics { get; }

    public IBookAuditLogsClient BookAuditLogs { get; }

    public IBookAssetsClient BookAssets { get; }

    public IWebhooksClient Webhooks { get; }

    internal static PublishingPlatformClient CreateFromTransport(ISharedHttpTransport transport)
    {
        return new PublishingPlatformClient(
            new BooksClient(transport),
            new BookContentClient(transport),
            new BookPublishingClient(transport),
            new BookDistributionClient(transport),
            new BookAccessClient(transport),
            new BookAnalyticsClient(transport),
            new BookAuditLogsClient(transport),
            new BookAssetsClient(transport),
            new WebhooksClient(transport));
    }
}
