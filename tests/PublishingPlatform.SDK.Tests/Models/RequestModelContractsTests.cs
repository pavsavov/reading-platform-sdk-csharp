using Bogus;
using PublishingPlatform.SDK.Models;
using PublishingPlatform.SDK.Models.Common;

namespace PublishingPlatform.SDK.Tests.ModelContracts;

public sealed class RequestModelContractsTests
{
    [Fact]
    public void BookAccessGrantRequest_Defaults_AreSafe()
    {
        var request = new BookAccessGrantRequest();

        request.BookId.Should().BeEmpty();
        request.PrincipalId.Should().BeEmpty();
        request.PrincipalType.Should().Be("user");
        request.AccessLevel.Should().BeEmpty();
        request.ExpiresAt.Should().BeNull();
        request.IdempotencyKey.Should().BeNull();
    }

    [Fact]
    public void GetBookAnalyticsRequest_Defaults_AreStable()
    {
        var request = new GetBookAnalyticsRequest();

        request.BookId.Should().BeEmpty();
        request.Granularity.Should().Be("day");
        request.IncludeUniqueReaders.Should().BeTrue();
    }

    [Fact]
    public void ListBookAuditLogsRequest_Defaults_AreStable()
    {
        var request = new ListBookAuditLogsRequest();

        request.Page.Should().Be(0);
        request.PageSize.Should().Be(50);
        request.ContinuationToken.Should().BeNull();
        request.ActorId.Should().BeNull();
        request.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void WebhookRequests_SupportStronglyTypedEventLists()
    {
        Randomizer.Seed = new Random(17);
        var faker = new Faker();
        var events = new[] { faker.Hacker.Verb(), faker.Hacker.Verb() };

        var registerRequest = new RegisterWebhookRequest
        {
            EndpointUrl = "https://hooks.example.com/publishing-platform",
            Events = events,
            SigningKeyId = "kid-01",
        };

        var updateRequest = new UpdateWebhookRequest
        {
            WebhookId = "whk-001",
            EndpointUrl = registerRequest.EndpointUrl,
            Events = registerRequest.Events,
            IsActive = false,
        };

        registerRequest.Events.Should().HaveCount(2);
        registerRequest.IsActive.Should().BeTrue();
        updateRequest.WebhookId.Should().Be("whk-001");
        updateRequest.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ListWebhooksRequest_Defaults_AreSafe()
    {
        var request = new ListWebhooksRequest();

        request.PageSize.Should().Be(50);
        request.ContinuationToken.Should().BeNull();
        request.Event.Should().BeNull();
        request.IsActive.Should().BeNull();
    }

    [Fact]
    public void Webhook_Defaults_AreSafe()
    {
        var webhook = new Webhook();

        webhook.Id.Should().BeEmpty();
        webhook.EndpointUrl.Should().BeEmpty();
        webhook.Events.Should().BeEmpty();
        webhook.IsActive.Should().BeFalse();
        webhook.SigningKeyId.Should().BeNull();
        webhook.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void BookAccessRevokeRequest_Defaults_AreSafe()
    {
        var request = new BookAccessRevokeRequest();

        request.BookId.Should().BeEmpty();
        request.PrincipalId.Should().BeEmpty();
        request.PrincipalType.Should().Be("user");
        request.Reason.Should().BeNull();
    }

    [Fact]
    public void BookAccessCheckRequest_Defaults_AreSafe()
    {
        var request = new BookAccessCheckRequest();

        request.BookId.Should().BeEmpty();
        request.PrincipalId.Should().BeEmpty();
        request.PrincipalType.Should().Be("user");
    }

    [Fact]
    public void ListBookAccessRequest_Defaults_AreSafe()
    {
        var request = new ListBookAccessRequest();

        request.Should().BeAssignableTo<PaginationRequest>();
        request.BookId.Should().BeNull();
        request.PageSize.Should().Be(50);
        request.ContinuationToken.Should().BeNull();
    }

    [Fact]
    public void PaginationBackedRequests_ReuseSharedPaginationContract()
    {
        new ListBooksRequest().Should().BeAssignableTo<PaginationRequest>();
        new ListBookAuditLogsRequest().Should().BeAssignableTo<PaginationRequest>();
        new ListWebhooksRequest().Should().BeAssignableTo<PaginationRequest>();
    }

    [Fact]
    public void RequestModels_AssignedValues_ArePreserved()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-7);
        var to = DateTimeOffset.UtcNow;

        var analytics = new GetBookAnalyticsRequest
        {
            BookId = "book-22",
            From = from,
            To = to,
            Granularity = "week",
            IncludeUniqueReaders = false,
        };

        var audit = new ListBookAuditLogsRequest
        {
            BookId = "book-22",
            ActorId = "actor-1",
            Action = "book.updated",
            From = from,
            To = to,
            CorrelationId = "corr-1",
            Page = 2,
            PageSize = 25,
        };

        analytics.BookId.Should().Be("book-22");
        analytics.Granularity.Should().Be("week");
        analytics.IncludeUniqueReaders.Should().BeFalse();
        audit.Page.Should().Be(2);
        audit.PageSize.Should().Be(25);
        audit.CorrelationId.Should().Be("corr-1");
    }
}
