using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Infrastructure.Http.Handlers;

/// <summary>
/// Ensures outbound HTTP requests carry a correlation identifier header.
/// </summary>
public sealed class CorrelationHandler : DelegatingHandler
{
    /// <summary>
    /// The outbound correlation header name.
    /// </summary>
    public const string CorrelationHeader = TransportHeaderNames.CorrelationId;

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(CorrelationHeader))
        {
            request.Headers.Add(CorrelationHeader, Guid.NewGuid().ToString("N"));
        }

        return base.SendAsync(request, cancellationToken);
    }
}
