using ApiContracts;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UploadsController : ControllerBase
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string> allowedTypes = new()
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp"
    };

    private readonly IWebHostEnvironment environment;

    public UploadsController(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }

    [HttpPost]
    [RequestSizeLimit(MaxFileSize + 64 * 1024)]
    public async Task<ActionResult<UploadResultDto>> Upload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("No file was uploaded.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException("Images can be at most 5 MB.");
        }

        string extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedTypes.TryGetValue(extension, out string? contentType) || file.ContentType != contentType)
        {
            throw new ArgumentException("Only png, jpg, gif and webp images are allowed.");
        }

        string uploadsFolder = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsFolder);

        string fileName = $"{Guid.NewGuid():N}{extension}";
        await using (FileStream stream = System.IO.File.Create(Path.Combine(uploadsFolder, fileName)))
        {
            await file.CopyToAsync(stream);
        }

        string url = $"/uploads/{fileName}";
        return Created(url, new UploadResultDto { Url = url });
    }
}
