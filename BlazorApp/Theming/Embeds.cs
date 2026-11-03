using System.Text.RegularExpressions;

namespace BlazorApp.Theming;

// Only YouTube and Spotify can be embedded. Any other address gives null, and the module
// shows a hint instead of an iframe.
public static class Embeds
{
    private static readonly Regex youTubeRegex = new(
        @"^https?://(www\.|m\.)?(youtube\.com/watch\?v=|youtu\.be/|youtube\.com/shorts/)([A-Za-z0-9_-]{6,20})");
    private static readonly Regex spotifyRegex = new(
        @"^https?://open\.spotify\.com/(intl-[a-z]+/)?(track|album|playlist|episode|show|artist)/([A-Za-z0-9]{10,40})");

    public static string? ToEmbedUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        Match youTube = youTubeRegex.Match(url.Trim());
        if (youTube.Success)
        {
            return $"https://www.youtube-nocookie.com/embed/{youTube.Groups[3].Value}";
        }

        Match spotify = spotifyRegex.Match(url.Trim());
        if (spotify.Success)
        {
            return $"https://open.spotify.com/embed/{spotify.Groups[2].Value}/{spotify.Groups[3].Value}";
        }

        return null;
    }
}
