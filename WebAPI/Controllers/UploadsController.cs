using ApiContracts;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UploadsController : ControllerBase
{
    private const long MaxImageSize = 5 * 1024 * 1024;
    private const long MaxVideoSize = 50 * 1024 * 1024;

    private static readonly Dictionary<string, (string ContentType, string Kind)> allowedTypes = new()
    {
        [".png"] = ("image/png", "image"),
        [".jpg"] = ("image/jpeg", "image"),
        [".jpeg"] = ("image/jpeg", "image"),
        [".webp"] = ("image/webp", "image"),
        [".gif"] = ("image/gif", "gif"),
        [".mp4"] = ("video/mp4", "video"),
        [".webm"] = ("video/webm", "video")
    };

    private readonly IWebHostEnvironment environment;

    public UploadsController(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }

    [HttpPost]
    [RequestSizeLimit(MaxVideoSize + 256 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxVideoSize + 256 * 1024)]
    public async Task<ActionResult<UploadResultDto>> Upload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("No file was uploaded.");
        }

        string extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedTypes.TryGetValue(extension, out (string ContentType, string Kind) type) || file.ContentType != type.ContentType)
        {
            throw new ArgumentException("Only png, jpg, webp, gif, mp4 and webm files are allowed.");
        }

        long limit = type.Kind == "video" ? MaxVideoSize : MaxImageSize;
        if (file.Length > limit)
        {
            throw new ArgumentException(type.Kind == "video" ? "Videos can be at most 50 MB." : "Images can be at most 5 MB.");
        }

        string uploadsFolder = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsFolder);

        string fileName = $"{Guid.NewGuid():N}{extension}";
        await using (FileStream stream = System.IO.File.Create(Path.Combine(uploadsFolder, fileName)))
        {
            await file.CopyToAsync(stream);
        }

        string url = $"/uploads/{fileName}";
        return Created(url, new UploadResultDto { Url = url, Kind = type.Kind });
    }
}
