namespace BlazorApp.Services;

public class PostQuery
{
    public string? TitleContains { get; set; }
    public int? UserId { get; set; }
    public string? OrderBy { get; set; }
    public int? Take { get; set; }
    public int? TagId { get; set; }
    public int? FollowedBy { get; set; }

    public string ToQueryString()
    {
        List<string> parts = new();
        if (!string.IsNullOrWhiteSpace(TitleContains)) parts.Add($"titleContains={Uri.EscapeDataString(TitleContains)}");
        if (UserId is not null) parts.Add($"userId={UserId}");
        if (!string.IsNullOrWhiteSpace(OrderBy)) parts.Add($"orderBy={Uri.EscapeDataString(OrderBy)}");
        if (Take is not null) parts.Add($"take={Take}");
        if (TagId is not null) parts.Add($"tagId={TagId}");
        if (FollowedBy is not null) parts.Add($"followedBy={FollowedBy}");
        return parts.Count == 0 ? "" : "?" + string.Join("&", parts);
    }
}
