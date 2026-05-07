using System.Net;
using System.Net.Http.Json;
using System.Text;
using Bogus;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Clients.BookAuditLogs.Serialization;
using PublishingPlatform.SDK.Clients.BookAuditLogs.Validation;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Tests.BookAuditLogs;

public sealed class BookAuditLogsClientTests
{
    [Fact]
    public async Task ListAsync_SendsGetWithExpectedRouteAndCancellation()
    {
        Randomizer.Seed = new Random(55);
        var faker = new Faker();
        var token = new CancellationTokenSource().Token;
        var request = new ListBookAuditLogsRequest
        {
            Page = 2,
            PageSize = 25,
            ContinuationToken = faker.Random.AlphaNumeric(8),
            BookId = $"book {faker.Random.Number(100, 999)}+{faker.Random.Number(1, 9)}",
            Action = "book.updated",
        };
        var expectedPath = BuildExpectedPath(request);
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Get,
            expectedPath,
            null,
            null,
            "BookAuditLogs.List",
            token).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new PagedResult<AuditLog>
                {
                    Items =
                    [
                        new AuditLog
                        {
                            Id = "log-1",
                            Action = "book.updated",
                            Timestamp = DateTimeOffset.UtcNow,
                        },
                    ],
                }),
            }));
        var sut = new BookAuditLogsClient(transport);

        var result = await sut.ListAsync(request, token);

        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("log-1");
        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            expectedPath,
            null,
            null,
            "BookAuditLogs.List",
            token);
    }

    [Fact]
    public async Task ListAsync_ThrowsArgumentNullException_ForNullRequest()
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        var sut = new BookAuditLogsClient(transport);

        var act = async () => await sut.ListAsync(null!);

        var ex = await act.Should().ThrowAsync<ArgumentNullException>();
        ex.Which.ParamName.Should().Be("request");
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Theory]
    [InlineData(-1, 10, "Page must be greater than or equal to 0.")]
    [InlineData(0, 0, "PageSize must be between 1 and 500.")]
    [InlineData(0, 501, "PageSize must be between 1 and 500.")]
    public async Task ListAsync_ThrowsBookValidationException_ForInvalidPagination(int page, int pageSize, string expectedMessage)
    {
        var transport = Substitute.For<ISharedHttpTransport>();
        var sut = new BookAuditLogsClient(transport);

        var act = async () => await sut.ListAsync(new ListBookAuditLogsRequest
        {
            Page = page,
            PageSize = pageSize,
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain(expectedMessage);
        await transport.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default, default, default!, default);
    }

    [Fact]
    public async Task ListAsync_OmitsOptionalFilters_WhenNullOrWhitespace()
    {
        var request = new ListBookAuditLogsRequest
        {
            Page = 0,
            PageSize = 50,
            ContinuationToken = " ",
            BookId = " ",
            Action = null,
        };
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Get,
            "/book-audit-logs?page=0&pageSize=50",
            null,
            null,
            "BookAuditLogs.List",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new PagedResult<AuditLog>
                {
                    Items = Array.Empty<AuditLog>(),
                }),
            }));
        var sut = new BookAuditLogsClient(transport);

        _ = await sut.ListAsync(request);

        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/book-audit-logs?page=0&pageSize=50",
            null,
            null,
            "BookAuditLogs.List",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAsync_ThrowsBookValidationException_WhenPayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookAuditLogsClient(transport);

        var act = async () => await sut.ListAsync(new ListBookAuditLogsRequest
        {
            Page = 0,
            PageSize = 10,
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Paged book audit logs payload was empty");
    }

    [Fact]
    public void QueryStringBuilder_BuildsDeterministicEncodedPath()
    {
        var builder = new DefaultBookAuditLogsQueryStringBuilder();
        var request = new ListBookAuditLogsRequest
        {
            Page = 3,
            PageSize = 15,
            ContinuationToken = "ct+1/2",
            BookId = "book 1/2",
            Action = "book.updated+published",
            ActorId = "actor-ignored",
            CorrelationId = "corr-ignored",
        };

        var path = builder.BuildListPath(request);

        path.Should().Be("/book-audit-logs?page=3&pageSize=15&continuationToken=ct%2B1%2F2&bookId=book%201%2F2&action=book.updated%2Bpublished");
        path.Should().NotContain("actorId=");
        path.Should().NotContain("from=");
        path.Should().NotContain("to=");
        path.Should().NotContain("correlationId=");
    }

    [Fact]
    public async Task ResponseReader_ThrowsBookValidationException_WhenPayloadIsNull()
    {
        var reader = new DefaultBookAuditLogsResponseReader();
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        };

        var act = async () => await reader.ReadListAsync(response, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Paged book audit logs payload was empty");
    }

    [Fact]
    public void Validator_ThrowsBookValidationException_WhenPageSizeIsOutOfRange()
    {
        var validator = new DefaultBookAuditLogsRequestValidator();
        var request = new ListBookAuditLogsRequest
        {
            Page = 0,
            PageSize = 700,
        };

        Action act = () => validator.ValidateList(request);

        var ex = act.Should().Throw<BookValidationException>();
        ex.Which.Message.Should().Contain("PageSize must be between 1 and 500");
    }

    private static string BuildExpectedPath(ListBookAuditLogsRequest request)
    {
        return $"/book-audit-logs?page={request.Page}&pageSize={request.PageSize}&continuationToken={Uri.EscapeDataString(request.ContinuationToken!)}&bookId={Uri.EscapeDataString(request.BookId)}&action={Uri.EscapeDataString(request.Action!)}";
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
