namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal interface ISharedHttpTransport
{
    Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePath,
        HttpContent? content,
        CancellationToken cancellationToken = default);
}
