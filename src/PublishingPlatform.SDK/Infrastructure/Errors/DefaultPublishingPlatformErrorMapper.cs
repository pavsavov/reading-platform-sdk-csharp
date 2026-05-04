using PublishingPlatform.SDK.Abstractions;

namespace PublishingPlatform.SDK.Infrastructure.Errors;

internal sealed class DefaultPublishingPlatformErrorMapper : IPublishingPlatformErrorMapper
{
    public Exception Map(PublishingPlatformErrorContext context)
    {
        return ErrorMapper.Map(context.StatusCode, context.Message);
    }
}
