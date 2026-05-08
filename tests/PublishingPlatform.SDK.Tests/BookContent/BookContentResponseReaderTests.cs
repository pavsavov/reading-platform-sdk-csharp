using System.Net;
using System.Net.Http.Json;
using System.Text;
using PublishingPlatform.SDK.Clients.BookContent.Serialization;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.BookContent;

public sealed class BookContentResponseReaderTests
{
    [Fact]
    public async Task ReadUploadSessionAsync_Throws_WhenPayloadIsNull()
    {
        var reader = new DefaultBookContentResponseReader();
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json"),
        };

        var act = async () => await reader.ReadUploadSessionAsync(response, CancellationToken.None);
        var ex = await act.Should().ThrowAsync<BookValidationException>();
        ex.Which.Message.Should().Contain("Upload session payload was empty");
    }

    [Fact]
    public async Task ReadUploadChunkResultAsync_MapsPayload()
    {
        var reader = new DefaultBookContentResponseReader();
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new UploadChunkResult
            {
                UploadSessionId = "upl-1",
                AcceptedRangeStart = 0,
                AcceptedRangeEnd = 4,
                UploadedBytes = 5,
                IsComplete = false,
            }),
        };

        var result = await reader.ReadUploadChunkResultAsync(response, CancellationToken.None);
        result.UploadSessionId.Should().Be("upl-1");
        result.UploadedBytes.Should().Be(5);
    }
}
