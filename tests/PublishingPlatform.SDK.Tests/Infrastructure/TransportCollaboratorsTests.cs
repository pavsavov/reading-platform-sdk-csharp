using PublishingPlatform.SDK.Abstractions;
using PublishingPlatform.SDK.Infrastructure.Transport.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport.Requests;
using System.Net;
using System.Net.Http.Json;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class TransportCollaboratorsTests
{
    [Fact]
    public void RequestFactory_AddsCorrelationAndAcceptHeaders()
    {
        var factory = new DefaultTransportRequestFactory();

        using var request = factory.Create(HttpMethod.Get, "/books", null, "corr-1");

        request.Headers.Contains("X-Correlation-Id").Should().BeTrue();
        request.Headers.Accept.ToString().Should().Contain("application/json");
    }

    [Fact]
    public async Task ResponseErrorReader_FallsBackForNonJsonPayload()
    {
        var reader = new DefaultTransportResponseErrorReader();
        using var response = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("oops"),
        };

        var error = await reader.ReadAsync(response, CancellationToken.None);

        error.Message.Should().Be("HTTP 502 returned by Publishing Platform API.");
        error.ErrorCode.Should().Be("http_502");
        error.RequestId.Should().BeNull();
        error.CorrelationId.Should().BeNull();
    }

    [Fact]
    public async Task ResponseErrorReader_UsesNormalizedPayloadFields_WhenPresent()
    {
        var reader = new DefaultTransportResponseErrorReader();
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new
            {
                Message = "invalid",
                ErrorCode = "book.invalid",
                RequestId = "req-1",
                CorrelationId = "corr-from-api",
            }),
        };

        var error = await reader.ReadAsync(response, CancellationToken.None);

        error.Message.Should().Be("invalid");
        error.ErrorCode.Should().Be("book.invalid");
        error.RequestId.Should().Be("req-1");
        error.CorrelationId.Should().Be("corr-from-api");
    }

    [Fact]
    public async Task ResponseErrorReader_FallsBackForUnknownPayloadShape()
    {
        var reader = new DefaultTransportResponseErrorReader();
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = JsonContent.Create(new { Detail = "shape not normalized" }),
        };

        var error = await reader.ReadAsync(response, CancellationToken.None);

        error.Message.Should().Be("HTTP 500 returned by Publishing Platform API.");
        error.ErrorCode.Should().Be("http_500");
    }

    [Fact]
    public void ErrorContextFactory_PreservesNormalizedMetadata()
    {
        var factory = new DefaultTransportErrorContextFactory();
        var error = new NormalizedTransportError
        {
            Message = "x",
            ErrorCode = "book.failed",
            RequestId = "req-1",
            CorrelationId = "api-corr",
        };

        PublishingPlatformErrorContext context = factory.Create(HttpMethod.Get, "/books", 500, error, "generated-corr", "Books.List");

        context.OperationName.Should().Be("Books.List");
        context.CorrelationId.Should().Be("api-corr");
        context.RequestId.Should().Be("req-1");
        context.ErrorCode.Should().Be("book.failed");
        context.Message.Should().Be("x");
    }
}
