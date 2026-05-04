using PublishingPlatform.SDK.Clients.Books.Pagination;
using PublishingPlatform.SDK.Clients.Books.Requests;
using PublishingPlatform.SDK.Clients.Books.Serialization;
using PublishingPlatform.SDK.Clients.Books.Validation;

namespace PublishingPlatform.SDK.Clients.Books.Builders;

internal sealed class BooksClientDependencies
{
    private BooksClientDependencies(
        IBookRequestValidator validator,
        IBookQueryStringBuilder queryStringBuilder,
        IBookResponseReader responseReader,
        IBookRequestHeadersFactory requestHeadersFactory,
        IBookPaginationIteratorFactory paginationIteratorFactory)
    {
        Validator = validator;
        QueryStringBuilder = queryStringBuilder;
        ResponseReader = responseReader;
        RequestHeadersFactory = requestHeadersFactory;
        PaginationIteratorFactory = paginationIteratorFactory;
    }

    public IBookRequestValidator Validator { get; }

    public IBookQueryStringBuilder QueryStringBuilder { get; }

    public IBookResponseReader ResponseReader { get; }

    public IBookRequestHeadersFactory RequestHeadersFactory { get; }

    public IBookPaginationIteratorFactory PaginationIteratorFactory { get; }

    public static BooksClientDependencies CreateDefault()
    {
        return new BooksClientDependencies(
            new DefaultBookRequestValidator(),
            new DefaultBookQueryStringBuilder(),
            new DefaultBookResponseReader(),
            new DefaultBookRequestHeadersFactory(),
            new DefaultBookPaginationIteratorFactory());
    }
}
