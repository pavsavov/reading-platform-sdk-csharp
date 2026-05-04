namespace PublishingPlatform.SDK.Infrastructure.Transport.Errors;

internal interface ITransportResponseErrorReader
{
    Task<string> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken);
}
