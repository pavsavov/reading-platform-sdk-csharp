namespace PublishingPlatform.SDK.Clients.BookContent.Serialization;

using System.Net.Http.Headers;
using PublishingPlatform.SDK.Models;

internal sealed class DefaultBookContentMultipartFormFactory : IBookContentMultipartFormFactory
{
    public MultipartFormDataContent Create(UploadBookContentRequest request)
    {
        var form = new MultipartFormDataContent();

        var fileContent = new StreamContent(request.File);
        if (!string.IsNullOrWhiteSpace(request.ContentType))
        {
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);
        }

        form.Add(fileContent, "file", request.FileName);

        if (!string.IsNullOrWhiteSpace(request.Format))
        {
            form.Add(new StringContent(request.Format), "format");
        }

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            form.Add(new StringContent(request.IdempotencyKey), "idempotencyKey");
        }

        return form;
    }
}
