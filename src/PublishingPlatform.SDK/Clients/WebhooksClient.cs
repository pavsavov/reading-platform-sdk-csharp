using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class WebhooksClient : IWebhooksClient
{
    private readonly ISharedHttpTransport _transport;

    internal WebhooksClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }
}
