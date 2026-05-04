using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients;

public sealed class BookAuditLogsClient : IBookAuditLogsClient
{
    private readonly ISharedHttpTransport _transport;

    internal BookAuditLogsClient(ISharedHttpTransport transport)
    {
        _transport = transport;
    }
}
