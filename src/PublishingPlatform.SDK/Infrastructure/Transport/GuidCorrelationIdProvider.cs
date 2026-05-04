namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal sealed class GuidCorrelationIdProvider : ICorrelationIdProvider
{
    public string Create()
    {
        return Guid.NewGuid().ToString("N");
    }
}
