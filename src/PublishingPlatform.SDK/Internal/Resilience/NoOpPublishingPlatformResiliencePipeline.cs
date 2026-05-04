using PublishingPlatform.SDK.Abstractions;

namespace PublishingPlatform.SDK.Internal.Resilience;

internal sealed class NoOpPublishingPlatformResiliencePipeline : IPublishingPlatformResiliencePipeline
{
    public Task<HttpResponseMessage> ExecuteAsync(
        PublishingPlatformResilienceContext context,
        Func<CancellationToken, Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return operation(cancellationToken);
    }
}
