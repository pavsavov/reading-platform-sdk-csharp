using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Infrastructure.Http.Handlers;

/// <summary>
/// Adds the configured backend API key to outbound SDK HTTP requests.
/// </summary>
internal sealed class ApiKeyAuthHandler : DelegatingHandler
{
    private readonly string _apiKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyAuthHandler"/> class.
    /// </summary>
    /// <param name="apiKey">The backend API key from SDK configuration.</param>
    internal ApiKeyAuthHandler(string apiKey)
    {
        _apiKey = apiKey;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove(TransportHeaderNames.ApiKey);
        request.Headers.TryAddWithoutValidation(TransportHeaderNames.ApiKey, _apiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
