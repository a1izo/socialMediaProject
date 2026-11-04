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

    public async Task<UploadResultDto> UploadAsync(IBrowserFile file)
    {
        bool isVideo = file.ContentType.StartsWith("video/");
        long limit = isVideo ? IUploadService.MaxVideoSize : IUploadService.MaxImageSize;
        if (file.Size > limit)
        {
            throw new Exception(isVideo ? "Videos can be at most 50 MB." : "Images can be at most 5 MB.");
        }

        await using Stream stream = file.OpenReadStream(limit);
        using StreamContent fileContent = new(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        using MultipartFormDataContent form = new();
        form.Add(fileContent, "file", file.Name);

        HttpResponseMessage httpResponse = await client.PostAsync("uploads", form);
        return await HttpResponseReader.ReadAsync<UploadResultDto>(httpResponse);
    }
}
