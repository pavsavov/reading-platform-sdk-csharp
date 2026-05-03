namespace PublishingPlatform.SDK.Abstractions;

public interface IPublishingPlatformClient
{
    IBooksClient Books { get; }

    IBookContentClient BookContent { get; }

    IBookPublishingClient BookPublishing { get; }

    IBookDistributionClient BookDistribution { get; }

    IBookAccessClient BookAccess { get; }

    IBookAnalyticsClient BookAnalytics { get; }

    IBookAuditLogsClient BookAuditLogs { get; }

    IBookAssetsClient BookAssets { get; }

    IWebhooksClient Webhooks { get; }
}
