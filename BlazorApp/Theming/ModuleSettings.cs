using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp.Theming;

// The settings of every module type in one class. Each module only reads the properties
// that belong to it. Stored on the server as an opaque JSON string (ProfileModuleDto.SettingsJson),
// so new module types never need a server change.
public class ModuleSettings
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // bio, text
    public string? Markdown { get; set; }
    public string? Align { get; set; }
    public string? Size { get; set; }

    // info
    public List<InfoField>? Fields { get; set; }

    // links
    public List<LinkItem>? Links { get; set; }

    // latestPosts, followers, tagCloud
    public int? Count { get; set; }
    public bool? ShowExcerpt { get; set; }

    // pinnedPosts, topFriends
    public List<int>? PostIds { get; set; }
    public List<int>? UserIds { get; set; }

    // stats
    public string? Layout { get; set; }

    // gallery
    public List<string>? Images { get; set; }
    public int? Columns { get; set; }
    public string? Aspect { get; set; }

    // status
    public string? Emoji { get; set; }
    public string? Label { get; set; }
    public string? Text { get; set; }

    // embed, divider
    public string? Url { get; set; }
    public int? Height { get; set; }
    public string? Style { get; set; }

    public static ModuleSettings Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ModuleSettings();
        }

        try
        {
            return JsonSerializer.Deserialize<ModuleSettings>(json, options) ?? new ModuleSettings();
        }
        catch (JsonException)
        {
            return new ModuleSettings();
        }
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, options);
    }
}

public class InfoField
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

public class LinkItem
{
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
    public string Icon { get; set; } = "link";
}
