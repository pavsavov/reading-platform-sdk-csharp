namespace PublishingPlatform.SDK.Infrastructure.Transport.Errors;

internal interface ITransportResponseErrorReader
{
    Task<NormalizedTransportError> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken);
}
