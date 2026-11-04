using ApiContracts;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp.Services;

public interface IUploadService
{
    public const long MaxImageSize = 5 * 1024 * 1024;
    public const long MaxVideoSize = 50 * 1024 * 1024;

    public const string ImageTypes = "image/png,image/jpeg,image/gif,image/webp";
    public const string MediaTypes = "image/png,image/jpeg,image/gif,image/webp,video/mp4,video/webm";

    // stores the file on the Web API and returns its path (e.g. "/uploads/abc.png") and kind
    public Task<UploadResultDto> UploadAsync(IBrowserFile file);
}
