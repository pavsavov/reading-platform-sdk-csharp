using PublishingPlatform.SDK.Clients.BookContent.Validation;
using PublishingPlatform.SDK.Exceptions;
using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Tests.BookContent;

public sealed class BookContentResumableValidatorTests
{
    private static readonly DefaultBookContentRequestValidator Sut = new();

    [Fact]
    public void ValidateUploadSessionId_Throws_WhenMissing()
    {
        var act = () => Sut.ValidateUploadSessionId(" ");
        act.Should().Throw<BookValidationException>().WithMessage("*Upload session id is required*");
    }

    [Fact]
    public void ValidateStartResumableUploadRequest_Throws_WhenTotalBytesInvalid()
    {
        var act = () => Sut.ValidateStartResumableUploadRequest(new StartResumableUploadRequest
        {
            FileName = "demo.epub",
            TotalBytes = 0,
        });

        act.Should().Throw<BookValidationException>().WithMessage("*Total bytes must be greater than zero*");
    }

    [Fact]
    public async Task ValidateUploadChunkRequest_Throws_WhenRangeInvalid()
    {
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var act = () => Sut.ValidateUploadChunkRequest(new UploadChunkRequest
        {
            Chunk = stream,
            ChunkStart = 10,
            ChunkEnd = 5,
            TotalBytes = 20,
        });

        act.Should().Throw<BookValidationException>().WithMessage("*Chunk start must be less than or equal to chunk end*");
    }

    [Fact]
    public async Task ValidateUploadChunkRequest_Throws_WhenChunkEndOutOfBounds()
    {
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var act = () => Sut.ValidateUploadChunkRequest(new UploadChunkRequest
        {
            Chunk = stream,
            ChunkStart = 0,
            ChunkEnd = 20,
            TotalBytes = 20,
        });

        act.Should().Throw<BookValidationException>().WithMessage("*Chunk end must be less than total bytes*");
    }
}
