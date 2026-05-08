using System.Net;
using System.Text;
using PublishingPlatform.SDK.Clients.Common.Requests;
using PublishingPlatform.SDK.Clients.Common.Serialization;
using PublishingPlatform.SDK.Clients.Common.Validation;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Infrastructure.Transport;

namespace PublishingPlatform.SDK.Tests.Infrastructure;

public sealed class CommonHelpersTests
{
    [Fact]
    public void RequestHeadersFactoryHelper_ReturnsNull_WhenIdempotencyKeyMissing()
    {
        var headers = RequestHeadersFactoryHelper.CreateIdempotencyHeaders(" ");

        headers.Should().BeNull();
    }

    [Fact]
    public void RequestHeadersFactoryHelper_ReturnsHeader_WhenIdempotencyKeyProvided()
    {
        var headers = RequestHeadersFactoryHelper.CreateIdempotencyHeaders("idem-001");

        headers.Should().NotBeNull();
        headers![TransportHeaderNames.IdempotencyKey].Should().Be("idem-001");
    }

    [Fact]
    public async Task ResponseReaderHelper_ThrowsBookValidationException_WhenPayloadMissing()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        };

        var act = async () => await ResponseReaderHelper.ReadRequiredAsync<object>(response, "payload missing", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Be("payload missing");
    }

    [Fact]
    public void ValidationGuards_ThrowsBookValidationException_WhenBookIdMissing()
    {
        Action act = () => ValidationGuards.ValidateBookId(" ");

        var ex = act.Should().Throw<BookValidationException>();
        ex.Which.Message.Should().Be(ValidationMessages.BookIdRequired);
    }
}
