using System.Text.RegularExpressions;

namespace WebAPI.Services;

internal static class MediaUrls
{
    private static readonly Regex allowed = new(@"^(/uploads/[A-Za-z0-9._-]+|/demo/[A-Za-z0-9._-]+|https://[^\s""'<>]+)$");
    private static readonly string[] kinds = { "image", "gif", "video" };

    public static void Validate(string url, string kind)
    {
        if (!allowed.IsMatch(url))
        {
            throw new ArgumentException("Media must be an uploaded file or an https address.");
        }

        if (!kinds.Contains(kind))
        {
            throw new ArgumentException("Media kind must be image, gif or video.");
        }
    }
}
