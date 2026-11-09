namespace ApiContracts;

// A lightweight version of the profile used for author chips and hover cards.
public class ProfileCardDto
{
    public int UserId { get; set; }
    public required string UserName { get; set; }
    public required string DisplayName { get; set; }
    public string? Tagline { get; set; }
    public string? AvatarUrl { get; set; }
    public string? BannerUrl { get; set; }
    public bool IsModerator { get; set; }
    public ProfileThemeDto Theme { get; set; } = new();
    public ProfileStatsDto Stats { get; set; } = new();
    // when the profile was last saved, for "recently restyled"
    public DateTime UpdatedDate { get; set; }
}
