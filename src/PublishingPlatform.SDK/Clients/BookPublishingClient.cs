using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class BookPublishingClient : IBookPublishingClient
{
    private readonly ISharedHttpTransport _transport;

    internal BookPublishingClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }
}
