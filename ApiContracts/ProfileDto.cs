namespace ApiContracts;

public class ProfileDto
{
    public int UserId { get; set; }
    public required string UserName { get; set; }
    public required string DisplayName { get; set; }
    public string? Tagline { get; set; }
    public string? AvatarUrl { get; set; }
    public string? BannerUrl { get; set; }
    public bool IsModerator { get; set; }
    public DateTime CreatedDate { get; set; }
    public ProfileThemeDto Theme { get; set; } = new();
    public string? CustomCss { get; set; }
    public List<ProfileModuleDto> Modules { get; set; } = new();
    public List<StickerDto> Stickers { get; set; } = new();
    public ProfileStatsDto Stats { get; set; } = new();
}
