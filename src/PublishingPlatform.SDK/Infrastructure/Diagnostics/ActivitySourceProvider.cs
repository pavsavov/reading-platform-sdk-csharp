using System.Diagnostics;

namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

public static class ActivitySourceProvider
{
    public static ActivitySource ActivitySource { get; } = new("PublishingPlatform.SDK");
}
