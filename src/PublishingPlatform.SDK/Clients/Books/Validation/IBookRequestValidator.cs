using PublishingPlatform.SDK.Models;

namespace PublishingPlatform.SDK.Clients.Books.Validation;

internal interface IBookRequestValidator
{
    void ValidateBookId(string bookId);

    void ValidateCreate(CreateBookRequest request);

    void ValidateList(ListBooksRequest request);

    void ValidateUpdate(UpdateBookMetadataRequest request);

    void ValidatePatch(UpdateBookPatchRequest request);
}
