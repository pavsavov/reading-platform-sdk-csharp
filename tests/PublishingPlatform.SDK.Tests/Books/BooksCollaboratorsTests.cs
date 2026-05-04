using PublishingPlatform.SDK.Clients.Books.Requests;
using PublishingPlatform.SDK.Clients.Books.Serialization;
using PublishingPlatform.SDK.Clients.Books.Validation;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;
using System.Text;

namespace PublishingPlatform.SDK.Tests.Books;

public sealed class BooksCollaboratorsTests
{
    [Fact]
    public void Validator_RejectsInvalidSortField()
    {
        var validator = new DefaultBookRequestValidator();

        Action act = () => validator.ValidateList(new ListBooksRequest
        {
            SortBy = "unsupported",
            Page = 0,
            PageSize = 10,
        });

        act.Should().Throw<BookValidationException>();
    }

    [Fact]
    public void QueryBuilder_OrdersTagsDeterministically()
    {
        var builder = new DefaultBookQueryStringBuilder();
        var request = new ListBooksRequest
        {
            SortBy = "title",
            Page = 0,
            PageSize = 10,
            Tags = ["z", "a"],
        };

        var path = builder.BuildListPath(request);

        path.Should().Contain("&tag=a&tag=z");
    }

    [Fact]
    public void HeadersFactory_EmitsConcurrencyHeader_WhenTokenPresent()
    {
        var factory = new DefaultBookRequestHeadersFactory();

        var headers = factory.CreateConcurrencyHeaders("\"v1\"");

        headers.Should().NotBeNull();
        headers!["If-Match"].Should().Be("\"v1\"");
    }

    [Fact]
    public async Task ResponseReader_Throws_WhenBookPayloadNull()
    {
        var reader = new DefaultBookResponseReader();
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        };

        Func<Task> act = async () => await reader.ReadBookAsync(response, CancellationToken.None);

        _ = await act.Should().ThrowAsync<BookValidationException>();
    }
}
