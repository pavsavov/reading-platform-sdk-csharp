namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal interface ICorrelationIdProvider
{
    string Create();
}
