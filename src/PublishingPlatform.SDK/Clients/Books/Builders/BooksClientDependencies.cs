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
        IBookRequestHeadersFactory requestHeadersFactory)
    {
        Validator = validator;
        QueryStringBuilder = queryStringBuilder;
        ResponseReader = responseReader;
        RequestHeadersFactory = requestHeadersFactory;
    }

    public IBookRequestValidator Validator { get; }

    public IBookQueryStringBuilder QueryStringBuilder { get; }

    public IBookResponseReader ResponseReader { get; }

    public IBookRequestHeadersFactory RequestHeadersFactory { get; }

    public static BooksClientDependencies CreateDefault()
    {
        return new BooksClientDependencies(
            new DefaultBookRequestValidator(),
            new DefaultBookQueryStringBuilder(),
            new DefaultBookResponseReader(),
            new DefaultBookRequestHeadersFactory());
    }
}
