using PublishingPlatform.SDK.Infrastructure.GoogleBooks;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class ArchitectureBoundariesTests
{
    [Fact]
    public void GoogleProviderPayloadTypes_AreNotPublic()
    {
        typeof(GoogleBooksVolumePayload).IsPublic.Should().BeFalse();
        typeof(GoogleBooksVolumeInfoPayload).IsPublic.Should().BeFalse();
    }

    [Fact]
    public void SharedHttpTransport_DoesNotReferenceBookModelNamespace()
    {
        var transportType = typeof(SharedHttpTransport);
        var referencedTypes = transportType.Assembly
            .GetTypes()
            .Where(t => t.Namespace == transportType.Namespace)
            .ToArray();

        referencedTypes.Should().NotBeEmpty();
        typeof(Book).Namespace.Should().Be("PublishingPlatform.SDK.Models");
    }
}
