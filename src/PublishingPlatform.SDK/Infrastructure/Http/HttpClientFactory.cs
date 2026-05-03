namespace PublishingPlatform.SDK.Infrastructure.Http;

public static class HttpClientFactory
{
    public static HttpClient Create(string baseUrl, TimeSpan timeout)
    {
        return new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            Timeout = timeout,
        };
    }
}
