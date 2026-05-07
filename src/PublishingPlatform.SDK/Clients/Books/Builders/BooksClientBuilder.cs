using PublishingPlatform.SDK.Clients.Books.Requests;
using PublishingPlatform.SDK.Clients.Books.Serialization;
using PublishingPlatform.SDK.Clients.Books.Validation;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Clients.Books.Builders;

internal sealed class BooksClientBuilder
{
    private readonly ISharedHttpTransport _transport;
    private IBookRequestValidator _validator = new DefaultBookRequestValidator();
    private IBookQueryStringBuilder _queryStringBuilder = new DefaultBookQueryStringBuilder();
    private IBookResponseReader _responseReader = new DefaultBookResponseReader();
    private IBookRequestHeadersFactory _requestHeadersFactory = new DefaultBookRequestHeadersFactory();

    private BooksClientBuilder(ISharedHttpTransport transport)
    {
        _transport = transport;
    }

    public static BooksClientBuilder Create(ISharedHttpTransport transport)
    {
        return new BooksClientBuilder(transport);
    }

    public BooksClientBuilder WithValidator(IBookRequestValidator validator)
    {
        _validator = validator;
        return this;
    }

    public BooksClientBuilder WithQueryBuilder(IBookQueryStringBuilder queryStringBuilder)
    {
        _queryStringBuilder = queryStringBuilder;
        return this;
    }

    public BooksClientBuilder WithResponseReader(IBookResponseReader responseReader)
    {
        _responseReader = responseReader;
        return this;
    }

    public BooksClientBuilder WithHeadersFactory(IBookRequestHeadersFactory requestHeadersFactory)
    {
        _requestHeadersFactory = requestHeadersFactory;
        return this;
    }

    public BooksClient Build()
    {
        return new BooksClient(
            _transport,
            _validator,
            _queryStringBuilder,
            _responseReader,
            _requestHeadersFactory);
    }
}
