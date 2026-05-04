using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class BookAccessClient : IBookAccessClient
{
    private readonly ISharedHttpTransport _transport;

    internal BookAccessClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }
}
