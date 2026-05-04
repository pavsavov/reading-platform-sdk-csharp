using PublishingPlatform.SDK.Infrastructure.Transport.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport.Requests;

namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal sealed class TransportDependencies
{
    private TransportDependencies(
        ITransportRequestFactory requestFactory,
        ITransportResponseErrorReader responseErrorReader,
        ITransportErrorContextFactory errorContextFactory)
    {
        RequestFactory = requestFactory;
        ResponseErrorReader = responseErrorReader;
        ErrorContextFactory = errorContextFactory;
    }

    public ITransportRequestFactory RequestFactory { get; }

    public ITransportResponseErrorReader ResponseErrorReader { get; }

    public ITransportErrorContextFactory ErrorContextFactory { get; }

    public static TransportDependencies CreateDefault()
    {
        return new TransportDependencies(
            new DefaultTransportRequestFactory(),
            new DefaultTransportResponseErrorReader(),
            new DefaultTransportErrorContextFactory());
    }
}
