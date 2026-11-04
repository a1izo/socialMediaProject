using System.Text.Json;
using ApiContracts;
using BlazorApp.Components.Profile.Modules;

namespace BlazorApp.Components.Profile;

public record ModuleInfo(
    string Type,
    string Label,
    string Description,
    string Icon,
    int DefaultSpan,
    string? DefaultTitle,
    object DefaultSettings,
    Type Component);

// Everything the editor needs to know about each module type, and which component draws it.
public static class ModuleCatalog
{
    public static IReadOnlyList<ModuleInfo> All { get; } = new List<ModuleInfo>
    {
        new(ProfileModuleTypes.Bio, "bio", "a few lines about you", "user", 8, "about",
            new { Markdown = "write something about yourself." }, typeof(BioModule)),
        new(ProfileModuleTypes.Text, "text", "a block of text. start a line with # for a big heading", "type", 12, null,
            new { Markdown = "# big statement", Align = "left", Size = "lg" }, typeof(TextModule)),
        new(ProfileModuleTypes.Status, "status", "what you're doing right now", "sparkle", 4, "currently",
            new { Emoji = "✦", Label = "status", Text = "online" }, typeof(StatusModule)),
        new(ProfileModuleTypes.Info, "info", "short facts, like where you live or your pronouns", "layers", 4, "info",
            new { Fields = new[] { new { Key = "location", Value = "somewhere" } } }, typeof(InfoModule)),
        new(ProfileModuleTypes.Links, "links", "your other places on the internet", "link", 4, "links",
            new { Links = new[] { new { Label = "my site", Url = "https://example.com", Icon = "globe" } } }, typeof(LinksModule)),
        new(ProfileModuleTypes.Stats, "stats", "posts, followers, following, likes", "grid", 4, "stats",
            new { Layout = "tiles" }, typeof(StatsModule)),
        new(ProfileModuleTypes.LatestPosts, "latest posts", "your most recent posts", "send", 8, "posts",
            new { Count = 5, ShowExcerpt = true }, typeof(LatestPostsModule)),
        new(ProfileModuleTypes.PinnedPosts, "pinned posts", "posts you choose to keep at the top", "pin", 6, "pinned",
            new { PostIds = Array.Empty<int>() }, typeof(PinnedPostsModule)),
        new(ProfileModuleTypes.TopFriends, "top friends", "up to 8 people you follow", "users", 6, "top friends",
            new { UserIds = Array.Empty<int>() }, typeof(TopFriendsModule)),
        new(ProfileModuleTypes.Followers, "followers", "the avatars of your followers", "users", 6, "followers",
            new { Count = 12 }, typeof(FollowersModule)),
        new(ProfileModuleTypes.Gallery, "gallery", "your images in a grid", "image", 6, "gallery",
            new { Images = Array.Empty<string>(), Columns = 3, Aspect = "square" }, typeof(GalleryModule)),
        new(ProfileModuleTypes.Embed, "embed", "a youtube video or spotify track", "music", 6, "listening",
            new { Url = "", Height = 352 }, typeof(EmbedModule)),
        new(ProfileModuleTypes.TagCloud, "tag cloud", "the tags you use most", "tag", 4, "tags",
            new { Count = 10 }, typeof(TagCloudModule)),
        new(ProfileModuleTypes.Divider, "divider", "a line, stars, ticks or empty space", "minus", 12, null,
            new { Style = "stars", Height = 48 }, typeof(DividerModule))
    };

    public static ModuleInfo Get(string type)
    {
        return All.FirstOrDefault(m => m.Type == type) ?? All[0];
    }

    public static ProfileModuleDto Create(string type)
    {
        ModuleInfo info = Get(type);
        return new ProfileModuleDto
        {
            Type = info.Type,
            Span = info.DefaultSpan,
            Title = info.DefaultTitle,
            ShowTitle = info.DefaultTitle is not null,
            IsVisible = true,
            SettingsJson = JsonSerializer.Serialize(info.DefaultSettings),
            Style = new ModuleStyleDto()
        };
    }
}
