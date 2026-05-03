namespace PublishingPlatform.SDK.Options;

public sealed class PublishingPlatformClientOptions
{
    public string BaseUrl { get; set; } = "https://api.publishing-platform.local";

    public string ApiKey { get; set; } = string.Empty;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
