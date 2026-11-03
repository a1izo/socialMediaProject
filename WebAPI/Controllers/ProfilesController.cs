using System.Text.Json;
using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using WebAPI.Profiles;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ProfilesController : ControllerBase
{
    private const int MaxModules = 40;

    private readonly IProfileRepository profileRepo;
    private readonly IUserRepository userRepo;
    private readonly IPostRepository postRepo;

    public ProfilesController(IProfileRepository profileRepo, IUserRepository userRepo, IPostRepository postRepo)
    {
        this.profileRepo = profileRepo;
        this.userRepo = userRepo;
        this.postRepo = postRepo;
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<ProfileDto>> GetProfile([FromRoute] string userName)
    {
        User? user = userRepo.GetMany().SingleOrDefault(u => u.UserName.ToLower() == userName.ToLower());
        if (user is null)
        {
            throw new NotFoundException($"User '{userName}' not found");
        }

        Profile profile = await ProfileDefaults.GetOrCreateAsync(profileRepo, user);
        List<ProfileModule> modules = profileRepo.GetModules(user.Id).ToList();
        return Ok(DtoMapper.ToDto(profile, user, modules, await GetStatsAsync(user.Id)));
    }

    [HttpGet("cards")]
    public async Task<ActionResult<IEnumerable<ProfileCardDto>>> GetCards([FromQuery] List<int> userIds)
    {
        List<User> users = userRepo.GetMany().Where(u => userIds.Contains(u.Id)).ToList();
        List<ProfileCardDto> cards = new();
        foreach (User user in users)
        {
            Profile profile = await ProfileDefaults.GetOrCreateAsync(profileRepo, user);
            cards.Add(DtoMapper.ToCardDto(profile, user, await GetStatsAsync(user.Id)));
        }

        return Ok(cards);
    }

    [HttpPut("{userId:int}")]
    public async Task<ActionResult> UpdateProfile([FromRoute] int userId, [FromBody] UpdateProfileDto request)
    {
        DtoMapper.RequireNotBlank(request.DisplayName, "Display name");
        if (request.DisplayName.Length > 40)
        {
            throw new ArgumentException("Display name can be at most 40 characters.");
        }

        if (request.Tagline?.Length > 140)
        {
            throw new ArgumentException("Tagline can be at most 140 characters.");
        }

        if (request.Modules.Count > MaxModules)
        {
            throw new ArgumentException($"A profile can have at most {MaxModules} modules.");
        }

        foreach (ProfileModuleDto module in request.Modules)
        {
            if (!ProfileModuleTypes.All.Contains(module.Type))
            {
                throw new ArgumentException($"Unknown module type '{module.Type}'.");
            }

            if (!ProfileModuleTypes.AllowedSpans.Contains(module.Span))
            {
                throw new ArgumentException($"Module width must be one of: {string.Join(", ", ProfileModuleTypes.AllowedSpans)}.");
            }
        }

        User user = await userRepo.GetSingleAsync(userId);
        Profile profile = await ProfileDefaults.GetOrCreateAsync(profileRepo, user);

        profile.DisplayName = request.DisplayName.Trim();
        profile.Tagline = string.IsNullOrWhiteSpace(request.Tagline) ? null : request.Tagline.Trim();
        profile.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl;
        profile.BannerUrl = string.IsNullOrWhiteSpace(request.BannerUrl) ? null : request.BannerUrl;
        profile.ThemeJson = JsonSerializer.Serialize(request.Theme);
        profile.CustomCss = ProfileCss.Sanitize(request.CustomCss);
        profile.UpdatedDate = DateTime.Now;
        await profileRepo.UpdateAsync(profile);

        List<ProfileModule> modules = request.Modules
            .Select((m, index) => new ProfileModule
            {
                Type = m.Type,
                Order = index,
                Span = m.Span,
                Title = string.IsNullOrWhiteSpace(m.Title) ? null : m.Title.Trim(),
                ShowTitle = m.ShowTitle,
                IsVisible = m.IsVisible,
                SettingsJson = string.IsNullOrWhiteSpace(m.SettingsJson) ? "{}" : m.SettingsJson,
                StyleJson = JsonSerializer.Serialize(m.Style)
            })
            .ToList();
        await profileRepo.ReplaceModulesAsync(userId, modules);

        return NoContent();
    }

    private async Task<ProfileStatsDto> GetStatsAsync(int userId)
    {
        List<int> postIds = postRepo.GetMany().Where(p => p.UserId == userId).Select(p => p.Id).ToList();
        int likesReceived = 0;
        foreach (int postId in postIds)
        {
            likesReceived += await postRepo.GetLikeCountAsync(postId);
        }

        return new ProfileStatsDto
        {
            PostCount = postIds.Count,
            FollowerCount = userRepo.GetFollowers(userId).Count(),
            FollowingCount = userRepo.GetFollowing(userId).Count(),
            LikesReceived = likesReceived
        };
    }
}
