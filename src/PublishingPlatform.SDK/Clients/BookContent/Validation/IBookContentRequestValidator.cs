namespace PublishingPlatform.SDK.Clients.BookContent.Validation;

using PublishingPlatform.SDK.Models;

internal interface IBookContentRequestValidator
{
    void ValidateBookId(string bookId);

    void ValidateUploadRequest(UploadBookContentRequest request);

    void ValidateUploadSessionId(string uploadSessionId);

    void ValidateStartResumableUploadRequest(StartResumableUploadRequest request);

    void ValidateUploadChunkRequest(UploadChunkRequest request);
}
