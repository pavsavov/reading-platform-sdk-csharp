namespace PublishingPlatform.SDK.Clients.Webhooks;

/// <summary>
/// Defines webhook route templates and route builders.
/// </summary>
internal static class WebhooksEndpoints
{
    internal const string Collection = "/webhooks";

    internal static string ById(string webhookId)
    {
        return $"/webhooks/{Uri.EscapeDataString(webhookId)}";
    }
}
