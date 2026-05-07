namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

/// <summary>
/// Resolves effective diagnostics options for SDK modules.
/// </summary>
internal interface IDiagnosticsOptionsResolver
{
    ResolvedDiagnosticsOptions Resolve(string moduleName);
}
