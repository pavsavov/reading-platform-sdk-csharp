using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class BookDistributionClient : IBookDistributionClient
{
    private readonly ISharedHttpTransport _transport;

    internal BookDistributionClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }
}
