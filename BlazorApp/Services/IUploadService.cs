using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp.Services;

public interface IUploadService
{
    public const long MaxFileSize = 5 * 1024 * 1024;

    // returns the path of the stored image, e.g. "/uploads/abc.png"
    public Task<string> UploadImageAsync(IBrowserFile file);
}
