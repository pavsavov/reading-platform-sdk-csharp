using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Clients.Books.Builders;
using PublishingPlatform.SDK.Clients.Books.Pagination;
using PublishingPlatform.SDK.Clients.Books.Requests;
using PublishingPlatform.SDK.Clients.Books.Serialization;
using PublishingPlatform.SDK.Clients.Books.Validation;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Tests.Books;

public sealed class BooksBuilderAndDependenciesTests
{
    [Fact]
    public void BooksClientDependencies_CreateDefault_ProvidesExpectedCollaborators()
    {
        var dependencies = BooksClientDependencies.CreateDefault();

        dependencies.Validator.Should().BeOfType<DefaultBookRequestValidator>();
        dependencies.QueryStringBuilder.Should().BeOfType<DefaultBookQueryStringBuilder>();
        dependencies.ResponseReader.Should().BeOfType<DefaultBookResponseReader>();
        dependencies.RequestHeadersFactory.Should().BeOfType<DefaultBookRequestHeadersFactory>();
        dependencies.PaginationIteratorFactory.Should().BeOfType<DefaultBookPaginationIteratorFactory>();
    }

    [Fact]
    public void BooksClientBuilder_BuildsClient_WithConfiguredCollaborators()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        var validator = Substitute.For<IBookRequestValidator>();
        var queryBuilder = Substitute.For<IBookQueryStringBuilder>();
        var responseReader = Substitute.For<IBookResponseReader>();
        var headersFactory = Substitute.For<IBookRequestHeadersFactory>();
        var paginationFactory = Substitute.For<IBookPaginationIteratorFactory>();

        var client = BooksClientBuilder.Create(transport)
            .WithValidator(validator)
            .WithQueryBuilder(queryBuilder)
            .WithResponseReader(responseReader)
            .WithHeadersFactory(headersFactory)
            .WithPaginationIteratorFactory(paginationFactory)
            .Build();

        client.Should().BeOfType<BooksClient>();
    }
}
