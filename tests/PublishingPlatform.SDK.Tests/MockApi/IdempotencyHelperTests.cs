using Microsoft.AspNetCore.Http;
using MockPublishingPlatform.Api.Services;

namespace PublishingPlatform.SDK.Tests.MockApi;

public sealed class IdempotencyHelperTests
{
    [Fact]
    public void Resolve_ReturnsNullWhenHeaderMissing()
    {
        var request = new DefaultHttpContext().Request;

        IdempotencyHelper.Resolve(request).Should().BeNull();
    }

    [Fact]
    public void Resolve_TrimsAndReturnsHeaderValue()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Idempotency-Key"] = "  demo-key  ";

        IdempotencyHelper.Resolve(context.Request).Should().Be("demo-key");
    }

    [Fact]
    public void Resolve_ReturnsNullForWhitespaceHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Idempotency-Key"] = "   ";

        IdempotencyHelper.Resolve(context.Request).Should().BeNull();
    }

    [Fact]
    public void TryReplay_ReturnsTrueWhenStoredTypeMatches()
    {
        var state = CreateState();
        state.IdempotencyResponses["op:1"] = "payload";

        var found = IdempotencyHelper.TryReplay<string>(state, "op:1", out var replayed);

        found.Should().BeTrue();
        replayed.Should().Be("payload");
    }

    [Fact]
    public void TryReplay_ReturnsFalseWhenTypeDiffersOrKeyMissing()
    {
        var state = CreateState();
        state.IdempotencyResponses["op:1"] = 42;

        IdempotencyHelper.TryReplay<string>(state, "op:1", out var replayed).Should().BeFalse();
        replayed.Should().BeNull();

        IdempotencyHelper.TryReplay<string>(state, "missing", out var replayedMissing).Should().BeFalse();
        replayedMissing.Should().BeNull();
    }

    private static MockApiState CreateState() => new();
}
