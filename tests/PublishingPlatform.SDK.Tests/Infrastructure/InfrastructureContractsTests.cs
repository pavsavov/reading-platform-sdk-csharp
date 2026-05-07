using PublishingPlatform.SDK.Infrastructure.Diagnostics;
using PublishingPlatform.SDK.Infrastructure.GoogleBooks;
using PublishingPlatform.SDK.Infrastructure.Http.Handlers;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;
using System.Text.Json;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class InfrastructureContractsTests
{
    [Fact]
    public void HttpPipelinePolicy_ExposesExpectedHandlerOrder()
    {
        var policy = new HttpPipelinePolicy();

        policy.HandlerOrder.Should().Equal("ApiKeyAuthHandler", "CorrelationHandler", "DiagnosticsHandler");
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
    public void GoogleBooksMapping_ToBook_UsesSafeFallbacksWhenVolumeInfoMissing()
    {
        var payload = new GoogleBooksVolumePayload
        {
            Id = "gid-fallback",
            VolumeInfo = null,
        };

        var mapped = payload.ToBook();

        mapped.Id.Should().Be("gid-fallback");
        mapped.Title.Should().BeEmpty();
        mapped.Author.Should().BeEmpty();
        mapped.Tags.Should().BeEmpty();
    }

    [Fact]
    public void GoogleBooksMapping_ToBooks_ReturnsEmptyWhenItemsMissing()
    {
        var nullItemsPayload = new GoogleBooksVolumesResponsePayload { Items = null };
        var emptyItemsPayload = new GoogleBooksVolumesResponsePayload { Items = [] };

        nullItemsPayload.ToBooks().Should().BeEmpty();
        emptyItemsPayload.ToBooks().Should().BeEmpty();
    }

    [Fact]
    public void GoogleBooksPayloadContracts_DeserializeExpectedJsonFields()
    {
        const string json = """
                            {
                              "kind": "books#volumes",
                              "totalItems": 1,
                              "items": [
                                {
                                  "id": "vol-1",
                                  "volumeInfo": {
                                    "title": "Sample",
                                    "subtitle": "Sub",
                                    "authors": ["A1"],
                                    "categories": ["cat1"],
                                    "description": "Desc",
                                    "publishedDate": "2025-01-01",
                                    "pageCount": 120,
                                    "language": "en",
                                    "imageLinks": {
                                      "smallThumbnail": "https://img/small.jpg",
                                      "thumbnail": "https://img/thumb.jpg"
                                    },
                                    "industryIdentifiers": [
                                      { "type": "ISBN_13", "identifier": "9780000000000" }
                                    ]
                                  }
                                }
                              ]
                            }
                            """;

        var payload = JsonSerializer.Deserialize<GoogleBooksVolumesResponsePayload>(json);

        payload.Should().NotBeNull();
        payload!.Kind.Should().Be("books#volumes");
        payload.TotalItems.Should().Be(1);
        payload.Items.Should().HaveCount(1);
        var first = payload.Items![0];
        first.VolumeInfo.Should().NotBeNull();
        first.VolumeInfo!.ImageLinks.Should().NotBeNull();
        first.VolumeInfo.IndustryIdentifiers.Should().NotBeNull();
        first.VolumeInfo.ImageLinks!.Thumbnail.Should().Be("https://img/thumb.jpg");
        first.VolumeInfo.IndustryIdentifiers!.Single().Type.Should().Be("ISBN_13");
    }

    [Fact]
    public void DiagnosticsAndPaginationDefaults_AreStable()
    {
        var diagnostics = new DiagnosticsOptions();
        var pagination = new PaginationRequest();

        diagnostics.EnableLogging.Should().BeFalse();
        diagnostics.EnableTracing.Should().BeFalse();
        diagnostics.EnableMetrics.Should().BeFalse();
        diagnostics.LogRequestBody.Should().BeFalse();
        diagnostics.LogResponseBody.Should().BeFalse();
        diagnostics.CorrelationHeaderName.Should().Be("X-Correlation-Id");
        diagnostics.GenerateCorrelationIds.Should().BeTrue();
        pagination.PageSize.Should().Be(50);
        pagination.ContinuationToken.Should().BeNull();
    }

    [Fact]
    public void DiagnosticsResolver_InheritsGlobalOptions_WhenModuleOverrideUnset()
    {
        var resolver = new DefaultDiagnosticsOptionsResolver(new PublishingPlatform.SDK.Options.PublishingPlatformClientOptions
        {
            Diagnostics = new DiagnosticsOptions
            {
                EnableLogging = true,
                EnableTracing = true,
                EnableMetrics = true,
                CorrelationHeaderName = "X-Trace-Id",
                GenerateCorrelationIds = false,
            },
        });

        var resolved = resolver.Resolve("Books");

        resolved.EnableLogging.Should().BeTrue();
        resolved.EnableTracing.Should().BeTrue();
        resolved.EnableMetrics.Should().BeTrue();
        resolved.CorrelationHeaderName.Should().Be("X-Trace-Id");
        resolved.GenerateCorrelationIds.Should().BeFalse();
    }

    [Fact]
    public void DiagnosticsResolver_AppliesExplicitModuleOverrides()
    {
        var resolver = new DefaultDiagnosticsOptionsResolver(new PublishingPlatform.SDK.Options.PublishingPlatformClientOptions
        {
            Diagnostics = new DiagnosticsOptions
            {
                EnableLogging = true,
                EnableTracing = false,
            },
            BookContentDiagnostics = new ModuleDiagnosticsOptions
            {
                EnableLogging = false,
                EnableTracing = true,
            },
        });

        var resolved = resolver.Resolve("BookContent");

        resolved.EnableLogging.Should().BeFalse();
        resolved.EnableTracing.Should().BeTrue();
    }
}
