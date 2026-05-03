namespace PublishingPlatform.SDK.Infrastructure.Http.Handlers;

public sealed class DiagnosticsHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var activity = Diagnostics.ActivitySourceProvider.ActivitySource.StartActivity("PublishingPlatform.HttpRequest");
        activity?.SetTag("http.method", request.Method.Method);
        activity?.SetTag("http.url", request.RequestUri?.ToString());

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        activity?.SetTag("http.status_code", (int)response.StatusCode);

        return response;
    }
}
