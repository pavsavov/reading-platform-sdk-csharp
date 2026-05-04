namespace PublishingPlatform.SDK.Abstractions;

public interface IPublishingPlatformResiliencePipeline
{
    Task<HttpResponseMessage> ExecuteAsync(
        PublishingPlatformResilienceContext context,
        Func<CancellationToken, Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken = default);
}
