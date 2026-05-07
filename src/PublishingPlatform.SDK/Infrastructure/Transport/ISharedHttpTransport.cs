namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal interface ISharedHttpTransport
{
    Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePath,
        HttpContent? content,
        IReadOnlyDictionary<string, string>? headers = null,
        string? operationName = null,
        CancellationToken cancellationToken = default,
        string? moduleName = null);
}
