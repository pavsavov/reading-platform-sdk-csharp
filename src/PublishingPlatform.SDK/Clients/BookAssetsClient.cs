using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class BookAssetsClient : IBookAssetsClient
{
    private readonly ISharedHttpTransport _transport;

    internal BookAssetsClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }
}
