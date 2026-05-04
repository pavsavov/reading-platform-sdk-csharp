using PublishingPlatform.SDK.Models.Builders;

namespace PublishingPlatform.SDK.Tests.Books;

public sealed class BookRequestBuildersTests
{
    [Fact]
    public void CreateBookRequestBuilder_BuildsExpectedPayload()
    {
        var request = CreateBookRequestBuilder.Create()
            .WithTitle("T")
            .WithAuthor("A")
            .WithTag("x")
            .WithIdempotencyKey("k1")
            .Build();

        request.Title.Should().Be("T");
        request.Author.Should().Be("A");
        request.Tags.Should().ContainSingle().Which.Should().Be("x");
        request.IdempotencyKey.Should().Be("k1");
    }

    [Fact]
    public void ListBooksRequestBuilder_BuildsExpectedQueryInput()
    {
        var request = ListBooksRequestBuilder.Create()
            .WithAuthor("A")
            .SortedByTitle()
            .Descending()
            .WithPage(2, 25)
            .WithTag("z")
            .Build();

        request.Author.Should().Be("A");
        request.SortBy.Should().Be("title");
        request.Descending.Should().BeTrue();
        request.Page.Should().Be(2);
        request.PageSize.Should().Be(25);
        request.Tags.Should().ContainSingle().Which.Should().Be("z");
    }
}
