using PublishingPlatform.SDK.Infrastructure.Diagnostics;

namespace PublishingPlatform.SDK.Infrastructure.Http.Handlers;

/// <summary>
/// Provides an opt-in diagnostics delegating handler for HTTP pipelines that use handlers directly.
/// </summary>
public sealed class DiagnosticsHandler : DelegatingHandler
{
    private readonly DiagnosticsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsHandler"/> class with safe diagnostics defaults.
    /// </summary>
    public DiagnosticsHandler()
        : this(new DiagnosticsOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsHandler"/> class.
    /// </summary>
    /// <param name="options">The diagnostics options that control handler behavior.</param>
    public DiagnosticsHandler(DiagnosticsOptions options)
    {
        _options = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_options.EnableTracing)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        using var activity = ActivitySourceProvider.ActivitySource.StartActivity("PublishingPlatform.HttpRequest");
        activity?.SetTag("http.method", request.Method.Method);
        activity?.SetTag("url.path", request.RequestUri?.AbsolutePath);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        activity?.SetTag("http.status_code", (int)response.StatusCode);

        return response;
    }
}
