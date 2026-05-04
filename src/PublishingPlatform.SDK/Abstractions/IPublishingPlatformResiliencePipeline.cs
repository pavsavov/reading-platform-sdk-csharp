namespace PublishingPlatform.SDK.Abstractions;

public interface IPublishingPlatformResiliencePipeline
{
    Task<HttpResponseMessage> ExecuteAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken = default);
}
