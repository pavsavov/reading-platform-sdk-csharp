using PublishingPlatform.SDK.Internal;

namespace PublishingPlatform.SDK.Tests.Internal;

public sealed class InternalGuardsTests
{
    [Fact]
    public void NotNull_ThrowsArgumentNullException_WhenValueIsNull()
    {
        Action act = () => Guards.NotNull<string>(null!, "value");

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Fact]
    public void NotNull_DoesNotThrow_WhenValueIsPresent()
    {
        Action act = () => Guards.NotNull("ok", "value");

        act.Should().NotThrow();
    }
}
