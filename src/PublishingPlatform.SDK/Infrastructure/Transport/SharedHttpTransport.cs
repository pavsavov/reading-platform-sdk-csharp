using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport.Requests;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Infrastructure.Transport;

/// <summary>
/// Sends SDK HTTP requests through resilience, diagnostics, correlation, and error-mapping infrastructure.
/// </summary>
internal sealed class SharedHttpTransport : ISharedHttpTransport
{
    internal const string CorrelationHeaderName = TransportHeaderNames.CorrelationId;

    private readonly HttpClient _httpClient;
    private readonly IPublishingPlatformResiliencePipeline _resiliencePipeline;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private readonly IPublishingPlatformErrorMapper _errorMapper;
    private readonly ITransportRequestFactory _requestFactory;
    private readonly ITransportResponseErrorReader _responseErrorReader;
    private readonly ITransportErrorContextFactory _errorContextFactory;
    private readonly IDiagnosticsOptionsResolver _diagnosticsOptionsResolver;
    private readonly ILogger<SharedHttpTransport> _logger;

    public SharedHttpTransport(
        HttpClient httpClient,
        IPublishingPlatformResiliencePipeline resiliencePipeline,
        ICorrelationIdProvider correlationIdProvider,
        IPublishingPlatformErrorMapper errorMapper)
        : this(
            httpClient,
            resiliencePipeline,
            correlationIdProvider,
            errorMapper,
            TransportDependencies.CreateDefault(),
            new DefaultDiagnosticsOptionsResolver(new PublishingPlatformClientOptions()),
            NullLogger<SharedHttpTransport>.Instance)
    {
    }

    private SharedHttpTransport(
        HttpClient httpClient,
        IPublishingPlatformResiliencePipeline resiliencePipeline,
        ICorrelationIdProvider correlationIdProvider,
        IPublishingPlatformErrorMapper errorMapper,
        TransportDependencies dependencies,
        IDiagnosticsOptionsResolver diagnosticsOptionsResolver,
        ILogger<SharedHttpTransport> logger)
        : this(
            httpClient,
            resiliencePipeline,
            correlationIdProvider,
            errorMapper,
            dependencies.RequestFactory,
            dependencies.ResponseErrorReader,
            dependencies.ErrorContextFactory,
            diagnosticsOptionsResolver,
            logger)
    {
    }

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Internal composition constructor wires transport collaborators explicitly for deterministic test seams and DI-free bootstrap paths.")]
    internal SharedHttpTransport(
        HttpClient httpClient,
        IPublishingPlatformResiliencePipeline resiliencePipeline,
        ICorrelationIdProvider correlationIdProvider,
        IPublishingPlatformErrorMapper errorMapper,
        ITransportRequestFactory requestFactory,
        ITransportResponseErrorReader responseErrorReader,
        ITransportErrorContextFactory errorContextFactory)
        : this(
            httpClient,
            resiliencePipeline,
            correlationIdProvider,
            errorMapper,
            requestFactory,
            responseErrorReader,
            errorContextFactory,
            new DefaultDiagnosticsOptionsResolver(new PublishingPlatformClientOptions()),
            NullLogger<SharedHttpTransport>.Instance)
    {
    }

