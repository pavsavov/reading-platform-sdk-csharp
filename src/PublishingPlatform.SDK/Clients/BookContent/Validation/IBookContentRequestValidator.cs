namespace PublishingPlatform.SDK.Clients.BookContent.Validation;

using PublishingPlatform.SDK.Models;

internal interface IBookContentRequestValidator
{
    void ValidateBookId(string bookId);

    void ValidateUploadRequest(UploadBookContentRequest request);
}
