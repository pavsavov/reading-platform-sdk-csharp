namespace PublishingPlatform.SDK.Infrastructure.Http.Handlers;

public sealed class CorrelationHandler : DelegatingHandler
{
    public const string CorrelationHeader = "X-Correlation-Id";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(CorrelationHeader))
        {
            request.Headers.Add(CorrelationHeader, Guid.NewGuid().ToString("N"));
        }

        return base.SendAsync(request, cancellationToken);
    }
}
