using System.Text;
using Bogus;
using PublishingPlatform.SDK.Clients.BookPublishing.Requests;
using PublishingPlatform.SDK.Clients.BookPublishing.Serialization;
using PublishingPlatform.SDK.Clients.BookPublishing.Validation;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.BookPublishing;

public sealed class BookPublishingCollaboratorsTests
{
    [Fact]
    public void Validator_RejectsWhitespaceNotes_WhenProvided()
    {
        var validator = new DefaultBookPublishingRequestValidator();
        var request = new PublishBookRequest
        {
            Notes = "   ",
        };

        Action act = () => validator.ValidatePublish(request);

        act.Should().Throw<BookValidationException>()
            .WithMessage("*Notes cannot be empty*");
    }

    [Fact]
    public void Validator_RejectsDefaultScheduleDate()
    {
        var validator = new DefaultBookPublishingRequestValidator();

        Action act = () => validator.ValidateSchedule(new ScheduleBookPublishingRequest());

        act.Should().Throw<BookValidationException>()
            .WithMessage("*ScheduledAt is required*");
    }

    [Fact]
    public void HeadersFactory_ReturnsNull_WhenIdempotencyKeyMissing()
    {
        var factory = new DefaultBookPublishingRequestHeadersFactory();

        var headers = factory.CreateIdempotencyHeaders(null);

        headers.Should().BeNull();
    }

    [Fact]
    public void HeadersFactory_EmitsIdempotencyHeader_WhenKeyProvided()
    {
        var factory = new DefaultBookPublishingRequestHeadersFactory();
        var idempotencyKey = new Faker().Random.Guid().ToString("N");

        var headers = factory.CreateIdempotencyHeaders(idempotencyKey);

        headers.Should().NotBeNull();
        headers!["Idempotency-Key"].Should().Be(idempotencyKey);
    }

    [Fact]
    public async Task ResponseReader_Throws_WhenPayloadIsNull()
    {
        var reader = new DefaultBookPublishingResponseReader();
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        };

        Func<Task> act = async () => await reader.ReadStatusAsync(response, CancellationToken.None);

        _ = await act.Should().ThrowAsync<BookValidationException>()
            .WithMessage("*payload was empty*");
    }
}
