using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

/// <summary>
/// Provides orchestration-only access to all Publishing Platform SDK book modules.
/// </summary>
public sealed class PublishingPlatformClient : IPublishingPlatformClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishingPlatformClient"/> class.
    /// </summary>
    /// <param name="books">The books module client.</param>
    /// <param name="bookContent">The book content module client.</param>
    /// <param name="bookPublishing">The book publishing module client.</param>
    /// <param name="bookDistribution">The book distribution module client.</param>
    /// <param name="bookAccess">The book access module client.</param>
    /// <param name="bookAnalytics">The book analytics module client.</param>
    /// <param name="bookAuditLogs">The book audit logs module client.</param>
    /// <param name="webhooks">The webhooks module client.</param>
    public PublishingPlatformClient(
        IBooksClient books,
        IBookContentClient bookContent,
        IBookPublishingClient bookPublishing,
        IBookDistributionClient bookDistribution,
        IBookAccessClient bookAccess,
        IBookAnalyticsClient bookAnalytics,
        IBookAuditLogsClient bookAuditLogs,
        IWebhooksClient webhooks)
    {
        Books = books;
        BookContent = bookContent;
        BookPublishing = bookPublishing;
        BookDistribution = bookDistribution;
        BookAccess = bookAccess;
        BookAnalytics = bookAnalytics;
        BookAuditLogs = bookAuditLogs;
        Webhooks = webhooks;
    }

    public IBooksClient Books { get; }

    public IBookContentClient BookContent { get; }

    public IBookPublishingClient BookPublishing { get; }

    public IBookDistributionClient BookDistribution { get; }

    public IBookAccessClient BookAccess { get; }

    public IBookAnalyticsClient BookAnalytics { get; }

    public IBookAuditLogsClient BookAuditLogs { get; }

    public IWebhooksClient Webhooks { get; }

    /// <summary>
    /// Creates a fully-wired root client using a shared transport.
    /// </summary>
    /// <param name="transport">The shared transport used by all module clients.</param>
    /// <returns>A concrete <see cref="PublishingPlatformClient"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transport"/> is <see langword="null"/>.</exception>
    internal static PublishingPlatformClient CreateFromTransport(ISharedHttpTransport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);

        return new PublishingPlatformClient(
            new BooksClient(transport),
            new BookContentClient(transport),
            new BookPublishingClient(transport),
            new BookDistributionClient(transport),
            new BookAccessClient(transport),
            new BookAnalyticsClient(transport),
            new BookAuditLogsClient(transport),
            new WebhooksClient(transport));
    }
}
