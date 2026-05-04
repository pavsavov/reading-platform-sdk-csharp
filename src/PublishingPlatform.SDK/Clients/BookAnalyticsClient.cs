using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class BookAnalyticsClient : IBookAnalyticsClient
{
    private readonly ISharedHttpTransport _transport;

    internal BookAnalyticsClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }
}
