using Bogus;
using MockPublishingPlatform.Api.Models;
using MockPublishingPlatform.Api.Services;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class MockApiContractsTests
{
    [Fact]
    public void FixtureLoader_LoadsDeterministicState_FromFixtureFiles()
    {
        var fixturesPath = ResolveFixturesPath();

        var state = FixtureLoader.Load(fixturesPath);

        state.Books.Should().NotBeEmpty();
        state.Webhooks.Should().NotBeEmpty();
        state.PublishingStatuses.Should().ContainKey("book-0002");
        state.PublishingStatuses["book-0002"].FailureReason.Should().NotBeNullOrWhiteSpace();
        state.DistributionOperations.Values.Should().Contain(operation => operation.Status == "completed");
        state.DistributionOperations.Values.Should().Contain(operation => operation.Status == "failed");
        state.Webhooks.Should().Contain(webhook => webhook.IsActive == false);
        state.ApiErrors.Should().ContainKey("notFound");
    }

    [Fact]
    public void IdempotencyHelper_ReplaysStoredResult_WhenOperationKeyExists()
    {
        var state = new MockApiState();
        var faker = new Faker("en");
        var operationKey = faker.Random.Hash();
        var expected = new ApiErrorFixture
        {
            StatusCode = 400,
            ErrorCode = "validation_error",
            Message = "Validation failed",
        };
        state.IdempotencyResponses[operationKey] = expected;

        var replayed = IdempotencyHelper.TryReplay(state, operationKey, out ApiErrorFixture result);

        replayed.Should().BeTrue();
        result.Should().NotBeNull();
        result.ErrorCode.Should().Be("validation_error");
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("offset:15", 15)]
    [InlineData("offset:-1", 0)]
    [InlineData("invalid", 0)]
    public void QueryParser_ParsesContinuationOffset_WithSafeFallbacks(string? value, int expected)
    {
        var parsed = QueryParser.ParseContinuationOffset(value);
        parsed.Should().Be(expected);
    }

    [Fact]
    public void UploadSessionState_CanTrackPartialAndCompleteProgress()
    {
        var session = new UploadSessionState
        {
            UploadSessionId = "upl-1",
            BookId = "book-1",
            FileName = "demo.epub",
            TotalBytes = 10,
            UploadedBytes = 0,
            Status = "pending",
        };

        session.UploadedBytes += 4;
        session.Status = "in_progress";
        session.UploadedBytes += 6;
        session.Status = session.UploadedBytes == session.TotalBytes ? "completed" : "in_progress";

        session.UploadedBytes.Should().Be(10);
        session.Status.Should().Be("completed");
    }

    private static string ResolveFixturesPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "examples", "MockPublishingPlatform.Api", "Fixtures");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate mock API fixtures directory.");
    }
}
