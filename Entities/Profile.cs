namespace Entities;

public class Profile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; }
    public string? Tagline { get; set; }
    public string? AvatarUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string ThemeJson { get; set; }
    public string? CustomCss { get; set; }
    public string? StickersJson { get; set; }
    public DateTime UpdatedDate { get; set; }
}
