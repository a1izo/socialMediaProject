namespace ApiContracts;

public static class ProfileModuleTypes
{
    public const string Bio = "bio";
    public const string Info = "info";
    public const string Links = "links";
    public const string LatestPosts = "latestPosts";
    public const string PinnedPosts = "pinnedPosts";
    public const string Stats = "stats";
    public const string TopFriends = "topFriends";
    public const string Followers = "followers";
    public const string Gallery = "gallery";
    public const string Status = "status";
    public const string Text = "text";
    public const string Embed = "embed";
    public const string Divider = "divider";
    public const string TagCloud = "tagCloud";

    public static IReadOnlyList<string> All { get; } = new[]
    {
        Bio, Info, Links, LatestPosts, PinnedPosts, Stats, TopFriends,
        Followers, Gallery, Status, Text, Embed, Divider, TagCloud
    };

    public static int[] AllowedSpans { get; } = { 3, 4, 6, 8, 12 };
}
