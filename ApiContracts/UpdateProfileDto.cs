namespace ApiContracts;

public class UpdateProfileDto
{
    public required string DisplayName { get; set; }
    public string? Tagline { get; set; }
    public string? AvatarUrl { get; set; }
    public string? BannerUrl { get; set; }
    public ProfileThemeDto Theme { get; set; } = new();
    public string? CustomCss { get; set; }
    public List<ProfileModuleDto> Modules { get; set; } = new();
    public List<StickerDto> Stickers { get; set; } = new();
}
