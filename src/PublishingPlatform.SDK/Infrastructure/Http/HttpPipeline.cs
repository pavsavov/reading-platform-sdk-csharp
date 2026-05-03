namespace PublishingPlatform.SDK.Infrastructure.Http;

public sealed class HttpPipeline
{
    private readonly HttpClient _httpClient;

    public HttpPipeline(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return _httpClient.SendAsync(request, cancellationToken);
    }
}
