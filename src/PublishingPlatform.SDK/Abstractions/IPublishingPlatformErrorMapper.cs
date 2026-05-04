namespace PublishingPlatform.SDK.Abstractions;

public interface IPublishingPlatformErrorMapper
{
    Exception Map(PublishingPlatformErrorContext context);
}
