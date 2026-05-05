namespace PublishingPlatform.SDK.Clients.BookContent.Serialization;

using PublishingPlatform.SDK.Models;

internal interface IBookContentMultipartFormFactory
{
    MultipartFormDataContent Create(UploadBookContentRequest request);
}
