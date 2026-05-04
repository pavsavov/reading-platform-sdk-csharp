using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class BooksClient : IBooksClient
{
    private readonly ISharedHttpTransport _transport;

    internal BooksClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }
}
