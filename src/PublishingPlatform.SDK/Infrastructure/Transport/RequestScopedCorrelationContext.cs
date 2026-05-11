using System.Threading;

namespace PublishingPlatform.SDK.Infrastructure.Transport;

/// <summary>
/// Stores an optional request-scoped correlation override for the current async flow.
/// </summary>
internal static class RequestScopedCorrelationContext
{
    private static readonly AsyncLocal<string?> CorrelationId = new();

    internal static string? CurrentCorrelationId => CorrelationId.Value;

    internal static IDisposable Push(string? correlationId)
    {
        var previous = CorrelationId.Value;
        CorrelationId.Value = correlationId;
        return new Scope(previous);
    }

    private sealed class Scope : IDisposable
    {
        private readonly string? _previous;
        private bool _disposed;

        internal Scope(string? previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            CorrelationId.Value = _previous;
            _disposed = true;
        }
    }
}
