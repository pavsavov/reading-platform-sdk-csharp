namespace PublishingPlatform.SDK.Infrastructure.Http.Handlers;

/// <summary>
/// Defines the intended handler ordering contract for outbound HTTP pipeline behavior.
/// </summary>
public sealed class HttpPipelinePolicy
{
    /// <summary>
    /// Gets the expected execution order for built-in handlers.
    /// </summary>
    public IReadOnlyList<string> HandlerOrder { get; } =
    [
        nameof(AuthHandler),
        nameof(CorrelationHandler),
        nameof(DiagnosticsHandler),
    ];
}
