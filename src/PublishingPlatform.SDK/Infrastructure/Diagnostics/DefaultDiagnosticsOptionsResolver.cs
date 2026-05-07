using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

/// <summary>
/// Resolves module diagnostics options by applying explicit module overrides to global defaults.
/// </summary>
internal sealed class DefaultDiagnosticsOptionsResolver : IDiagnosticsOptionsResolver
{
    private readonly PublishingPlatformClientOptions _clientOptions;

    public DefaultDiagnosticsOptionsResolver(PublishingPlatformClientOptions clientOptions)
    {
        _clientOptions = clientOptions;
    }

    public ResolvedDiagnosticsOptions Resolve(string moduleName)
    {
        var global = _clientOptions.Diagnostics ?? new DiagnosticsOptions();
        var module = GetModuleOptions(moduleName);
        var headerName = module?.CorrelationHeaderName;
        if (string.IsNullOrWhiteSpace(headerName))
        {
            headerName = global.CorrelationHeaderName;
        }

        if (string.IsNullOrWhiteSpace(headerName))
        {
            headerName = Transport.TransportHeaderNames.CorrelationId;
        }

        return new ResolvedDiagnosticsOptions(
            module?.EnableLogging ?? global.EnableLogging,
            module?.EnableTracing ?? global.EnableTracing,
            module?.EnableMetrics ?? global.EnableMetrics,
            module?.LogRequestBody ?? global.LogRequestBody,
            module?.LogResponseBody ?? global.LogResponseBody,
            headerName,
            module?.GenerateCorrelationIds ?? global.GenerateCorrelationIds);
    }

    private ModuleDiagnosticsOptions? GetModuleOptions(string moduleName)
    {
        return moduleName switch
        {
            DiagnosticsModuleNames.Books => _clientOptions.BooksDiagnostics,
            DiagnosticsModuleNames.BookContent => _clientOptions.BookContentDiagnostics,
            DiagnosticsModuleNames.BookPublishing => _clientOptions.BookPublishingDiagnostics,
            DiagnosticsModuleNames.BookDistribution => _clientOptions.BookDistributionDiagnostics,
            DiagnosticsModuleNames.BookAccess => _clientOptions.BookAccessDiagnostics,
            DiagnosticsModuleNames.BookAnalytics => _clientOptions.BookAnalyticsDiagnostics,
            DiagnosticsModuleNames.BookAuditLogs => _clientOptions.BookAuditLogsDiagnostics,
            DiagnosticsModuleNames.Webhooks => _clientOptions.WebhooksDiagnostics,
            _ => null,
        };
    }
}
