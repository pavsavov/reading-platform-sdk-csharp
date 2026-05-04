using PublishingPlatform.SDK.Abstractions;

namespace PublishingPlatform.SDK.Internal.Resilience;

internal sealed class NoOpPublishingPlatformResiliencePipeline : IPublishingPlatformResiliencePipeline
{
    public Task<HttpResponseMessage> ExecuteAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken = default)
    {
        return operation(cancellationToken);
    }
}
