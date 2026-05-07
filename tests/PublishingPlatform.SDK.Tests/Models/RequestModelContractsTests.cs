using Bogus;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.ModelContracts;

public sealed class RequestModelContractsTests
{
    [Fact]
    public void BookAccessGrantRequest_Defaults_AreSafe()
    {
        var request = new BookAccessGrantRequest();

        request.BookId.Should().BeEmpty();
        request.PrincipalId.Should().BeEmpty();
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
    public void AssetOperationRequests_ExposeIdempotencyAndIdentityFields()
    {
        var uploadRequest = new UploadBookAssetRequest
        {
            BookId = "book-01",
            AssetType = "cover",
            FileName = "cover.png",
            ContentType = "image/png",
            IdempotencyKey = "asset-upload-001",
        };

        var deleteRequest = new DeleteBookAssetRequest
        {
            BookId = uploadRequest.BookId,
            AssetId = "asset-01",
            Reason = "replace",
        };

        uploadRequest.IdempotencyKey.Should().Be("asset-upload-001");
        deleteRequest.AssetId.Should().Be("asset-01");
    }
}
