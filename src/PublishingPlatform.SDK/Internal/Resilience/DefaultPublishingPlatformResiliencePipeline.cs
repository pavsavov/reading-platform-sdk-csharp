using PublishingPlatform.SDK.Abstractions;
using Polly;

namespace PublishingPlatform.SDK.Internal.Resilience;

internal sealed class DefaultPublishingPlatformResiliencePipeline : IPublishingPlatformResiliencePipeline
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
    private static readonly ResiliencePropertyKey<HttpMethod> HttpMethodKey = new("PublishingPlatform.HttpMethod");

    internal DefaultPublishingPlatformResiliencePipeline(ResiliencePipeline<HttpResponseMessage> pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task<HttpResponseMessage> ExecuteAsync(
        PublishingPlatformResilienceContext context,
        Func<CancellationToken, Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operation);

        var resilienceContext = ResilienceContextPool.Shared.Get(cancellationToken);
        resilienceContext.Properties.Set(HttpMethodKey, context.Method);

        try
        {
            return await _pipeline.ExecuteAsync(
                async token => await operation(token.CancellationToken).ConfigureAwait(false),
                resilienceContext).ConfigureAwait(false);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(resilienceContext);
        }
    }

    internal static bool TryGetHttpMethod(ResilienceContext context, out HttpMethod method)
    {
        if (context.Properties.TryGetValue(HttpMethodKey, out var resolvedMethod) && resolvedMethod is not null)
        {
            method = resolvedMethod;
            return true;
        }

        method = HttpMethod.Get;
        return false;
    }
}
