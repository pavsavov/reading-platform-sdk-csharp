namespace PublishingPlatform.SDK.Abstractions;

public sealed class PublishingPlatformResilienceContext
{
    public required HttpMethod Method { get; init; }
}
