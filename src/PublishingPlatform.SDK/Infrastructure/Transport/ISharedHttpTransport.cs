using System.Diagnostics.CodeAnalysis;

namespace PublishingPlatform.SDK.Infrastructure.Transport;

internal interface ISharedHttpTransport
{
    [SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "moduleName is optional metadata for diagnostics and preserved to avoid broad callsite churn.")]
    Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePath,
        HttpContent? content,
        IReadOnlyDictionary<string, string>? headers = null,
        string? operationName = null,
        CancellationToken cancellationToken = default,
        string? moduleName = null);
}
