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

        var message = await reader.ReadAsync(response, CancellationToken.None);

        message.Should().Contain("HTTP 502");
    }

    [Fact]
    public async Task ResponseErrorReader_UsesPayloadMessage_WhenPresent()
    {
        var reader = new DefaultTransportResponseErrorReader();
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new { Message = "invalid" }),
        };

        var message = await reader.ReadAsync(response, CancellationToken.None);

        message.Should().Be("invalid");
    }

    [Fact]
    public void ErrorContextFactory_PreservesOperationName()
    {
        var factory = new DefaultTransportErrorContextFactory();

        PublishingPlatformErrorContext context = factory.Create(HttpMethod.Get, "/books", 500, "x", "corr", "Books.List");

        context.OperationName.Should().Be("Books.List");
        context.CorrelationId.Should().Be("corr");
    }
}
