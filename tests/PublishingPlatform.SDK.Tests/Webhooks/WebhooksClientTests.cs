using System.Net;
using System.Net.Http.Json;
using System.Text;
using Bogus;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Clients.Webhooks.Requests;
using PublishingPlatform.SDK.Clients.Webhooks.Serialization;
using PublishingPlatform.SDK.Clients.Webhooks.Validation;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Tests.Webhooks;

public sealed class WebhooksClientTests
{
    [Fact]
    public async Task RegisterAsync_SendsPostWithIdempotencyHeaderAndCancellation()
    {
        Randomizer.Seed = new Random(56);
        var token = new CancellationTokenSource().Token;
        var request = CreateRegisterRequest();
        var idempotencyKey = "webhook-registration-001";
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Post,
            "/webhooks",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(headers => headers["Idempotency-Key"] == idempotencyKey),
            "Webhooks.Register",
            token).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(CreateWebhook("whk-1")),
            }));
        var sut = new WebhooksClient(transport);

        var result = await sut.RegisterAsync(request, idempotencyKey, token);

        result.Id.Should().Be("whk-1");
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/webhooks",
            Arg.Any<HttpContent>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(headers => headers["Idempotency-Key"] == idempotencyKey),
            "Webhooks.Register",
            token);
    }

    [Fact]
    public async Task UpdateAsync_SendsPatchWithEncodedWebhookId()
    {
        var request = new UpdateWebhookRequest
        {
            WebhookId = "whk 1/2",
            EndpointUrl = "https://hooks.example.com/platform",
            Events = ["book.published"],
            IsActive = true,
        };
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Patch,
            "/webhooks/whk%201%2F2",
            Arg.Any<HttpContent>(),
            null,
            "Webhooks.Update",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(CreateWebhook("whk 1/2")),
            }));
        var sut = new WebhooksClient(transport);

        var result = await sut.UpdateAsync(request);

        result.Id.Should().Be("whk 1/2");
        await transport.Received(1).SendAsync(
            HttpMethod.Patch,
            "/webhooks/whk%201%2F2",
            Arg.Any<HttpContent>(),
            null,
            "Webhooks.Update",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteWithEncodedWebhookId()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Delete,
            "/webhooks/whk%2B1%2F2",
            null,
            null,
            "Webhooks.Delete",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent)));
        var sut = new WebhooksClient(transport);

        await sut.DeleteAsync("whk+1/2");

        await transport.Received(1).SendAsync(
            HttpMethod.Delete,
            "/webhooks/whk%2B1%2F2",
            null,
            null,
            "Webhooks.Delete",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAsync_BuildsDeterministicEncodedQuery()
    {
        var request = new ListWebhooksRequest
        {
            PageSize = 25,
            ContinuationToken = "ct+1/2",
            Event = "book.published+distribution",
            IsActive = true,
        };
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Get,
            "/webhooks?pageSize=25&continuationToken=ct%2B1%2F2&event=book.published%2Bdistribution&isActive=true",
            null,
            null,
            "Webhooks.List",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new PagedResult<Webhook>
                {
                    Items = [CreateWebhook("whk-list")],
                    TotalCount = 1,
                }),
            }));
        var sut = new WebhooksClient(transport);

        var result = await sut.ListAsync(request);

        result.Items.Should().ContainSingle();
        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/webhooks?pageSize=25&continuationToken=ct%2B1%2F2&event=book.published%2Bdistribution&isActive=true",
            null,
            null,
            "Webhooks.List",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_ThrowsArgumentNullException_ForNullRequest()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        var sut = new WebhooksClient(transport);

        var act = async () => await sut.RegisterAsync(null!);

        var ex = await act.Should().ThrowAsync<ArgumentNullException>();
        ex.Which.ParamName.Should().Be("request");
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("http://hooks.example.com/platform")]
    [InlineData("not-a-url")]
    public async Task RegisterAsync_ThrowsBookValidationException_ForInvalidEndpointUrl(string endpointUrl)
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        var sut = new WebhooksClient(transport);
        var request = CreateRegisterRequest();
        request.EndpointUrl = endpointUrl;

        var act = async () => await sut.RegisterAsync(request);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Webhook endpoint URL");
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsBookValidationException_WhenEventsAreEmpty()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        var sut = new WebhooksClient(transport);

        var act = async () => await sut.UpdateAsync(new UpdateWebhookRequest
        {
            WebhookId = "whk-1",
            EndpointUrl = "https://hooks.example.com/platform",
            Events = [],
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("At least one webhook event");
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public async Task ListAsync_ThrowsBookValidationException_ForInvalidPageSize(int pageSize)
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        var sut = new WebhooksClient(transport);

        var act = async () => await sut.ListAsync(new ListWebhooksRequest
        {
            PageSize = pageSize,
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("PageSize must be between 1 and 500");
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsBookValidationException_WhenPayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(
            client,
            new NoOpPublishingPlatformResiliencePipeline(),
            new FixedCorrelationIdProvider(),
            new DefaultPublishingPlatformErrorMapper());
        var sut = new WebhooksClient(transport);

        var act = async () => await sut.RegisterAsync(CreateRegisterRequest());

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Webhook payload was empty");
    }

    [Fact]
    public async Task ListAsync_ThrowsBookValidationException_WhenPagedPayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(
            client,
            new NoOpPublishingPlatformResiliencePipeline(),
            new FixedCorrelationIdProvider(),
            new DefaultPublishingPlatformErrorMapper());
        var sut = new WebhooksClient(transport);

        var act = async () => await sut.ListAsync(new ListWebhooksRequest());

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Paged webhooks payload was empty");
    }

    [Fact]
    public void QueryStringBuilder_OmitsOptionalFilters_WhenMissing()
    {
        var builder = new DefaultWebhookQueryStringBuilder();

        var path = builder.BuildListPath(new ListWebhooksRequest
        {
            PageSize = 50,
            ContinuationToken = " ",
            Event = null,
        });

        path.Should().Be("/webhooks?pageSize=50");
    }

    [Fact]
    public void HeadersFactory_ReturnsNull_WhenIdempotencyKeyIsMissing()
    {
        var factory = new DefaultWebhookRequestHeadersFactory();

        var headers = factory.CreateIdempotencyHeaders(" ");

        headers.Should().BeNull();
    }

    [Fact]
    public void Validator_ThrowsBookValidationException_WhenWebhookIdIsMissing()
    {
        var validator = new DefaultWebhookRequestValidator();

        Action act = () => validator.ValidateWebhookId(" ");

        var ex = act.Should().Throw<BookValidationException>();
        ex.Which.Message.Should().Contain("Webhook id is required");
    }

    [Fact]
    public void Validator_ThrowsBookValidationException_WhenEventIsWhitespace()
    {
        var validator = new DefaultWebhookRequestValidator();

        Action act = () => validator.ValidateRegister(new RegisterWebhookRequest
        {
            EndpointUrl = "https://hooks.example.com/platform",
            Events = ["book.published", " "],
        });

        var ex = act.Should().Throw<BookValidationException>();
        ex.Which.Message.Should().Contain("Webhook events cannot contain empty values");
    }

    private static RegisterWebhookRequest CreateRegisterRequest()
    {
        var faker = new Faker();

        return new RegisterWebhookRequest
        {
            EndpointUrl = "https://hooks.example.com/publishing-platform",
            Events = [$"book.{faker.Hacker.Verb()}", $"book.{faker.Hacker.Verb()}"],
            IsActive = true,
            SigningKeyId = "signing-key-01",
        };
    }

    private static Webhook CreateWebhook(string id)
    {
        return new Webhook
        {
            Id = id,
            EndpointUrl = "https://hooks.example.com/publishing-platform",
            Events = ["book.published"],
            IsActive = true,
            SigningKeyId = "signing-key-01",
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    private sealed class SingleResponseHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public SingleResponseHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }

    private sealed class FixedCorrelationIdProvider : ICorrelationIdProvider
    {
        public string Create()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
