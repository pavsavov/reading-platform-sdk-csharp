using PublishingPlatform.SDK.Abstractions;
using Polly;

namespace PublishingPlatform.SDK.Internal.Resilience;

internal sealed class DefaultPublishingPlatformResiliencePipeline : IPublishingPlatformResiliencePipeline
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    internal DefaultPublishingPlatformResiliencePipeline(ResiliencePipeline<HttpResponseMessage> pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task<HttpResponseMessage> ExecuteAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            async token => await operation(token).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }
}
