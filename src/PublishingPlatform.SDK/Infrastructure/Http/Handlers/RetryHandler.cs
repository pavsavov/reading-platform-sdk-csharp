namespace PublishingPlatform.SDK.Infrastructure.Http.Handlers;

public sealed class RetryHandler : DelegatingHandler
{
    private readonly int _maxRetries;

    public RetryHandler(int maxRetries = 2)
    {
        _maxRetries = maxRetries;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode || attempt >= _maxRetries)
            {
                return response;
            }
        }
    }
}
