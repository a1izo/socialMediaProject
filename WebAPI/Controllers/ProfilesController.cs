using System.Text.Json;
using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using WebAPI.Profiles;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ProfilesController : ControllerBase
{
    private const int MaxModules = 40;
    private const int MaxStickers = 60;
    private static readonly string[] stickerKinds = { "glyph", "emoji", "image" };

    private readonly IProfileRepository profileRepo;
    private readonly IUserRepository userRepo;
    private readonly ProfileCardBuilder cardBuilder;

    public ProfilesController(IProfileRepository profileRepo, IUserRepository userRepo, ProfileCardBuilder cardBuilder)
    {
        this.profileRepo = profileRepo;
        this.userRepo = userRepo;
        this.cardBuilder = cardBuilder;
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
        return Ok(DtoMapper.ToDto(profile, user, modules, await cardBuilder.GetStatsAsync(user.Id)));
    }

    [HttpGet("cards")]
    public async Task<ActionResult<IEnumerable<ProfileCardDto>>> GetCards([FromQuery] List<int> userIds)
    {
        List<User> users = userRepo.GetMany().Where(u => userIds.Contains(u.Id)).ToList();
        List<ProfileCardDto> cards = new();
        foreach (User user in users)
        {
            cards.Add(await cardBuilder.BuildCardAsync(user));
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

        if (request.Stickers.Count > MaxStickers)
        {
            throw new ArgumentException($"A profile can have at most {MaxStickers} stickers.");
        }

        foreach (StickerDto sticker in request.Stickers)
        {
            if (!stickerKinds.Contains(sticker.Kind) || string.IsNullOrWhiteSpace(sticker.Value) || sticker.Value.Length > 300)
            {
                throw new ArgumentException("One of the stickers is not valid.");
            }

            if (sticker.Kind == "image")
            {
                MediaUrls.Validate(sticker.Value, "image");
            }

            sticker.X = Math.Clamp(sticker.X, 0, 100);
            sticker.Y = Math.Clamp(sticker.Y, 0, 20000);
            sticker.Size = Math.Clamp(sticker.Size, 12, 600);
            sticker.Rotation = Math.Clamp(sticker.Rotation, -180, 180);
        }

        User user = await userRepo.GetSingleAsync(userId);
        Profile profile = await ProfileDefaults.GetOrCreateAsync(profileRepo, user);

        profile.DisplayName = request.DisplayName.Trim();
        profile.Tagline = string.IsNullOrWhiteSpace(request.Tagline) ? null : request.Tagline.Trim();
        profile.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl;
        profile.BannerUrl = string.IsNullOrWhiteSpace(request.BannerUrl) ? null : request.BannerUrl;
        profile.ThemeJson = JsonSerializer.Serialize(request.Theme);
        profile.CustomCss = ProfileCss.Sanitize(request.CustomCss);
        profile.StickersJson = JsonSerializer.Serialize(request.Stickers);
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
}
