using System.Net;
using System.Net.Http.Json;
using System.Text;
using PublishingPlatform.SDK.Clients;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Errors;
using PublishingPlatform.SDK.Infrastructure.Transport;
using PublishingPlatform.SDK.Internal.Resilience;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Tests.BookAccess;

public sealed class BookAccessClientTests
{
    [Fact]
    public async Task GrantAsync_SendsPostWithExpectedRouteAndCancellation()
    {
        var token = new CancellationTokenSource().Token;
        var request = new BookAccessGrantRequest
        {
            BookId = "book-1",
            PrincipalId = "user-1",
            PrincipalType = "user",
            AccessLevel = "read",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
        };
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Post,
            "/books/book-1/access",
            Arg.Any<HttpContent>(),
            null,
            "BookAccess.Grant",
            token).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookAccessGrantResult
                {
                    GrantId = "grant-1",
                    BookId = request.BookId,
                    PrincipalId = request.PrincipalId,
                    PrincipalType = request.PrincipalType,
                    AccessLevel = request.AccessLevel,
                    Created = true,
                }),
            }));
        var sut = new BookAccessClient(transport);

        var result = await sut.GrantAsync(request, token);

        result.GrantId.Should().Be("grant-1");
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books/book-1/access",
            Arg.Any<HttpContent>(),
            null,
            "BookAccess.Grant",
            token);
    }

    [Fact]
    public async Task RevokeAsync_SendsPostWithExpectedRoute()
    {
        var request = new BookAccessRevokeRequest
        {
            BookId = "book-2",
            PrincipalId = "user-2",
            PrincipalType = "user",
            Reason = "refund",
        };
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Post,
            "/books/book-2/access/revoke",
            Arg.Any<HttpContent>(),
            null,
            "BookAccess.Revoke",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookAccessRevokeResult
                {
                    BookId = request.BookId,
                    PrincipalId = request.PrincipalId,
                    PrincipalType = request.PrincipalType,
                    Revoked = true,
                }),
            }));
        var sut = new BookAccessClient(transport);

        var result = await sut.RevokeAsync(request);

        result.Revoked.Should().BeTrue();
        await transport.Received(1).SendAsync(
            HttpMethod.Post,
            "/books/book-2/access/revoke",
            Arg.Any<HttpContent>(),
            null,
            "BookAccess.Revoke",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckAsync_BuildsDeterministicQuery()
    {
        var request = new BookAccessCheckRequest
        {
            BookId = "book 3",
            PrincipalId = "user+3",
            PrincipalType = "user",
        };
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Get,
            "/books/book%203/access/check?principalId=user%2B3&principalType=user",
            null,
            null,
            "BookAccess.Check",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new BookAccessStatus
                {
                    BookId = request.BookId,
                    PrincipalId = request.PrincipalId,
                    PrincipalType = request.PrincipalType,
                    HasAccess = true,
                    AccessLevel = "read",
                }),
            }));
        var sut = new BookAccessClient(transport);

        _ = await sut.CheckAsync(request);

        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/books/book%203/access/check?principalId=user%2B3&principalType=user",
            null,
            null,
            "BookAccess.Check",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAsync_UsesPerBookRoute_WhenBookIdIsProvided()
    {
        var request = new ListBookAccessRequest
        {
            BookId = "book-4",
            PrincipalType = "user",
            PageSize = 20,
            ContinuationToken = "ct-1",
        };
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Get,
            "/books/book-4/access?pageSize=20&continuationToken=ct-1&principalType=user",
            null,
            null,
            "BookAccess.List",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new PagedResult<BookAccessGrant>
                {
                    Items = Array.Empty<BookAccessGrant>(),
                }),
            }));
        var sut = new BookAccessClient(transport);

        _ = await sut.ListAsync(request);

        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/books/book-4/access?pageSize=20&continuationToken=ct-1&principalType=user",
            null,
            null,
            "BookAccess.List",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAsync_UsesGlobalRoute_WhenBookIdIsMissing()
    {
        var request = new ListBookAccessRequest
        {
            PrincipalId = "org-1",
            PrincipalType = "organization",
            AccessLevel = "read",
            PageSize = 10,
        };
        var transport = Substitute.For<ISharedHttpTransport>();
        transport.SendAsync(
            HttpMethod.Get,
            "/book-access?pageSize=10&principalId=org-1&principalType=organization&accessLevel=read",
            null,
            null,
            "BookAccess.List",
            Arg.Any<CancellationToken>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new PagedResult<BookAccessGrant>
                {
                    Items = Array.Empty<BookAccessGrant>(),
                }),
            }));
        var sut = new BookAccessClient(transport);

        _ = await sut.ListAsync(request);

        await transport.Received(1).SendAsync(
            HttpMethod.Get,
            "/book-access?pageSize=10&principalId=org-1&principalType=organization&accessLevel=read",
            null,
            null,
            "BookAccess.List",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GrantAsync_ThrowsBookValidationException_ForInvalidPrincipalType()
    {
        var sut = new BookAccessClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.GrantAsync(new BookAccessGrantRequest
        {
            BookId = "book-1",
            PrincipalId = "principal-1",
            PrincipalType = "tenant",
            AccessLevel = "read",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("PrincipalType must be one of");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public async Task ListAsync_ThrowsBookValidationException_ForInvalidPageSize(int pageSize)
    {
        var sut = new BookAccessClient(Substitute.For<ISharedHttpTransport>());

        var act = async () => await sut.ListAsync(new ListBookAccessRequest
        {
            PageSize = pageSize,
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("PageSize must be between 1 and 200");
    }

    [Fact]
    public async Task CheckAsync_ThrowsBookValidationException_WhenPayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookAccessClient(transport);

        var act = async () => await sut.CheckAsync(new BookAccessCheckRequest
        {
            BookId = "book-null",
            PrincipalId = "user-null",
            PrincipalType = "user",
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Book access status payload was empty");
    }

    [Fact]
    public async Task ListAsync_ThrowsBookValidationException_WhenPagedPayloadIsNull()
    {
        using var handler = new SingleResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookAccessClient(transport);

        var act = async () => await sut.ListAsync(new ListBookAccessRequest
        {
            PageSize = 10,
        });

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Paged book access payload was empty");
    }

    [Fact]
    public async Task ListAllAsync_YieldsItemsAcrossPages()
    {
        using var handler = new SequenceHandler(
        [
            JsonContent.Create(new PagedResult<BookAccessGrant>
            {
                Items = [new BookAccessGrant { GrantId = "grant-1", BookId = "book-1", PrincipalId = "user-1", PrincipalType = "user", AccessLevel = "read" }],
                ContinuationToken = "next-token",
            }),
            JsonContent.Create(new PagedResult<BookAccessGrant>
            {
                Items = [new BookAccessGrant { GrantId = "grant-2", BookId = "book-1", PrincipalId = "user-2", PrincipalType = "user", AccessLevel = "write" }],
            }),
        ]);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var transport = new SharedHttpTransport(client, new NoOpPublishingPlatformResiliencePipeline(), new FixedCorrelationIdProvider(), new DefaultPublishingPlatformErrorMapper());
        var sut = new BookAccessClient(transport);

        var grantIds = new List<string>();
        await foreach (var grant in sut.ListAllAsync(new ListBookAccessRequest { PageSize = 10 }))
        {
            grantIds.Add(grant.GrantId);
        }

        grantIds.Should().Equal("grant-1", "grant-2");
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

    private sealed class SequenceHandler : HttpMessageHandler
    {
        private readonly Queue<HttpContent> _responses;

        public SequenceHandler(IEnumerable<HttpContent> responses)
        {
            _responses = new Queue<HttpContent>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = _responses.Dequeue();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
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
