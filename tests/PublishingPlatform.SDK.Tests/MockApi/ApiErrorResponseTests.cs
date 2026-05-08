using MockPublishingPlatform.Api.Models;

namespace PublishingPlatform.SDK.Tests.MockApi;

public sealed class ApiErrorResponseTests
{
    [Fact]
    public void FromFixture_MapsFieldsAndRequestId()
    {
        var fixture = new ApiErrorFixture
        {
            StatusCode = 400,
            ErrorCode = "validation_error",
            Message = "Validation failed",
        };

        var result = ApiErrorResponse.FromFixture(fixture, "req-123");

        result.Message.Should().Be("Validation failed");
        result.ErrorCode.Should().Be("validation_error");
        result.RequestId.Should().Be("req-123");
        result.CorrelationId.Should().BeNull();
    }
}

