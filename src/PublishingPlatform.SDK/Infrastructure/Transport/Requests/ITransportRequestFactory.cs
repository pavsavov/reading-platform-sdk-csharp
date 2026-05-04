namespace PublishingPlatform.SDK.Infrastructure.Transport.Requests;

internal interface ITransportRequestFactory
{
    HttpRequestMessage Create(
        HttpMethod method,
        string relativePath,
        HttpContent? content,
        string correlationId,
        IReadOnlyDictionary<string, string>? headers = null);
}
