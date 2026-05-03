namespace PublishingPlatform.SDK.Models;

public sealed class AuditLog
{
    public string Id { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; }
}