    internal SharedHttpTransport(
        HttpClient httpClient,
        IPublishingPlatformResiliencePipeline resiliencePipeline,
        ICorrelationIdProvider correlationIdProvider,
        IPublishingPlatformErrorMapper errorMapper,
        ITransportRequestFactory requestFactory,
        ITransportResponseErrorReader responseErrorReader,
        ITransportErrorContextFactory errorContextFactory,
        IDiagnosticsOptionsResolver diagnosticsOptionsResolver,
        ILogger<SharedHttpTransport> logger)
    {
        _httpClient = httpClient;
        _resiliencePipeline = resiliencePipeline;
        _correlationIdProvider = correlationIdProvider;
        _errorMapper = errorMapper;
        _requestFactory = requestFactory;
        _responseErrorReader = responseErrorReader;
        _errorContextFactory = errorContextFactory;
        _diagnosticsOptionsResolver = diagnosticsOptionsResolver;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePath,
        HttpContent? content,
        IReadOnlyDictionary<string, string>? headers = null,
        string? operationName = null,
        CancellationToken cancellationToken = default,
        string? moduleName = null)
    {
        var effectiveOperationName = string.IsNullOrWhiteSpace(operationName)
            ? "SDK.HttpRequest"
            : operationName;
        var effectiveModuleName = string.IsNullOrWhiteSpace(moduleName)
            ? ResolveModuleName(effectiveOperationName)
            : moduleName;
        var options = _diagnosticsOptionsResolver.Resolve(effectiveModuleName);
        var correlationId = ResolveCorrelationId(headers, options);
        var hasIdempotencyKey = HasHeader(headers, TransportHeaderNames.IdempotencyKey);
        var canReplayContent = IsReplayableContent(content);
        var diagnosticsContext = new RequestDiagnosticsContext(
            effectiveModuleName,
            effectiveOperationName,
            method,
            relativePath,
            correlationId,
            options);

        using var activity = StartActivity(diagnosticsContext);
        LogRequestStarting(diagnosticsContext);

        var started = Stopwatch.GetTimestamp();
        var attempts = 0;

        try
        {
            var response = await _resiliencePipeline.ExecuteAsync(
                new PublishingPlatformResilienceContext
                {
                    Method = method,
                    HasIdempotencyKey = hasIdempotencyKey,
                    CanReplayContent = canReplayContent,
                },
                async ct =>
                {
                    attempts++;
                    using var request = _requestFactory.Create(
                        method,
                        relativePath,
                        content,
                        options.CorrelationHeaderName,
                        correlationId,
                        headers);
                    return await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);

            var elapsed = GetElapsedMilliseconds(started);
            activity?.SetTag("http.status_code", (int)response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                LogRequestCompleted(diagnosticsContext, response, elapsed);
                RecordMetrics(diagnosticsContext, response, elapsed, attempts);
                return response;
            }

            LogRequestCompleted(diagnosticsContext, response, elapsed);
            RecordMetrics(diagnosticsContext, response, elapsed, attempts);

            var error = await _responseErrorReader.ReadAsync(response, cancellationToken).ConfigureAwait(false);
            var context = _errorContextFactory.Create(
                method,
                relativePath,
                (int)response.StatusCode,
                error,
                correlationId,
                operationName);

            throw _errorMapper.Map(context);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var elapsed = GetElapsedMilliseconds(started);
            activity?.SetTag("error.type", ex.GetType().Name);
            LogRequestFailed(diagnosticsContext, ex, elapsed);
            RecordFailureMetrics(diagnosticsContext, elapsed, attempts);
            throw;
        }
    }

    private string? ResolveCorrelationId(
        IReadOnlyDictionary<string, string>? headers,
        ResolvedDiagnosticsOptions options)
    {
        if (headers is not null)
        {
            var correlationHeader = headers
                .Where(header => string.Equals(header.Key, options.CorrelationHeaderName, StringComparison.OrdinalIgnoreCase))
                .Select(header => header.Value)
                .FirstOrDefault();
            if (correlationHeader is not null)
            {
                return correlationHeader;
            }
        }

        var requestScopedCorrelationId = RequestScopedCorrelationContext.CurrentCorrelationId;
        if (!string.IsNullOrWhiteSpace(requestScopedCorrelationId))
        {
            return requestScopedCorrelationId;
        }

        return options.GenerateCorrelationIds ? _correlationIdProvider.Create() : null;
    }

    private static string ResolveModuleName(string operationName)
    {
        var separatorIndex = operationName.IndexOf('.', StringComparison.Ordinal);
        return separatorIndex <= 0 ? "SDK" : operationName[..separatorIndex];
    }

    private static bool HasHeader(IReadOnlyDictionary<string, string>? headers, string headerName)
    {
        if (headers is null)
        {
            return false;
        }

        return headers.Any(header =>
            string.Equals(header.Key, headerName, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(header.Value));
    }

    private static bool IsReplayableContent(HttpContent? content)
    {
        if (content is null)
        {
            return true;
        }

        return content is not StreamContent and not MultipartFormDataContent;
    }

    private static Activity? StartActivity(RequestDiagnosticsContext context)
    {
        if (!context.Options.EnableTracing)
        {
            return null;
        }

        var activity = ActivitySourceProvider.ActivitySource.StartActivity(context.OperationName, ActivityKind.Client);
        activity?.SetTag("sdk.module", context.ModuleName);
        activity?.SetTag("sdk.operation", context.OperationName);
        activity?.SetTag("http.method", context.Method.Method);
        activity?.SetTag("url.path", context.SanitizedPath);
        activity?.SetTag("correlation.id", context.CorrelationId);
        return activity;
    }

    private static double GetElapsedMilliseconds(long started)
    {
        return Stopwatch.GetElapsedTime(started).TotalMilliseconds;
    }

    private void LogRequestStarting(RequestDiagnosticsContext context)
    {
        if (!context.Options.EnableLogging)
        {
            return;
        }

        _logger.LogInformation(
            "SDK request started: {Module} {Operation} {Method} {Path} {CorrelationId}",
            context.ModuleName,
            context.OperationName,
            context.Method.Method,
            context.SanitizedPath,
            context.CorrelationId);
    }

    private void LogRequestCompleted(
        RequestDiagnosticsContext context,
        HttpResponseMessage response,
        double elapsedMilliseconds)
    {
        if (!context.Options.EnableLogging)
        {
            return;
        }

        _logger.LogInformation(
            "SDK request completed: {Module} {Operation} {Method} {Path} {StatusCode} {ElapsedMilliseconds} {CorrelationId}",
            context.ModuleName,
            context.OperationName,
            context.Method.Method,
            context.SanitizedPath,
            (int)response.StatusCode,
            elapsedMilliseconds,
            context.CorrelationId);
    }

    private void LogRequestFailed(
        RequestDiagnosticsContext context,
        Exception exception,
        double elapsedMilliseconds)
    {
        if (!context.Options.EnableLogging)
        {
            return;
        }

        _logger.LogError(
            exception,
            "SDK request failed: {Module} {Operation} {Method} {Path} {ElapsedMilliseconds} {CorrelationId}",
            context.ModuleName,
            context.OperationName,
            context.Method.Method,
            context.SanitizedPath,
            elapsedMilliseconds,
            context.CorrelationId);
    }

    private static void RecordMetrics(
        RequestDiagnosticsContext context,
        HttpResponseMessage response,
        double elapsedMilliseconds,
        int attempts)
    {
        if (!context.Options.EnableMetrics)
        {
            return;
        }

        var tags = CreateMetricTags(context);
        tags.Add("http.status_code", (int)response.StatusCode);
        DiagnosticsMeterProvider.RequestCounter.Add(1, tags);
        DiagnosticsMeterProvider.RequestDuration.Record(elapsedMilliseconds, tags);
        if (!response.IsSuccessStatusCode)
        {
            DiagnosticsMeterProvider.FailureCounter.Add(1, tags);
        }

        RecordRetryCount(context, attempts);
    }

    private static void RecordFailureMetrics(
        RequestDiagnosticsContext context,
        double elapsedMilliseconds,
        int attempts)
    {
        if (!context.Options.EnableMetrics)
        {
            return;
        }

        var tags = CreateMetricTags(context);
        DiagnosticsMeterProvider.FailureCounter.Add(1, tags);
        DiagnosticsMeterProvider.RequestDuration.Record(elapsedMilliseconds, tags);
        RecordRetryCount(context, attempts);
    }

    private static void RecordRetryCount(RequestDiagnosticsContext context, int attempts)
    {
        if (!context.Options.EnableMetrics || attempts <= 1)
        {
            return;
        }

        DiagnosticsMeterProvider.RetryCounter.Add(attempts - 1, CreateMetricTags(context));
    }

    private static TagList CreateMetricTags(RequestDiagnosticsContext context)
    {
        var tags = new TagList
        {
            { "sdk.module", context.ModuleName },
            { "sdk.operation", context.OperationName },
            { "http.method", context.Method.Method },
        };
        return tags;
    }
}
