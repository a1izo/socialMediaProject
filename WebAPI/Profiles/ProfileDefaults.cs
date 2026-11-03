using System.Text.Json;
using ApiContracts;
using Entities;
using RepositoryContracts;

namespace WebAPI.Profiles;

internal static class ProfileDefaults
{
    public static async Task<Profile> GetOrCreateAsync(IProfileRepository profileRepo, User user)
    {
        Profile? profile = profileRepo.GetMany().SingleOrDefault(p => p.UserId == user.Id);
        return profile ?? await CreateAsync(profileRepo, user);
    }

    public static async Task<Profile> CreateAsync(IProfileRepository profileRepo, User user)
    {
        Profile profile = await profileRepo.AddAsync(new Profile
        {
            UserId = user.Id,
            DisplayName = user.UserName,
            Tagline = "new signal on the network",
            ThemeJson = JsonSerializer.Serialize(ThemePresets.Create(ThemePresets.Default)),
            UpdatedDate = DateTime.Now
        });

        await profileRepo.ReplaceModulesAsync(user.Id, new List<ProfileModule>
        {
            Module(ProfileModuleTypes.Bio, 8, "about", new { Markdown = "hello, cybernet. this is my corner of the network." }),
            Module(ProfileModuleTypes.Stats, 4, "signal", new { Layout = "tiles" }),
            Module(ProfileModuleTypes.LatestPosts, 12, "transmissions", new { Count = 5, ShowExcerpt = true })
        });

        return profile;
    }

    public static ProfileModule Module(string type, int span, string? title, object settings, ModuleStyleDto? style = null)
    {
        return new ProfileModule
        {
            Type = type,
            Span = span,
            Title = title,
            ShowTitle = title is not null,
            IsVisible = true,
            SettingsJson = JsonSerializer.Serialize(settings),
            StyleJson = JsonSerializer.Serialize(style ?? new ModuleStyleDto())
        };
    }
}
