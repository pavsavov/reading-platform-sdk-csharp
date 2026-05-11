using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class RequestScopedCorrelationContextTests
{
    [Fact]
    public void Push_SetsCurrentCorrelationId_AndRestoresOnDispose()
    {
        RequestScopedCorrelationContext.CurrentCorrelationId.Should().BeNull();

        using (RequestScopedCorrelationContext.Push("corr-outer"))
        {
            RequestScopedCorrelationContext.CurrentCorrelationId.Should().Be("corr-outer");

            using (RequestScopedCorrelationContext.Push("corr-inner"))
            {
                RequestScopedCorrelationContext.CurrentCorrelationId.Should().Be("corr-inner");
            }

            RequestScopedCorrelationContext.CurrentCorrelationId.Should().Be("corr-outer");
        }

        RequestScopedCorrelationContext.CurrentCorrelationId.Should().BeNull();
    }

    [Fact]
    public void Dispose_CanBeCalledTwice_WithoutMutatingStateAgain()
    {
        var scope = RequestScopedCorrelationContext.Push("corr-1");
        RequestScopedCorrelationContext.CurrentCorrelationId.Should().Be("corr-1");

        scope.Dispose();
        RequestScopedCorrelationContext.CurrentCorrelationId.Should().BeNull();

        scope.Dispose();
        RequestScopedCorrelationContext.CurrentCorrelationId.Should().BeNull();
    }
}
