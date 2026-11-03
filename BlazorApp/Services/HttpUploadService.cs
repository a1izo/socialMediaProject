using System.Net.Http.Headers;
using ApiContracts;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp.Services;

public class HttpUploadService : IUploadService
{
    private readonly HttpClient client;

    public HttpUploadService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<string> UploadImageAsync(IBrowserFile file)
    {
        if (file.Size > IUploadService.MaxFileSize)
        {
            throw new Exception("Images can be at most 5 MB.");
        }

        await using Stream stream = file.OpenReadStream(IUploadService.MaxFileSize);
        using StreamContent fileContent = new(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        using MultipartFormDataContent form = new();
        form.Add(fileContent, "file", file.Name);

        HttpResponseMessage httpResponse = await client.PostAsync("uploads", form);
        UploadResultDto result = await HttpResponseReader.ReadAsync<UploadResultDto>(httpResponse);
        return result.Url;
    }
}
