using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class BookContentClient : IBookContentClient
{
    private readonly ISharedHttpTransport _transport;

    internal BookContentClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }
}
