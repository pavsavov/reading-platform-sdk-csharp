using PublishingPlatform.SDK.Infrastructure.Auth;
using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.GoogleBooks;
using PublishingPlatform.SDK.Infrastructure.Http.Handlers;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class InfrastructureContractsTests
{
    [Fact]
    public async Task ApiKeyTokenProvider_ReturnsConfiguredToken()
    {
        var provider = new ApiKeyTokenProvider("api-key");

        var token = await provider.GetTokenAsync(CancellationToken.None);

        token.Should().Be("api-key");
    }

    [Fact]
    public void HttpPipelinePolicy_ExposesExpectedHandlerOrder()
    {
        var policy = new HttpPipelinePolicy();

        policy.HandlerOrder.Should().Equal("AuthHandler", "CorrelationHandler", "DiagnosticsHandler");
    }

    [Fact]
    public void GuidCorrelationIdProvider_ReturnsNonEmptyIds()
    {
        var provider = new GuidCorrelationIdProvider();

        var first = provider.Create();
        var second = provider.Create();

        first.Should().NotBeNullOrWhiteSpace();
        second.Should().NotBeNullOrWhiteSpace();
        first.Should().NotBe(second);
    }

    [Fact]
    public async Task NoOpGoogleBooksVolumesAdapter_ReturnsSameBookInstance()
    {
        var adapter = new NoOpGoogleBooksVolumesAdapter();
        var book = new Book { Id = "1", Title = "T", Author = "A" };

        var result = await adapter.EnrichAsync(book, CancellationToken.None);

        result.Should().BeSameAs(book);
    }

    [Fact]
    public void GoogleBooksMapping_ToBook_UsesFirstAuthorAndFallbacks()
    {
        var payload = new GoogleBooksVolumePayload
        {
            Id = "gid",
            VolumeInfo = new GoogleBooksVolumeInfoPayload
            {
                Title = "GTitle",
                Authors = ["A1", "A2"],
            },
        };

        var mapped = payload.ToBook();

        mapped.Id.Should().Be("gid");
        mapped.Title.Should().Be("GTitle");
        mapped.Author.Should().Be("A1");
    }

    [Fact]
    public void GoogleBooksMapping_ToBooks_MapsCollectionAndCategories()
    {
        var payload = new GoogleBooksVolumesResponsePayload
        {
            Items =
            [
                new GoogleBooksVolumePayload
                {
                    Id = "gid-1",
                    VolumeInfo = new GoogleBooksVolumeInfoPayload
                    {
                        Title = "GTitle1",
                        Authors = ["A1", "A2"],
                        Categories = ["tech", "architecture"],
                    },
                },
            ],
        };

        var mapped = payload.ToBooks();

        mapped.Should().HaveCount(1);
        mapped[0].Id.Should().Be("gid-1");
        mapped[0].Tags.Should().Equal("tech", "architecture");
    }

    [Fact]
    public void DiagnosticsAndPaginationDefaults_AreStable()
    {
        var diagnostics = new DiagnosticsOptions();
        var pagination = new PaginationRequest();

        diagnostics.EnableTracing.Should().BeTrue();
        diagnostics.EnableMetrics.Should().BeTrue();
        pagination.PageSize.Should().Be(50);
        pagination.ContinuationToken.Should().BeNull();
    }
}
