using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Polly.Timeout;
using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Options;

namespace PublishingPlatform.SDK.Internal.Resilience;

internal static class ResiliencePipelineFactory
{
    internal static IPublishingPlatformResiliencePipeline Create(PublishingPlatformResilienceOptions? options)
    {
        if (options is null || !options.Enabled)
        {
            return new NoOpPublishingPlatformResiliencePipeline();
        }

        Validate(options);

        var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();

        if (options.TotalTimeout.Enabled)
        {
            builder.AddTimeout(options.TotalTimeout.Timeout);
        }

        if (options.Retry.Enabled)
        {
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = options.Retry.MaxRetryAttempts,
                Delay = options.Retry.BaseDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = args => new ValueTask<bool>(ShouldRetry(args, options.Retry.RetryNonIdempotentMethods)),
            });
        }

        if (options.CircuitBreaker.Enabled)
        {
            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = options.CircuitBreaker.FailureRatio,
                MinimumThroughput = options.CircuitBreaker.MinimumThroughput,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = options.CircuitBreaker.BreakDuration,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(response => !response.IsSuccessStatusCode),
            });
        }

        if (options.AttemptTimeout.Enabled)
        {
            builder.AddTimeout(options.AttemptTimeout.Timeout);
        }

        return new DefaultPublishingPlatformResiliencePipeline(builder.Build());
    }

    private static bool ShouldRetry(RetryPredicateArguments<HttpResponseMessage> args, bool retryNonIdempotentMethods)
    {
        if (args.Outcome.Exception is HttpRequestException or TimeoutRejectedException or BrokenCircuitException)
        {
            return IsMethodAllowed(args.Context, retryNonIdempotentMethods);
        }

        if (args.Outcome.Result is null)
        {
            return false;
        }

        if (!DefaultPublishingPlatformResiliencePipeline.TryGetHttpMethod(args.Context, out var method))
        {
            return false;
        }

        if (!IsMethodAllowed(method, retryNonIdempotentMethods))
        {
            return false;
        }

        return IsTransientStatusCode(args.Outcome.Result.StatusCode, IsNonIdempotentRetryCandidate(method));
    }

    private static bool IsMethodAllowed(ResilienceContext context, bool retryNonIdempotentMethods)
    {
        if (!DefaultPublishingPlatformResiliencePipeline.TryGetHttpMethod(context, out var method))
        {
            return false;
        }

        return IsMethodAllowed(method, retryNonIdempotentMethods);
    }

    private static bool IsMethodAllowed(HttpMethod method, bool retryNonIdempotentMethods)
    {
        if (IsIdempotentMethod(method))
        {
            return true;
        }

        return retryNonIdempotentMethods && IsNonIdempotentRetryCandidate(method);
    }

    private static bool IsIdempotentMethod(HttpMethod method)
    {
        return method == HttpMethod.Get
            || method == HttpMethod.Put
            || method == HttpMethod.Delete
            || method == HttpMethod.Head
            || method == HttpMethod.Options
            || method.Method.Equals("TRACE", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNonIdempotentRetryCandidate(HttpMethod method)
    {
        return method == HttpMethod.Post || method == HttpMethod.Patch;
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode, bool isNonIdempotentMethod)
    {
        if (isNonIdempotentMethod)
        {
            return statusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable;
        }

        return statusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
    }

    private static void Validate(PublishingPlatformResilienceOptions options)
    {
        if (options.Retry.Enabled)
        {
            if (options.Retry.MaxRetryAttempts < 0)
            {
                throw new PublishingPlatformConfigurationException("Retry MaxRetryAttempts must be greater than or equal to 0 when retry is enabled.");
            }

            if (options.Retry.BaseDelay <= TimeSpan.Zero)
            {
                throw new PublishingPlatformConfigurationException("Retry BaseDelay must be greater than zero when retry is enabled.");
            }
        }

        if (options.CircuitBreaker.Enabled)
        {
            if (options.CircuitBreaker.FailureRatio <= 0 || options.CircuitBreaker.FailureRatio > 1)
            {
                throw new PublishingPlatformConfigurationException("Circuit breaker FailureRatio must be in the range (0, 1] when circuit breaker is enabled.");
            }

            if (options.CircuitBreaker.MinimumThroughput <= 0)
            {
                throw new PublishingPlatformConfigurationException("Circuit breaker MinimumThroughput must be greater than zero when circuit breaker is enabled.");
            }

            if (options.CircuitBreaker.BreakDuration <= TimeSpan.Zero)
            {
                throw new PublishingPlatformConfigurationException("Circuit breaker BreakDuration must be greater than zero when circuit breaker is enabled.");
            }
        }

        ValidateTimeout(options.AttemptTimeout, "AttemptTimeout");
        ValidateTimeout(options.TotalTimeout, "TotalTimeout");
    }

    private static void ValidateTimeout(TimeoutResilienceOptions timeoutOptions, string optionName)
    {
        if (!timeoutOptions.Enabled)
        {
            return;
        }

        if (timeoutOptions.Timeout <= TimeSpan.Zero)
        {
            throw new PublishingPlatformConfigurationException($"{optionName} Timeout must be greater than zero when timeout strategy is enabled.");
        }
    }
}
