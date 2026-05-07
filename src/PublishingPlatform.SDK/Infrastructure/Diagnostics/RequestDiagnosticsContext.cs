namespace PublishingPlatform.SDK.Infrastructure.Diagnostics;

/// <summary>
/// Carries diagnostics metadata for one outbound SDK request.
/// </summary>
internal sealed class RequestDiagnosticsContext
{
    public RequestDiagnosticsContext(
        string moduleName,
        string operationName,
        HttpMethod method,
        string relativePath,
        string? correlationId,
        ResolvedDiagnosticsOptions options)
    {
        ModuleName = moduleName;
        OperationName = operationName;
        Method = method;
        RelativePath = relativePath;
        CorrelationId = correlationId;
        Options = options;
    }

    public string ModuleName { get; }

    public string OperationName { get; }

    public HttpMethod Method { get; }

    public string RelativePath { get; }

    public string? CorrelationId { get; }

    public ResolvedDiagnosticsOptions Options { get; }

    public string SanitizedPath
    {
        get
        {
            var queryIndex = RelativePath.IndexOf('?', StringComparison.Ordinal);
            return queryIndex < 0 ? RelativePath : RelativePath[..queryIndex];
        }
    }
}
